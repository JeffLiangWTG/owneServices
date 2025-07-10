using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ApprovedShapeDetailsUserControlTest : BMSTestCaseWithFactory
	{
		public void TestOpenDiagramButton()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "wrokflow");

			var diagram = Factory.New<IBMNCNShape>();
			diagram.BNS_RelatedEntityID = jobHeader.PK;
			diagram.Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ApprovedShapeDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				form.SetDataBinding(jobHeader, string.Empty);

				control.OpenDiagramButton.PerformClick();

				using (var networkDiagramForm = Application.OpenForms.OfType<ZForm>().SingleOrDefault(f => f.ControllerID == ControllerIDs.NetworkDiagram))
				{
					AssertNotNull("Should open a network diagram form", networkDiagramForm);
					var boundDiagram = (IBMNCNShape)networkDiagramForm.BusinessEntity;
					AssertEquals(diagram.Identifier, boundDiagram.Identifier);
				}
			}
		}

		public void TestDeletedProcessHeader()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow 1");

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ApprovedShapeDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				form.SetDataBinding(jobHeader, string.Empty);

				workflow.Delete();

				AssertNoExceptionThrown(() => control.SetDataBinding(workflow, string.Empty));
			}
		}
	}
}
