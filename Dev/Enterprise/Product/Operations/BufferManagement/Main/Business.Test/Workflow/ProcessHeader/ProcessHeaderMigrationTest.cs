using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderMigrationTest : BMSTestCaseWithFactory
	{
		public void TestHistoricJobDontHasChangesOnLoad()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			CreateTaskWithoutBMS(job, "On the day");
			CreateTaskWithoutBMS(job, "I saw a bee");
			CreateTaskWithoutBMS(job, "I felt things");
			CreateTaskWithoutBMS(job, "Inside of me");

			Factory.Save();

			BMSTestHelper.EnableBMSInRegistry();
			var newFactory = new BusinessObjectFactory();
			BMSTestHelper.CreateSystem(newFactory, "WKI");
			newFactory.Save();

			AssertEquals("If this fails, that's wild.", false, ((BusinessObject)job).HasChanges);

			var header = ProcessJobHeader.GetForParent(job, Factory);
			AssertNotNull(header);
			AssertEquals("But more likely this fails, if the test has regressed.", false, ((BusinessObject)job).HasChanges);
		}
	}
}
