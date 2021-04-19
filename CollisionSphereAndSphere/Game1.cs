using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CollisionSphereAndSphere
{
	/// <summary>
	/// ゲームメインクラス
	/// </summary>
	public class Game1 : Game
	{
    /// <summary>
    /// グラフィックデバイス管理クラス
    /// </summary>
    private readonly GraphicsDeviceManager _graphics = null;

    /// <summary>
    /// スプライトのバッチ化クラス
    /// </summary>
    private SpriteBatch _spriteBatch = null;

    /// <summary>
    /// スプライトでテキストを描画するためのフォント
    /// </summary>
    private SpriteFont _font = null;

    /// <summary>
    /// モデル
    /// </summary>
    private Model _model = null;

    /// <summary>
    /// モデルの基本包括球
    /// </summary>
    private BoundingSphere _baseBoundingSphere = new BoundingSphere();

    /// <summary>
    /// 球１の位置
    /// </summary>
    private Vector3 _sphere1Position = new Vector3(0.0f, 0.0f, 0.0f);

    /// <summary>
    /// 球２の位置
    /// </summary>
    private Vector3 _sphere2Position = new Vector3(1.5f, 0.0f, -3.0f);

    /// <summary>
    /// 球１の包括球
    /// </summary>
    private BoundingSphere _sphere1BoundingSphere = new BoundingSphere();

    /// <summary>
    /// 球２の包括球
    /// </summary>
    private BoundingSphere _sphere2BoundingSphere = new BoundingSphere();

    /// <summary>
    /// 衝突フラグ
    /// </summary>
    private bool _isCollision = false;

    /// <summary>
    /// 前回のマウスの状態
    /// </summary>
    private MouseState _oldMouseState;


    /// <summary>
    /// GameMain コンストラクタ
    /// </summary>
    public Game1()
    {
      // グラフィックデバイス管理クラスの作成
      _graphics = new GraphicsDeviceManager(this);

      // ゲームコンテンツのルートディレクトリを設定
      Content.RootDirectory = "Content";

      // マウスカーソルを表示
      IsMouseVisible = true;
    }

    /// <summary>
    /// ゲームが始まる前の初期化処理を行うメソッド
    /// グラフィック以外のデータの読み込み、コンポーネントの初期化を行う
    /// </summary>
    protected override void Initialize()
    {
      // TODO: ここに初期化ロジックを書いてください

      // コンポーネントの初期化などを行います
      base.Initialize();
    }

    /// <summary>
    /// ゲームが始まるときに一回だけ呼ばれ
    /// すべてのゲームコンテンツを読み込みます
    /// </summary>
    protected override void LoadContent()
    {
      // テクスチャーを描画するためのスプライトバッチクラスを作成します
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      // フォントをコンテントパイプラインから読み込む
      _font = Content.Load<SpriteFont>("Font");

      // モデルを作成
      _model = Content.Load<Model>("Sphere");

      // 包括球取得
      _baseBoundingSphere = _model.Meshes[0].BoundingSphere;

      // 各モデル用の包括球半径設定
      _sphere1BoundingSphere.Radius = _baseBoundingSphere.Radius;
      _sphere2BoundingSphere.Radius = _baseBoundingSphere.Radius;

      // あらかじめパラメータを設定しておく
      foreach (ModelMesh mesh in _model.Meshes)
      {
        foreach (BasicEffect effect in mesh.Effects)
        {
          // デフォルトのライト適用
          effect.EnableDefaultLighting();

          // ビューマトリックスをあらかじめ設定
          effect.View = Matrix.CreateLookAt(
              new Vector3(0.0f, 10.0f, 1.0f),
              Vector3.Zero,
              Vector3.Up
          );

          // プロジェクションマトリックスをあらかじめ設定
          effect.Projection = Matrix.CreatePerspectiveFieldOfView(
              MathHelper.ToRadians(45.0f),
              (float)GraphicsDevice.Viewport.Width /
                  (float)GraphicsDevice.Viewport.Height,
              1.0f,
              100.0f
          );
        }
      }
    }

    /// <summary>
    /// ゲームが終了するときに一回だけ呼ばれ
    /// すべてのゲームコンテンツをアンロードします
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: ContentManager で管理されていないコンテンツを
      //       ここでアンロードしてください
    }

    /// <summary>
    /// 描画以外のデータ更新等の処理を行うメソッド
    /// 主に入力処理、衝突判定などの物理計算、オーディオの再生など
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Update(GameTime gameTime)
    {
      KeyboardState keyboardState = Keyboard.GetState();
      MouseState mouseState = Mouse.GetState();
      GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

      // ゲームパッドの Back ボタン、またはキーボードの Esc キーを押したときにゲームを終了させます
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      {
        Exit();
      }

      float speed = 0.1f;

      // 球１の位置を移動させる
      if (gamePadState.IsConnected)
      {
        _sphere1Position.X += gamePadState.ThumbSticks.Left.X * speed;
        _sphere1Position.Z -= gamePadState.ThumbSticks.Left.Y * speed;
      }
      if (keyboardState.IsKeyDown(Keys.Left))
      {
        _sphere1Position.X -= speed;
      }
      if (keyboardState.IsKeyDown(Keys.Right))
      {
        _sphere1Position.X += speed;
      }
      if (keyboardState.IsKeyDown(Keys.Down))
      {
        _sphere1Position.Z += speed;
      }
      if (keyboardState.IsKeyDown(Keys.Up))
      {
        _sphere1Position.Z -= speed;
      }
      if (mouseState.LeftButton == ButtonState.Pressed)
      {
        // 直前にマウスの左ボタンが押されていない場合は差分を０にする
        if (_oldMouseState.LeftButton == ButtonState.Released)
        {
          _oldMouseState = mouseState;
        }

        _sphere1Position += new Vector3((mouseState.X - _oldMouseState.X) * 0.01f,
                                           0,
                                           (mouseState.Y - _oldMouseState.Y) * 0.01f);
      }

      // マウスの状態記憶
      _oldMouseState = mouseState;

      // 衝突判定用の球を設定
      _sphere1BoundingSphere.Center =
          _sphere1Position + _baseBoundingSphere.Center;
      _sphere2BoundingSphere.Center =
          _sphere2Position + _baseBoundingSphere.Center;

      // 衝突判定
      _isCollision =
          _sphere1BoundingSphere.Intersects(_sphere2BoundingSphere);

      // 登録された GameComponent を更新する
      base.Update(gameTime);
    }

    /// <summary>
    /// 描画処理を行うメソッド
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Draw(GameTime gameTime)
    {
      // 画面を指定した色でクリアします
      GraphicsDevice.Clear(Color.CornflowerBlue);

      // 深度バッファを有効にする
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;

      foreach (ModelMesh mesh in _model.Meshes)
      {
        // 球１を描画
        foreach (BasicEffect effect in mesh.Effects)
        {
          // ワールドマトリックス（位置指定）
          effect.World = Matrix.CreateTranslation(_sphere1Position);
        }
        mesh.Draw();

        // 球２を描画
        foreach (BasicEffect effect in mesh.Effects)
        {
          // ワールドマトリックス（位置指定）
          effect.World = Matrix.CreateTranslation(_sphere2Position);
        }
        mesh.Draw();
      }

      // スプライトの描画準備
      _spriteBatch.Begin();

      // 衝突判定表示
      _spriteBatch.DrawString(_font,
          "IsCollision : " + _isCollision,
          new Vector2(30.0f, 30.0f), Color.White);

      // スプライトの一括描画
      _spriteBatch.End();

      // 登録された DrawableGameComponent を描画する
      base.Draw(gameTime);
    }
  }
}
