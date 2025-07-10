using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRAirCargoHouseUserControl : BaseAirCargoHouseUserControl
	{
		public CMRAirCargoHouseUserControl()
		{
			InitializeComponent();
		}

		protected internal override AirCargoHAWBProviderContainerControl ChildControl => cmrHouseDetailsUserControl;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MasterTabControl.PlugIns.Add(ControllerIDs.Customs.AU.AirCargoDeclarationCusUnderbondController);
		}
	}
}
