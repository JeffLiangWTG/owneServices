namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class LADTTemporyStorageUserControlForPlugin : CINTemporyStorageUserControlForPlugin
	{
		public LADTTemporyStorageUserControlForPlugin() : base()
		{
			InitializeComponent();
		}

		protected override CusTempStorageDecUserControl GetTemporaryDeclarationUserControl() => new ISTAndLADTCusTempStorageDecUserControl();
	}
}
