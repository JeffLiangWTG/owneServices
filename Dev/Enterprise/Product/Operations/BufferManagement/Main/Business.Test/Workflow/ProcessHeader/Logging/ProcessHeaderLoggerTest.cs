using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderLoggerTest : BMSTestCaseWithFactory
	{
		#region Duplicates

		public void TestAddStatusChangeEvent_WhenRunningInEarlierLocalTimeThanLastEvent_ShouldNotAddEventAgain()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "CAYHZ"; // Halifax, Nova Scotia (time zone way behind the default of Brisbane)
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_GB_HomeBranch = branch.PK;
			user.GS_GE_HomeDepartment = department.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var job = workflow.Parent;

			Factory.Save();

			BMSTestHelper.AssertWorkflowsHaveLog("There should be a job close log because the workflow had no tasks. SAD!", job, Events.JobClose, jobHeader, workflow);

			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				BMSTestHelper.CreateTask(workflow);
				Factory.Save();
			}

			var jobCloseLogs = workflow.Logs.Find(x => x.SL_SE_NKEvent == Events.JobCloseCode).ToArray();
			var jobOpenLogs = workflow.Logs.Find(x => x.SL_SE_NKEvent == Events.JobOpenCode).ToArray();

			AssertEquals(1, jobCloseLogs.Length);
			AssertEquals(1, jobOpenLogs.Length);
			AssertGreaterThan("The job close log should have a later event time than the job open log, because we've simulated it being added by a service task in an earlier time zone, and SL_EventTime is stored in local time (shocking!). SAD!",
				jobCloseLogs.Single().SL_EventTime, jobOpenLogs.Single().SL_EventTime);

			ProcessHeaderLogger_ForTest.AddJobStatusChangeEvent(workflow, true);
			Factory.Save();

			BMSTestHelper.AssertWorkflowsHaveLog("There should only be one job open event, because the workflow already had one, even though it was added in a local time zone which made it look earlier than the JobClose event. SAD!", job, Events.JobOpen, jobHeader, workflow);
		}

		public void TestAddStatusChangeEvent_WhenAlreadyAddedInMemory_ShouldNotAddEventAgain()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var job = workflow.Parent;

			Factory.Save();

			ProcessHeaderLogger_ForTest.AddJobStatusChangeEvent(workflow, true);
			ProcessHeaderLogger_ForTest.AddJobStatusChangeEvent(workflow, true);

			Factory.Save();

			BMSTestHelper.AssertWorkflowsHaveLog("The logger shouldn't add the same event twice in a row, even when the first log added hasn't been saved to the database yet. SAD!", job, Events.JobOpen, workflow);
		}

		#endregion

		#region Reference

		public void TestJobOpenReference_NotSpecified_ShouldCreateEvent()
		{
			AssertJobOpenOrCloseReference_ShouldCreateEvent(true, string.Empty, string.Empty);
		}

		public void TestJobCloseReference_NotSpecified_ShouldCreateEvent()
		{
			AssertJobOpenOrCloseReference_ShouldCreateEvent(false, string.Empty, string.Empty);
		}

		void AssertJobOpenOrCloseReference_ShouldCreateEvent(bool shouldOpenJob, string expectedReference, string expectedDisplayReference)
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			ProcessHeaderLogger_ForTest.AddJobStatusChangeEvent(workflow, shouldOpenJob);
			Factory.Save();

			var eventType = shouldOpenJob ? Events.JobOpen : Events.JobClose;
			var reloadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var log = reloadedWorkflow.Logs.MostRecentLogByEventTime(eventType);

			AssertEquals(expectedReference, log.SL_Reference);
			AssertEquals(expectedDisplayReference, log.DisplayEventReference);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "INQ");
		}

		#endregion
	}
}
