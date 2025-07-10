using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class ShapeScheduleUserControlTest : NetworkTestCase
	{
		public void TestChangeCurrentDataItem_ShapesLinkedToWorkflows_ShouldUpdateScheduleVisibility()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagramShape);
			network.SwitchToScaled();
			var diagram = network.DiagramEntity;
			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);

			var workflowShape = CreateShape(workflow, diagram);
			((IApprovable)workflowShape).Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			ShowFormAndAssertPanelsVisible(diagram, isDurationPanelVisible: true, isTimePanelVisible: false);
			ShowFormAndAssertPanelsVisible(workflowShape, isDurationPanelVisible: true, isTimePanelVisible: false);

			diagram.ScheduledStartTimeLocal = ZDateTime.Now;
			network.RefreshSchedules();

			ShowFormAndAssertPanelsVisible(diagram, isDurationPanelVisible: false, isTimePanelVisible: true);
			ShowFormAndAssertPanelsVisible(workflowShape, isDurationPanelVisible: false, isTimePanelVisible: true);
		}

		public void TestChangeCurrentDataItem_ShapesNotLinkedToWorkflows_ShouldUpdateScheduleVisibility()
		{
			var diagramShape = CreateDiagram(Factory);
			var network = CreateNetwork(diagramShape);
			network.SwitchToScaled();
			var diagram = network.DiagramEntity;
			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);

			var shape = CreateShape(diagram);
			((IApprovable)shape).Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			ShowFormAndAssertPanelsVisible(diagram, isDurationPanelVisible: true, isTimePanelVisible: false);
			ShowFormAndAssertPanelsVisible(shape, isDurationPanelVisible: true, isTimePanelVisible: false);

			diagram.ScheduledStartTimeLocal = ZDateTime.Now;
			network.RefreshSchedules();

			ShowFormAndAssertPanelsVisible(diagram, isDurationPanelVisible: false, isTimePanelVisible: true);
			ShowFormAndAssertPanelsVisible(shape, isDurationPanelVisible: false, isTimePanelVisible: true);

			diagram.ScheduledStartTimeLocal = ZDateTime.Empty;
			diagram.ScheduledFinishTimeLocal = ZDateTime.Now;
			network.RefreshSchedules();

			ShowFormAndAssertPanelsVisible(diagram, isDurationPanelVisible: false, isTimePanelVisible: true);
			ShowFormAndAssertPanelsVisible(shape, isDurationPanelVisible: false, isTimePanelVisible: true);
		}

		static void ShowFormAndAssertPanelsVisible(ShapeNetworkEntity entity, bool isDurationPanelVisible, bool isTimePanelVisible)
		{
			using (var form = new ShapeEntityDetailsForm(entity))
			{
				form.Show();

				var shapeDetailsControl = form.FindAll<ShapeScheduleUserControl>().Single();

				AssertEquals("DurationsPanel visibility", isDurationPanelVisible, shapeDetailsControl.DurationsPanel.Visible);
				AssertEquals("TimesPanel visibility", isTimePanelVisible, shapeDetailsControl.TimesPanel.Visible);

				AssertEquals(shapeDetailsControl.DurationsPanel.Left, shapeDetailsControl.TimesPanel.Left);
			}
		}

		public void TestTimeFields_ShouldBeReadonly()
		{
			var diagramShape = CreateDiagram(Factory);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;

			using (var form = new ShapeEntityDetailsForm(diagram))
			{
				form.Show();

				var shapeDetailsControl = form.FindSingle<ShapeScheduleUserControl>();
				var timeEditControls = shapeDetailsControl.FindAll<ZDateEdit>();

				CombineAssertions("All date edit fields should be readonly as this is information only. The edit fields are shown on the scheduling tab.", () =>
				{
					foreach (var control in timeEditControls)
					{
						AssertEquals(control.Name, true, control.ReadOnly);
					}
				});
			}
		}

		public void TestTimeFields_ShouldBeReadonlyIfShapeHasParent()
		{
			var diagramShape = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			Assert("Non-scaled diagram, therefore schedule fields should be readonly", diagram.ScheduledStartTimeLocalInfo.ReadOnly);
			Assert("Non-scaled diagram, therefore schedule fields should be readonly", diagram.ScheduledFinishTimeLocalInfo.ReadOnly);

			network.SwitchToScaled();
			Assert("This is top level diagram without a parent", !diagram.ScheduledStartTimeLocalInfo.ReadOnly);
			Assert("This is top level diagram without a parent", !diagram.ScheduledFinishTimeLocalInfo.ReadOnly);

			var subDiagram = networkViewModel.CreateNewShape(diagram);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDiagram = newFactory.Load<BMNCNShape>(subDiagram.Shape.PK);
			var reloadedDiagramEntity = NetworkTestCase.CreateNetwork(reloadedDiagram).DiagramEntity;

			using (var form = new ShapeEntityDetailsForm(reloadedDiagramEntity))
			{
				form.Show();

				var propertiesControl = form.FindSingle<ShapePropertiesUserControl>();
				var dateControls = propertiesControl.FindAll<ZDateEdit>();

				foreach (var control in dateControls)
				{
					if (control.Name == "ScheduledStartDateEdit" || control.Name == "ScheduledFinishDateEdit")
					{
						AssertEquals(control.Name + ": date control should be readonly for this diagram because it has a parent", true, control.ReadOnly);
					}
				}
			}
		}

		public void TestHintLabel_ShouldIndicateWhenScheduleIsOutOfDate()
		{
			var diagramShape = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var diagram = network.DiagramEntity;
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			network.Refresh(RefreshType.Saving);
			Factory.Save();

			using (var form = new ShapeEntityDetailsForm(shape2))
			{
				form.Show();

				var label = (ZLabel)form.Controls.Find("SchedulesOutOfDateLabel", true)[0];
				var splitContainer = form.FindSingle<KSplitContainer>();

				AssertEquals(string.Empty, label.Text);
				AssertEquals(true, splitContainer.Panel1Collapsed);
			}

			shape1.Width += 100;

			using (var form = new ShapeEntityDetailsForm(shape2))
			{
				form.Show();

				var label = (ZLabel)form.Controls.Find("SchedulesOutOfDateLabel", true)[0];
				var splitContainer = form.FindSingle<KSplitContainer>();

				AssertEquals("Changing the schedules on another shape should cause all shapes to display the warning since all schedules could be impacted by a network change", "Save the diagram to ensure this information is up-to-date.", label.Text);
				AssertEquals(false, splitContainer.Panel1Collapsed);
			}

			network.Refresh(RefreshType.Saving);
			Factory.Save();

			using (var form = new ShapeEntityDetailsForm(shape2))
			{
				form.Show();

				var label = (ZLabel)form.Controls.Find("SchedulesOutOfDateLabel", true)[0];
				var splitContainer = form.FindSingle<KSplitContainer>();

				AssertEquals(string.Empty, label.Text);
				AssertEquals(true, splitContainer.Panel1Collapsed);
			}
		}
	}
}
