using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class EDIDepartmentChooserTest : DepartmentChooserTest
	{
		public void TestUseDefaultLoginDepartment()
		{
			Assert(((EDIDepartmentChooser)DepartmentChooser).UseDefaultLoginDepartmentForTest(EDIJobInvoicingConsumerTypes.Incident.Code));
			Assert(((EDIDepartmentChooser)DepartmentChooser).UseDefaultLoginDepartmentForTest(EDIJobInvoicingConsumerTypes.PSQuote.Code));
		}

		public void TestIncidentMainDepartments()
		{
			IJobInvoicingPlugIn testPlugin = Factory.New<NewWorkItem>();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, DepartmentChooser.GetDepartment(testPlugin));

			testPlugin = Factory.New<SupportIncident>();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, DepartmentChooser.GetDepartment(testPlugin));

			testPlugin = Factory.New<EDIProject>();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, DepartmentChooser.GetDepartment(testPlugin));
		}
	}
}
