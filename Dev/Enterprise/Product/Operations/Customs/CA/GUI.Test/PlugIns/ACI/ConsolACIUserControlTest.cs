using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ConsolACIUserControlTest : TestCaseWithFactory
	{
		public void TestContainerGridReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var house1 = oceanBill.HouseBills.AddNew();
			oceanBill.HouseBills.AddNew();
			using (var form = new ZForm(oceanBill))
			using (var userControl = new ConsolACIUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				Assert(userControl.containersGrid.ReadOnly);
				house1.CA_OverrideFreightDefaults = true;
				Assert(!userControl.containersGrid.ReadOnly);
			}
		}
	}
}
