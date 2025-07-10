using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVSSubHeadersUserControlTest : TestCaseWithFactory
	{
		public void TestLVSSubHeadersGridColumneStyles()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Invoices.AddNew();

			using (var control = new LVSSubHeadersUserControl())
			{
				control.SetDataBinding(declaration, "");
				control.Show();

				Assert(control.LVSSubHeadersGrid.Visible);
				AssertEquals("Number of Column Styles.", 28, control.LVSSubHeadersGrid.ColumnStyles.Count);
			}
		}
	}
}
