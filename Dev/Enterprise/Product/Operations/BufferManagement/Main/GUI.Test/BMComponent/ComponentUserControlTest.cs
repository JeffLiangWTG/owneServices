using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ComponentUserControlTest : BMSTestCaseWithFactory
	{
		public void TestTryReallocateTask_SameComponent()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = workflow.Parent.WorkflowItems.AddNew();
			workflow.FH_FC_CurrentComponent = bucket.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new ComponentUserControl(viewModel))
			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				AssertEquals(true, control.TryReallocateTask(taskCard));
			}
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}
}
