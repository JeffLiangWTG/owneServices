using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccComplianceReportProcessTask))]
	public class AccComplianceReportProcessTaskTest : ProcessTaskTest
	{
		public void TestParentCotrollerID()
		{
			var task = (AccComplianceReportProcessTask)GetNewBusinessObject();
			AssertEquals(ControllerIDs.AccComplianceReport, task.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			return ((IWorkflowProvider)report).WorkflowItems.AddNew();
		}
	}
}
