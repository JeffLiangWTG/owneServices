using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class HeaderControlProviderTest : BMSTestCaseWithFactory
	{
		#region Show Channel Capacity

		public void TestShowChannelCapacity_ForZoneHeading()
		{
			var section = CreateBoardSection(CreateBuffer(CreateSystem()));
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var map = CardAllocationMap.NewAllocationMap(section, viewModel, TaskChannelMap.Empty);
			viewModel.ComponentGrid.RefreshComponent(Factory, viewModel, map, false); // If there's no CardAllocationMap, we don't show capacity!

			var cell = new CellContent(0, 0, CellContentType.ZoneHeading);

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);
				headerControl.ShowChannelCapacity();
				AssertEquals(@"buffer Total: 0 hours
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowChannelCapacityWithNullSection()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			var resource = CreateStaffInCurrentBranchDept("T0X", "Arne");

			var map = CardAllocationMap.NewAllocationMap(section, viewModel, TaskChannelMap.Empty);
			viewModel.ComponentGrid.RefreshComponent(Factory, viewModel, map, false); // If there's no CardAllocationMap, we don't show capacity!

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
				var cell = new CellContent(0, 0, CellContentType.ZoneHeading);
				var vbChannel = viewModel.CreateChannelForTest(resource);
				cell.Channel = vbChannel;

				var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);

				section.Delete();

				Factory.Save();

				AssertNoExceptionThrown(() => headerControl.ShowChannelCapacity());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		[TestDate(2000, 1, 3)]
		public void TestShowChannelCapacity_CCRHeader_TotalTasksDuration()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			var section = config.Section;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			Factory.Save();

			var viewModel = config.SectionViewModel;
			var ccrPrimaryAxis = viewModel.ComponentGrid.Cells.Where(c => c.Channel != null && c.Channel.IsCCRChannel(section)).Select(c => c.PrimaryAxis).First();

			var allTasks = new List<ProcessTask>();
			var tuples = new List<Tuple<int, int, ProcessTask[]>>();

			for (int day = 1; day <= 12; day++)
			{
				var durationInMinutes = day <= 7
					? 15 // pre-constrained
					: 30; // post-constrained

				var workflow = BMSTestHelper.CreateWorkflowAndTask(
					Factory,
					completionStatement: $"Workflow {day}",
					currentComponent: config.Buffer,
					releaseDateTime: ZDateTime.Today.AddDays(-day),
					staffCode: config.CCR.GS_Code,
					lowEstMinutes: durationInMinutes,
					description: $"workflow {day} - task - {config.CCR.GS_Code}");

				config.Workflows.Add(workflow);

				var row = day + 1; // skip 1 for channel-header
				var col = ccrPrimaryAxis;

				var task = workflow.Tasks.First();
				allTasks.Add(task);
				tuples.Add(Tuple.Create(row, col, new[] { task }));
			}

			Factory.Save();

			viewModel = config.ResetViewModel();

			var map = CardAllocationMap.NewAllocationMap(section, viewModel, TaskChannelMap.Empty);
			viewModel.ComponentGrid.RefreshComponent(Factory, viewModel, map, false); // If there's no CardAllocationMap, we don't show capacity!

			AssertTasksInCell("GIVEN 1 task per day for 12-days aging", section, viewModel, allTasks.ToArray(), tuples.ToArray());

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var ccrTargetSecondaryAxis = 9;
				var ccrTargetCellControl = componentControl.Table.GetControlFromPosition(ccrPrimaryAxis - 1, ccrTargetSecondaryAxis);
				var ccrTargetControl = ccrTargetCellControl is HeaderControlProvider.HeaderWrapperControl ? ccrTargetCellControl.Controls[0] : ccrTargetCellControl;
				var ccrTargetLabel = ccrTargetControl as Label;

				AssertEquals("CCR-Target's secondary-axis=5", "CCR target", ccrTargetLabel.Text);

				CombineAssertions(() =>
				{
					AssertCCRHeaderMessageContents("4x0.25hr", viewModel, componentControl, button, secondaryAxis: 1, expectedPopupMessage: "buffer Total: 1 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 3);
					AssertCCRHeaderMessageContents("4x0.25hr", viewModel, componentControl, button, secondaryAxis: 2, expectedPopupMessage: "buffer Total: 1 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 3);
					AssertCCRHeaderMessageContents("4x0.25hr", viewModel, componentControl, button, secondaryAxis: 3, expectedPopupMessage: "buffer Total: 1 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 3);
					AssertCCRHeaderMessageContents("4x0.25hr", viewModel, componentControl, button, secondaryAxis: 4, expectedPopupMessage: "buffer Total: 1 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 3);
					AssertCCRHeaderMessageContents("4x0.25hr", viewModel, componentControl, button, secondaryAxis: 5, expectedPopupMessage: "buffer Total: 1 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 3);
					AssertCCRHeaderMessageContents("3x0.25hr + 1x0.5hr", viewModel, componentControl, button, secondaryAxis: 6, expectedPopupMessage: "buffer Total: 1.25 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 2);
					AssertCCRHeaderMessageContents("3x0.25hr + 1x0.5hr", viewModel, componentControl, button, secondaryAxis: 7, expectedPopupMessage: "buffer Total: 1.25 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 2);
					AssertCCRHeaderMessageContents("3x0.25hr + 1x0.5hr", viewModel, componentControl, button, secondaryAxis: 8, expectedPopupMessage: "buffer Total: 1.25 hours\r\n", expectedConstraintStatus: ConstraintStatus.PreConstraint, expectedZone: 2);
					AssertCCRHeaderMessageContents("3x0.25hr + 1x0.5hr", viewModel, componentControl, button, secondaryAxis: 9, expectedPopupMessage: "buffer Total: 1.25 hours\r\n", expectedConstraintStatus: ConstraintStatus.PostConstraint, expectedZone: 2);
					AssertCCRHeaderMessageContents("3x0.5hr", viewModel, componentControl, button, secondaryAxis: 10, expectedPopupMessage: "buffer Total: 1.5 hours\r\n", expectedConstraintStatus: ConstraintStatus.PostConstraint, expectedZone: 1);
					AssertCCRHeaderMessageContents("3x0.5hr", viewModel, componentControl, button, secondaryAxis: 11, expectedPopupMessage: "buffer Total: 1.5 hours\r\n", expectedConstraintStatus: ConstraintStatus.PostConstraint, expectedZone: 1);
					AssertCCRHeaderMessageContents("3x0.5hr", viewModel, componentControl, button, secondaryAxis: 12, expectedPopupMessage: "buffer Total: 1.5 hours\r\n", expectedConstraintStatus: ConstraintStatus.PostConstraint, expectedZone: 1);
				});
			}
		}

		static void AssertCCRHeaderMessageContents(string durationMessage, BMBoardSectionViewModel viewModel, BMComponentControl componentControl, ButtonForTest button, int secondaryAxis, string expectedPopupMessage, ConstraintStatus expectedConstraintStatus, int expectedZone)
		{
			var message = string.Format("CCR header index {0}: ", secondaryAxis);
			var cell = viewModel.ComponentGrid.Cells.First(c => c.ContentType == CellContentType.CCRHeading && c.SecondaryAxis == secondaryAxis);

			AssertEquals(message + "zone", expectedZone, cell.Zone);
			AssertEquals(message + "constraint-status", expectedConstraintStatus, ((ICCRHeadingCellContent)cell).CCRStatus);

			var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);
			headerControl.ShowChannelCapacity();
			AssertEquals(message + "duration: " + durationMessage, expectedPopupMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowChannelCapacity_CCRHeader_ShouldNotDisplayZone()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			var section = config.Section;

			Factory.Save();

			var viewModel = config.SectionViewModel;

			var map = CardAllocationMap.NewAllocationMap(section, viewModel, TaskChannelMap.Empty);
			viewModel.ComponentGrid.RefreshComponent(Factory, viewModel, map, false); // If there's no CardAllocationMap, we don't show capacity!

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCCRHeaderPopupCaption("CCR-heading in Zone-0 row: popup caption should show zone-0", viewModel, button, componentControl, 1, "Zone 0");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 2, "CCRs 75 % to 100 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 3, "CCRs 75 % to 100 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 4, "CCRs 75 % to 100 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 5, "CCRs 41.7 % to 75 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 6, "CCRs 41.7 % to 75 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 7, "CCRs 41.7 % to 75 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 8, "CCRs 41.7 % to 75 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 9, "CCRs 0 % to 41.7 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 10, "CCRs 0 % to 41.7 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 11, "CCRs 0 % to 41.7 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 12, "CCRs 0 % to 41.7 %");
				AssertCCRHeaderPopupCaption("CCR-heading in NON-Zone-0 row: popup caption should show aging", viewModel, button, componentControl, 13, "CCRs 0 % to 41.7 %");
			}
		}

		static void AssertCCRHeaderPopupCaption(string message, BMBoardSectionViewModel viewModel, ButtonForTest button, BMComponentControl componentControl, int secondaryAxis, string expectedCaption)
		{
			var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.CCRHeading && c.SecondaryAxis == secondaryAxis);
			var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);
			headerControl.ShowChannelCapacity();
			AssertEquals(message, expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		#endregion

		public void TestBackColorChanged_ShouldSetHeaderControlBackColor()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel).WrapWithShownForm())
			{
				var cell = viewModel.ComponentGrid[0, 0];

				using (var headerControl = HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button).WrapWithShownForm())
				{
					cell.BackColor = Color.AliceBlue;
					AssertEquals(Color.AliceBlue, headerControl.BackColor);
				}
			}
		}

		public void TestBackColor_ShouldSetHeaderControlBackColorDefault_WhenNoColor()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel).WrapWithShownForm())
			{
				var cell = viewModel.ComponentGrid[0, 0];

				using (var headerControl = HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button).WrapWithShownForm())
				{
					cell.BackColor = null;
					AssertEquals(BMConstants.BackgroundDefaultColor, headerControl.BackColor);
				}
			}
		}

		public void TestCellContentSubscription_ShouldNotLeakControl()
		{
			var cell = new CellContent(0, 0, CellContentType.ZoneHeading);
			var reference = CreateHeaderControl(cell);

			GC.Collect(); // It's a unit test
			GC.WaitForFullGCComplete(); // It's a unit test

			AssertNull(reference.Target);
		}

		WeakReference CreateHeaderControl(CellContent cell)
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);
				var reference = new WeakReference(headerControl);

				headerControl.Dispose();

				return reference;
			}
		}

		public void TestClickWrappedControl_HorizontalOrientation()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = HeaderControlProvider.GetHeaderControl(viewModel.ComponentGrid[0, 0], componentControl, viewModel, () => button);
				var table = componentControl.Table;
				var startingColumnWidth = table.ColumnStyles[0].Width;
				AssertNotEquals(0, startingColumnWidth);

				button.OnMouseClick_Exposed(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				AssertEquals(3 * startingColumnWidth, table.ColumnStyles[0].Width);
			}
		}

		public void TestClickWrappedControl_RightClick_ShouldNotExpand()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = HeaderControlProvider.GetHeaderControl(viewModel.ComponentGrid[0, 0], componentControl, viewModel, () => button);
				var table = componentControl.Table;
				var startingColumnWidth = table.ColumnStyles[0].Width;
				AssertNotEquals(0, startingColumnWidth);

				button.OnMouseClick_Exposed(new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
				AssertEquals(startingColumnWidth, table.ColumnStyles[0].Width);
			}
		}

		public void TestClickWrappedControl_VerticalOrientation()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = HeaderControlProvider.GetHeaderControl(viewModel.ComponentGrid[0, 0], componentControl, viewModel, () => button);
				var table = componentControl.Table;
				var startingRowHeight = table.RowStyles[0].Height;
				AssertNotEquals(0, startingRowHeight);

				button.OnMouseClick_Exposed(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				AssertEquals(3 * startingRowHeight, table.RowStyles[0].Height);
			}
		}

		public void TestClickWrappedControl_NestedControls_ShouldNotSubscribeToOuterControl()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var outerButton = new ButtonForTest())
			using (var innerButton = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				outerButton.Controls.Add(innerButton);

				var headerControl = HeaderControlProvider.GetHeaderControl(viewModel.ComponentGrid[0, 0], componentControl, viewModel, () => outerButton);
				var table = componentControl.Table;
				var startingRowHeight = table.RowStyles[0].Height;
				AssertNotEquals(0, startingRowHeight);

				outerButton.OnMouseClick_Exposed(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				AssertEquals("Should not expand for outer control click otherwise it'll get fired twice and the column/row won't expand", startingRowHeight, table.RowStyles[0].Height);

				innerButton.OnMouseClick_Exposed(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				AssertEquals(3 * startingRowHeight, table.RowStyles[0].Height);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestCapacityMessage()
		{
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, showZones: null, channels: resource);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, Array.Empty<ProcessTask>());
			section.Component.System.FS_Name = "systemName";

			Factory.Save();
			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
				var cell = new CellContent(0, 0, CellContentType.ZoneHeading);
				var vbChannel = viewModel.CreateChannelForTest(resource);
				cell.Channel = vbChannel;

				var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);

				headerControl.ShowChannelCapacity();
				AssertEquals("Frodo Baggins", UnitTestUserNotification.Instance.LastMessage.Caption);
				const string expected =
@"Total Capacity: 19 hours

Allocated Capacity in buffer: 0 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0 hours

Available Capacity: 19 hours

Calculated at: 14-Jul-2015 10:00:00
";
				AssertMultilineASCIIEquals("", expected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		SchematicTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}
	}

	#region NonTransactionedTestCase

	class HeaderControlProviderNonTransactionedTest : NonTransactionedTestCase
	{
		[TestDate(2019, 4, 5)]
		public void TestShowCapacity_ShouldNotUseChannelCreatedOnBackgroundThread()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory)));

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var map = CardAllocationMap.NewAllocationMap(section, viewModel, TaskChannelMap.Empty);
			viewModel.ComponentGrid.RefreshComponent(Factory, viewModel, map, false); // If there's no CardAllocationMap, we don't show capacity!

			var channel = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryInAnotherThread = new BusinessObjectFactory();
					var resourceInNewFactory = factoryInAnotherThread.Load<GlbStaff>(resource.PK);

					return viewModel.CreateChannelForTest(resourceInNewFactory);
				}
			}).Result;

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = channel };

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);
				headerControl.ShowChannelCapacity();
				AssertEquals(@"Total Capacity: 19 hours

Allocated Capacity in buffer: 0 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0 hours

Available Capacity: 19 hours

Calculated at: 05-Apr-2019 10:00:00
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacity_DoesNotShowWhenBoardLoadCacheIsNotPopulated()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory)));

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var channel = viewModel.CreateChannelForTest(resource);

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = channel };
			Factory.Save();

			using (var button = new ButtonForTest())
			using (var componentControl = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var headerControl = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, () => button);

				headerControl.ShowChannelCapacity();
				AssertEquals("Expected no popup", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, Array.Empty<ProcessTask>());
				headerControl.ShowChannelCapacity();
				AssertEquals("Expected no popup", string.Empty, UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}
	}

	#endregion

	class ButtonForTest : Button
	{
		internal void OnMouseClick_Exposed(MouseEventArgs e)
		{
			OnMouseClick(e);
		}
	}
}
