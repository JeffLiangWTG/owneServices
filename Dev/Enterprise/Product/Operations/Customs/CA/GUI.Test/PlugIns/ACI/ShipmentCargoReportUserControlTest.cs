using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ShipmentCargoReportUserControlTest : TestCaseWithFactory
	{
		public void TestContainersGridReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = oceanBill.HouseBills.AddNew();
			using (var form = new ZForm(house))
			using (var userControl = new ShipmentCargoReportUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				Assert(userControl.containersGrid.ReadOnly);
				house.CA_OverrideFreightDefaults = true;
				Assert(!userControl.containersGrid.ReadOnly);
			}
		}

		public void TestOceanBillDetailsTabPagetext()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			using (var form = new ZForm(house))
			{
				var control = new ShipmentCargoReportUserControl();
				form.Controls.Add(control);
				control.SetDataBinding(house, "");
				AssertEquals("Ocean Bill", control.OceanBillDetailsTabPage.Text);
				oceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
				control.SetDataBinding(house, "");
				AssertEquals("Master Bill", control.OceanBillDetailsTabPage.Text);
			}
		}
	}
}
