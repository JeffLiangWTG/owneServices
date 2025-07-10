using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	class OrganisationConsigneePluginUserControlTest : TestCaseWithFactory
	{
		public void TestDeltaG1SubProcedureControl()
		{
			using (var form = new ZForm())
			using (var testUserControl = new OrganisationConsigneePlugInUserControl())
			{
				form.Controls.Add(testUserControl);
				form.Show();

				var deltaG1SubProcedureTypeDropEdit = (ZDropEdit)testUserControl.Controls.Find("DeltaG1SubProcedureTypeDropEdit", true)[0];
				AssertEquals("A DeltaG1SubProcedureType field should show", true, deltaG1SubProcedureTypeDropEdit.Visible);

				var vatProcedureDateLimitDateEdit = (ZDateEdit)testUserControl.Controls.Find("VatProcedureDateLimitDateEdit", true)[0];
				AssertEquals("A VATProcedureDateLimit field should show", true, vatProcedureDateLimitDateEdit.Visible);
			}
		}
	}
}
