using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Test;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	abstract class RelatedDiagramsUserControlTestCase : BMSTestCaseWithFactory
	{
#if !WINZOR
		public void TestDragShapeOnDiagramOpenedFromRelatedDiagrams_ShouldNotThrowException()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Sadeness", currentComponent: buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(org);

			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Enigma");
			var shape = NetworkTestCase.CreateShape(workflow, diagram, name: "Return to Innocence");
			shape.Left = 100;
			shape.Top = 100;
			shape.Width = 300;
			shape.Height = 300;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);

			using (var form = (ZOrganisationsForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedTask))
			{
				Application.DoEvents();
				var tabControl = form.FindSingle<ZTabControl>(c => c.Name == "WorkflowTasksAndNotesTabControl");
				var tabPage = form.FindSingle<WorkflowRelatedDiagramsTabPage>();
				tabControl.SelectTab(tabPage);
				Application.DoEvents();

				var control = form.FindAll<WorkflowRelatedDiagramsUserControl>().Single();
				var grid = control.Controls.Find("DiagramsGrid", true)[0] as ZDisplayGrid;
				AssertNotNull("RelatedDiagramsUserControl should have a ZDisplayGrid on it", grid);

				grid.ListManager.Position = 0;
				grid.Select(0);
				Application.DoEvents();

				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, 10, 10, 0);
				control.DiagramsGrid_MouseDoubleClick(grid, mouseEvent);
				Application.DoEvents();

				using (var diagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().Single())
				{
					var elementHost = diagramForm.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
					var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

					var diagramControl = networkDiagram.MainDiagramControl.FindName("NetworkControl") as NetworkView;

					var node = networkDiagram.FindNodeItemOrDefault("Return to Innocence").DataContext as NodeViewModel;
					NetworkTestHelper.SimulateNodeDragCompletedEventOnNodeItem(diagramControl, node);
					Application.DoEvents();
				}
			}
		}
#endif
	}
}
