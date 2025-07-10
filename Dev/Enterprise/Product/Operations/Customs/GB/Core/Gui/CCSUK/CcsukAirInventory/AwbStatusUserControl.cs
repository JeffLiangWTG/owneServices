using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class AwbStatusUserControl : ZUserControl
	{
		public AwbStatusUserControl()
		{
			InitializeComponent();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(LinkLabelEntry, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void OnAfterFirstBinding(System.EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var bindingHawb = BindingSource.DataSource as CusHAWB;
			if (bindingHawb != null && bindingHawb.Shipment != null)
			{
				LinkLabelEntry.Visible = false;
			}
		}

		public void LinkLabelEntry_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var bindingHawb = BindingSource.DataSource as CusHAWB;
			CusHAWB declarationHawb = null;
			if (bindingHawb != null && bindingHawb.Declaration != null)
			{
				declarationHawb = bindingHawb;
			}
			else
			{
				var basic = BindingSource.DataSource as CusMAWB;
				if (basic != null && basic.MasterLevelHouseHelper.Declaration != null)
				{
					declarationHawb = basic.MasterLevelHouseHelper;
				}
			}
			if (declarationHawb != null)
			{
				if (declarationHawb.Declaration.Shipment != null)
				{
					new DependentObjectControllerHelper(new DependentFormPresenter()).EditExisting(declarationHawb.Declaration, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				}
				else
				{
					new DependentObjectControllerHelper(new DependentFormPresenter()).EditExisting(declarationHawb.Declaration, ControllerIDs.Customs.JobDeclaration);
				}
			}
		}
	}
}
