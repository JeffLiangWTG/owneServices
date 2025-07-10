namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class BaseAirCargoHouseUserControl : AirCargoHAWBProviderContainerControl
	{
		public BaseAirCargoHouseUserControl()
		{
			InitializeComponent();
			this.AirCargoHouseCustomFieldsControl.NothingSetupMessageLabelText = Enterprise.Customs.AU.Declaration.GUI.Res.GetString("BaseACAStandaAlone|BBD8F692-D0E0-48D4-A1A7-0344B348EF9F", "To make use of this tab, please setup Air Cargo House (HAC) custom fields in Workflow Manager");
		}

		public virtual void SetupPlugins()
		{
		}
	}
}
