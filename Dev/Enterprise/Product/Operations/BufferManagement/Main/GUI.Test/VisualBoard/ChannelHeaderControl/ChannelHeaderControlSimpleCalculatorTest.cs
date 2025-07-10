using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ChannelHeaderControlSimpleCalculatorTest : ChannelHeaderControlTest
	{
		[TestDate(2013, 4, 16)]
		public override void TestShowCapacityMenuItem()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			CapacitySimpleQueryContentProviderTest.EnableSimpleCapacityUsingExperimentalSettings(Factory, config.System.PK, true);
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, config.CCR.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, 60 * 3);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Bucket);
			var task2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			viewModel.ComponentGrid.AllocateTasks_ForTest(config.BufferSection, viewModel, new[] { task1, task2 });

			using (var control = (ChannelHeaderControl)ChannelHeaderControlTestHelpers.GetControl(config.CCR, system, config.BufferSection, viewModel).Controls[0])
			{
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);
				var menuItem = (ZToolStripMenuItem)control.ContextMenuStrip.Items[0];
				AssertEquals("Show Capacity Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("CCR", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals(@"Total Capacity: 64 hours

Allocated Capacity in buffer: 4.5 hours

Please note: Capacity breakdowns by zone are not available because the simple capacity calculator is enabled for a Buffer Management System that this visual board is based on.
Enabling the simple capacity calculator provides a significant performance improvement and is recommended.
If capacity breakdowns by zone are required for your organization, please raise a CR5 incident for the Buffer Management (BUF) module and include the visual board name.

Available Capacity: 59.5 hours

Calculated at: 16-Apr-2013 10:00:00", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();
			base.SetUp();
		}
	}
}
