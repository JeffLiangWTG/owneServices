using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class TaskJobWorkflowCacheHelperTest : BMSTestCaseWithFactory
	{
		public void TestPopulateCacheForTasks_NoHeader()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Main Workflow", currentComponent: config.Buffer);
			var task = CreateTask(workflow, staff.GS_Code, lowEstMinutes: 1, sequence: 1);

			Factory.Save();

			var bmBoardSectionViewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var taskChannelMap = TaskChannelMap.ForTest(section, bmBoardSectionViewModel, workflow);
			task.P9_FH_ProcessHeader = ZGuid.Empty;
			var cache = new PropertyCache();
			TaskJobWorkflowCacheHelper.PopulateCacheForTasks(taskChannelMap, section, cache);

			AssertEquals(0m, cache.GetCachedValue<decimal>(task.PK, TaskJobWorkflowCacheHelper.CacheConstants.Penetration));
		}
	}
}
