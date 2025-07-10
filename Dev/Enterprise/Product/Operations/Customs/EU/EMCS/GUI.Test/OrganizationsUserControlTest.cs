using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	class OrganizationsUserControlTest : TestCaseWithFactory
	{
		public void TestDocAddressControls()
		{
			using (var form = new ZForm(declaration))
			using (var control = new OrganizationsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var dispatchWarehouseDocAddressControl = control.FindSingle<ZDocAddressControl>("DispatchWarehouseDocAddressControl");
					AssertEquals("DispatchWarehouseDocAddressControl", ZDocAddressControlDisplayMode.ShowOverrideAndTabs, dispatchWarehouseDocAddressControl.DisplayMode);

					var destinationWarehouseDocAddressControl = control.FindSingle<ZDocAddressControl>("DestinationWarehouseDocAddressControl");
					AssertEquals("DestinationWarehouseDocAddressControl", ZDocAddressControlDisplayMode.ShowOverrideAndTabs, destinationWarehouseDocAddressControl.DisplayMode);

					var carrierAgentDocAddressControl = control.FindSingle<ZDocAddressControl>("CarrierAgentDocAddressControl");
					AssertEquals("CarrierAgentDocAddressControl", ZDocAddressControlDisplayMode.ShowOverrideAndTabs, carrierAgentDocAddressControl.DisplayMode);

					var transporterDocAddressControl = control.FindSingle<ZDocAddressControl>("TransporterDocAddressControl");
					AssertEquals("TransporterDocAddressControl", ZDocAddressControlDisplayMode.ShowOverrideAndTabs, transporterDocAddressControl.DisplayMode);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
