using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	[UseSnapshotProtection]
	public class ReleaseGateFailureLoggerTest : BMSTestCaseWithFactory
	{
		public void TestLogAndFlush()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>();
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var component = BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory));

			Factory.Save();

			var logger = new ReleaseGateLoggerWithFailureServices(releaseLogFailureService);
			logger.LogReleaseFailure(workflow1, component, "workflow1 failed");
			logger.LogReleaseFailure(workflow2, component, "workflow2 failed");
			logger.LogReleaseFailure(workflow3, component, "workflow3 failed");

			CombineAssertions("The failures are only queued, not submitted to the cache yet, so no cache, and yet...", () =>
			{
				AssertEquals(ProcessHeader.ReleaseFailureReasonNotAvailableMessage, workflow1.GetReleaseFailureReasonForBuffer(component));
				AssertEquals(ProcessHeader.ReleaseFailureReasonNotAvailableMessage, workflow2.GetReleaseFailureReasonForBuffer(component));
				AssertEquals(ProcessHeader.ReleaseFailureReasonNotAvailableMessage, workflow3.GetReleaseFailureReasonForBuffer(component));
			});

			var newFactory = new BusinessObjectFactory();

			workflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			workflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			workflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			var startingHitCount = Factory.GetTableHitCount(StmNoteSchema.Constants.TableName);

			logger.CommitAllLogs(Factory);
			Factory.Save();
			AssertTableHitCount("StmNote should no longer be used to log release failures, and yet...", startingHitCount, StmNoteSchema.Constants.TableName);
			CombineAssertions("Should cache", () =>
			{
				AssertEquals("workflow1 failed", workflow1.GetReleaseFailureReasonForBuffer(component));
				AssertEquals("workflow2 failed", workflow2.GetReleaseFailureReasonForBuffer(component));
				AssertEquals("workflow3 failed", workflow3.GetReleaseFailureReasonForBuffer(component));
			});

			newFactory = new BusinessObjectFactory();

			workflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			workflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			workflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			logger.LogReleaseFailure(workflow1, component, "workflow1 failed");
			logger.LogReleaseFailure(workflow2, component, "workflow2 failed again");

			newFactory = new BusinessObjectFactory();

			workflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			workflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			workflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			var dbHits = Factory.GetTableHitCount(StmNoteSchema.Constants.TableName);
			logger.CommitAllLogs(Factory);
			Factory.Save();
			AssertTableHitCount("StmNote should no longer be used to log release failures, and yet...", dbHits, StmNoteSchema.Constants.TableName);
			CombineAssertions("Should cache again", () =>
			{
				AssertEquals("workflow1 failed", workflow1.GetReleaseFailureReasonForBuffer(component));
				AssertEquals("workflow2 failed again", workflow2.GetReleaseFailureReasonForBuffer(component));
				AssertEquals(ProcessHeader.ReleaseFailureReasonNotAvailableMessage, workflow3.GetReleaseFailureReasonForBuffer(component));
			});
		}

		public void TestClearReleaseFailure()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var buffer = config.Buffer;
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Bucket);
			Factory.Save();

			var logger = new ReleaseGateLoggerWithFailureServices(releaseLogFailureService);
			logger.LogReleaseFailure(workflow, config.Buffer, "You are stuck😀");
			logger.CommitAllLogs(Factory);

			AssertEquals("You are stuck😀", workflow.GetReleaseFailureReasonForBuffer(buffer));
			AssertEquals(0, releaseLogFailureService.Logs.Count);
		}

		readonly ReleaseGateFailureLogService releaseLogFailureService = new ReleaseGateFailureLogService();

		protected override void SetUp()
		{
			base.SetUp();
			Factory.ServiceContainer.RemoveService<ReleaseGateFailureLogService>();
			Factory.ServiceContainer.AddService(releaseLogFailureService, typeof(ReleaseGateFailureLogService));
		}
	}
}
