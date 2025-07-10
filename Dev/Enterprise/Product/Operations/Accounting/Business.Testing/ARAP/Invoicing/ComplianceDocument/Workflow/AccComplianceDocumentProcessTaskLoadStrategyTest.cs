using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccComplianceDocumentProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestTypeDeciderWorks()
		{
			var arComplianceDocument = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			var apComplianceDocument = Factory.NewWithValidTestData<APComplianceDocumentHeader>();

			var arTask = MasterFilesTestHelper.CreateTask(arComplianceDocument);
			var apTask = MasterFilesTestHelper.CreateTask(apComplianceDocument);

			arTask.P9_Description = "Arrr";
			apTask.P9_Description = "App?";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var typeDecider = ProcessTaskTypeDecider.GetInstance();
			var arResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new ARComplianceDocumentWorkflowDescriptor()));
			var apResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new APComplianceDocumentWorkflowDescriptor()));

			AssertEquals(1, arResults.Length);
			AssertEquals(1, apResults.Length);

			AssertEquals("Arrr", arResults[0].P9_Description);
			AssertEquals("App?", apResults[0].P9_Description);
		}
	}
}