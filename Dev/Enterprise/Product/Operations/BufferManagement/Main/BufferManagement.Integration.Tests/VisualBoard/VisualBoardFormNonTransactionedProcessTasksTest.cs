using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class VisualBoardFormNonTransactionedProcessTasksTest : VisualBoardFormBaseNonTransactionedTest
	{
		[RequiresSTA]
		public void TestOpenFormFromProcessTasksModuleOnVisualBoardModulePanel_NoThreadSentryErrors_Job()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BufferBoard;
			var moduleSection = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessTasks, board);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, string.Format("workflow", config.Bucket));
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			AssertShowFormFromBoard_ShouldOpenInMainThread<ZOrganisationsForm>(board, "View");
		}

		[RequiresSTA]
		public void TestOpenFormFromProcessTasksModuleOnVisualBoardModulePanel_NoThreadSentryErrors_Standalone()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BufferBoard;
			var moduleSection = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessTasks, board);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			AssertShowFormFromBoard_ShouldOpenInMainThread<TaskManagementForm>(board, "View");
		}
	}
}
