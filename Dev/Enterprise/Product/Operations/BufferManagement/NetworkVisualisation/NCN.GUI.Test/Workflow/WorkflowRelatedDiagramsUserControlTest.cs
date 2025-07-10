using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class WorkflowRelatedDiagramsUserControlTest : RelatedDiagramsUserControlTestCase
	{
		public void TestGridDisplayingCorrectDiagrams()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Naxxramas Raid");

			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "WoW raiding development diagram");
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Balance tuning diagram");
			var diagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Loot distribution balancing diagram");
			var defaultDiagram = NetworkTestCase.CreateDefaultDiagram(workflow.JobHeader, name: "Default Diagram");

			var shape1 = NetworkTestCase.CreateShape(workflow, diagram1, name: "Naxx 10 man");
			var shape2 = NetworkTestCase.CreateShape(workflow, diagram2, name: "Naxx 25 man");
			var shape3 = NetworkTestCase.CreateShape(workflow, diagram3, name: "Naxx 40 man");
			var defaultWorkflow = NetworkTestCase.CreateShape(workflow, defaultDiagram, name: "Default Workflow", shapeType: ShapeTypeList.Codes.DefaultWorkflow);

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowRelatedDiagramsUserControl(workflow))
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("DiagramsGrid", true)[0] as ZDisplayGrid;
				AssertNotNull("RelatedDigramsUserControl should have a ZDisplayGrid on it", grid);

				var shapeList = new List<BMNCNShape>() { shape1, shape2, shape3 };
				var message = @"
Given an open form with a related diagrams grid
When workflow1 appears in 3 diagrams
Then shapes 1,2,3 should be displayed in the grid
";
				AssertContainsExactElementsInAnyOrder(message, shapeList, grid.List);
			}
		}
	}
}
