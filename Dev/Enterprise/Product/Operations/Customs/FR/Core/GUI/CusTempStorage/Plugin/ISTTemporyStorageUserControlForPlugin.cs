namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class ISTTemporyStorageUserControlForPlugin : CINTemporyStorageUserControlForPlugin
	{
		public ISTTemporyStorageUserControlForPlugin() : base()
		{
			InitializeComponent();
			UpdateVisibilityOfDDTNumber();
		}

		void UpdateVisibilityOfDDTNumber()
		{
			var ddtNumberText = (ISTAndLADTCusTempStorageDecUserControl)this.tempStorageDecTabPageUserControl;
			ddtNumberText.updateVisibilityOfDDTNumberTextBox(true);
		}

		protected override CusTempStorageDecUserControl GetTemporaryDeclarationUserControl() => new ISTAndLADTCusTempStorageDecUserControl();
	}
}
