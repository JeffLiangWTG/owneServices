using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class JobRelatedDiagramsUserControlTest : RelatedDiagramsUserControlTestCase
	{
		public void TestGridShouldDisplayCorrectDiagrams()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 3");

			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 2");
			var diagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 3");
			var defaultDiagram = NetworkTestCase.CreateDefaultDiagram(workflow1.JobHeader, name: "Default Diagram");

			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram1, name: "Shape 1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram2, name: "Shape 2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram3, name: "Shape 3");
			var unlinkedShape = NetworkTestCase.CreateShape(diagram3, name: "Unlinked shape");
			var defaultWorkflow = NetworkTestCase.CreateShape(workflow1, defaultDiagram, name: "Default Workflow", shapeType: ShapeTypeList.Codes.DefaultWorkflow);

			var unrelatedJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, "Unrelated Job Header");
			var unrelatedWorkflow = BMSTestHelper.CreateWorkflow(unrelatedJobHeader, "Unrelated Workflow");

			Factory.Save();

			using (var form = new ZForm(workflow1))
			using (var userControl = new JobRelatedDiagramsUserControlProvider().GetUserControl())
			{
				var control = (ZUserControl)userControl;
				form.Controls.Add(control);
				control.SetDataBinding(jobHeader.Parent, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("DiagramsGrid", true)[0] as ZDisplayGrid;
				AssertNotNull("RelatedDigramsUserControl should have a ZDisplayGrid on it", grid);

				var shapeList = new List<BMNCNShape>() { shape1, shape2, shape3 };
				AssertContainsExactElementsInAnyOrder(shapeList, grid.List);
			}
		}

		public void TestGridShouldBeEmpty_WhenBMSIsNotConfigured()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram1, name: "Shape 1");

			Factory.Save();

			using (var form = new ZForm(workflow1))
			using (var userControl = new JobRelatedDiagramsUserControlProvider().GetUserControl())
			{
				var control = (ZUserControl)userControl;
				form.Controls.Add(control);
				control.SetDataBinding(jobHeader.Parent, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("DiagramsGrid", true)[0] as ZDisplayGrid;
				AssertNotNull("RelatedDigramsUserControl should have a ZDisplayGrid on it", grid);

				AssertNull(grid.List);
			}
		}
	}
}
