using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccComplianceReportProcessTaskCollection))]
	public class AccComplianceReportProcessTaskCollectionTest : ProcessTaskCollectionTest<AccComplianceReportProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(AccComplianceReportProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override AccComplianceReportProcessTaskCollection GetCollectionToTestCore()
		{
			return (AccComplianceReportProcessTaskCollection)((IWorkflowProvider)Factory.NewWithValidTestData<AccComplianceReport>()).WorkflowItems;
		}

		#endregion
	}
}
