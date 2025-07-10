using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.VisualBoards.GUI.Test
{
	[UseSnapshotProtection]
	class VisualBoardDeadlockTest : NonTransactionedTestCase
	{
#if !WINZOR
		public void TestDisplayDisplayCardWithWrongDataType()
		{
			var originalIsUnitTestingProductionFunctionality = Globals.GetIsUnitTestingProductionFunctionality();

			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				ExceptionReporter.Instance.TestingDoReportException.Value = true;

				var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
				var board = BMSTestHelper.CreateBoard(config.System);
				var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
				var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "A Aa");

				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

				var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
				customisation.FM_JobType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
				customisation.Height = 300;

				BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);
				var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_Description, PropertyTypeList.Codes.DateTime);

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Worm", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
				var cardTask = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, description: "Task on a card");

				Factory.Save();

				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(board))
				{
					form.AwaitAll();
					AssertNull("Opening the same board multiple time shouldn't do much.", VisualBoardFormDisplayer.ShowBoard(board));
				}
			}
			finally
			{
				Globals.SetIsUnitTestingProductionFunctionality(originalIsUnitTestingProductionFunctionality);
			}
		}
#endif
	}
}
