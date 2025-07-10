using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	class NudgeControlsTest : BMSTestCaseWithFactory
	{
		public void TestVisible()
		{
			var component = CreateBucket(CreateSystem("ORG"));
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = component.PK;
			var task = Factory.New<ProcessTask>();
			task.P9_FH_ProcessHeader = workflow.PK;

			var viewModel = new ControlCustomisationViewModel(Factory.New<BMControlCustomisation>(), new TaskCardContent(task, null));

			using (var control = new NudgeControls(viewModel))
			{
				AssertEquals(true, control.Visible);
			}

			component.FC_Type = BMComponentTypeList.Codes.Buffer;

			using (var control = new NudgeControls(viewModel))
			{
				AssertEquals(false, control.Visible);
			}
		}
	}
}
