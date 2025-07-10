using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Pipes.Test;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.BufferManagement.GUI.HeaderControlProvider;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BMComponentControlTest : BMSGUITestCase
	{
		#region Section Header

		public void TestSectionHeaderDisplaysTooltipWithLoadingTimeForAllUsers()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var controller = Factory.NewWithValidTestData<GlbStaff>();
			controller.GS_IsController = true;

			var ordinaryStaff = Factory.NewWithValidTestData<GlbStaff>();
			ordinaryStaff.GS_IsController = false;

			Factory.Save();

			using (var context = EnvProxy.Instance.SetTemporaryUserContext(controller.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertSectionHeaderDisplaysTooltipWithLoadingTime();
			}

			using (var context = EnvProxy.Instance.SetTemporaryUserContext(ordinaryStaff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertSectionHeaderDisplaysTooltipWithLoadingTime();
			}
		}

		void AssertSectionHeaderDisplaysTooltipWithLoadingTime()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				((IBoardSectionControl)control).Refresh(new BoardRefreshEventArgs());

				AssertContains("The section's tooltip should show section Loading time", "Loaded section in", ToolTipService.GetToolTip(control.SectionLabel));
			}
		}

		#endregion

		#region AcceptabilityBands

		public void TestShouldNotShowAcceptabilityBandTiles_WhenThereAreNoABs()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				AssertNull("Should not show when there are no ABs", control.AcceptabilityBandTiles);
			}
		}

		public void TestShouldShowAcceptabilityBandTiles_WhenThereAreActiveABs()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			var band1 = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Enabled AB", type: AcceptabilityBandTypes.Codes.Count);
			band1.BAB_IsActive = true;
			BMSTestHelper.AddAcceptabilityBandToSection(section, band1);

			var band2 = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Disabled AB", type: AcceptabilityBandTypes.Codes.Count);
			band2.BAB_IsActive = false;
			BMSTestHelper.AddAcceptabilityBandToSection(section, band2);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				AssertNotNull("Should show when there are active ABs", control.AcceptabilityBandTiles);
			}
		}

		public void TestShouldNotShowAcceptabilityBandTiles_WhenAllABsAreDisabled()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Disabled AB", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_IsActive = false;
			BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				AssertNull("Should not show when all ABs are inactive", control.AcceptabilityBandTiles);
			}
		}

		public void TestViewModelPropertyChangedShouldHandleControlDisposal()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;

			var viewModel = pair.Item2;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				form.Dispose();

				AssertEquals(true, control.IsDisposed);
				AssertEquals(false, control.IsHandleCreated);

				control.ViewModel_PropertyChanged(null, new PropertyChangedEventArgs("SubHeadingAppearance"));
			}
		}

		public void TestViewModelPropertyChangedShouldHandleControlDisposal_NoReally()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;

			var viewModel = pair.Item2;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				AssertNoExceptionThrown(() => control.ViewModel.SubHeadingAppearance = new LoadingBandsSubHeadingAppearance());
				form.Show();
			}
		}

		public void TestHeading_WithoutAcceptabilityBandDetails_ShouldNotHaveShowCalculationDetailsMenuItem()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNull(control.SectionLabel.ContextMenuStrip);
			}
		}

		public void TestHeading_WithAcceptabilityBandDetails_ShouldShowDetailsWhenMenuItemClicked()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;

			var viewModel = pair.Item2;
			viewModel.SubHeadingAppearance = new SubHeadingAppearenceForTest("Squanch", "Getting on with the job");

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				((LazyContextMenuStrip)control.SectionLabel.ContextMenuStrip).AddItems_ForTest(control.SectionLabel);

				var menuItem = control.SectionLabel.ContextMenuStrip.Items[0];
				AssertEquals("Show Calculation Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("Squanch", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Getting on with the job", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubheading_WithAcceptabilityBandDetails_ShouldShowDetailsWhenMenuItemClicked()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;

			var viewModel = pair.Item2;
			viewModel.SubHeadingAppearance = new SubHeadingAppearenceForTest("Squanch", "Getting on with the job");

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var subHeading = control.FindSingle<ZLabel>("SubHeadingLabel");
				((LazyContextMenuStrip)subHeading.ContextMenuStrip).AddItems_ForTest(subHeading);

				var menuItem = subHeading.ContextMenuStrip.Items[0];
				AssertEquals("Show Calculation Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("Squanch", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Getting on with the job", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class SubHeadingAppearenceForTest : ISectionSubHeadingAppearance
		{
			public SubHeadingAppearenceForTest(string heading, string details)
			{
				SectionSubHeading = heading;
				SectionSubHeadingDetailText = details;
			}

			public string SectionSubHeading { get; }

			public string SectionSubHeadingDetailText { get; }

			public Color SectionHeadingBackgroundColor => Color.LemonChiffon;

			public Color SectionHeadingForegroundColor => Color.OliveDrab;
		}

		#endregion

		#region Buffer layouts

		public void TestStackedPanelWhenNotShowingZonesOnBuffer_StackedOption()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			section.SectionConfiguration.ShowZones = false;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			var viewModel = pair.Item2;
			section.Component.FC_BufferTimespanInMinutes = 60 * 8;
			section.BackgroundColor = "Red";

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertEquals(4, control.FindAll<TaskPanel>().Count());
			}
		}

		public void TestStackedPanelWhenNotShowingZonesOnBuffer_StaggeredOption()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			section.SectionConfiguration.ShowZones = false;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			var viewModel = pair.Item2;
			section.Component.FC_BufferTimespanInMinutes = 60 * 8;
			section.BackgroundColor = "Red";

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertEquals(4, control.FindAll<TaskPanel>().Count());
			}
		}

		public void TestShouldNotThrow_WhenShowingBufferWithCCRAndNoZones()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, string.Empty, ChannelTypeList.Codes.Resource, showZones: false, releaseGroup: config.ReleaseGroup.PK);

			Factory.Save();

			var section = pair.Item1;
			var viewModel = pair.Item2;
			Assert("Should be in constrained mode", viewModel.IsInConstrainedMode);

			AssertNoExceptionThrown(() =>
			{
				using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
				{
				}
			});
		}

		public void TestCellContent_Buffer_NoChannels_Vertical()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			section.Component.FC_BufferTimespanInMinutes = 60 * 8;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				for (int i = 0; i < 4; i++)
				{
					AssertCellContents(control, zoneHeadingPosition, i, CellContentType.ZoneHeading);
					AssertCellContents(control, ageHeadingPosition, i, CellContentType.AgeHeading);
					AssertCellContents(control, 2, i, CellContentType.Cards);
				}
			}
		}

		public void TestCellContent_Buffer_NoChannels_Horizontal()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Left, string.Empty, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			section.Component.FC_BufferTimespanInMinutes = 60 * 8;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				for (int i = 0; i < 4; i++)
				{
					AssertCellContents(control, i, ageHeadingPosition, CellContentType.AgeHeading);
					AssertCellContents(control, i, zoneHeadingPosition, CellContentType.ZoneHeading);
					AssertCellContents(control, i, 2, CellContentType.Cards);
				}
			}
		}

		public void TestCellContent_Buffer_TwoChannels_Vertical()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 7, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.Resource).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.Component.FC_BufferTimespanInMinutes = 60 * 8;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 0, 0, CellContentType.Label);
				AssertCellContents(control, 2, 0, CellContentType.ChannelHeading, viewModel.GetOrCreateChannelForTest(channel1, section.Factory));
				AssertCellContents(control, 3, 0, CellContentType.ChannelHeading, viewModel.GetOrCreateChannelForTest(channel2, section.Factory));

				AssertCellContents(control, zoneHeadingPosition, 1, CellContentType.ZoneHeading); // Zone 0 - one row
				AssertCellContents(control, ageHeadingPosition, 1, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 1, CellContentType.Cards);
				AssertCellContents(control, 3, 1, CellContentType.Cards);

				AssertCellContents(control, zoneHeadingPosition, 2, CellContentType.ZoneHeading); // Zone 1 - two rows
				AssertCellContents(control, ageHeadingPosition, 2, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 2, CellContentType.Cards);
				AssertCellContents(control, 3, 2, CellContentType.Cards);
				AssertCellContents(control, ageHeadingPosition, 3, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 3, CellContentType.Cards);
				AssertCellContents(control, 3, 3, CellContentType.Cards);

				AssertCellContents(control, zoneHeadingPosition, 4, CellContentType.ZoneHeading); // Zone 2 - two rows
				AssertCellContents(control, ageHeadingPosition, 4, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 4, CellContentType.Cards);
				AssertCellContents(control, 3, 4, CellContentType.Cards);
				AssertCellContents(control, ageHeadingPosition, 5, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 5, CellContentType.Cards);
				AssertCellContents(control, 3, 5, CellContentType.Cards);

				AssertCellContents(control, zoneHeadingPosition, 6, CellContentType.ZoneHeading); // Zone 3 - two rows
				AssertCellContents(control, ageHeadingPosition, 6, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 6, CellContentType.Cards);
				AssertCellContents(control, 3, 6, CellContentType.Cards);
				AssertCellContents(control, ageHeadingPosition, 7, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 7, CellContentType.Cards);
				AssertCellContents(control, 3, 7, CellContentType.Cards);
			}
		}

		public void TestCellContent_Buffer_TwoChannels_Horizontal()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 7, FlowDirectionList.Codes.Left, string.Empty, ChannelTypeList.Codes.Resource).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.Component.FC_BufferTimespanInMinutes = 60 * 8;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 0, 0, CellContentType.Label);
				AssertCellContents(control, 0, 2, CellContentType.ChannelHeading, viewModel.GetOrCreateChannelForTest(channel1, section.Factory));
				AssertCellContents(control, 0, 3, CellContentType.ChannelHeading, viewModel.GetOrCreateChannelForTest(channel2, section.Factory));

				AssertCellContents(control, 1, zoneHeadingPosition, CellContentType.ZoneHeading); // Zone 0 - one row
				AssertCellContents(control, 1, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 2, CellContentType.Cards);
				AssertCellContents(control, 1, 3, CellContentType.Cards);

				AssertCellContents(control, 2, zoneHeadingPosition, CellContentType.ZoneHeading); // Zone 1 - two rows
				AssertCellContents(control, 2, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 2, CellContentType.Cards);
				AssertCellContents(control, 2, 3, CellContentType.Cards);
				AssertCellContents(control, 3, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 3, 2, CellContentType.Cards);
				AssertCellContents(control, 3, 3, CellContentType.Cards);

				AssertCellContents(control, 4, zoneHeadingPosition, CellContentType.ZoneHeading); // Zone 2 - two rows
				AssertCellContents(control, 4, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 4, 2, CellContentType.Cards);
				AssertCellContents(control, 4, 3, CellContentType.Cards);
				AssertCellContents(control, 5, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 5, 2, CellContentType.Cards);
				AssertCellContents(control, 5, 3, CellContentType.Cards);

				AssertCellContents(control, 6, zoneHeadingPosition, CellContentType.ZoneHeading); // Zone 3 - two rows
				AssertCellContents(control, 6, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 6, 2, CellContentType.Cards);
				AssertCellContents(control, 6, 3, CellContentType.Cards);
				AssertCellContents(control, 7, ageHeadingPosition, CellContentType.AgeHeading);
				AssertCellContents(control, 7, 2, CellContentType.Cards);
				AssertCellContents(control, 7, 3, CellContentType.Cards);
			}
		}

		#endregion

		#region Bucket layouts

		public void TestCellContent_Bucket_SingleCell()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, BMConstants.ChannelByTimeCode);
			using (var control = new BMComponentControl(pair.Item1.SectionConfiguration, pair.Item2))
			{
				AssertCellContents(control, 0, 0, CellContentType.Cards);
			}
		}

		public void TestCellContent_Bucket_ChannelByTime_Vertical()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, BMConstants.ChannelByTimeCode);
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			using (var control = new BMComponentControl(pair.Item1.SectionConfiguration, pair.Item2))
			{
				AssertCellContents(control, 0, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 1, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 0, CellContentType.Cards);
				AssertCellContents(control, 1, 1, CellContentType.Cards);
			}
		}

		public void TestCellContent_Bucket_ChannelByTime_Horizontal()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, BMConstants.ChannelByTimeCode);
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			using (var control = new BMComponentControl(pair.Item1.SectionConfiguration, pair.Item2))
			{
				AssertCellContents(control, 0, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 1, CellContentType.Cards);
				AssertCellContents(control, 1, 1, CellContentType.Cards);
			}
		}

		public void TestCellContent_Bucket_MultipleSubsections_Vertical()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 4, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, BMConstants.ChannelByTimeCode);
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			using (var control = new BMComponentControl(pair.Item1.SectionConfiguration, pair.Item2))
			{
				AssertCellContents(control, 0, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 1, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 0, CellContentType.Cards);
				AssertCellContents(control, 1, 1, CellContentType.Cards);

				AssertCellContents(control, 2, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 1, CellContentType.AgeHeading);
				AssertCellContents(control, 3, 0, CellContentType.Cards);
				AssertCellContents(control, 3, 1, CellContentType.Cards);

				AssertCellContents(control, 4, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 4, 1, CellContentType.AgeHeading);
				AssertCellContents(control, 5, 0, CellContentType.Cards);
				AssertCellContents(control, 5, 1, CellContentType.Cards);

				AssertCellContents(control, 6, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 6, 1, CellContentType.AgeHeading);
				AssertCellContents(control, 7, 0, CellContentType.Cards);
				AssertCellContents(control, 7, 1, CellContentType.Cards);
			}
		}

		public void TestCellContent_Bucket_MultipleSubsections_Horizontal()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 4, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, BMConstants.ChannelByTimeCode);
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			using (var control = new BMComponentControl(pair.Item1.SectionConfiguration, pair.Item2))
			{
				AssertCellContents(control, 0, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 1, CellContentType.Cards);
				AssertCellContents(control, 1, 1, CellContentType.Cards);

				AssertCellContents(control, 0, 2, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 2, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 3, CellContentType.Cards);
				AssertCellContents(control, 1, 3, CellContentType.Cards);

				AssertCellContents(control, 0, 4, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 4, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 5, CellContentType.Cards);
				AssertCellContents(control, 1, 5, CellContentType.Cards);

				AssertCellContents(control, 0, 6, CellContentType.AgeHeading);
				AssertCellContents(control, 1, 6, CellContentType.AgeHeading);
				AssertCellContents(control, 0, 7, CellContentType.Cards);
				AssertCellContents(control, 1, 7, CellContentType.Cards);
			}
		}

		public void TestCellContent_Bucket_TwoChannels_Horizontal()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 0, 0, CellContentType.Label);
				AssertCellContents(control, 1, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 2, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 3, 0, CellContentType.AgeHeading);
				AssertCellContents(control, 4, 0, CellContentType.AgeHeading);

				AssertCellContents(control, 0, 1, CellContentType.ChannelHeading, viewModel.GetOrCreateChannelForTest(channel1, section.Factory));
				AssertCellContents(control, 1, 1, CellContentType.Cards);
				AssertCellContents(control, 2, 1, CellContentType.Cards);
				AssertCellContents(control, 3, 1, CellContentType.Cards);
				AssertCellContents(control, 4, 1, CellContentType.Cards);

				AssertCellContents(control, 0, 2, CellContentType.ChannelHeading, viewModel.GetOrCreateChannelForTest(channel2, section.Factory));
				AssertCellContents(control, 1, 2, CellContentType.Cards);
				AssertCellContents(control, 2, 2, CellContentType.Cards);
				AssertCellContents(control, 3, 2, CellContentType.Cards);
				AssertCellContents(control, 4, 2, CellContentType.Cards);
			}
		}

		#endregion

		#region Channel expansion

		public void TestChannelExpansion_ResizeChannelDuringBoardLoad()
		{
			var staff1 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var section = config.BufferSection;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "I saw you coming");
			var task = CreateTask(workflow, staff1.GS_Code, 60);

			VisualBoardsTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK, true);
			VisualBoardsTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK, true);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				control.ResetRefreshedData();
				Application.DoEvents();

				var loadCardSetup = new LoadCardContentSetup(viewModel, () => section.Factory);
				var engine = BoardSectionRefreshPipeEngine.Create(new BoardRefreshEventArgs(), loadCardSetup, control);

				var dispatcher = new MockDispatcher();
				engine.ExecuteAll(dispatcher, dispatcher);

				while (dispatcher.TryDispatchOne())
				{
					AssertNoExceptionThrown("There is no race condition between expanding channels and board refresh.", () => control.ExpandNextHeader());
				}
			}
		}

		public void TestHeaderClicked_Bucket_Horizontal()
		{
			var section = config.BucketSection;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(2, viewModel.ComponentGrid.TotalRows);
			AssertEquals(2, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 2x2 section", 4, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[0, 0];
				var channel2Cell = viewModel.ComponentGrid[1, 0];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);

				var startingChannel1Height = control.Table.RowStyles[0].Height;
				var startingChannel2Height = control.Table.RowStyles[1].Height;

				AssertNotEquals(0, startingChannel1Height);
				AssertNotEquals(0, startingChannel2Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height * 3, control.Table.RowStyles[0].Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height, control.Table.RowStyles[0].Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height * 3, control.Table.RowStyles[0].Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height, control.Table.RowStyles[0].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height * 3, control.Table.RowStyles[1].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height, control.Table.RowStyles[1].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height * 3, control.Table.RowStyles[1].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height, control.Table.RowStyles[1].Height);
			}
		}

		public void TestHeaderClicked_Bucket_Vertical()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(2, viewModel.ComponentGrid.TotalRows);
			AssertEquals(2, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 2x2 section", 4, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[0, 0];
				var channel2Cell = viewModel.ComponentGrid[0, 1];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);

				var startingChannel1Width = control.Table.ColumnStyles[0].Width;
				var startingChannel2Width = control.Table.ColumnStyles[1].Width;

				AssertNotEquals(0, startingChannel1Width);
				AssertNotEquals(0, startingChannel2Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[0].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width, control.Table.ColumnStyles[0].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[0].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width, control.Table.ColumnStyles[0].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width * 3, control.Table.ColumnStyles[1].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width, control.Table.ColumnStyles[1].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width * 3, control.Table.ColumnStyles[1].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width, control.Table.ColumnStyles[1].Width);
			}
		}

		public void TestHeaderClicked_Buffer_Horizontal()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			section.SectionConfiguration.CellsPerSubsection = 7;
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(4, viewModel.ComponentGrid.TotalRows);
			AssertEquals(8, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 4x8 section", 32, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[2, 0];
				var channel2Cell = viewModel.ComponentGrid[3, 0];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);

				var startingChannel1Height = control.Table.RowStyles[2].Height;
				var startingChannel2Height = control.Table.RowStyles[3].Height;

				AssertNotEquals(0, startingChannel1Height);
				AssertNotEquals(0, startingChannel2Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height * 3, control.Table.RowStyles[2].Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height, control.Table.RowStyles[2].Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height * 3, control.Table.RowStyles[2].Height);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Height, control.Table.RowStyles[2].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height * 3, control.Table.RowStyles[3].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height, control.Table.RowStyles[3].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height * 3, control.Table.RowStyles[3].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Height, control.Table.RowStyles[3].Height);

				var zone2HeaderCell = viewModel.ComponentGrid[zoneHeadingPosition, 3];
				AssertEquals(CellContentType.ZoneHeading, zone2HeaderCell.ContentType);

				var startingZone2Column1Width = control.Table.ColumnStyles[3].Width;
				var startingZone2Column2Width = control.Table.ColumnStyles[4].Width;
				AssertNotEquals("Column One Width", 0, startingZone2Column1Width);
				AssertNotEquals("Column Two Width", 0, startingZone2Column2Width);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should expand all columns in zone", startingZone2Column1Width * 3, control.Table.ColumnStyles[3].Width);
				AssertEquals("Selecting zone header should expand all columns in zone", startingZone2Column2Width * 3, control.Table.ColumnStyles[4].Width);

				var zone2Column1Cell = viewModel.ComponentGrid[ageHeadingPosition, 3];
				AssertEquals(CellContentType.AgeHeading, zone2Column1Cell.ContentType);

				control.HeaderClicked(zone2Column1Cell);
				AssertEquals("Should restore day column width", startingZone2Column1Width, control.Table.ColumnStyles[3].Width);
				AssertEquals("Should not affect other columns in zone", startingZone2Column2Width * 3, control.Table.ColumnStyles[4].Width);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should re-expand all columns in zone that are expanded. And yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[3].Width);
				AssertEquals("Selecting zone header should re-expand all columns in zone that are expanded. And yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[4].Width);

				var zone2Column2Cell = viewModel.ComponentGrid[ageHeadingPosition, 4];
				AssertEquals(CellContentType.AgeHeading, zone2Column2Cell.ContentType);

				control.HeaderClicked(zone2Column2Cell);
				AssertEquals("Should not affect other rows in zone, and yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[3].Width);
				AssertEquals("Should restore the second column (the one that was clicked), and yet...", startingZone2Column2Width, control.Table.ColumnStyles[4].Width);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should restore to default width all columns in zone that are expanded. And yet...", startingZone2Column1Width, control.Table.ColumnStyles[3].Width);
				AssertEquals("Selecting zone header should restore to default width all columns in zone that are expanded and leave non-expanded columns alone (not un-expand them further or toggle them back to expanded). And yet...", startingZone2Column1Width, control.Table.ColumnStyles[4].Width);

				control.HeaderClicked(zone2Column1Cell);
				AssertEquals("Clicking the specific column should expand the selected column, and yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[3].Width);
				AssertEquals("Clicking a different column in the same zone should only expand the clicked column, and yet...", startingZone2Column1Width, control.Table.ColumnStyles[4].Width);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should restore all columns in zone because the first column was not expanded. And yet...", startingZone2Column1Width, control.Table.ColumnStyles[3].Width);
				AssertEquals("Selecting zone header should restore all columns in zone because the first column was not expanded. And yet...", startingZone2Column1Width, control.Table.ColumnStyles[4].Width);

				control.HeaderClicked(zone2Column2Cell);
				AssertEquals("Clicking a different column in the same zone should only expand the clicked column, and yet...", startingZone2Column1Width, control.Table.ColumnStyles[3].Width);
				AssertEquals("Clicking the specific column should expand the selected column, and yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[4].Width);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should expand all columns in zone that are not yet expanded, and leave expanded columns as they are (not expand them further or toggle them back to default). And yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[3].Width);
				AssertEquals("Selecting zone header should expand all columns in zone that are not yet expanded, and yet...", startingZone2Column1Width * 3, control.Table.ColumnStyles[4].Width);
			}
		}

		public void TestHeaderClicked_Buffer_Vertical()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 7;
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(8, viewModel.ComponentGrid.TotalRows);
			AssertEquals(4, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 8x4 section", 32, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[0, 2];
				var channel2Cell = viewModel.ComponentGrid[0, 3];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);

				var startingChannel1Width = control.Table.ColumnStyles[2].Width;
				var startingChannel2Width = control.Table.ColumnStyles[3].Width;

				AssertNotEquals(0, startingChannel1Width);
				AssertNotEquals(0, startingChannel2Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width * 3, control.Table.ColumnStyles[3].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width, control.Table.ColumnStyles[3].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width * 3, control.Table.ColumnStyles[3].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width, control.Table.ColumnStyles[3].Width);

				var zone2HeaderCell = viewModel.ComponentGrid[3, zoneHeadingPosition];
				AssertEquals(CellContentType.ZoneHeading, zone2HeaderCell.ContentType);

				var startingZone2Row1Height = control.Table.RowStyles[3].Height;
				var startingZone2Row2Height = control.Table.RowStyles[4].Height;
				AssertNotEquals(0, startingZone2Row1Height);
				AssertNotEquals(0, startingZone2Row2Height);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should expand all rows in zone", startingZone2Row1Height * 3, control.Table.RowStyles[3].Height);
				AssertEquals("Selecting zone header should expand all rows in zone", startingZone2Row2Height * 3, control.Table.RowStyles[4].Height);

				var zone2Row1Cell = viewModel.ComponentGrid[3, ageHeadingPosition];
				AssertEquals(CellContentType.AgeHeading, zone2Row1Cell.ContentType);

				control.HeaderClicked(zone2Row1Cell);
				AssertEquals("Should restore the first row's height", startingZone2Row1Height, control.Table.RowStyles[3].Height);
				AssertEquals("Should not affect other rows in zone", startingZone2Row2Height * 3, control.Table.RowStyles[4].Height);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should expand all cells again because the first cell was un-expanded. And yet...", startingZone2Row1Height * 3, control.Table.RowStyles[3].Height);
				AssertEquals("Selecting zone header should expand all cells again because the first cell was un-expanded. And yet...", startingZone2Row2Height * 3, control.Table.RowStyles[4].Height);

				var zone2Row2Cell = viewModel.ComponentGrid[4, ageHeadingPosition];
				AssertEquals(CellContentType.AgeHeading, zone2Row2Cell.ContentType);

				control.HeaderClicked(zone2Row2Cell);
				AssertEquals("Should not affect other rows in zone, and yet...", startingZone2Row1Height * 3, control.Table.RowStyles[3].Height);
				AssertEquals("Should restore the second row (the one that was clicked), and yet...", startingZone2Row2Height, control.Table.RowStyles[4].Height);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should restore to default height all rows in zone that are expanded. And yet...", startingZone2Row1Height, control.Table.RowStyles[3].Height);
				AssertEquals("Selecting zone header should restore to default height all rows in zone that are expanded and leave non-expanded rows alone (not un-expand them further or toggle them back to expanded). And yet...", startingZone2Row2Height, control.Table.RowStyles[4].Height);

				control.HeaderClicked(zone2Row1Cell);
				AssertEquals("Clicking the specific row should expand the selected row, and yet...", startingZone2Row1Height * 3, control.Table.RowStyles[3].Height);
				AssertEquals("Clicking a different row in the same zone should only expand the clicked row, and yet...", startingZone2Row2Height, control.Table.RowStyles[4].Height);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should restore all cells because the first cell was expanded. And yet...", startingZone2Row1Height, control.Table.RowStyles[3].Height);
				AssertEquals("Selecting zone header should restore all cells because the first cell was expanded. And yet...", startingZone2Row2Height, control.Table.RowStyles[4].Height);

				control.HeaderClicked(zone2Row2Cell);
				AssertEquals("Should not affect other rows in zone, and yet...", startingZone2Row1Height, control.Table.RowStyles[3].Height);
				AssertEquals("Should expand the second row (the one that was clicked), and yet...", startingZone2Row2Height * 3, control.Table.RowStyles[4].Height);

				control.HeaderClicked(zone2HeaderCell);
				AssertEquals("Selecting zone header should expand all rows in zone that are not yet expanded, and leave expanded rows as they are (not expand them further or toggle them back to default). And yet...", startingZone2Row1Height * 3, control.Table.RowStyles[3].Height);
				AssertEquals("Selecting zone header should expand all rows in zone that are not yet expanded, and yet...", startingZone2Row2Height * 3, control.Table.RowStyles[4].Height);
			}
		}

		BMBoardSection CreateSectionWithChannelsForMeetingModeNavigationTest()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource4.PK, overrideChannels: true);

			return section;
		}

		public void TestExpandSelectedChannelAndCollapseOthers()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 7;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(8, viewModel.ComponentGrid.TotalRows);
			AssertEquals(6, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 8x6 section", 48, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[0, 2];
				var channel2Cell = viewModel.ComponentGrid[0, 3];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);

				var startingChannel1Width = control.Table.ColumnStyles[2].Width;
				var startingChannel2Width = control.Table.ColumnStyles[3].Width;

				AssertNotEquals(0, startingChannel1Width);
				AssertNotEquals(0, startingChannel2Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(startingChannel2Width * 3, control.Table.ColumnStyles[3].Width);

				control.ExpandCurrentHeaderAndCollapseOthers(channel1Cell);

				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[2].Width);
				AssertEquals(startingChannel2Width, control.Table.ColumnStyles[3].Width);

				control.ExpandCurrentHeaderAndCollapseOthers(channel2Cell);

				AssertEquals(startingChannel1Width, control.Table.ColumnStyles[2].Width);
				AssertEquals(startingChannel2Width * 3, control.Table.ColumnStyles[3].Width);
			}
		}

		public void TestCorrectlyCyclesBetweenChannels_Vertical()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 7;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(8, viewModel.ComponentGrid.TotalRows);
			AssertEquals(6, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 8x6 section", 48, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[0, 2];
				var channel2Cell = viewModel.ComponentGrid[0, 3];
				var channel3Cell = viewModel.ComponentGrid[0, 4];
				var channel4Cell = viewModel.ComponentGrid[0, 5];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel3Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel4Cell.ContentType);

				var channelStyles = new[] { control.Table.ColumnStyles[2], control.Table.ColumnStyles[3], control.Table.ColumnStyles[4], control.Table.ColumnStyles[5] };

				var startingChannel1Width = control.Table.ColumnStyles[2].Width;

				AssertChannelsExpandedCorrectly(-1, startingChannel1Width, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(0, startingChannel1Width, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(1, startingChannel1Width, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(2, startingChannel1Width, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(3, startingChannel1Width, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(0, startingChannel1Width, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(1, startingChannel1Width, channelStyles);

				control.ExpandPreviousHeader();

				AssertChannelsExpandedCorrectly(0, startingChannel1Width, channelStyles);

				control.ExpandPreviousHeader();

				AssertChannelsExpandedCorrectly(3, startingChannel1Width, channelStyles);

				control.ExpandPreviousHeader();

				AssertChannelsExpandedCorrectly(2, startingChannel1Width, channelStyles);
			}
		}

		public void TestCorrectlyCyclesBetweenChannels_Horizontal()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			section.SectionConfiguration.CellsPerSubsection = 7;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(6, viewModel.ComponentGrid.TotalRows);
			AssertEquals(8, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 6x8 section", 48, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[2, 0];
				var channel2Cell = viewModel.ComponentGrid[3, 0];
				var channel3Cell = viewModel.ComponentGrid[4, 0];
				var channel4Cell = viewModel.ComponentGrid[5, 0];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel3Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel4Cell.ContentType);

				var startingChannel1Height = control.Table.RowStyles[2].Height;
				var channelStyles = new[] { control.Table.RowStyles[2], control.Table.RowStyles[3], control.Table.RowStyles[4], control.Table.RowStyles[5] };

				AssertChannelsExpandedCorrectly(-1, startingChannel1Height, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(0, startingChannel1Height, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(1, startingChannel1Height, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(2, startingChannel1Height, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(3, startingChannel1Height, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(0, startingChannel1Height, channelStyles);

				control.ExpandNextHeader();

				AssertChannelsExpandedCorrectly(1, startingChannel1Height, channelStyles);

				control.ExpandPreviousHeader();

				AssertChannelsExpandedCorrectly(0, startingChannel1Height, channelStyles);

				control.ExpandPreviousHeader();

				AssertChannelsExpandedCorrectly(3, startingChannel1Height, channelStyles);

				control.ExpandPreviousHeader();

				AssertChannelsExpandedCorrectly(2, startingChannel1Height, channelStyles);
			}
		}

		public void TestExpandNextPreviousWhenOtherHeadersClicked_ShouldExpandCorrectHeader_Vertical()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 7;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel0Cell = viewModel.ComponentGrid[0, 2];
				var channel1Cell = viewModel.ComponentGrid[0, 3];
				var channel2Cell = viewModel.ComponentGrid[0, 4];
				var channel3Cell = viewModel.ComponentGrid[0, 5];
				var channelStyles = new[] { control.Table.ColumnStyles[2], control.Table.ColumnStyles[3], control.Table.ColumnStyles[4], control.Table.ColumnStyles[5] };
				var defaultWidth = control.Table.ColumnStyles[2].Width;
				var expandedWidth = defaultWidth * 3;

				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.ExpandNextHeader();
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(expandedWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel2Cell);
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.ExpandNextHeader();
				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals("The channel adjacent to the rightmost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", expandedWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel3Cell);
				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals(expandedWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(expandedWidth, channelStyles[3].Width);

				control.ExpandNextHeader();
				AssertEquals("The channel adjacent to the rightmost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(expandedWidth, channelStyles[3].Width);

				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.ExpandPreviousHeader();
				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals("The channel adjacent to the leftmost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", defaultWidth, channelStyles[2].Width);
				AssertEquals(expandedWidth, channelStyles[3].Width);

				control.HeaderClicked(channel1Cell);
				control.HeaderClicked(channel3Cell);
				control.HeaderClicked(channel2Cell);
				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals(expandedWidth, channelStyles[1].Width);
				AssertEquals(expandedWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.ExpandPreviousHeader();
				AssertEquals("The channel adjacent to the leftmost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(expandedWidth, channelStyles[3].Width);

				control.ExpandPreviousHeader();
				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals("The channel to the left of the leftmost expanded channel is already expanded, so the newly expanded channel should be one more to the left, and yet...", expandedWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel0Cell);
				control.HeaderClicked(channel2Cell);
				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(expandedWidth, channelStyles[3].Width);

				control.ExpandNextHeader();
				AssertEquals("The channel to the right of the rightmost expanded channel is already expanded, so the newly expanded channel should be one more to the right, and yet...", defaultWidth, channelStyles[0].Width);
				AssertEquals(expandedWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);
			}
		}

		public void TestExpandNextPreviousWhenOtherHeadersClicked_ShouldExpandCorrectHeader_Horizontal()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			section.SectionConfiguration.CellsPerSubsection = 7;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel0Cell = viewModel.ComponentGrid[2, 0];
				var channel1Cell = viewModel.ComponentGrid[3, 0];
				var channel2Cell = viewModel.ComponentGrid[4, 0];
				var channel3Cell = viewModel.ComponentGrid[5, 0];
				var channelStyles = new[] { control.Table.RowStyles[2], control.Table.RowStyles[3], control.Table.RowStyles[4], control.Table.RowStyles[5] };
				var defaultHeight = control.Table.RowStyles[2].Height;
				var expandedHeight = defaultHeight * 3;

				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.ExpandNextHeader();
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(expandedHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel2Cell);
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.ExpandNextHeader();
				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals("The channel adjacent to the bottommost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", expandedHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel3Cell);
				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals(expandedHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(expandedHeight, channelStyles[3].Height);

				control.ExpandNextHeader();
				AssertEquals("The channel adjacent to the bottommost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(expandedHeight, channelStyles[3].Height);

				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.ExpandPreviousHeader();
				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals("The channel adjacent to the topmost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", defaultHeight, channelStyles[2].Height);
				AssertEquals(expandedHeight, channelStyles[3].Height);

				control.HeaderClicked(channel1Cell);
				control.HeaderClicked(channel3Cell);
				control.HeaderClicked(channel2Cell);
				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals(expandedHeight, channelStyles[1].Height);
				AssertEquals(expandedHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.ExpandPreviousHeader();
				AssertEquals("The channel adjacent to the topmost expanded channel should have been expanded rather than the most recently expanded channel, and yet...", expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(expandedHeight, channelStyles[3].Height);

				control.ExpandPreviousHeader();
				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals("The channel above the topmost expanded channel is already expanded, so the newly expanded channel should be one more up, and yet...", expandedHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel0Cell);
				control.HeaderClicked(channel2Cell);
				control.HeaderClicked(channel3Cell);
				AssertEquals(expandedHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(expandedHeight, channelStyles[3].Height);

				control.ExpandNextHeader();
				AssertEquals("The channel below the bottommost expanded channel is already expanded, so the newly expanded channel should be one more down, and yet...", defaultHeight, channelStyles[0].Height);
				AssertEquals(expandedHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);
			}
		}

		public void TestAllHeadersExpanded_ShouldSetAllHeadersToCollapsed_Vertical()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 7;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel0Cell = viewModel.ComponentGrid[0, 2];
				var channel1Cell = viewModel.ComponentGrid[0, 3];
				var channel2Cell = viewModel.ComponentGrid[0, 4];
				var channel3Cell = viewModel.ComponentGrid[0, 5];
				var channelStyles = new[] { control.Table.ColumnStyles[2], control.Table.ColumnStyles[3], control.Table.ColumnStyles[4], control.Table.ColumnStyles[5] };
				var defaultWidth = control.Table.ColumnStyles[2].Width;

				AssertEquals(defaultWidth, channelStyles[0].Width);
				AssertEquals(defaultWidth, channelStyles[1].Width);
				AssertEquals(defaultWidth, channelStyles[2].Width);
				AssertEquals(defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel0Cell);
				control.HeaderClicked(channel1Cell);
				control.HeaderClicked(channel2Cell);
				control.HeaderClicked(channel3Cell);

				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultWidth, channelStyles[0].Width);
				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultWidth, channelStyles[1].Width);
				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultWidth, channelStyles[2].Width);
				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultWidth, channelStyles[3].Width);

				control.HeaderClicked(channel0Cell);
				AssertEquals("Channel header should be expanded, not collapsed, becasuse all headers were expanded which should have reset the board's headers' expanded status. And yet...", defaultWidth * 3, channelStyles[0].Width);

				var ageCells = viewModel.ComponentGrid.Cells.Where(x => x.ContentType == CellContentType.AgeHeading).ToArray();

				foreach (var ageCell in ageCells)
				{
					control.HeaderClicked(ageCell);
				}

				for (var i = 0; i < control.Table.RowCount; i++)
				{
					AssertEquals("All rows were expanded, which means all rows should be set to collapsed, and yet...", viewModel.GetOriginalRowHeight(i), control.Table.RowStyles[i].Height);
				}

				var row = ageCells.First().Row;
				control.HeaderClicked(ageCells.First());
				AssertEquals("Age row should be expanded, not collapsed, becasuse all rows were expanded which should have reset the board's rows' expanded status. And yet...", viewModel.GetOriginalRowHeight(row) * 3, control.Table.RowStyles[row].Height);
			}
		}

		public void TestAllHeadersExpanded_ShouldSetAllHeadersToCollapsed_Horizontal()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			section.SectionConfiguration.CellsPerSubsection = 7;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel0Cell = viewModel.ComponentGrid[2, 0];
				var channel1Cell = viewModel.ComponentGrid[3, 0];
				var channel2Cell = viewModel.ComponentGrid[4, 0];
				var channel3Cell = viewModel.ComponentGrid[5, 0];
				var channelStyles = new[] { control.Table.RowStyles[2], control.Table.RowStyles[3], control.Table.RowStyles[4], control.Table.RowStyles[5] };
				var defaultHeight = control.Table.RowStyles[2].Height;

				AssertEquals(defaultHeight, channelStyles[0].Height);
				AssertEquals(defaultHeight, channelStyles[1].Height);
				AssertEquals(defaultHeight, channelStyles[2].Height);
				AssertEquals(defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel0Cell);
				control.HeaderClicked(channel1Cell);
				control.HeaderClicked(channel2Cell);
				control.HeaderClicked(channel3Cell);

				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultHeight, channelStyles[0].Height);
				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultHeight, channelStyles[1].Height);
				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultHeight, channelStyles[2].Height);
				AssertEquals("All headers were expanded, which means all headers should be set to collapsed, and yet...", defaultHeight, channelStyles[3].Height);

				control.HeaderClicked(channel0Cell);
				AssertEquals("Channel header should be expanded, not collapsed, becasuse all headers were expanded which should have reset the board's headers' expanded status. And yet...", defaultHeight * 3, channelStyles[0].Height);

				var ageCells = viewModel.ComponentGrid.Cells.Where(x => x.ContentType == CellContentType.AgeHeading).ToArray();

				foreach (var ageCell in ageCells)
				{
					control.HeaderClicked(ageCell);
				}

				for (var i = 0; i < control.Table.RowCount; i++)
				{
					AssertEquals("All columns were expanded, which means all columns should be set to collapsed, and yet...", viewModel.GetOriginalColumnWidth(i), control.Table.ColumnStyles[i].Width);
				}

				var column = ageCells.First().Column;
				control.HeaderClicked(ageCells.First());
				AssertEquals("Age column should be expanded, not collapsed, becasuse all columns were expanded which should have reset the board's columns' expanded status. And yet...", viewModel.GetOriginalColumnWidth(column) * 3, control.Table.ColumnStyles[column].Width);
			}
		}

		public void TestExpandedHeaderClicked_DoesNotExpandItMore()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 7;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(8, viewModel.ComponentGrid.TotalRows);
			AssertEquals(6, viewModel.ComponentGrid.TotalColumns);
			AssertEquals("Should be a 8x6 section", 48, viewModel.ComponentGrid.Cells.Count());

			Factory.Save();

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var channel1Cell = viewModel.ComponentGrid[0, 2];
				var channel2Cell = viewModel.ComponentGrid[0, 3];

				AssertEquals(CellContentType.ChannelHeading, channel1Cell.ContentType);
				AssertEquals(CellContentType.ChannelHeading, channel2Cell.ContentType);

				var startingChannel1Width = control.Table.ColumnStyles[2].Width;
				var startingChannel2Width = control.Table.ColumnStyles[3].Width;

				AssertNotEquals(0, startingChannel1Width);
				AssertNotEquals(0, startingChannel2Width);

				control.ExpandNextHeader();
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width, control.Table.ColumnStyles[2].Width);

				control.HeaderClicked(channel1Cell);
				AssertEquals(startingChannel1Width * 3, control.Table.ColumnStyles[2].Width);
			}
		}

		public void TestCorrectlyExpandZones_ConstrainedResources_ZeroSubComponentZoneHeaders()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			var board = config.System.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = config.Buffer.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.ShowZones = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(14, viewModel.ComponentGrid.TotalRows);
			AssertEquals(5, viewModel.ComponentGrid.TotalColumns);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				const int zone0CellRow = 1;

				const int zone1Cell1Row = 2;
				const int zone1Cell2Row = 3;
				const int zone1Cell3Row = 4;
				const int zone1Cell4Row = 5;

				const int zone2Cell1Row = 6;
				const int zone2Cell2Row = 7;
				const int zone2Cell3Row = 8;

				var zone0Cell = viewModel.ComponentGrid[zone0CellRow, 0];
				AssertEquals("100 %+", zone0Cell.Label);
				var zone1Cell1 = viewModel.ComponentGrid[zone1Cell1Row, 0];
				AssertEquals("100 %", zone1Cell1.Label);
				var zone1Cell2 = viewModel.ComponentGrid[zone1Cell2Row, 0];
				AssertEquals("91.7 %", zone1Cell2.Label);
				var zone1Cell3 = viewModel.ComponentGrid[zone1Cell3Row, 0];
				AssertEquals("83.3 %", zone1Cell3.Label);
				var zone1Cell4 = viewModel.ComponentGrid[zone1Cell4Row, 0];
				AssertEquals("75 %", zone1Cell4.Label);
				var preConstraintZone1Cell1 = viewModel.ComponentGrid[zone2Cell1Row, 0];
				AssertEquals("66.7 %", preConstraintZone1Cell1.Label);
				var preConstraintZone1Cell2 = viewModel.ComponentGrid[zone2Cell2Row, 0];
				AssertEquals("58.3 %", preConstraintZone1Cell2.Label);
				var preConstraintZone1Cell3 = viewModel.ComponentGrid[zone2Cell3Row, 0];
				AssertEquals("50 %", preConstraintZone1Cell3.Label);

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;

				var startingCellHeight = control.Table.RowStyles[zone0CellRow].Height;

				form.EnterBoardMeetingMode();

				AssertRowExpansion(control, zone0CellRow, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell1Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell2Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell3Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell4Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone2Cell1Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone2Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone2Cell3Row, startingCellHeight, shouldBeExpanded: false);

				form.LeaveBoardMeetingMode();

				AssertRowExpansion(control, zone0CellRow, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell1Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell3Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell4Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone2Cell1Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone2Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone2Cell3Row, startingCellHeight, shouldBeExpanded: false);
			}
		}

		public void TestCorrectlyExpandZones_ConstrainedMode_OneSubComponentZoneHeader()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			var board = config.System.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = config.Buffer.PK;
			section.MS_GG_ReleaseGroup = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.ShowZones = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(14, viewModel.ComponentGrid.TotalRows);
			AssertEquals(6, viewModel.ComponentGrid.TotalColumns);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				const int zone0CellRow = 1;

				const int zone1Cell1Row = 2;
				const int zone1Cell2Row = 3;
				const int zone1Cell3Row = 4;
				const int zone1Cell4Row = 5;

				const int preConstraintZone1Cell1Row = 6;
				const int preConstraintZone1Cell2Row = 7;
				const int preConstraintZone1Cell3Row = 8;

				var zone0Cell = viewModel.ComponentGrid[zone0CellRow, 0];
				AssertEquals("100 %+", zone0Cell.Label);
				var zone1Cell1 = viewModel.ComponentGrid[zone1Cell1Row, 0];
				AssertEquals("100 %", zone1Cell1.Label);
				var zone1Cell2 = viewModel.ComponentGrid[zone1Cell2Row, 0];
				AssertEquals("91.7 %", zone1Cell2.Label);
				var zone1Cell3 = viewModel.ComponentGrid[zone1Cell3Row, 0];
				AssertEquals("83.3 %", zone1Cell3.Label);
				var zone1Cell4 = viewModel.ComponentGrid[zone1Cell4Row, 0];
				AssertEquals("75 %", zone1Cell4.Label);
				var preConstraintZone1Cell1 = viewModel.ComponentGrid[preConstraintZone1Cell1Row, 0];
				AssertEquals("66.7 %", preConstraintZone1Cell1.Label);
				var preConstraintZone1Cell2 = viewModel.ComponentGrid[preConstraintZone1Cell2Row, 0];
				AssertEquals("58.3 %", preConstraintZone1Cell2.Label);
				var preConstraintZone1Cell3 = viewModel.ComponentGrid[preConstraintZone1Cell3Row, 0];
				AssertEquals("50 %", preConstraintZone1Cell3.Label);

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;

				var startingCellHeight = control.Table.RowStyles[zone0CellRow].Height;

				form.EnterBoardMeetingMode();

				AssertRowExpansion(control, zone0CellRow, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell1Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell2Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell3Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell4Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, preConstraintZone1Cell1Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, preConstraintZone1Cell2Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, preConstraintZone1Cell3Row, startingCellHeight, shouldBeExpanded: true);

				form.LeaveBoardMeetingMode();

				AssertRowExpansion(control, zone0CellRow, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell1Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell3Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell4Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, preConstraintZone1Cell1Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, preConstraintZone1Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, preConstraintZone1Cell3Row, startingCellHeight, shouldBeExpanded: false);
			}
		}

		public void TestCorrectlyExpandZones_ConstrainedMode_TwoSubComponentZoneHeaders()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			var board = config.System.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = config.Buffer.PK;
			section.MS_GG_ReleaseGroup = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR2.PK, overrideChannels: true);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(14, viewModel.ComponentGrid.TotalRows);
			AssertEquals(11, viewModel.ComponentGrid.TotalColumns);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				const int zone0CellRow = 1;

				const int zone1Cell1Row = 2;
				const int zone1Cell2Row = 3;
				const int zone1Cell3Row = 4;
				const int zone1Cell4Row = 5;

				const int preConstraintZone1Cell1Row = 6;
				const int preConstraintZone1Cell2Row = 7;
				const int preConstraintZone1Cell3Row = 8;

				var zone0Cell = viewModel.ComponentGrid[zone0CellRow, 0];
				AssertEquals("100 %+", zone0Cell.Label);
				var zone1Cell1 = viewModel.ComponentGrid[zone1Cell1Row, 0];
				AssertEquals("100 %", zone1Cell1.Label);
				var zone1Cell2 = viewModel.ComponentGrid[zone1Cell2Row, 0];
				AssertEquals("91.7 %", zone1Cell2.Label);
				var zone1Cell3 = viewModel.ComponentGrid[zone1Cell3Row, 0];
				AssertEquals("83.3 %", zone1Cell3.Label);
				var zone1Cell4 = viewModel.ComponentGrid[zone1Cell4Row, 0];
				AssertEquals("75 %", zone1Cell4.Label);
				var preConstraintZone1Cell1 = viewModel.ComponentGrid[preConstraintZone1Cell1Row, 0];
				AssertEquals("66.7 %", preConstraintZone1Cell1.Label);
				var preConstraintZone1Cell2 = viewModel.ComponentGrid[preConstraintZone1Cell2Row, 0];
				AssertEquals("58.3 %", preConstraintZone1Cell2.Label);
				var preConstraintZone1Cell3 = viewModel.ComponentGrid[preConstraintZone1Cell3Row, 0];
				AssertEquals("50 %", preConstraintZone1Cell3.Label);

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;

				var startingCellHeight = control.Table.RowStyles[zone0CellRow].Height;

				form.EnterBoardMeetingMode();

				AssertRowExpansion(control, zone0CellRow, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell1Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell2Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell3Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, zone1Cell4Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, preConstraintZone1Cell1Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, preConstraintZone1Cell2Row, startingCellHeight, shouldBeExpanded: true);
				AssertRowExpansion(control, preConstraintZone1Cell3Row, startingCellHeight, shouldBeExpanded: true);

				form.LeaveBoardMeetingMode();

				AssertRowExpansion(control, zone0CellRow, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell1Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell3Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, zone1Cell4Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, preConstraintZone1Cell1Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, preConstraintZone1Cell2Row, startingCellHeight, shouldBeExpanded: false);
				AssertRowExpansion(control, preConstraintZone1Cell3Row, startingCellHeight, shouldBeExpanded: false);
			}
		}

		public void TestCorrectlyExpandZones_NonConstrainedMode()
		{
			var section = CreateSectionWithChannelsForMeetingModeNavigationTest();
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(6, viewModel.ComponentGrid.TotalRows);
			AssertEquals(14, viewModel.ComponentGrid.TotalColumns);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				const int zone0CellColumn = 1;
				const int zone1CellColumn = 2;
				const int lastCellBeforeZone1Column = 6;

				var zone0Cell = viewModel.ComponentGrid[0, zone0CellColumn];
				AssertEquals("100 %+", zone0Cell.Label);
				var zone1Cell = viewModel.ComponentGrid[0, zone1CellColumn];
				AssertEquals("100 %", zone1Cell.Label);
				var lastCellBeforeZone1 = viewModel.ComponentGrid[0, lastCellBeforeZone1Column];
				AssertEquals("66.7 %", lastCellBeforeZone1.Label);

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;

				var startingZone0CellWidth = control.Table.ColumnStyles[zone0CellColumn].Width;
				var startingZone1CellWidth = control.Table.ColumnStyles[zone1CellColumn].Width;
				var startinglastCellBeforeZone1Width = control.Table.ColumnStyles[lastCellBeforeZone1Column].Width;

				form.EnterBoardMeetingMode();

				AssertColumnExpansion(control, zone0CellColumn, startingZone0CellWidth, shouldBeExpanded: true);
				AssertColumnExpansion(control, zone1CellColumn, startingZone1CellWidth, shouldBeExpanded: true);
				AssertColumnExpansion(control, lastCellBeforeZone1Column, startinglastCellBeforeZone1Width, shouldBeExpanded: true);

				form.LeaveBoardMeetingMode();

				AssertColumnExpansion(control, zone0CellColumn, startingZone0CellWidth, shouldBeExpanded: false);
				AssertColumnExpansion(control, zone1CellColumn, startingZone1CellWidth, shouldBeExpanded: false);
				AssertColumnExpansion(control, lastCellBeforeZone1Column, startinglastCellBeforeZone1Width, shouldBeExpanded: false);
			}
		}

		void AssertColumnExpansion(BMComponentControl control, int column, float startingSize, bool shouldBeExpanded)
		{
			var newCellSize = control.Table.ColumnStyles[column].Width;

			if (shouldBeExpanded)
			{
				AssertEquals("Column should be expanded", startingSize * 3, newCellSize);
			}
			else
			{
				AssertEquals("Column should not be expanded", startingSize, newCellSize);
			}
		}

		void AssertRowExpansion(BMComponentControl control, int row, float startingSize, bool shouldBeExpanded)
		{
			var newCellSize = control.Table.RowStyles[row].Height;

			if (shouldBeExpanded)
			{
				AssertEquals("Row should be expanded", startingSize * 3, newCellSize);
			}
			else
			{
				AssertEquals("Row should not be expanded", startingSize, newCellSize);
			}
		}

		#endregion

		#region 2D Channels

		public void TestCellContent_2DChannels()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, (bool?)null, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>()).Item1;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(resource1, resource2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(2, BMBoardSectionTestHelper.GetSecondaryAxisChannelCount(section));
			section.SectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == resource1.PK).MSC_Sequence = 1;
			section.SectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == resource2.PK).MSC_Sequence = 2;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(3, channels.Length);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 0, 0, CellContentType.Label);

				AssertCellContents(control, 1, 0, CellContentType.ChannelHeading, viewModel.CreateChannelForTest(resource1));
				AssertCellContents(control, 2, 0, CellContentType.ChannelHeading, viewModel.CreateChannelForTest(resource2));

				AssertCellContents(control, 0, 1, CellContentType.ChannelHeading, channels[0]);
				AssertCellContents(control, 0, 2, CellContentType.ChannelHeading, channels[1]);
				AssertCellContents(control, 0, 3, CellContentType.ChannelHeading, channels[2]);
			}
		}

		#endregion

		#region ContextMenu

		public void TestComponentGrid_LoadFailed_ShouldDisableTheContextMenu()
		{
			var maxItemsPerSection = 100;
			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maxItemsPerSection);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3");

			var board = BMSTestHelper.CreateBoard(system);

			var group1 = BMSTestHelper.CreateGroup(Factory, "AAA");
			var group2 = BMSTestHelper.CreateGroup(Factory, "BBB");
			var group3 = BMSTestHelper.CreateGroup(Factory, "CCC");

			var section1 = BMSTestHelper.CreateBoardSection(bucket1, board, row: 0);
			section1.SectionConfiguration.ReleaseGroupPK = group1.PK;
			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board, row: 1);
			section2.SectionConfiguration.ReleaseGroupPK = group2.PK;
			var section3 = BMSTestHelper.CreateBoardSection(bucket3, board, row: 2);
			section3.SectionConfiguration.ReleaseGroupPK = group3.PK;

			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: maxItemsPerSection + 10, releaseGroup: group1);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket2, numberOfWorkflows: 1, numberOfTasksPerWorkflow: maxItemsPerSection + 20, releaseGroup: group2);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket3, numberOfWorkflows: 1, numberOfTasksPerWorkflow: maxItemsPerSection - 10, releaseGroup: group3);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var controls = form.FindAll<BMComponentControl>().ToArray();
				AssertEquals("Precondition", 3, controls.Length);

				var control0Label = controls[0].FindAll<ZLabel>().SingleOrDefault(l => l.Name == "LoadFailedLabel");
				var control1Label = controls[1].FindAll<ZLabel>().SingleOrDefault(l => l.Name == "LoadFailedLabel");
				var control2Label = controls[2].FindAll<ZLabel>().SingleOrDefault(l => l.Name == "LoadFailedLabel");

				AssertEquals("Precondition", "This section (bucket1) cannot be displayed, as the number of items to be shown exceeds the maximum allowed (100). Please revise filters and configuration of this section.", control0Label.Text);
				AssertEquals("Precondition", "This section (bucket2) cannot be displayed, as the number of items to be shown exceeds the maximum allowed (100). Please revise filters and configuration of this section.", control1Label.Text);
				AssertNull("Precondition", control2Label);

				AssertNull("Section (bucket1) should not have a context menu", controls[0].ContextMenuStrip);
				AssertNull("Section (bucket2) should not have a context menu", controls[1].ContextMenuStrip);
				AssertNotNull("Section (bucket3) should have a context menu", controls[2].ContextMenuStrip);
			}
		}

		#region Lazy

		public void TestComponentControlMenuIsLazy()
		{
			var section = config.BucketSection;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			BMComponentControl.AutoGenerateComponentMenuItems.Value = false;

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = (LazyContextMenuStrip)control.ContextMenuStrip;
				AssertNotNull(contextMenu);
				AssertEquals(1, contextMenu.Items.Count);

				contextMenu.AddItems_ForTest(control);

				var itemCount = contextMenu.Items.Count;
				AssertNotEquals(1, itemCount);

				contextMenu.AddItems_ForTest(control);

				AssertEquals("Opening the menu mutliple times doesn't cause duplication", itemCount, contextMenu.Items.Count);
			}
		}

		#endregion

		#region Startable Tasks

		[UseSnapshotProtection]
		class CurrentTasksContextMenuNonTransactionedTest : TestCase
		{
			public void TestCurrentTasksContextMenu()
			{
				var factory = new BusinessObjectFactory();

				var system = BMSTestHelper.CreateSystem(factory);
				var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
				var board = system.Boards.AddNew();
				var section = board.Sections.AddNew();
				section.MS_FC_Component = buffer.PK;
				section.SectionConfiguration.CellsPerSubsection = 4;

				var workflow = ProcessJobHeader.GetForParent(factory.NewWithValidTestData<OrgHeader>(), factory).ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = buffer.PK;

				var task1 = workflow.Parent.WorkflowItems.AddNew();
				task1.P9_FH_ProcessHeader = workflow.PK;
				task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

				var task2 = workflow.Parent.WorkflowItems.AddNew();
				task2.P9_FH_ProcessHeader = workflow.PK;
				task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

				AssertEquals(true, task1.IsStartable());
				AssertEquals(false, task2.IsStartable());

				factory.Save();

				var viewModel = BMSTestHelper.CreateViewModel(section);

				using (DisableAsyncBehaviour())
				using (var form = new ZForm { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
				using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Size = form.Size })
				{
					form.Controls.Add(control);
					form.Show();
					control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
					Thread.Sleep(TimeSpan.FromSeconds(2));
					Application.DoEvents();

					var taskCards = control.FindAll<TaskCardControl>().ToArray();
					AssertEquals(2, taskCards.Length);
					AssertEquals(true, taskCards[0].Visible);
					AssertEquals(true, taskCards[1].Visible);

					var contextMenu = control.ContextMenuStrip;
					AssertNotNull(contextMenu);

					var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().ElementAt(0);

					AssertEquals("Show Startable Items", menuItem.Text);
					AssertEquals(false, menuItem.Checked);
					menuItem.PerformClick();

					taskCards = control.FindAll<TaskCardControl>().ToArray();
					AssertEquals("We don't create controls for filtered out cards anymore, so there should only be controls for visible cards, and yet...", 1, taskCards.Length);
					AssertEquals(true, taskCards[0].Visible);

					AssertEquals(true, menuItem.Checked);
				}
			}
		}

		#endregion

		#region Risk Filter

		[TestDate(2017, 2, 2)]
		public void TestRiskFilterContextMenu_nonCCRTaskSequenceEqualToLastCCRTask()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			var section = config.Section;

			var workflow = config.Workflows.First(w => w.FH_CompletionStatement == "workflow for CCR");
			var ccrTask = workflow.TaskCollection.First() as ProcessTask;
			var nonCCRTask = CreateTask(workflow: workflow, staffCode: config.NonCCR1.GS_Code, lowEstMinutes: 15, sequence: 1, description: "non-ccr task X");

			AssertEquals("GIVEN last-ccrTask.sequence = nonCCRTask.sequence", ccrTask.P9_Sequence, nonCCRTask.P9_Sequence);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().Single();

				var taskCards = control.FindAll<TaskCardControl>().ToArray();
				AssertEquals("GIVEN 1 CCR-task, 2 NCR1 tasks, and 1 NCR2 task", 4, taskCards.Length);

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Risk Filter");
				AssertEquals(false, menuItem.Checked);

				menuItem.PerformClick();

				AssertEquals("WHEN applying Risk-Filter", "Risk Filter", menuItem.Text);

				AssertEquals(true, menuItem.Checked);

				// ShouldTaskBeOnBoardMettingAgenda use approximation to determine zone hence risk-task at the bottom of zone-1 pre-constraint is not shown
				// otherwise, this value should be = 5.
				var agingForBottomCellZone1 = 6;

				for (var aging = 0; aging < 15; aging++)
				{
					workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-aging);
					Factory.Save();

					form.RefreshNow_ForTest(forceReload: false);

					var foundCCRTask = control.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == ccrTask.PK);
					var workflowRow = control.Table.GetRow(foundCCRTask.Parent);
					var subComponentHeadingCell = config.SectionViewModel.ComponentGrid.Cells
						.FirstOrDefault(c => c.ContentType == CellContentType.SubComponentZoneHeading && c.SubComponentZones.ContainsKey(config.PreConstraintBuffer.PK) && c.Row == workflowRow);

					var foundNonCCRTask = control.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == nonCCRTask.PK);

					if (aging < agingForBottomCellZone1)
					{
						// aging == 5 has task at bottom of zone-1 pre-constraint buffer
						// it is not shown even at risk because ShouldTaskBeOnBoardMettingAgenda just use approximation to determine zone
						if (subComponentHeadingCell != null && aging != 5)
						{
							AssertCollectionContains(string.Format(CultureInfo.InvariantCulture, "WHEN modifying aging={0} THEN workflow should be in zone 2 or 3", aging), subComponentHeadingCell.SubComponentZones[config.PreConstraintBuffer.PK], new[] { 2, 3 });
						}
						AssertNull(string.Format(CultureInfo.InvariantCulture, "THEN should not show non-CCR-task", aging), foundNonCCRTask);
					}
					else
					{
						if (subComponentHeadingCell != null)
						{
							AssertCollectionContains(string.Format(CultureInfo.InvariantCulture, "WHEN modifying aging={0} THEN workflow should be in zone 0 or 1", aging), subComponentHeadingCell.SubComponentZones[config.PreConstraintBuffer.PK], new[] { 0, 1 });
						}
						AssertNotNull(string.Format(CultureInfo.InvariantCulture, "THEN should show non-CCR-task", aging), foundNonCCRTask);
					}
				}
			}
		}

		[TestDate(2015, 4, 17)]
		public void TestRiskFilterContextMenu_BufferSection()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Buffer, ZDateTime.UtcNow.AddDays(-20));
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer, ZDateTime.UtcNow.AddDays(-1));

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			var section = config.BufferSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });

				var taskCards = control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var taskCard1 = taskCards.Single(t => t.CardContent.TaskIdentifier == task1.PK);
				var taskCard2 = taskCards.Single(t => t.CardContent.TaskIdentifier == task2.PK);

				AssertEquals(0, taskCard1.Cell.Zone.Value);
				AssertEquals(3, taskCard2.Cell.Zone.Value);

				AssertEquals(true, taskCard1.Visible);
				AssertEquals(true, taskCard2.Visible);

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				//There should have the context menu displayed
				AssertEquals(12, contextMenu.Items.Count);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Risk Filter");
				AssertEquals(false, menuItem.Checked);

				menuItem.PerformClick();
				var filter = viewModel.FilterManager.AppliedFilters.OfType<BoardMeetingModeFilter>().Single();
				AssertEquals("Risk Filter", filter.FilterName);
				taskCards = control.FindAll<TaskCardControl>().ToArray();
				taskCard1 = taskCards.SingleOrDefault(t => t.CardContent.TaskIdentifier == task1.PK);
				taskCard2 = taskCards.SingleOrDefault(t => t.CardContent.TaskIdentifier == task2.PK);

				AssertEquals(true, taskCard1.Visible);
				AssertNull(taskCard2);
				AssertEquals(true, menuItem.Checked);

				menuItem.PerformClick();

				taskCards = control.FindAll<TaskCardControl>().ToArray();
				taskCard1 = taskCards.SingleOrDefault(t => t.CardContent.TaskIdentifier == task1.PK);
				taskCard2 = taskCards.SingleOrDefault(t => t.CardContent.TaskIdentifier == task2.PK);
				AssertEquals(true, taskCard1.Visible);
				AssertEquals(true, taskCard2.Visible);
				AssertEquals(false, menuItem.Checked);
			}
		}

		public void TestRiskFilterContextMenu_BucketSection()
		{
			var section = config.BucketSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(i => i.Text == "Risk Filter");

				AssertNull("Risk Filter is only relevant for buffer board sections", menuItem);
			}
		}

		public void TestRiskFilterContextMenu_ReleaseSchedulerSection()
		{
			var section = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(i => i.Text == "Risk Filter");

				AssertNull("Risk Filter is only relevant for buffer board sections", menuItem);
			}
		}

		[TestDate(2015, 4, 17)]
		public void TestRiskFilterContextMenu_ToggleFilter_DbHits()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aaronson");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ZZZ", "Zutroy Z. Zzyzwicz");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", config.Buffer);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4", config.Buffer);
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5", config.Buffer);
			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow6", config.Buffer);
			var workflow7 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow7", config.Buffer);
			var workflow8 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow8", config.Buffer);
			var workflow9 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow9", config.Buffer);
			var workflow10 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow10", config.Buffer);

			BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow2, staff2.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow3, staff1.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow4, staff2.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow5, staff1.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow6, staff2.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow7, staff1.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow8, staff2.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow9, staff1.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow10, staff2.GS_Code, 60);

			var section = config.BufferSection;
			section.SectionConfiguration.OverrideChannels = true;
			staff2.DesignateAsCCR(config.Buffer);

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Risk Filter");
				AssertEquals(false, menuItem.Checked);

				var expectedHitsOnFilterApply = new Dictionary<string, int>
				{
					{ BMComponentResourceLinkSchema.Constants.TableName, 0 }, //  we have already calculated and cached CCR status on board load
					{ ProcessHeaderSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
				};

				using (AssertDbHitsForAllFactories(expectedHitsOnFilterApply, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, thresholdForUnspecified: 0))
				{
					menuItem.PerformClick();
					var filter = viewModel.FilterManager.AppliedFilters.OfType<BoardMeetingModeFilter>().SingleOrDefault();
					AssertEquals("Risk Filter", filter.FilterName);
				}

				var expectedHitsOnFilterUnApply = new Dictionary<string, int>
				{
					{ BMComponentResourceLinkSchema.Constants.TableName, 0 },
					{ ProcessHeaderSchema.Constants.TableName, 0 },
					{ ProcessTasksSchema.Constants.TableName, 0 },
				};

				using (AssertDbHitsForAllFactories(expectedHitsOnFilterUnApply, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, thresholdForUnspecified: 0))
				{
					menuItem.PerformClick();
					var filter = viewModel.FilterManager.AppliedFilters.OfType<BoardMeetingModeFilter>().SingleOrDefault();
					AssertNull(filter);
				}

				var expectedHitsOnFilterReApply = new Dictionary<string, int>
				{
					{ BMComponentResourceLinkSchema.Constants.TableName, 0 }, // we have already calculated and cached CCR status on board load
					{ ProcessHeaderSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
				};

				using (AssertDbHitsForAllFactories(expectedHitsOnFilterReApply, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, thresholdForUnspecified: 0))
				{
					menuItem.PerformClick();
					var filter = viewModel.FilterManager.AppliedFilters.OfType<BoardMeetingModeFilter>().SingleOrDefault();
					AssertEquals("Risk Filter", filter.FilterName);
				}
			}
		}

		#endregion

		#region Constrained Mode

		public void TestConstrainedModeContextMenu_BucketComponent_ShouldNotAddMenuItem()
		{
			var section = config.BucketSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				AssertCollectionNotContains(contextMenu.Items.OfType<ZToolStripMenuItem>(), t => t.Text.Contains("Constrained Mode"));
				AssertEquals("Show Startable Items", contextMenu.Items[0].Text);
			}
		}

		public void TestConstrainedModeContextMenu_ExistingConstrainedResource_AndSubConstraint()
		{
			var system = config.System;
			var buffer = config.Buffer;
			var constraint = CreateConstraint(buffer, offsetMinutes: 96 * 60 / 30);

			var section = config.BufferSection;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Dat Release Group";
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var releaseGroup = CreateReleaseGroup(system, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2);

			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = resource1.GS_Code;
			resourceLink.FD_IsCapacityConstrained = true;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var constrainedMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");

				AssertIsConstrainedMode(releaseGroup, false);

				constrainedMenuItem.PerformClick();

				AssertIsConstrainedMode(releaseGroup, true);
				AssertEquals("Switch to non-Constrained Mode", constrainedMenuItem.Text);
				AssertMultilineASCIIEquals("",
@"Switch Dat Release Group to Constrained Mode?

This will cause Visual Boards to display Buffer sub-components, and the Release Gate to use the Constraint sub-component as the basis for constrained resources' total capacity.", UnitTestUserNotification.Instance.LastMessage.Text);

				constrainedMenuItem.PerformClick();

				AssertIsConstrainedMode(releaseGroup, false);
				AssertEquals("Switch to Constrained Mode", constrainedMenuItem.Text);
				AssertMultilineASCIIEquals("",
@"Switch Dat Release Group to non-Constrained Mode?

This will cause Visual Boards to no longer display Buffer sub-components, and the Release Gate to use the standard process to determine total capacity for all resources.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConstrainedModeContextMenu_WhenNoSecurity()
		{
			var buffer = config.Buffer;
			var constraint = CreateConstraint(buffer, offsetMinutes: 96 * 60 / 30);

			var section = config.BufferSection;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Dat Release Group";
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var releaseGroup = CreateReleaseGroup(config.System, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2);

			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = resource1.GS_Code;
			resourceLink.FD_IsCapacityConstrained = true;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (Env.SetTemporaryUserContext(resource1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var constrainedMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");

				AssertIsConstrainedMode(releaseGroup, false);

				constrainedMenuItem.PerformClick();

				AssertIsConstrainedMode(releaseGroup, false);
				AssertEquals("Switch to Constrained Mode", constrainedMenuItem.Text);
				AssertMultilineASCIIEquals("",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Visual Boards -> Switch To Constrained Mode", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConstrainedModeContextMenu_ExistingConstrainedResource_AndSubConstraint_ButNoReleaseGroup()
		{
			var buffer = config.Buffer;
			var constraint = CreateConstraint(buffer, offsetMinutes: 96 * 60 / 30);

			var section = config.BufferSection;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Dat Release Group";
			var releaseGroup = CreateReleaseGroup(config.System, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2);

			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = resource1.GS_Code;
			resourceLink.FD_IsCapacityConstrained = true;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var constrainedMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");

				AssertIsConstrainedMode(releaseGroup, false);

				constrainedMenuItem.PerformClick();

				AssertIsConstrainedMode(releaseGroup, false);
				AssertEquals("Switch to Constrained Mode", constrainedMenuItem.Text);
				AssertEquals("Cannot switch to Constrained Mode since there is no Release Group configured for this Visual Board or section.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConstrainedModeContextMenu_ExistingConstrainedResource_NoSubConstraint()
		{
			var system = config.System;
			var buffer = CreateBuffer(system, "Mai Buffer");

			var board = system.Boards.AddNew();
			var section = CreateBoardSection(buffer, board);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Dat Release Group";
			board.MB_GG_ReleaseGroup = group.PK;
			var releaseGroup = CreateReleaseGroup(system, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2);

			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = resource1.GS_Code;
			resourceLink.FD_IsCapacityConstrained = true;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var constrainedMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");

				AssertIsConstrainedMode(releaseGroup, false);

				constrainedMenuItem.PerformClick();

				AssertIsConstrainedMode(releaseGroup, false);
				AssertEquals("Switch to Constrained Mode", constrainedMenuItem.Text);
				AssertMultilineASCIIEquals("",
@"Cannot switch Dat Release Group to Constrained Mode since there is no Constraint sub-component within Mai Buffer.

A Constraint sub-component is needed for the Release Gate to use as the basis for constrained resources' total capacity.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConstrainedModeContextMenu_NoConstrainedResource_ExistingSubConstraint()
		{
			var buffer = config.Buffer;
			var constraint = CreateConstraint(buffer, offsetMinutes: 96 * 60 / 30);

			var section = config.BufferSection;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Dat Release Group";
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var releaseGroup = CreateReleaseGroup(config.System, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2);

			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = resource1.GS_Code;
			resourceLink.FD_IsCapacityConstrained = false;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var constrainedMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");

				AssertIsConstrainedMode(releaseGroup, false);

				constrainedMenuItem.PerformClick();

				AssertIsConstrainedMode(releaseGroup, false);
				AssertEquals("Switch to Constrained Mode", constrainedMenuItem.Text);
				AssertMultilineASCIIEquals("",
@"Cannot switch Dat Release Group to Constrained Mode since the group contains no resources marked as capacity constrained.

Candidate capacity constrained resource channels will have an hourglass next to their name. Right-click the channel to mark it as capacity constrained.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertIsConstrainedMode(BMSystemReleaseGroup releaseGroup, bool isConstrainedMode)
		{
			var query = new ZDBOnlyQuery(typeof(BMComponentReleaseGroupLink));
			query.AddToFilter(BMComponentReleaseGroupLinkSchema.FO_GG_ReleaseGroup, releaseGroup.FSG_GG_Group);

			var componentLink = Factory.LoadTop1<BMComponentReleaseGroupLink>(query);
			if (!isConstrainedMode)
			{
				AssertNull(componentLink);
			}
			else
			{
				AssertNotNull(componentLink);
				AssertEquals(true, componentLink.FO_IsConstrainedMode);
			}
		}

		#endregion

		public void TestRefreshSectionMenuItem()
		{
			ContextMenuItemTestHelper("Refresh section [buffer]", testRefresh: true);
		}

		public void TestLegendMenuItem()
		{
			ContextMenuItemTestHelper("Legend");
		}

		public void TestFiltersMenuItem()
		{
			ContextMenuItemTestHelper("Filter Strips", testFilterForm: true);
		}

		public void TestSwitchToConstrainedModeMenuItem()
		{
			ContextMenuItemTestHelper("Switch to Constrained Mode");
		}

		public void TestShowCurrentTasksMenuItem()
		{
			ContextMenuItemTestHelper("Show Startable Items");
		}

		public void TestEditSchedulesThisSectionMenuItem()
		{
			ContextMenuItemTestHelper("Edit Schedules (this section)");
		}

		void ContextMenuItemTestHelper(string menuItemTextAssertion, bool testRefresh = false, bool testFilterForm = false)
		{
			var section = config.BufferSection;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Slick Times", config.Buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });

				var taskCard = control.FindAll<TaskCardControl>().Single();

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(m => m.Text == menuItemTextAssertion);
				AssertNotNull(menuItem);

				if (testRefresh)
				{
					menuItem.PerformClick();

					var newTaskCard = control.FindAll<TaskCardControl>().Single();
					AssertNotEquals("Task card should have been re-created", taskCard, newTaskCard);
				}

				if (testFilterForm)
				{
					menuItem.PerformClick();
					var filterForm = ZFormModaliser.LastFormShownDialogForTest as FilterStripsForm;
					AssertNotNull(filterForm);
				}
			}
		}

		public void TestContextMenu_EditSchedulesThisSectionMenuItem_ShouldShowMessageIfNoJobsInSection()
		{
			ClickMenuStripItemAndAssertError("Edit Schedules (this section)", "This section has no workflows to edit schedules for.");
		}

		public void TestContextMenu_EditSchedulesThisSectionMenuItem_ShouldOpenFormWithAllWorkflowsFromSection()
		{
			var form = ClickMenuStripItemNotOnCellAndGetForm<MultiJobHeaderEditorForm>("Edit Schedules (this section)", "Edit Schedules");
			var jobWorkflows = form.SchedulesGrid.List;
			AssertNotNull(jobWorkflows);
			var message = "\n" +
				"Given there are 6 tasks (belonging to 6 workflows, 2 staff and 2 capabilities) on 2 sections, \n" +
				"And the section is configured to only show a chanel for staff1 \n" +
				"When right clicking on the first section \n" +
				"And selecting Edit Schedules (this section), \n" +
				"Then a new form should open \n" +
				"And its grid should show 4 workflows (3 belonging to staff1 and 1 capability) \n";

			AssertContainsExactElementsInAnyOrder(message,
				new[] { "workflow1 - on buffer 1", "workflow2 - on buffer 1", "workflow3 - on buffer 1", "workflow6 - on buffer 1 - nostaff - capability 2" },
				jobWorkflows.Cast<JobHeaderView>().Select(x => x.ProcessHeader.FH_CompletionStatement));
			form.Close();
		}

		public void TestContextMenu_OpenJobWorkflowsThisSectionMenuItem_ShouldShowMessageIfNoJobsInSection()
		{
			ClickMenuStripItemAndAssertError("Open Job Workflows Module (this section)", "This section has no workflows to open.");
		}

		public void TestContextMenu_OpenJobWorkflowsThisSectionMenuItem_ShouldOpenFormWithAllWorkflowsFromSection()
		{
			var form = ClickMenuStripItemNotOnCellAndGetForm<EmbeddedModulePopup>("Open Job Workflows Module (this section)", "Open Job Workflows");
			var jobWorkflows = form.Module_ForTest.GridCollection;
			AssertNotNull(jobWorkflows);
			var message = "\n" +
				"Given there are 6 tasks (belonging to 6 workflows 2 staff and 2 capabilities) on 2 sections, \n" +
				"And the section is configured to only show a chanel for staff1 \n" +
				"When right clicking on the first section \n" +
				"And selecting Open Job Workflows Module (this section), \n" +
				"Then a new form should open \n" +
				"And its grid should show 4 workflows (3 belonging to staff1 and 1 capability) \n";

			AssertContainsExactElementsInAnyOrder(message,
				new[] { "workflow1 - on buffer 1", "workflow2 - on buffer 1", "workflow3 - on buffer 1", "workflow6 - on buffer 1 - nostaff - capability 2" },
				jobWorkflows.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));
			form.Close();
		}

		void ClickMenuStripItemAndAssertError(string sectionMenuStripItem, string errorMessage)
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });

				var contextMenu = control.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(m => m.Text == sectionMenuStripItem);
				AssertNotNull("\n" +
					"Given a visual board with a section, \n" +
					"When right clicking anywhere on a section, \n" +
					$"Then {sectionMenuStripItem} menu item should be available\n",
					menuItem);

				menuItem.PerformClick();

				AssertEquals("\n" +
					"Given a visual board with no tasks in a section, \n" +
					"When right clicking anywhere in the section \n" +
					$"And selecting {sectionMenuStripItem}, \n" +
					"Then a message dialog should show that there are no jobs.\n",
					errorMessage,
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		T ClickMenuStripItemNotOnCellAndGetForm<T>(string sectionMenuStripItem, string cellSpecificMenuStripItem_ExpectNotExist) where T : ZForm
		{
			var capability1 = BMSTestHelper.CreateCapability(Factory, "BND", "Bandicoots");
			var capability2 = BMSTestHelper.CreateCapability(Factory, "SJW", "Summer Juicy Website");

			var staff1 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "GDM", "Girls Dead Monster", capability1, capability2);
			var staff2 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "DMC", "Detroit Metal City", capability1);

			var buffer1 = config.Buffer;
			var buffer2 = CreateBuffer(config.System, "buffer2");

			var section2 = CreateBoardSection(buffer2, config.BufferBoard, row: 1);

			var jobHeader = CreateJobHeader<OrgHeader>(false, "jh1");

			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1 - on buffer 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-3), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2 - on buffer 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-2), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow3 - on buffer 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-2), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow4 - on buffer 2", buffer2, releaseDateTime: ZDateTime.Now.AddDays(-3), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow5 - on buffer 1 - staff 2 - capability 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff2.GS_Code, capability: capability1);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow6 - on buffer 1 - nostaff - capability 2", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-1), capability: capability2);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff1.PK, true);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(config.BufferBoard);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				AssertEquals(config.BufferBoard.PK, viewModel.CurrentBoardViewModel.BoardPK);
				Application.DoEvents();

				var sectionControls = form.FindAll<BMComponentControl>();
				AssertEquals(2, sectionControls.Count());

				var contextMenu = sectionControls?.FirstOrDefault()?.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var cellSpecificMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(m => m.Text == cellSpecificMenuStripItem_ExpectNotExist);
				AssertNull(cellSpecificMenuItem);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(m => m.Text == sectionMenuStripItem);
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				var popupForm = BMSFormTestHelper.GetOpenForms<T>().FirstOrDefault();
				AssertNotNull("\n" +
					"Given a visual board with tasks in chanels, \n" +
					"When right clicking anywhere in the section \n" +
					$"And selecting {sectionMenuStripItem}, \n" +
					$"Then a new {nameof(T)} should open\n\n",
					popupForm);
				AssertNull("Form should not be modal.", popupForm.Parent);

				return popupForm;
			}
		}

		#region Show Capacity Details

		[TestDate(2019, 5, 4)]
		public void TestShowCapacityDetails_ChannelHeader()
		{
			var constrainedConfig = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(constrainedConfig.Board))
			{
				var control = form.FindAll<ChannelHeaderControl>().First();
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				var menuItem = control.ContextMenuStrip.Items[0];
				AssertEquals("The capacity menu item should be the first menu item in all cases. SAD!", "Show Capacity Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(@"Total Capacity: 25.33 hours

Allocated Capacity in buffer: 0.25 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0.25 hours

Available Capacity: 25.08 hours

Calculated at: 04-May-2019 10:00:00
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacityDetails_AgingHeader()
		{
			var constrainedConfig = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(constrainedConfig.Board))
			{
				var control = form.FindAll<FadeLabel>().First(x => x.Text == "100 %");
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				var menuItem = control.ContextMenuStrip.Items[0];
				AssertEquals("The capacity menu item should be the first menu item in all cases. SAD!", "Show Capacity Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(@"buffer Total: 0 hours
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacityDetails_ZoneHeader()
		{
			var constrainedConfig = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(constrainedConfig.Board))
			{
				var control = form.FindAll<FadeLabel>().First(x => x.Text == "Zone 3");
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				var menuItem = control.ContextMenuStrip.Items[0];
				AssertEquals("The capacity menu item should be the first menu item in all cases. SAD!", "Show Capacity Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(@"buffer Total: 0 hours
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacityDetails_CCRTargetHeader()
		{
			var constrainedConfig = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(constrainedConfig.Board))
			{
				var control = form.FindAll<FadeLabel>().First(x => x.Text == "CCR target");
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				var menuItem = control.ContextMenuStrip.Items[0];
				AssertEquals("The capacity menu item should be the first menu item in all cases. SAD!", "Show Capacity Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(@"buffer Total: 0 hours
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacityDetails_SubComponent()
		{
			var constrainedConfig = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(constrainedConfig.Board))
			{
				var componentControl = form.FindSingle<BMComponentControl>();
				var componentFilterMenuItem = componentControl.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Component View Filter");
				componentFilterMenuItem.ShowDropDown();

				var subComponentFilterMenuItem = componentFilterMenuItem.DropDownItems[1];
				AssertEquals("   Pre-Constraint", subComponentFilterMenuItem.Text);

				subComponentFilterMenuItem.PerformClick();
				Application.DoEvents();

				var control = form.FindAll<FadeLabel>().First(x => x.Text == "Zone 3");
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				var menuItem = control.ContextMenuStrip.Items[0];
				AssertEquals("The capacity menu item should be the first menu item in all cases. SAD!", "Show Capacity Details", menuItem.Text);

				var otherMenuItem = control.ContextMenuStrip.Items[1];
				AssertEquals("Just checking that we've got the Sub-component menu, otherwise this test is sort of meaningless.", "Open Sub-Component Board", otherMenuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(@"buffer Total: 0 hours
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#endregion

		#region Sub-components

		public void TestCellContent_Buffer_NoChannels_SubComponent()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 96 * 60;

			BMSTestHelper.CreateConstraint(section.Component, "Constraint", 16 * 60);

			var subComponent = section.Component.ChildComponents.AddNew();
			subComponent.FC_Type = BMComponentTypeList.Codes.Buffer;
			subComponent.FC_Name = "sub-buffer";
			subComponent.FC_BufferTimespanInMinutes = 48 * 60;
			subComponent.FC_OffsetInMinutes = 16 * 60;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				for (int i = 0; i < 13; i++)
				{
					AssertCellContents(control, ageHeadingPosition, i, CellContentType.AgeHeading);
					AssertCellContents(control, zoneHeadingPosition, i, CellContentType.ZoneHeading);
					AssertCellContents(control, 2, i, CellContentType.SubComponentZoneHeading);
					AssertCellContents(control, 3, i, CellContentType.Cards);
				}
			}
		}

		public void TestCellContent_Buffer_NoChannels_SubComponents()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 96 * 60;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var subComponent1 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer1", 48 * 60);
			section.Component.ChildComponents.Add(subComponent1);

			BMSTestHelper.CreateConstraint(section.Component, "cons", 48 * 60);

			var subComponent2 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer2", 48 * 60, 48 * 60);
			section.Component.ChildComponents.Add(subComponent2);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 1, 0, CellContentType.ZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 1, 1, CellContentType.ZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 1, 2, CellContentType.ZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 1, 3, CellContentType.ZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 1, 4, CellContentType.ZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 1, 5, CellContentType.ZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 1, 6, CellContentType.ZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 1, 7, CellContentType.ZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 1, 8, CellContentType.ZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 1, 9, CellContentType.ZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 1, 10, CellContentType.ZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 1, 11, CellContentType.ZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 1, 12, CellContentType.ZoneHeading, labelText: "Zone 3");

				AssertCellContents(control, 2, 0, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 1, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 2, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 3, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 4, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 5, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 6, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 7, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 2, 8, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 2, 9, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 2, 10, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 2, 11, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 12, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");

				AssertCellContents(control, 3, 0, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 3, 1, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 3, 2, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 3, 3, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 3, 4, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 3, 5, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 6, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 7, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 8, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 9, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 10, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 11, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 12, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
			}
		}

		public void TestSectionConfigurationFontSize_ShouldStaySameWhenDPIScaled()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 96 * 60;

			BMSTestHelper.CreateConstraint(section.Component);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var col = 1;
				var row = 0;

				var table = control.Table;
				var cellControl = table.GetControlFromPosition(col, row);
				var headerControl = cellControl.Controls[0];
				var label = headerControl as Label;
				AssertNotNull(string.Format("Cell at col={0}, row={1} should be a zone header label, but was a {2}", col, row, headerControl.GetType().Name), label);

				form.Show();

				AssertEquals("Value should be explicitly set to 8.25pt on any DPI scale", 8.25f, label.Font.SizeInPoints);
			}
		}

		public void TestCellContent_Buffer_NoChannels_StackedSubComponents()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 10, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 72 * 60;

			var subComponent1 = section.Component.ChildComponents.AddNew();
			subComponent1.FC_Type = BMComponentTypeList.Codes.Buffer;
			subComponent1.FC_Name = "sub-buffer1";
			subComponent1.FC_BufferTimespanInMinutes = 48 * 60;
			subComponent1.FC_OffsetInMinutes = 16 * 60;

			BMSTestHelper.CreateConstraint(section.Component);

			var subComponent2 = section.Component.ChildComponents.AddNew();
			subComponent2.FC_Type = BMComponentTypeList.Codes.Buffer;
			subComponent2.FC_Name = "sub-buffer2";
			subComponent2.FC_BufferTimespanInMinutes = 40 * 60;
			subComponent2.FC_OffsetInMinutes = 32 * 60;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 2, 0, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 1, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 2, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 2, 3, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 2, 4, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 2, 5, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 2, 6, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 7, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 8, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 9, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");

				AssertCellContents(control, 3, 0, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 3, 1, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 3, 2, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 3, 3, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 3, 4, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 5, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 6, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 7, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 8, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 9, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
			}
		}

		public void TestCellContent_Buffer_TwoChannels_StackedSubComponents()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 10, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 72 * 60;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var subComponent1 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer1", 48 * 60, 16 * 60);
			section.Component.ChildComponents.Add(subComponent1);

			BMSTestHelper.CreateConstraint(section.Component);

			var subComponent2 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer2", 40 * 60, 32 * 60);
			section.Component.ChildComponents.Add(subComponent2);

			section.SectionConfiguration.OverrideChannels = true;
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 2, 0, CellContentType.Label, labelText: string.Empty);
				AssertCellContents(control, 2, 1, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 2, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 2, 3, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 2, 4, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 2, 5, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 2, 6, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 2, 7, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 8, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 9, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 2, 10, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");

				AssertCellContents(control, 3, 0, CellContentType.Label, labelText: string.Empty);
				AssertCellContents(control, 3, 1, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				AssertCellContents(control, 3, 2, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 3, 3, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				AssertCellContents(control, 3, 4, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				AssertCellContents(control, 3, 5, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 6, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 7, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 8, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 9, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				AssertCellContents(control, 3, 10, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
			}
		}

		public void TestCellContent_CCRHeadings_Vertical()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCC", "Non CCR Resource 1");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR1, resourceNonCCR1);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				AssertCellContents(control, 1, 5, CellContentType.CCRHeading, labelText: "CCR target");
				AssertCellContents(control, 1, 6, CellContentType.CCRHeading, labelText: "CCR target");
				AssertCellContents(control, 1, 7, CellContentType.CCRHeading, labelText: "CCR target");
			}
		}

		public void TestShowSubComponentBoard_NoBoardsDefined()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 10, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 72 * 60;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var subComponent1 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer1", 48 * 60, 16 * 60);
			section.Component.ChildComponents.Add(subComponent1);

			BMSTestHelper.CreateConstraint(section.Component);

			var subComponent2 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer2", 40 * 60, 32 * 60);
			section.Component.ChildComponents.Add(subComponent2);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var zoneHeading = control.Table.GetControlFromPosition(3, 1);

				var label = zoneHeading.FindAll<FadeLabel>().Single();

				AssertNotNull(label.ContextMenuStrip);
				((LazyContextMenuStrip)label.ContextMenuStrip).AddItems_ForTest(label);

				AssertEquals(2, label.ContextMenuStrip.Items.Count);

				var menuItem = (ZToolStripMenuItem)label.ContextMenuStrip.Items[1];
				menuItem.ShowDropDown();

				try
				{
					AssertEquals("Open Sub-Component Board", menuItem.Text);
					AssertEquals(0, menuItem.DropDownItems.Count);
					menuItem.PerformClick();

					AssertEquals("There are no Visual Boards configured with this sub-component as a section.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					menuItem.HideDropDown();
				}
			}
		}

		public void TestShowSubComponentBoard_BoardsDefined_OnePreConstraint_OnePostConstraint()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 10, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 72 * 60;

			var buffer = config.Buffer;

			var subComponent1 = BMSTestHelper.CreateSubBuffer(buffer, "pre-constraint", 48 * 60);
			section.Component.ChildComponents.Add(subComponent1);

			BMSTestHelper.CreateConstraint(section.Component, "constraint", 48 * 60);

			var subComponent2 = BMSTestHelper.CreateSubBuffer(buffer, "post-constraint", 24 * 60, 48 * 60);
			section.Component.ChildComponents.Add(subComponent2);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var system = section.Component.System;

			var board1 = system.Boards.AddNew();
			board1.MB_Name = "board1";
			var section1 = board1.Sections.AddNew();
			section1.MS_FC_Component = subComponent1.PK;

			var board2 = system.Boards.AddNew();
			board2.MB_Name = "board2";
			var section2 = board2.Sections.AddNew();
			section2.MS_FC_Component = subComponent2.PK;

			var board3 = system.Boards.AddNew();
			board3.MB_Name = "board3";
			var section3 = board3.Sections.AddNew();
			section3.MS_FC_Component = subComponent2.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var preConstraintHeadingRow = 2;
			AssertSubComponentBoardDropDownItems(preConstraintHeadingRow, section, viewModel, 1, "board1");

			var postConstraintHeadingRow = 3;
			AssertSubComponentBoardDropDownItems(postConstraintHeadingRow, section, viewModel, 2, "board2", "board3");
		}

		public void TestShowSubComponentBoard_BoardsDefined_OnePreConstraint_TwoPostConstraints()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 10, FlowDirectionList.Codes.Up, string.Empty, ChannelTypeList.Codes.NotChanneled).Item1;
			section.Component.FC_BufferTimespanInMinutes = 72 * 60;

			var system = section.Component.System;
			var buffer = config.Buffer;

			var subComponent1 = BMSTestHelper.CreateSubBuffer(buffer, "pre-constraint", 48 * 60);
			section.Component.ChildComponents.Add(subComponent1);

			BMSTestHelper.CreateConstraint(section.Component, "constraint", 48 * 60);

			var subComponent2 = BMSTestHelper.CreateSubBuffer(buffer, "post-constraint 1", 24 * 60, 48 * 60);
			section.Component.ChildComponents.Add(subComponent2);

			var subComponent3 = BMSTestHelper.CreateSubBuffer(buffer, "post-constraint 2", 12 * 60, 48 * 60);
			section.Component.ChildComponents.Add(subComponent3);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(section.Component.System, group, constrainedModeComponent: section.Component);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var board1 = system.Boards.AddNew();
			board1.MB_Name = "board1";
			var section1 = board1.Sections.AddNew();
			section1.MS_FC_Component = subComponent1.PK;

			var board2 = system.Boards.AddNew();
			board2.MB_Name = "board2";
			var section2 = board2.Sections.AddNew();
			section2.MS_FC_Component = subComponent2.PK;

			var board3 = system.Boards.AddNew();
			board3.MB_Name = "board3";
			var section3 = board3.Sections.AddNew();
			section3.MS_FC_Component = subComponent3.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var preConstraintHeadingRow = 2;
			AssertSubComponentBoardDropDownItems(preConstraintHeadingRow, section, viewModel, 1, "board1");

			var postConstraintHeadingRow1 = 3;
			AssertSubComponentBoardDropDownItems(postConstraintHeadingRow1, section, viewModel, 1, "board2");

			var postConstraintHeadingRow2 = 4;
			AssertSubComponentBoardDropDownItems(postConstraintHeadingRow2, section, viewModel, 1, "board3");
		}

		void AssertSubComponentBoardDropDownItems(int row, BMBoardSection section, BMBoardSectionViewModel viewModel, int dropDownItemsCount, params string[] dropDownItemsText)
		{
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				var zoneHeading = control.Table.GetControlFromPosition(row, 1);

				var label = zoneHeading.FindAll<FadeLabel>().Single();

				AssertNotNull(label.ContextMenuStrip);
				((LazyContextMenuStrip)label.ContextMenuStrip).AddItems_ForTest(label);

				AssertEquals(2, label.ContextMenuStrip.Items.Count);

				var menuItem = (ZToolStripMenuItem)label.ContextMenuStrip.Items[1];
				menuItem.ShowDropDown();

				try
				{
					AssertEquals("Open Sub-Component Board", menuItem.Text);
					AssertEquals(dropDownItemsCount, menuItem.DropDownItems.Count);

					var i = 0;
					foreach (var text in dropDownItemsText)
					{
						AssertEquals(text, menuItem.DropDownItems[i++].Text);
					}
				}
				finally
				{
					menuItem.HideDropDown();
				}
			}
		}

		public void TestSectionHeaderIsInCenter()
		{
			var section = config.BucketSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var label = control.Controls.OfType<FlowLayoutPanel>().First().Controls.OfType<TableLayoutPanel>().First().Controls.OfType<ZLabel>().First();
				var table = control.Controls.OfType<FlowLayoutPanel>().First().Controls.OfType<TableLayoutPanel>().First();

				AssertEquals(25, table.Padding.Left);

				control.Size = ControlDpiScalingHelper.NewScaledSize(1000, 1000);
				AssertCloseEnough(ControlDpiScalingHelper.ScaleToCurrentDpiX(470), table.Padding.Left);

				control.Size = ControlDpiScalingHelper.NewScaledSize(2, 2);
				AssertEquals(0, table.Padding.Left);
			}
		}

		[TestDate(2013, 10, 4)]
		public void TestShowSubComponentBoard_CollapseSubComponentZoneHeading()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR Resource");
			var resourceNonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCC", "Non CCR Resource");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(section.Component, "c", 72 * 60);
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR, resourceNonCCR);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var task = workflow1.Parent.WorkflowItems[0];
			var taskPreConstraint = CreateTask(workflow1, resourceNonCCR.GS_Code, 60, sequence: 400, description: "Workflow 1 - task 2");
			Assert("Pre-constraint task", taskPreConstraint.P9_Sequence < task.P9_Sequence);
			var taskPostConstraint = CreateTask(workflow1, resourceNonCCR.GS_Code, 60, sequence: 600, description: "Workflow 1 - task 3");
			Assert("Post-constraint task", taskPostConstraint.P9_Sequence > task.P9_Sequence);

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0],
				workflow1.Parent.WorkflowItems[1],
				workflow1.Parent.WorkflowItems[2],
				workflow2.Parent.WorkflowItems[0],
			};

			var subComponentPosition = 4;

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				for (int i = 1; i < 5; i++)
				{
					AssertCellContents(control, subComponentPosition, i, CellContentType.SubComponentZoneHeading, labelText: "Zone 0");
				}

				for (int i = 5; i < 8; i++)
				{
					AssertCellContents(control, subComponentPosition, i, CellContentType.SubComponentZoneHeading, labelText: "Zone 1");
				}

				for (int i = 8; i < 11; i++)
				{
					AssertCellContents(control, subComponentPosition, i, CellContentType.SubComponentZoneHeading, labelText: "Zone 2");
				}

				for (int i = 11; i < 14; i++)
				{
					AssertCellContents(control, subComponentPosition, i, CellContentType.SubComponentZoneHeading, labelText: "Zone 3");
				}

				grid.AllocateTasks_ForTest(section, viewModel, tasks);

				var fadedZone3Color = BMConstants.Zone3DefaultColor.FadeTowardsWhite();
				var preConstraintZoneHeadingFadeCellCollapseTo = grid.Cells.SingleOrDefault(c => c.ContentType.In(CellContentType.SubComponentZoneHeading) && c.Row == 11);
				AssertFadeCell(preConstraintZoneHeadingFadeCellCollapseTo, 11, subComponentPosition, BMConstants.Zone3DefaultColor, fadedZone3Color);
			}
		}

		[ExpectNoExceptions]
		public void TestViewModelPropertyChanged_ShouldCatchAsyncException()
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			Factory.Save();

			using (var control = new ExceptionThrowingBMComponentControl(config.BufferSection.SectionConfiguration, viewModel))
			{
				control.CreateControl();
				control.ShouldThrowException = true;
				viewModel.SubHeadingAppearance = new LoadingBandsSubHeadingAppearance();
			}
		}

		class ExceptionThrowingBMComponentControl : BMComponentControl
		{
			public bool ShouldThrowException { get; set; }

			public ExceptionThrowingBMComponentControl(BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionViewModel viewModel)
				: base(sectionConfiguration, viewModel)
			{ }

			protected override void UpdateSubHeadingContent()
			{
				if (ShouldThrowException)
				{
					throw new InvalidAsynchronousStateException();
				}
				else
				{
					base.UpdateSubHeadingContent();
				}
			}
		}

		static void AssertFadeCell(CellContent fadeCell, int expectedRow, int expectedColumn, Color expectedBackColor, Color? expectedFadeColor)
		{
			AssertNotNull("Should have found a fade cell", fadeCell);
			AssertEquals("Cell row", expectedRow, fadeCell.Row);
			AssertEquals("Cell column", expectedColumn, fadeCell.Column);
			AssertColorEquals("BackColor", expectedBackColor, fadeCell.BackColor.Value);
			if (expectedFadeColor != null)
			{
				AssertColorEquals("FadeColor", (Color)expectedFadeColor, fadeCell.BackgroundFadeColor.Value);
			}
		}

		#endregion

		#region Filters

		[TestDate(2013, 12, 3, 7, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestFiltersStillAppliedOnHeaderExpansion()
		{
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Monday, WorkingDaysTestHelper.NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Tuesday, WorkingDaysTestHelper.NineToFive);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo");

			var section = config.BufferSection;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-2);
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				form.EnterBoardMeetingMode();

				var filter = form.SlideShowViewModel.FilterManager.AppliedFilters.OfType<BoardMeetingModeFilter>().Single();
				AssertEquals("Board Meeting", filter.FilterName);

				var control = form.FindAll<BMComponentControl>().Single();

				var taskCard = control.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNull(taskCard);

				control.HeaderClicked(new CellContent(0, 1, CellContentType.ChannelHeading));
				Application.DoEvents();

				taskCard = control.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNull(taskCard);
			}
		}

		#region Context Menu Custom Filters

		public void TestSettingCustomFilterInContextMenuShouldModifyTaskAndWorkflowFilter()
		{
			var section = config.BufferSection;
			FilterStripsTestHelper.AddStartsWithFilter(section.WorkflowFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Success is the ability to move from one failure to another without loss of enthusiasm. (Winston Churchill)");
			FilterStripsTestHelper.AddStartsWithFilter(section.TaskFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "A journey of a thousand miles begins with a single step. (Confucius)");

			Factory.Save();

			var workflowFilterCopy = (StmModuleFilter)section.WorkflowFilter.Clone();
			var taskFilterCopy = (StmModuleFilter)section.TaskFilter.Clone();

			var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "CustomFilter");
			FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Life is like riding a bicycle. To keep your balance, you must keep moving. (Albert Einstein)");

			CombineAssertions("Setting a workflow filter...", () =>
			{
				using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
				{
					form.Show();
					var control = form.FindAll<BMComponentControl>().Single();
					var controlViewModel = control.ViewModel;

					AssertEquals("Custom filters should not yet be set even if section filters are set in the database", false, controlViewModel.AreWorkflowOrTaskFiltersRedefined);

					control.SetCustomWorkflowFilterForTestingAndRefresh(customFilter);

					AssertEquals("Custom filters should be set", true, controlViewModel.AreWorkflowOrTaskFiltersRedefined);
					AssertContains("Custom filters should be set", "Albert Einstein", controlViewModel.OverriddenWorkflowSectionFilter.LiteralTextSqlFormatted);
				}
			});

			CombineAssertions("Setting a task filter...", () =>
			{
				using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
				{
					form.Show();
					var control = form.FindAll<BMComponentControl>().Single();
					var controlViewModel = control.ViewModel;

					AssertEquals("Custom filters should not yet be set even if section filters are set in the database", false, controlViewModel.AreWorkflowOrTaskFiltersRedefined);

					control.SetCustomTaskFilterForTestingAndRefresh(taskFilterCopy);
					AssertEquals("Setting the same filter should not be considered as changing the one", false, controlViewModel.AreWorkflowOrTaskFiltersRedefined);

					control.SetCustomTaskFilterForTestingAndRefresh(customFilter);

					AssertEquals("Custom filters should be set", true, controlViewModel.AreWorkflowOrTaskFiltersRedefined);
					AssertContains("Custom filters should be set", "Albert Einstein", controlViewModel.OverriddenTaskSectionFilter.LiteralTextSqlFormatted);
				}
			});
		}

		public void TestSettingCustomTaskFilterInContextMenuShouldNotBlow_WhenRemovingAllFilterStrips()
		{
			var section = config.BufferSection;
			Factory.Save();

			var sectionLoaded = (new BusinessObjectFactory()).Load<BMBoardSection>(section.PK);
			AssertNotNull("Precondition", sectionLoaded);

			var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "CustomFilter");
			FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Life is like riding a bicycle. To keep your balance, you must keep moving. (Albert Einstein)");

			var emptyFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "EmptyFilter");
			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				control.SetCustomTaskFilterForTestingAndRefresh(customFilter);
				AssertNoExceptionThrown(() => control.SetCustomTaskFilterForTestingAndRefresh(emptyFilter));
			}
		}

		#endregion

		#endregion

		#region Task Aging

		[TestDate(2013, 12, 3, 7, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestTaskAging_UsingEnvironmentBranchDept()
		{
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Monday, WorkingDaysTestHelper.NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Tuesday, WorkingDaysTestHelper.NineToFive);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo");

			var section = config.BucketSection;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-2);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 1024, Height = 768 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskCard = control.FindAll<TaskCardControl>().Single();
				var taskPanel = taskCard.Parent;
				AssertEquals("Should be in content column", 1, control.Table.GetColumn(taskPanel));
				AssertEquals("Should be in third time unit row", 2, control.Table.GetRow(taskPanel));
			}
		}

		[TestDate(2013, 12, 3, 17, 0, 0)]
		public void TestTaskAging_UsingBoardBranchDept()
		{
			var boardBranch = Factory.NewWithValidTestData<GlbBranch>();
			boardBranch.GB_RL_NKHomePort = "GBLON";
			var boardDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var boardWorkTime = Factory.NewWithValidTestData<GlbWorkTime>();
			boardWorkTime.GW_ParentID = boardDepartment.PK;
			boardWorkTime.GW_ParentTableCode = GlbDepartmentSchema.Constants.Prefix;

			Factory.Save();

			const string nineToFive = WorkingDaysTestHelper.NineToFive;

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, boardDepartment.PK, DayOfWeek.Monday, nineToFive.Substring(0, nineToFive.Length - 2));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, boardDepartment.PK, DayOfWeek.Tuesday, nineToFive.Substring(0, nineToFive.Length - 2));

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo");

			var bucket = config.Bucket;
			var board = CreateBoard(config.System);
			board.MB_GB_AgingBranch = boardBranch.PK;
			board.MB_GE_AgingDepartment = boardDepartment.PK;

			var section = CreateBoardSection(bucket, board);
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddHours(-2);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 1024, Height = 768 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskCard = control.FindAll<TaskCardControl>().Single();
				var taskPanel = taskCard.Parent;
				AssertEquals("Should be in content column", 1, control.Table.GetColumn(taskPanel));
				AssertEquals("Should be in second time unit row", 1, control.Table.GetRow(taskPanel));
			}
		}

		[TestDate(2013, 12, 3, 17, 0, 0)]
		public void TestTaskAging_UsingComponentBranchDept()
		{
			var componentBranch = Factory.NewWithValidTestData<GlbBranch>();
			componentBranch.GB_RL_NKHomePort = "GBLON";
			var componentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var componentWorkTime = Factory.NewWithValidTestData<GlbWorkTime>();
			componentWorkTime.GW_ParentID = componentDepartment.PK;
			componentWorkTime.GW_ParentTableCode = GlbDepartmentSchema.Constants.Prefix;

			Factory.Save();

			const string nineToFive = WorkingDaysTestHelper.NineToFive;

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, componentDepartment.PK, DayOfWeek.Monday, nineToFive.Substring(0, nineToFive.Length - 2));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, componentDepartment.PK, DayOfWeek.Tuesday, nineToFive.Substring(0, nineToFive.Length - 2));

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo");

			var bucket = config.Bucket;
			bucket.FC_GB_AgingBranch = componentBranch.PK;
			bucket.FC_GE_AgingDepartment = componentDepartment.PK;

			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddHours(-2);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 1024, Height = 768 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskCard = control.FindAll<TaskCardControl>().Single();
				var taskPanel = taskCard.Parent;
				AssertEquals("Should be in content column", 1, control.Table.GetColumn(taskPanel));
				AssertEquals("Should be in second time unit row", 1, control.Table.GetRow(taskPanel));
			}
		}

		[TestDate(2013, 12, 3, 17, 0, 0)]
		public void TestTaskAging_UsingBoardOverridingComponentBranchDept()
		{
			var boardBranch = Factory.NewWithValidTestData<GlbBranch>();
			boardBranch.GB_RL_NKHomePort = "GBLON";
			var boardDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var boardWorkTime = Factory.NewWithValidTestData<GlbWorkTime>();
			boardWorkTime.GW_ParentID = boardDepartment.PK;
			boardWorkTime.GW_ParentTableCode = GlbDepartmentSchema.Constants.Prefix;

			var componentBranch = Factory.NewWithValidTestData<GlbBranch>();
			componentBranch.GB_RL_NKHomePort = "GBLON";
			var componentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var componentWorkTime = Factory.NewWithValidTestData<GlbWorkTime>();
			componentWorkTime.GW_ParentID = componentDepartment.PK;
			componentWorkTime.GW_ParentTableCode = GlbDepartmentSchema.Constants.Prefix;

			Factory.Save();

			const string nineToFive = WorkingDaysTestHelper.NineToFive;

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, componentDepartment.PK, DayOfWeek.Monday, nineToFive.Substring(0, nineToFive.Length - 2));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, componentDepartment.PK, DayOfWeek.Tuesday, nineToFive.Substring(0, nineToFive.Length - 2));

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, boardDepartment.PK, DayOfWeek.Monday, nineToFive.Substring(0, nineToFive.Length - 4));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, boardDepartment.PK, DayOfWeek.Tuesday, nineToFive.Substring(0, nineToFive.Length - 4));

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo");

			var bucket = config.Bucket;
			bucket.FC_GB_AgingBranch = componentBranch.PK;
			bucket.FC_GE_AgingDepartment = componentDepartment.PK;

			var board = CreateBoard(config.System);
			board.MB_GB_AgingBranch = boardBranch.PK;
			board.MB_GE_AgingDepartment = boardDepartment.PK;

			var section = CreateBoardSection(bucket, board);
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddHours(-2);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 1024, Height = 768 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskCard = control.FindAll<TaskCardControl>().Single();
				var taskPanel = taskCard.Parent;
				AssertEquals("Should be in content column", 1, control.Table.GetColumn(taskPanel));
				AssertEquals("Should be in first time unit row - using board working time instead of component", 0, control.Table.GetRow(taskPanel));
			}
		}

		[TestDate(2014, 1, 29, 7, 0, 0)] // 17:00 AEDT
		[TestUtcOffset(11, 0, 0)] // AEDT
		public void TestTaskAging_ShouldAgeWithinAppropriateTimeZone()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			config.Buffer.FC_BufferTimespanInMinutes = 8 * 60;
			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var minutesPerCell = (int)section.SectionConfiguration.TimePerCell.GetMinutesFromDateTimeSpan() + 1;

			for (int row = 12; row >= 0; row--)
			{
				using (var form = new ZForm { Width = 1024, Height = 768 })
				using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
				{
					form.Controls.Add(control);
					form.Show();

					control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
					Application.DoEvents();

					var taskCard = control.FindAll<TaskCardControl>().First();
					var taskPanel = (TaskPanel)taskCard.Parent;
					AssertEquals(row, control.Table.GetRow(taskPanel));
					AssertEquals(2, control.Table.GetColumn(taskPanel));
				}

				workflow.FH_ReleaseDateTime = workflow.FH_ReleaseDateTime.AddMinutes(-minutesPerCell);
				Factory.Save();
			}
		}

		#endregion

		#region Channel Status

		[TestDate(2014, 1, 31)]
		public void TestChannelStatus_ShouldHaveAgePercentageOnFirstLoad()
		{
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var section = config.BufferSection;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Nataliia is back! :D", section.Component);
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });

				var channelHeaderControl = control.FindAll<ChannelHeaderControl>().Single();
				AssertEquals("Zone 3, Idle", channelHeaderControl.ChannelStatus);
			}
		}

		#endregion

		#region SetupTasks

		[ExpectNoExceptions]
		public void TestControlWithNullForm()
		{
			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");

			var section = config.BufferSection;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
			}
		}

		#endregion

		#region TaskCardRefresh

		public void TestTaskConsolidatedStatusShownOnCard_JobWorkflow()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");

			var system = config.System;
			var bucket = config.Bucket;
			var board = VisualBoardsTestHelper.CreateBoard(system, "Why do we call it an board, thats stupid");
			var section = BMSTestHelper.CreateBoardSection(bucket, board, setReleaseGroupIfRequired: false);
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Release", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Group", releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, description: "task1");
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, description: "task2");
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Test Status", releaseGroupPK: config.ReleaseGroup.PK);

			var task1_workflow1_2 = BMSTestHelper.CreateTask(workflow1_2, resource.GS_Code, description: "task1_workflow1_2");
			var task2_workflow1_2 = BMSTestHelper.CreateTask(workflow1_2, resource.GS_Code, description: "task2_workflow1_2");
			task2_workflow1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			using (VisualBoardsTestHelper.UseBizoCardContents())
			using (TaskCardRendererFactory.OverrideRenderer(new NonDisposingTaskCardRenderer()))
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().Single();
				var taskCards = form.FindAll<TaskCardControl>();

				var card1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task2.PK ||
													c.CardContent.TaskIdentifier == task1.PK);
				var card2 = taskCards.Single(c => c.CardContent.TaskIdentifier == task1_workflow1_2.PK ||
													c.CardContent.TaskIdentifier == task2_workflow1_2.PK);

				var pics1 = card1.FindAll<StatusIndicatorControl>().Single();
				var pics2 = card2.FindAll<StatusIndicatorControl>().Single();

				AssertEquals(StatusIndicatorControl.WorkingStatusImage, pics1.ShownImages.Single());
				AssertEquals(StatusIndicatorControl.SuspendedStatusImage, pics2.ShownImages.FirstOrDefault());

				card1.ShowDetailedCard();
				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				var button = detailedCard.FindAll<GenericStatusChangeButton>().SingleOrDefault(b => b.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				AssertNull(button); // play/suspend buttons are not shown on Workflow/JobWorkflow detailed card.

				card2.ShowDetailedCard();
				detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				button = detailedCard.FindAll<GenericStatusChangeButton>().SingleOrDefault(b => b.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Suspended);
				AssertNull(button); // play/suspend buttons are not shown on Workflow/JobWorkflow detailed card.
			}
		}

		public void TestTaskConsolidatedStatusShownOnCard_Workflow()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");

			var system = config.System;
			var bucket = config.Bucket;
			var board = VisualBoardsTestHelper.CreateBoard(system, "Why do we call it an board, thats stupid");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Release", releaseGroupPK: config.ReleaseGroup.PK);
			var task1_workflow1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code);
			task1_workflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task2_workflow1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code); // status 'assigned'

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Group", releaseGroupPK: config.ReleaseGroup.PK);
			var task1_workflow2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code); // status 'assigned'
			var task2_workflow2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code); // status 'assigned'

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "Group", releaseGroupPK: config.ReleaseGroup.PK);
			var task1_workflow3 = BMSTestHelper.CreateTask(workflow3, resource.GS_Code);
			task1_workflow3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			using (VisualBoardsTestHelper.UseBizoCardContents())
			using (TaskCardRendererFactory.OverrideRenderer(new NonDisposingTaskCardRenderer()))
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().Single();
				var taskCards = form.FindAll<TaskCardControl>();

				var card1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task1_workflow1.PK ||
														c.CardContent.TaskIdentifier == task2_workflow1.PK);

				var card2 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task1_workflow2.PK ||
														c.CardContent.TaskIdentifier == task2_workflow2.PK);

				var card3 = taskCards.Single(c => c.CardContent.TaskIdentifier == task1_workflow3.PK);

				var pics1 = card1.FindAll<StatusIndicatorControl>().Single();
				var pics2 = card2.FindAll<StatusIndicatorControl>().Single();
				var pics3 = card3.FindAll<StatusIndicatorControl>().Single();

				AssertEquals(StatusIndicatorControl.WorkingStatusImage, pics1.ShownImages.FirstOrDefault());
				AssertEquals(StatusIndicatorControl.CurrentTaskImage, pics2.ShownImages.Single());
				AssertEquals(StatusIndicatorControl.SuspendedStatusImage, pics3.ShownImages.FirstOrDefault());

				card1.ShowDetailedCard();
				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				var button = detailedCard.FindAll<GenericStatusChangeButton>().SingleOrDefault(b => b.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				AssertNull(button); // play/suspend buttons are not shown on Workflow/JobWorkflow detailed card.

				card2.ShowDetailedCard();
				detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				button = detailedCard.FindAll<GenericStatusChangeButton>().SingleOrDefault(b => b.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Suspended);
				AssertNull(button); // play/suspend buttons are not shown on Workflow/JobWorkflow detailed card.

				card3.ShowDetailedCard();
				detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				button = detailedCard.FindAll<GenericStatusChangeButton>().SingleOrDefault(b => b.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				AssertNull(button); // play/suspend buttons are not shown on Workflow/JobWorkflow detailed card.
			}
		}

		public void TestTaskConsolidatedStatusShownOnCard_JobWorkflowPrerequisites()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");

			var system = config.System;
			var bucket = config.Bucket;
			var board = VisualBoardsTestHelper.CreateBoard(system, "Why do we call it an board, thats stupid");
			var section = BMSTestHelper.CreateBoardSection(bucket, board, setReleaseGroupIfRequired: false);
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Release", releaseGroupPK: config.ReleaseGroup.PK);
			var task1_workflow1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Group", releaseGroupPK: config.ReleaseGroup.PK);
			var task1_workflow2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code); // status 'assigned'
			var task2_workflow2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code); // status 'assigned'

			workflow1.MakePrerequisiteOf(workflow2);

			Factory.Save();

			using (VisualBoardsTestHelper.UseBizoCardContents())
			using (TaskCardRendererFactory.OverrideRenderer(new NonDisposingTaskCardRenderer()))
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().Single();
				var taskCards = form.FindAll<TaskCardControl>();

				var card = taskCards.FirstOrDefault();
				var pics2 = card.FindAll<StatusIndicatorControl>().Single();

				AssertEquals(StatusIndicatorControl.CurrentTaskImage, pics2.ShownImages.Single());
			}
		}

		class NonDisposingTaskCardRenderer : ITaskCardRenderer
		{
			TaskCardControl ITaskCardRenderer.ConstructTaskCardControl(KContextMenuStrip menuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
			{
				return new TaskCardControl(menuStrip, cardContent, viewModel, cell, shouldEnableDragDrop: true, canUseBitmapCache: false, renderAsBitmapOnLoad: false, model: null, bitmaps: null);
			}

			TaskCardControl ITaskCardRenderer.ConstructTaskCardBasedOnTemplate(KContextMenuStrip menuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
			{
				throw new NotImplementedException();
			}

			void ITaskCardRenderer.OnRenderingCardsCompleted()
			{
				// Nothing necessary;
			}
		}

		#endregion

		#region Zero-Cell Section

		public void TestZeroCellSections_ShouldNotLoadTasksAndThusReportException()
		{
			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(config.Buffer, 0, 0, 0, 0, 0, 0, "Acceptaboop Bandacoot");
			band.BAB_FiltersByReleaseGroup = true;

			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 0);
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);
			section.SectionConfiguration.CellsPerSubsection = 0;

			BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		#region Refresh Board

		#region Differential Refresh

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestProcessUpdatedCard_OtherThreadRemoveCardFromPanel()
		{
			config.BufferSection.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;

			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS1", "Test 1");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff1.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow1", description: "task1", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff1.GS_Code, lowEstMinutes: 15);
			var task1 = workflow1.Tasks.First();

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow2", description: "task2", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff1.GS_Code, lowEstMinutes: 15);
			var task2 = workflow2.Tasks.First();

			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow3", description: "task3", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff1.GS_Code, lowEstMinutes: 15);
			var task3 = workflow3.Tasks.First();

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == config.BufferSection.PK);
				var cards = bmComponentControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals("Precondition", 3, cards.Length);

				var newFactory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TestDifferentialRefresh_Reordering_TopCardWasClosed", RefreshEnabled = false };

				var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
				loadedTask1.P9_Description = loadedTask1.P9_Description + " (updated)";

				var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
				loadedTask2.P9_Description = loadedTask2.P9_Description + " (updated)";

				var loadedTask3 = newFactory.Load<ProcessTask>(task3.PK);

				newFactory.Save();

				var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
				AssertEquals("THEN workflow1 FH_SystemLastEditTimeUtc should be updated", new DateTime(2000, 1, 1, 10, 0, 0), loadedWorkflow1.FH_SystemLastEditTimeUtc);

				var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
				AssertEquals("THEN workflow2 FH_SystemLastEditTimeUtc should be updated", new DateTime(2000, 1, 1, 10, 0, 0), loadedWorkflow2.FH_SystemLastEditTimeUtc);

				var loadedWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);
				AssertEquals("THEN workflow3 FH_SystemLastEditTimeUtc should not change", new DateTime(2000, 1, 1, 9, 0, 0), loadedWorkflow3.FH_SystemLastEditTimeUtc);

				var task1Card = cards.First(t => t.CardContent.TaskIdentifier == task1.PK);
				var task2Card = cards.First(t => t.CardContent.TaskIdentifier == task2.PK);
				var task3Card = cards.First(t => t.CardContent.TaskIdentifier == task3.PK);

				var taskPanel = (TaskPanel)task1Card.Parent;
				taskPanel.PreProcessUpdateCardAction_ForTest = () =>
				{
					if (taskPanel.Controls.Contains(task2Card))
					{
						taskPanel.Controls.Remove(task2Card);
					}
				};

				form.RefreshNow_ForTest(forceReload: false);

				AssertEquals("task1 - was disposed because it was updated", true, task1Card.IsDisposed);
				AssertEquals("task2 - was NOT disposed because it was removed before got updated", false, task2Card.IsDisposed);
				AssertEquals("task3 - nothing changes so not being disposed/updated", false, task3Card.IsDisposed);
			}
		}

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestDifferentialRefresh_TaskCard()
		{
			TestDifferentialRefresh(CardTypeList.Codes.Task);
		}

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestDifferentialRefresh_WorkflowCard()
		{
			TestDifferentialRefresh(CardTypeList.Codes.Workflow);
		}

		void TestDifferentialRefresh(string cardType)
		{
			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			config.BufferSection.SectionConfiguration.CardType = cardType;

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var workflow0 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow0", description: "task0", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff.GS_Code, lowEstMinutes: 15);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow1", description: "task1", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-2), staffCode: staff.GS_Code, lowEstMinutes: 15);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow2", description: "task2", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-3), staffCode: staff.GS_Code, lowEstMinutes: 15);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow3", description: "task3", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-4), staffCode: staff.GS_Code, lowEstMinutes: 15);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow4", description: "task4", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff.GS_Code, lowEstMinutes: 15);
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow5", description: "task5", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-5), staffCode: staff.GS_Code, lowEstMinutes: 15);

			var workflows = new[] { workflow0, workflow1, workflow2, workflow3, workflow4, workflow5 };

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single();
				AssertEquals("GIVEN 6 cards are shown", 6, bmComponentControl.FindAll<ITasksDetails>().Sum(p => p.CardsCount));

				var cardControls = workflows.Select(workflow => bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => IsEqual(cardType, t.CardContent, workflow))).ToArray();

				cardControls.ForEach(cardControl => AssertEquals("GIVEN all cards LastEditTime = 2000/1/1 09:00:00", new DateTime(2000, 1, 1, 9, 0, 0), cardControl.CardContent.LastEditTime));

				var newFactory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TestDifferentialRefresh", RefreshEnabled = false };

				var loadedWorkflow0 = newFactory.Load<ProcessHeader>(workflow0.PK);
				loadedWorkflow0.FH_CompletionStatement = loadedWorkflow0.FH_CompletionStatement + "(updated)";

				var loadedParent1 = newFactory.Load<OrgHeader>(workflow1.Parent.PK);
				loadedParent1.OH_Language = "GBR";

				var loadedTask2 = newFactory.Load<ProcessHeader>(workflow2.PK).Tasks.First();
				loadedTask2.P9_Description = loadedTask2.P9_Description + "(updated)";

				var loadedTask5 = newFactory.Load<ProcessHeader>(workflow5.PK).Tasks.First();
				loadedTask5.Delete();

				var orgHeader6 = newFactory.NewWithValidTestData<OrgHeader>();
				orgHeader6.OH_Code = "NEWORGHEADER";
				var jobHeader6 = ProcessJobHeader.GetForParent(orgHeader6, newFactory, true);
				jobHeader6.FH_CompletionStatement = "jobHeader6";
				var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader6, "wokflow6");
				workflow6.FH_FC_CurrentComponent = config.Buffer.PK;
				var task6 = BMSTestHelper.CreateTask(workflow6, staff.GS_Code, 15, description: "task6");

				newFactory.Save();

				CombineAssertions("WHEN updating bizO may/may not updating FH_SystemLastEditTimeUtc", () =>
				{
					loadedWorkflow0 = newFactory.Load<ProcessHeader>(workflow0.PK);
					AssertEquals("WHEN updating workflow0 description", "workflow0(updated)", loadedWorkflow0.FH_CompletionStatement);
					AssertEquals("THEN workflow0 FH_SystemLastEditTimeUtc should be updated", new DateTime(2000, 1, 1, 10, 0, 0), loadedWorkflow0.FH_SystemLastEditTimeUtc);
					if (cardType == CardTypeList.Codes.Workflow)
					{
						var currentCardControl0 = bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.WorkflowIdentifier == workflow0.PK);
						AssertEquals("WHEN differential-refresh hasn't processed THEN cardControl description should not be updated", "workflow0", ((BusinessObject)currentCardControl0.CardContent.Bindable)["WFL_FH_CompletionStatement"]);
					}

					loadedParent1 = newFactory.Load<OrgHeader>(workflow1.Parent.PK);
					var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
					AssertEquals("WHEN updating job1 language", "GBR", loadedParent1.OH_Language);
					AssertEquals("job1 FH_SystemLastEditTimeUtc should be updated because jobCode was updated", new DateTime(2000, 1, 1, 10, 0, 0), loadedWorkflow1.FH_SystemLastEditTimeUtc);

					var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
					loadedTask2 = newFactory.Load<ProcessTask>(loadedWorkflow2.Tasks.First().PK);
					AssertEquals("WHEN updating task2 description", "task2(updated)", loadedTask2.P9_Description);
					AssertEquals("THEN task2 FH_SystemLastEditTimeUtc should be updated", new DateTime(2000, 1, 1, 10, 0, 0), loadedWorkflow2.FH_SystemLastEditTimeUtc);
					if (cardType == CardTypeList.Codes.Task)
					{
						var currentCardControl2 = bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == workflow2.Tasks.First().PK);
						AssertEquals("WHEN differential-refresh hasn't processed THEN cardControl description should not be updated", "task2", ((BusinessObject)currentCardControl2.CardContent.Bindable)["TSK_P9_Description"]);
					}

					var loadedWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);
					AssertEquals("WHEN not updating task3 THEN task3 FH_SystemLastEditTimeUtc should not be updated", new DateTime(2000, 1, 1, 9, 0, 0), loadedWorkflow3.FH_SystemLastEditTimeUtc);

					var loadedWorkflow4 = newFactory.Load<ProcessHeader>(workflow4.PK);
					AssertEquals("WHEN not updating task4 THEN task4 FH_SystemLastEditTimeUtc should not be updated", new DateTime(2000, 1, 1, 9, 0, 0), loadedWorkflow4.FH_SystemLastEditTimeUtc);

					var loadedWorkflow5 = newFactory.Load<ProcessHeader>(workflow5.PK);
					AssertEquals("WHEN deleted THEN should not exist", 0, loadedWorkflow5.Tasks.Count());
				});

				form.RefreshNow_ForTest(forceReload: false);

				workflows = new[] { workflow0, workflow1, workflow2, workflow3, workflow4, workflow5, workflow6 };
				var refreshedCardControls = workflows.Select(workflow => bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => IsEqual(cardType, t.CardContent, workflow))).ToArray();
				var refreshedCardControlsWithout5And6 = workflows
					.Where(workflow => !(new[] { workflow5.PK, workflow6.PK }).Contains(workflow.PK))
					.Select(workflow => bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => IsEqual(cardType, t.CardContent, workflow))).ToArray();

				CombineAssertions("GIVEN FH_SystemLastEditTimeUtc is updated WHEN differential-refresh THEN card should be disposed and refreshed", () =>
				{
					AssertEquals("cardControl0 FH_SystemLastEditTimeUtc is updated", new DateTime(2000, 1, 1, 10, 0, 0), refreshedCardControls[0].CardContent.LastEditTime);
					AssertEquals("cardControl0 should be disposed", true, cardControls[0].IsDisposed);
					if (cardType == CardTypeList.Codes.Workflow)
					{
						AssertEquals("cardControl0 description", "workflow0(updated)", ((BusinessObject)refreshedCardControls[0].CardContent.Bindable)["WFL_FH_CompletionStatement"]);
					}

					AssertEquals("cardControl1 FH_SystemLastEditTimeUtc is not updated", new DateTime(2000, 1, 1, 9, 0, 0), refreshedCardControls[1].CardContent.LastEditTime);
					AssertEquals("cardControl1 should not be disposed", false, cardControls[1].IsDisposed);

					AssertEquals("cardControl2 FH_SystemLastEditTimeUtc is updated", new DateTime(2000, 1, 1, 10, 0, 0), refreshedCardControls[2].CardContent.LastEditTime);
					AssertEquals("cardControl2 should be disposed", true, cardControls[2].IsDisposed);

					AssertEquals("cardControl3 FH_SystemLastEditTimeUtc is not updated", new DateTime(2000, 1, 1, 9, 0, 0), refreshedCardControls[3].CardContent.LastEditTime);
					AssertEquals("cardControl3 should not be disposed", false, cardControls[3].IsDisposed);

					AssertEquals("cardControl4 FH_SystemLastEditTimeUtc is not updated", new DateTime(2000, 1, 1, 9, 0, 0), refreshedCardControls[4].CardContent.LastEditTime);
					AssertEquals("cardControl4 should not be disposed because no change even it is @ same cell as cardControl0", false, cardControls[4].IsDisposed);

					AssertNull("cardControl5 should not exist because it was deleted", refreshedCardControls[5]);
					AssertEquals("cardControl5 should be disposed because it was deleted", true, cardControls[5].IsDisposed);

					AssertNotNull("cardControl6 should exist because it was added", refreshedCardControls[6]);
				});

				for (var i = 0; i < 4; i++)
				{
					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);
					form.RefreshNow_ForTest(forceReload: false);
					refreshedCardControlsWithout5And6.ForEach(refreshedCardControl => AssertEquals("GIVEN FullTicketRefreshDelayCountdown is not reached WHEN refreshing THEN should not full-refresh BUT differential-refresh", false, refreshedCardControl.IsDisposed));
				}

				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
				form.RefreshNow_ForTest(forceReload: false);
				refreshedCardControlsWithout5And6.ForEach(refreshedCardControl => AssertEquals("GIVEN FullTicketRefreshDelayCountdown is reached WHEN refreshing THEN should full-refresh regardless FH_SystemLastEditTimeUtc", true, refreshedCardControl.IsDisposed));
			}
		}

		bool IsEqual(string cardType, ICardContent cardContent, ProcessHeader workflow)
		{
			return cardType == CardTypeList.Codes.Task
				? cardContent.TaskIdentifier == workflow.Tasks.First().PK
				: cardContent.WorkflowIdentifier == workflow.PK;
		}

		public void TestDifferentialRefresh_ManualRefresh()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var workflowX = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflowX", description: "taskX", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-11), staffCode: staff.GS_Code, lowEstMinutes: 15);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single();
				var card = bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == workflowX.Tasks.First().PK);

				AssertEquals("GIVEN TaskX is not refreshed", false, card.IsDisposed);

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);

				AssertEquals("GIVEN taskX is not updated WHEN manual refresh by pressing F5 THEN taskX should be refreshed", true, card.IsDisposed);
			}
		}

		public void TestDifferentialRefresh_Disabled()
		{
			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var workflowX = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflowX", description: "taskX", currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-11), staffCode: staff.GS_Code, lowEstMinutes: 15);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single();
				var card = bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == workflowX.Tasks.First().PK);

				AssertEquals("GIVEN TaskX is not refreshed", false, card.IsDisposed);

				form.RefreshNow_ForTest(forceReload: false);

				AssertEquals("GIVEN taskX is not updated WHEN disable differential-refresh and refresh THEN taskX should be refreshed", true, card.IsDisposed);
			}
		}

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestDifferentialRefresh_Reordering_TopCardWasClosed()
		{
			config.BufferSection.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;

			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS1", "Test 1");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff1.PK);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS2", "Test 2");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff2.PK);
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS3", "Test 3");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff3.PK);

			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "workflow1", config.Buffer, releaseDateTime: ZDateTime.UtcNow);
			var task11 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, description: "task11");
			var task12 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, description: "task12");

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var workflow2 = BMSTestHelper.CreateWorkflow(Factory, "workflow2", config.Buffer, releaseDateTime: ZDateTime.UtcNow);
			var task21 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task21");
			var task22 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task22");

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("GIVEN workflow1: task11, task12", new[] { task11, task12 }, workflow1.Tasks);
			AssertContainsExactElementsInAnyOrder("GIVEN workflow2: task21, task22", new[] { task21, task22 }, workflow2.Tasks);

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == config.BufferSection.PK);
				var cards = bmComponentControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals("Precondition", 4, cards.Length);

				cards.ForEach(c => AssertEquals("GIVEN all tasks are shown on 1 cell @ VisualBoard", "2, 13", string.Format("{0}, {1}", c.Cell.PrimaryAxis, c.Cell.SecondaryAxis)));

				AssertEquals("GIVEN task11 is shown at the top", task11.PK, cards[0].CardContent.TaskIdentifier);

				var newFactory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TestDifferentialRefresh_Reordering_TopCardWasClosed", RefreshEnabled = false };
				var loadedTask11 = newFactory.Load<ProcessTask>(task11.PK);
				loadedTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				newFactory.Save();

				AssertEquals("GIVEN task11 is closed", ProcessTaskStatusCodeList.Codes.Closed, loadedTask11.P9_Status);

				form.RefreshNow_ForTest(forceReload: false);

				cards = bmComponentControl.FindAll<TaskCardControl>().ToArray();
				AssertEquals("WHEN partial-refresh THEN task12 should be at the top", task12.P9_Description, Factory.Load<ProcessTask>(cards[0].CardContent.TaskIdentifier).P9_Description);
			}
		}

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestDifferentialRefresh_Reordering_SameSequenceTasks_UndoMove()
		{
			config.BufferSection.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;

			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS1", "Test 1");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff1.PK);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS2", "Test 2");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff2.PK);
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS3", "Test 3");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff3.PK);

			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "workflow1", config.Buffer, releaseDateTime: ZDateTime.UtcNow);
			var task11 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, description: "task11");

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var workflow2 = BMSTestHelper.CreateWorkflow(Factory, "workflow2", config.Buffer, releaseDateTime: ZDateTime.UtcNow);
			var task21 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task21", sequence: 1);
			var task22 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task22", sequence: 1);
			var task23 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task23", sequence: 1);
			var task24 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task24", sequence: 1);

			Factory.Save();

			AssertArrayEqualsByElements("GIVEN workflow1: task11", new[] { task11 }, workflow1.Tasks.ToArray());
			AssertArrayEqualsByElements("GIVEN workflow2: task21, task22, task23, task24", new[] { task21, task22, task23, task24 }, workflow2.Tasks.ToArray());

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == config.BufferSection.PK);
				var cards = bmComponentControl.FindAll<TaskCardControl>();

				AssertEquals("Precondition", 5, cards.Count());

				cards.ForEach(c => AssertEquals("GIVEN all tasks are shown on 1 cell @ VisualBoard", "2, 13", $"{c.Cell.PrimaryAxis}, {c.Cell.SecondaryAxis}"));

				var newFactory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TestDifferentialRefresh_Reordering_TopCardWasClosed", RefreshEnabled = false };
				var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
				var originalReleaseTime = loadedWorkflow2.FH_ReleaseDateTime;
				loadedWorkflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-10);
				newFactory.Save();

				AssertEquals("GIVEN workflow2 is moved to different cell", ZDateTime.Now.AddDays(-10), loadedWorkflow2.FH_ReleaseDateTime);

				loadedWorkflow2.FH_ReleaseDateTime = originalReleaseTime;
				newFactory.Save();

				AssertEquals("GIVEN workflow2 is moved back", originalReleaseTime, loadedWorkflow2.FH_ReleaseDateTime);

				AssertDifferentialRefresh_TasksWithSameSequence_ShouldOrderByTaskID(form, bmComponentControl, workflow2.Tasks);
			}
		}

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestDifferentialRefresh_Reordering_SameSequenceTasks_Move()
		{
			config.BufferSection.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;

			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS1", "Test 1");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff1.PK);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS2", "Test 2");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff2.PK);
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TS3", "Test 3");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff3.PK);

			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "workflow1", config.Buffer, releaseDateTime: ZDateTime.UtcNow);
			var task11 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, description: "task11");

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var workflow2 = BMSTestHelper.CreateWorkflow(Factory, "workflow2", config.Buffer, releaseDateTime: ZDateTime.UtcNow);
			var task21 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task21", sequence: 1);
			var task22 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task22", sequence: 1);
			var task23 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task23", sequence: 1);
			var task24 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, description: "task24", sequence: 1);

			Factory.Save();

			AssertArrayEqualsByElements("GIVEN workflow1: task11", new[] { task11 }, workflow1.Tasks.ToArray());
			AssertArrayEqualsByElements("GIVEN workflow2: task21, task22, task23, task24", new[] { task21, task22, task23, task24 }, workflow2.Tasks.ToArray());

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == config.BufferSection.PK);
				var cards = bmComponentControl.FindAll<TaskCardControl>();

				AssertEquals("Precondition", 5, cards.Count());

				cards.ForEach(c => AssertEquals("GIVEN all tasks are shown on 1 cell @ VisualBoard", "2, 13", $"{c.Cell.PrimaryAxis}, {c.Cell.SecondaryAxis}"));

				var newFactory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TestDifferentialRefresh_Reordering_TopCardWasClosed", RefreshEnabled = false };
				var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
				loadedWorkflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-10);
				newFactory.Save();

				AssertEquals("GIVEN workflow2 is moved to different cell", ZDateTime.Now.AddDays(-10), loadedWorkflow2.FH_ReleaseDateTime);

				AssertDifferentialRefresh_TasksWithSameSequence_ShouldOrderByTaskID(form, bmComponentControl, workflow2.Tasks);
			}
		}

		void AssertDifferentialRefresh_TasksWithSameSequence_ShouldOrderByTaskID(VisualBoardForm form, BMComponentControl bmComponentControl, IEnumerable<ProcessTask> tasks)
		{
			tasks.ForEach(t => AssertEquals("GIVEN all tasks for workflow2 has same sequence", 1, t.P9_Sequence));

			var sortedTasks = tasks.OrderBy(t => t.P9_TaskID);

			for (var i = 0; i < 3; i++)
			{
				form.RefreshNow_ForTest(forceReload: false);

				var cardsOnBoard = bmComponentControl
					.FindAll<TaskCardControl>()
					.Where(c =>
						sortedTasks.Select(t => t.PK)
							.Contains(c.CardContent.TaskIdentifier));

				var expectedCell = cardsOnBoard.Select(c => $"{c.Cell.PrimaryAxis}, {c.Cell.SecondaryAxis}").First();

				cardsOnBoard.ForEach(c => AssertEquals("GIVEN all tasks for workflow2 are moved to 1 cell @ VisualBoard", expectedCell, $"{c.Cell.PrimaryAxis}, {c.Cell.SecondaryAxis}"));

				AssertArrayEqualsByElements("WHEN partial-refresh THEN card should be ordered by P9_TaskID",
					sortedTasks.Select(task => new ZString($"Task: [{task.P9_Description}]; Workflow: [{task.ProcessHeader.FH_CompletionStatement}]")).ToArray(),
					cardsOnBoard.Select(c => c.CardContent.DisplayTextForDebugging).ToArray());

				var previousCard = cardsOnBoard.ElementAt(0);
				var previousCardLocation = previousCard.PointToScreen(previousCard.Location);
				for (var index = 1; index < 4; index++)
				{
					var currentCard = cardsOnBoard.ElementAt(index);
					var currentCardLocation = currentCard.PointToScreen(currentCard.Location);
					Assert("THEN card order should also order the x-location properly ", previousCardLocation.X < currentCardLocation.X);
					previousCardLocation = currentCardLocation;
				}
			}
		}

		[TestDate(2000, 1, 1, 9, 0, 0)]
		public void TestDifferentialRefresh_CardsOrder()
		{
			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TST", "Test");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			// 6 cards: 2 x 3 cards i.e. not-modified, modified, delete
			var workflows = Enumerable.Range(1, 6).Select(index =>
				BMSTestHelper.CreateWorkflowAndTask(
					Factory,
					completionStatement: "workflow" + index.ToString(),
					description: "task" + index.ToString(),
					currentComponent: config.Buffer,
					releaseDateTime: ZDateTime.Now.AddDays(-1),
					staffCode: staff.GS_Code,
					lowEstMinutes: 15,
					sequence: index)).ToArray();

			var tasks = workflows.Select(w => w.Tasks.First()).ToArray();

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var bmComponentControl = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == config.BufferSection.PK);
				var cards = bmComponentControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals("Precondition", 6, cards.Length);

				Enumerable.Range(0, 5).ForEach(i => AssertEquals("GIVEN cards are orderd by sequence", tasks[i].PK, cards[i].CardContent.TaskIdentifier));

				var cardControls = workflows.Select(workflow => bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => IsEqual(CardTypeList.Codes.Task, t.CardContent, workflow))).ToArray();

				cardControls.ForEach(cardControl => AssertEquals("GIVEN all cards LastEditTime = 2000/1/1 09:00:00", new DateTime(2000, 1, 1, 9, 0, 0), cardControl.CardContent.LastEditTime));

				var newFactory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TestDifferentialRefresh_CardsOrder", RefreshEnabled = false };

				var updatedTask1 = newFactory.Load<ProcessHeader>(workflows[1].PK).Tasks.First();
				updatedTask1.P9_Description = updatedTask1.P9_Description + "(updated)";
				var updatedTask2 = newFactory.Load<ProcessHeader>(workflows[4].PK).Tasks.First();
				updatedTask2.P9_Description = updatedTask2.P9_Description + "(updated)";

				var deletedTask1 = newFactory.Load<ProcessHeader>(workflows[2].PK).Tasks.First();
				deletedTask1.Delete();
				var deletedTask2 = newFactory.Load<ProcessHeader>(workflows[5].PK).Tasks.First();
				deletedTask2.Delete();

				var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(newFactory, false);
				((OrgHeader)jobHeader.Parent).OH_Code = "ABCDEF";
				var newWorkflow = BMSTestHelper.CreateWorkflowAndTask(
					jobHeader,
					completionStatement: "workflow6",
					description: "task6",
					currentComponent: config.Buffer,
					releaseDateTime: ZDateTime.Now.AddDays(-1),
					staffCode: staff.GS_Code,
					lowEstMinutes: 15,
					sequence: 6);

				newFactory.Save();

				form.RefreshNow_ForTest(forceReload: false);

				AssertEquals("GIVEN task0 is not modified, card0 should not be disposed", false, cards[0].IsDisposed);
				AssertEquals("GIVEN task1 is modified, card1 should be disposed", true, cards[1].IsDisposed);
				AssertEquals("GIVEN task2 is deleted, card2 should be disposed", true, cards[2].IsDisposed);
				AssertEquals("GIVEN task3 is not modified, card3 should not be disposed", false, cards[3].IsDisposed);
				AssertEquals("GIVEN task4 is modified, card4 should be disposed", true, cards[4].IsDisposed);
				AssertEquals("GIVEN task5 is deleted, card5 should be disposed", true, cards[5].IsDisposed);

				cards = bmComponentControl.FindAll<TaskCardControl>().ToArray();
				AssertEquals(5, cards.Length);

				AssertEquals("THEN card sequence 0 should be task0(not modified)", tasks[0].PK, cards[0].CardContent.TaskIdentifier);
				AssertEquals("THEN card sequence 1 should be task1(modified)", tasks[1].PK, cards[1].CardContent.TaskIdentifier);
				AssertEquals("THEN card sequence 2 should be task3(not modified) - task2 was deleted", tasks[3].PK, cards[2].CardContent.TaskIdentifier);
				AssertEquals("THEN card sequence 3 should be task4(modified)", tasks[4].PK, cards[3].CardContent.TaskIdentifier);
				AssertEquals("THEN card sequence 4 should be newTask - task5 was deleted", newWorkflow.Tasks.First().PK, cards[4].CardContent.TaskIdentifier);
			}
		}

		#endregion

		public void TestSubComponentHeader_OffsetNonDividableByTimePerCell()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");

			var preConstraintOffset = 82;
			var preConstraintTimespan = 16;

			var preConstraintOffsetInMinutes = preConstraintOffset * 60;

			config.PreConstraintBuffer.FC_BufferTimespanInMinutes = preConstraintOffsetInMinutes;
			config.PreConstraintBuffer.FC_OffsetInMinutes = 0;

			config.Constraint.FC_OffsetInMinutes = preConstraintOffsetInMinutes;

			config.PostConstraintBuffer.FC_BufferTimespanInMinutes = preConstraintTimespan * 60;
			config.PostConstraintBuffer.FC_OffsetInMinutes = preConstraintOffsetInMinutes;

			Factory.Save();

			AssertEquals("Initialy Condition: Flow Direction is UP.", config.Section.SectionConfiguration.FlowDirection, "UP");

			Assert("Initialy Condition: offset is not dividable by time-per-cell",
				preConstraintOffsetInMinutes % config.Section.SectionConfiguration.TimePerCell.GetMinutesFromDateTimeSpan() != 0);

			using (var componentControl = new BMComponentControlForTest(config.Section.SectionConfiguration, config.SectionViewModel))
			{
				var constraintHeaderTextList = componentControl.Table.Controls.OfType<Control>().ToList()
					.Select(control => control as HeaderWrapperControlForTest)
					.Where(control => control != null && control.Cell.ContentType == CellContentType.SubComponentZoneHeading)
					.Select(control => $"Subcomponent Header => secondaryAxis: {control.Cell.SecondaryAxis}, label: '{control.Cell.Label}'");

				AssertContainsExactElementsInAnyOrder("GIVEN pre-constraint-offset is not dividable by time-per-cell, WHEN displaying preConstraintHeader SHOULD be displayed by ignoring the remainder of offset/time-per-cell", new[] {
					"Subcomponent Header => secondaryAxis: 1, label: 'Zone 0'",
					"Subcomponent Header => secondaryAxis: 1, label: 'Zone 0'",
					"Subcomponent Header => secondaryAxis: 2, label: 'Zone 1'",
					"Subcomponent Header => secondaryAxis: 3, label: 'Zone 3'",
					"Subcomponent Header => secondaryAxis: 4, label: 'Zone 1'",
					"Subcomponent Header => secondaryAxis: 7, label: 'Zone 2'",
					"Subcomponent Header => secondaryAxis: 11, label: 'Zone 3'",
				}, constraintHeaderTextList);
			}
		}

		public void TestSubComponentHeader()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");

			Factory.Save();

			using (var componentControl = new BMComponentControlForTest(config.Section.SectionConfiguration, config.SectionViewModel))
			{
				var constraintHeaderTextList = componentControl.Table.Controls.OfType<Control>().ToList()
					.Select(c => c as HeaderWrapperControlForTest)
					.Where(c => c != null && c.Cell.ContentType == CellContentType.SubComponentZoneHeading)
					.Select(c => string.Format("Subcomponent Header => primaryAxis: {0}, secondaryAxis: {1}, label: '{2}'",
						c.Cell.PrimaryAxis,
						c.Cell.SecondaryAxis,
						c.Cell.Label));

				AssertContainsExactElementsInAnyOrder(new[] {
					"Subcomponent Header => primaryAxis: 4, secondaryAxis: 1, label: 'Zone 0'",
					"Subcomponent Header => primaryAxis: 4, secondaryAxis: 6, label: 'Zone 1'",
					"Subcomponent Header => primaryAxis: 4, secondaryAxis: 9, label: 'Zone 2'",
					"Subcomponent Header => primaryAxis: 4, secondaryAxis: 11, label: 'Zone 3'",
					"Subcomponent Header => primaryAxis: 5, secondaryAxis: 1, label: 'Zone 0'",
					"Subcomponent Header => primaryAxis: 5, secondaryAxis: 2, label: 'Zone 1'",
					"Subcomponent Header => primaryAxis: 5, secondaryAxis: 3, label: 'Zone 2'",
					"Subcomponent Header => primaryAxis: 5, secondaryAxis: 5, label: 'Zone 3'"
				}, constraintHeaderTextList);
			}
		}

		public void TestRefreshBoard_ShouldRefreshChannelCapability()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");

			var bufferTask = config.Workflows[0].Parent.WorkflowItems[0];
			bufferTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			var capability = BMSTestHelper.CreateCapability(Factory, "DPR", "DPR");
			bufferTask.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var section = config.Section;
			var resource = config.Staffs[0];

			using (var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindAll<BMComponentControl>().Single();

				var channel = componentControl.ViewModel.PrimaryChannels.First();
				AssertEquals("resource has no capability, thus task should not be in channel", false, channel.IsInChannel(bufferTask));

				var resourceChannelColumn = 2;
				var totalTaskEstimates = Business.Test.BMSTestHelper.GetTaskEstimatesInCells(componentControl.ViewModel.ComponentGrid, primaryAxes: resourceChannelColumn.WrapWithEnumerable());
				AssertEquals("resource has no capability, thus task should not be in channel, thus task estimates is 0", 0m, totalTaskEstimates);

				resource.Capabilities.Add(capability);
				Factory.Save();

				form.RefreshNow_ForTest(forceReload: false);

				totalTaskEstimates = Business.Test.BMSTestHelper.GetTaskEstimatesInCells(componentControl.ViewModel.ComponentGrid, primaryAxes: resourceChannelColumn.WrapWithEnumerable());
				AssertEquals("After adding capability to resource and refresh VisualBoardForm, the task should be in channel", 0.25m, totalTaskEstimates);
			}
		}

		public void TestRefreshBoard_ModifyGroup()
		{
			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GP1");
			var capability = BMSTestHelper.CreateCapability(Factory, "DPR", "DPR");
			var section = config.Section;
			VisualBoardsTestHelper.CreatePrimaryChannelForSection(section, channelTypeCode: ChannelTypeList.Codes.Group, channelPK: group.PK, overrideChannels: true);
			Factory.Save();

			var bufferTask = config.Workflows[0].Parent.WorkflowItems[0];
			var resource = config.Staffs[0];

			using (var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindAll<BMComponentControl>().Single();

				var groupChannel = componentControl.ViewModel.PrimaryChannels.Last();
				AssertEquals("staff-with-task is not in group, thus task should not show", false, groupChannel.IsInChannel(bufferTask));

				var groupChannelColumn = 8;
				var totalTaskEstimates = Business.Test.BMSTestHelper.GetTaskEstimatesInCells(componentControl.ViewModel.ComponentGrid, primaryAxes: groupChannelColumn.WrapWithEnumerable());
				AssertEquals("staff-with-task is not in group, thus task should not show (estimates = 0)", 0m, totalTaskEstimates);

				group.Staff.Add(resource);
				Factory.Save();

				form.RefreshNow_ForTest(forceReload: false);

				totalTaskEstimates = Business.Test.BMSTestHelper.GetTaskEstimatesInCells(componentControl.ViewModel.ComponentGrid, primaryAxes: groupChannelColumn.WrapWithEnumerable());
				AssertEquals("After adding staff to group and refresh VisualBoardForm, the task should be in channel", 0.25m, totalTaskEstimates);
			}
		}

		public void TestLoadBoard_WhenTaskProcessHeaderNull_ShouldNotThrowException_ShouldShowSectionWithoutWorkflow()
		{
			var section = config.BufferSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SHP", "Shawn The Sheap");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MTB", "He Make The Beep");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "wkfl1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, description: "Owo");

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "orow2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, description: "whatsthis");

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var tickets = control.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
				AssertContainsExactElementsInAnyOrder("At first, all of our tickets should be visible", new[] { task1.PK, task2.PK }, tickets.Select(t => t.CardContent.TaskIdentifier));
				AssertEquals("Our tasks are in the same section cell", tickets[0].Cell, tickets[1].Cell);
				var cellToStayIn = tickets[0].Cell;

				form.RefreshNow_ForTest();
				Application.DoEvents();

				tickets = control.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
				AssertContainsExactElementsInAnyOrder("Refreshing with no null process headers should re-display previous tasks", new[] { task1.PK, task2.PK }, tickets.Select(t => t.CardContent.TaskIdentifier));

				var liveControl = form.FindAll<BMComponentControl>().First();

				liveControl.ViewModel.BeforeGetCardPenetration_ForTest += (sender, args) =>
				{
					if (args.Task.PK.Equals(task2.PK))
					{
						args.Task.P9_FH_ProcessHeader = ZGuid.Empty;
					}
				};

				AssertNoExceptionThrown("Refreshing a board with a processHeader that becomes null while trying to calculate buffer penetration shouldn't kill the section", () => form.RefreshNow_ForTest());

				Application.DoEvents();

				tickets = control.FindAll<TaskCardControl>().ToArray();

				AssertEquals(1, tickets.Length);
				AssertContainsExactElementsInAnyOrder("Now that a processHeader was 'deleted', its ticket should be removed from the board, but not touch existing tasks", new[] { task1.PK }, tickets.Select(t => t.CardContent.TaskIdentifier));
				AssertEquals("Null process header won't cause existing tasks to move around unnecessarily", cellToStayIn, tickets[0].Cell);
			}
		}

		#endregion

		#region Component not loaded errors

		public void TestDrawSection_ForInactiveComponent_ShouldNotDisplay()
		{
			var section = config.BufferSection;
			section.Component.FC_Name = "Jedi Scum";
			section.SectionConfiguration.SectionNameIsOverridden = true;
			section.SectionConfiguration.SectionNameOverride = "Nebucheznezzer";

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var control = form.FindSingle<BMComponentControl>();

				AssertNull(control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel"));
			}

			config.Buffer.FC_IsActive = false;
			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var control = form.FindSingle<BMComponentControl>();
				var label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");

				AssertNotNull(label);
				AssertEquals("The component [Jedi Scum] is inactive and cannot be displayed on a board section.", label.Text);
			}
		}

		public void TestRefreshSection_AfterComponentDisabled_WhenAcceptabilityBandTilePresent_ShouldNotThrowException()
		{
			var section = config.BufferSection;
			section.Component.FC_Name = "Jedi Scum";

			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(section.Component, 0, 0, 0, 0, 0, 0);
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Tappa tappa tappa.", section.Component);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();

				AssertNotNull(control.FindSingleOrDefault<AcceptabilityBandTileControl>());
				AssertNull(control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel"));

				config.Buffer.FC_IsActive = false;
				Factory.Save();

				AssertNoExceptionThrown("Should not attempt to draw acceptability band tiles/headings when they're not actually shown.", () =>
				{
					form.RefreshBoard();
					Application.DoEvents();
				});

				var label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");

				AssertNotNull(label);
				AssertEquals("The component [Jedi Scum] is inactive and cannot be displayed on a board section.", label.Text);
			}
		}

		public void TestLoadSection_WithFilterWithCountrySpecificModule_WhenModuleNotAvailableInCurrentContext_ShouldShowError()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			var section = config.BufferSection;
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(section.WorkflowFilter, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingleOrDefault<BMComponentControl>();
				AssertNotNull("If the component control isn't even created then it means the filters threw an exception. We should avoid/handle that exception instead. SAD!", control);

				var label = control.FindSingleOrDefault<ZLabel>("LoadFailedLabel");
				AssertNotNull("The section should have failed to load properly, thus creating the label that shows the failure. SAD!", label);
				AssertEquals("This section (buffer) has validation errors and cannot be shown. Please open the board configuration form and fix any validation errors.", label.Text);
			}
		}

		#endregion

		#region Detailed Tickets

		public void TestShowDetailedTicketAndSave_ShouldNotBringInTicketsWhichShouldBeFilteredOut()
		{
			var section = config.BufferSection;
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SPC", "Sean Spicer");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SPD", "Sean Spider");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Fake News", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var taskInWorkflow1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, description: "Malcolm");
			BMSTestHelper.CreateTask(workflow1, resource2.GS_Code, description: "Trumble");

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "The best news", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, description: "The");
			BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, description: "Donald");

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "No one has better news", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var taskInWorkflow3 = BMSTestHelper.CreateTask(workflow3, resource1.GS_Code, description: "The");
			BMSTestHelper.CreateTask(workflow3, resource2.GS_Code, description: "Donald");

			workflow1.RunPreSaveValidation();
			workflow3.RunPreSaveValidation();

			AssertNoErrors(workflow1);
			AssertNoErrors(workflow3);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(section.Board);
			var factoryProvider = new TrackingSharedBoardFactoryProvider(viewModel);
			viewModel.FactoryProvider = factoryProvider;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var tickets = control.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);

				AssertContainsExactElementsInAnyOrder(new[] { taskInWorkflow1.PK, taskInWorkflow3.PK }, tickets.Select(t => t.CardContent.TaskIdentifier));

				tickets[0].ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				detailedTicket.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				detailedTicket.Save_ForTest();

				tickets = control.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);

				var factory = factoryProvider.CreatedFactories.Single(f => f.NameForDebugging.Contains(nameof(PartialRefreshPipeEngine)));

				AssertBusinessObjectsHeldInFactory(factory,
					Tuple.Create(typeof(ProcessHeader), 2),
					Tuple.Create(typeof(ProcessJobHeader), 1),
					Tuple.Create(typeof(ProcessTask), 2));
			}
		}

		public void TestShowDetailedTicketAndSave_WhenTaskFiltersApplied_ShouldNotBringInTicketsWhichShouldBeFilteredOut()
		{
			var section = config.BufferSection;
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SPC", "Sean Spicer");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SPD", "Sean Spider");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			FilterStripsTestHelper.AddFilterStrip<ModuleTextFilter>(section.TaskFilter, "Description", f => f.Property = "Trumble", f => f.SqlComparisonOperator = SQLComparisonOperator.NotEqual);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Fake News", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, description: "Malcolm");
			BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, description: "Trumble");

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "The best news", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, description: "The");
			BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, description: "Donald");

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "No one has better news", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow3, resource2.GS_Code, description: "The");
			BMSTestHelper.CreateTask(workflow3, resource2.GS_Code, description: "Donald");

			workflow1.RunPreSaveValidation();

			AssertNoErrors(workflow1);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(section.Board);
			var factoryProvider = new TrackingSharedBoardFactoryProvider(viewModel);
			viewModel.FactoryProvider = factoryProvider;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var ticket = control.FindSingleOrDefault<TaskCardControl>();

				AssertNotNull(ticket);
				AssertEquals(task1.PK, ticket.CardContent.TaskIdentifier);

				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				detailedTicket.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				detailedTicket.Save_ForTest();

				ticket = control.FindSingleOrDefault<TaskCardControl>();

				AssertNotNull(ticket);
				AssertEquals(task1.PK, ticket.CardContent.TaskIdentifier);

				var factory = factoryProvider.CreatedFactories.Single(f => f.NameForDebugging.Contains(nameof(PartialRefreshPipeEngine)));

				AssertBusinessObjectsHeldInFactory(factory,
					Tuple.Create(typeof(ProcessHeader), 2),
					Tuple.Create(typeof(ProcessJobHeader), 1),
					Tuple.Create(typeof(ProcessTask), 2)); // Extra task is for calculating countdowns. Ideally this would be one task since only one is visible.
			}
		}

		#endregion

		#region Assertions

		internal static void AssertCellContents(BMComponentControl control, int col, int row, CellContentType contentType, IVisualBoardChannel channel = null, string labelText = null, int? taskCardCount = null)
		{
			var table = control.Table;
			var cellControl = table.GetControlFromPosition(col, row);
			AssertNotNull(string.Format("Should be a control at col={0}, row={1}", col, row), cellControl);

			switch (contentType)
			{
				case CellContentType.Cards:
					{
						var taskPanel = cellControl as TaskPanel;
						var message = string.Format("Cell at col={0}, row={1} should contain tasks, but was a {2}", col, row, cellControl.GetType().Name);
						AssertNotNull(message, taskPanel);

						if (taskCardCount != null)
						{
							var totalTasksButton = taskPanel.Controls.OfType<ZButton>().Single();
							message = string.Format("Should be {0} cards t col={1}, row={2}", taskCardCount.Value, col, row);
							AssertEquals(taskCardCount.Value.ToString(), totalTasksButton.Text);
						}
						break;
					}
				case CellContentType.ChannelHeading:
					{
						var headerControl = cellControl is HeaderWrapperControl ? cellControl.Controls[0] : cellControl;
						Assert(string.Format("Cell at col={0}, row={1} should be a channel header, but was a {2}", col, row, headerControl.GetType().Name), headerControl is ChannelHeaderControl);
						var channelHeader = (ChannelHeaderControl)headerControl;

						if (channel != null)
						{
							AssertEquals(channel, channelHeader.Channel);
						}
						break;
					}
				case CellContentType.AgeHeading:
					{
						var headerControl = cellControl is HeaderWrapperControl ? cellControl.Controls[0] : cellControl;
						var label = headerControl as Label;
						AssertNotNull(string.Format("Cell at col={0}, row={1} should be an age header label, but was a {2}", col, row, headerControl.GetType().Name), label);
						AssertMatch("Age label should start with a number", new Regex("^[0-9]+"), label.Text);
						break;
					}
				case CellContentType.ZoneHeading:
				case CellContentType.SubComponentZoneHeading:
				case CellContentType.CCRHeading:
					{
						var headerControl = cellControl is HeaderWrapperControl ? cellControl.Controls[0] : cellControl;
						var label = headerControl as Label;
						AssertNotNull(string.Format("Cell at col={0}, row={1} should be a zone header label, but was a {2}", col, row, headerControl.GetType().Name), label);

						var message = string.Format("Zone label at col={0}, row={1}", col, row);
						if (labelText != null)
						{
							AssertEquals(message, labelText, label.Text);
						}
						else
						{
							AssertStartsWith(message + " should start with 'Zone '", "Zone ", label.Text);
						}
						break;
					}
				default:
					Assert(string.Format("Cell at col={0}, row={1} should be a label, but was a {2}", col, row, cellControl.GetType().Name), cellControl is Label);
					break;
			}
		}

		internal void AssertChannelsExpandedCorrectly(int expandedChannelIndex, float startingSize, params TableLayoutStyle[] channels)
		{
			Assert("Index out of range", expandedChannelIndex < channels.Length);

			for (int i = 0; i < channels.Length; i++)
			{
				var columnStyle = channels[i] as ColumnStyle;
				var rowStyle = channels[i] as RowStyle;
				var sizeToCompare = columnStyle != null ? columnStyle.Width : rowStyle.Height;
				if (i == expandedChannelIndex)
				{
					AssertEquals(string.Format("Channel {0} should be expanded", expandedChannelIndex), startingSize * 3, sizeToCompare);
				}
				else
				{
					AssertEquals(string.Format("Channel {0} should be collapsed", i), startingSize, sizeToCompare);
				}
			}
		}

		#endregion

		#region Implementation

		readonly int zoneHeadingPosition = BMConstants.ZoneHeadingPosition;
		readonly int ageHeadingPosition = BMConstants.AgeHeadingPosition;
		VisualBoardTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	#region Task Allocation

	class BMComponentControlTaskAllocationTest : BMSGUITestCase
	{
		#region Ticket Drag/Drop

		public void TestTryAllocateTask_FromAdditionalComponent()
		{
			var bucket = config.Bucket;
			var sneakyBucket = BMSTestHelper.CreateBucket(config.System, "Sneaky Bucket");

			var board = CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(bucket, board, setReleaseGroupIfRequired: false);
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CellsPerSubsection = 4;
			sectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();
			sectionConfiguration.ChannelBy = ZString.Empty;
			sectionConfiguration.OverrideChannels = true;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			BMSTestHelper.CreateAdditionalComponent(section, sneakyBucket);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = sneakyBucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource1.GS_Code);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.WindowState = FormWindowState.Maximized;
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().Single();
				var taskCard = form.FindAll<TaskCardControl>().First();
				taskCard.Parent = form;
				taskCard.Top = control.Table.GetRowHeights().Take(2).Sum() + control.Table.Top;
				taskCard.Left += control.Table.GetColumnWidths().Take(2).Sum() + 50;
				taskCard.BringToFront();

				Application.DoEvents();

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				AssertEquals(true, control.TryReallocateTask(taskCard));
				Application.DoEvents();

				AssertEquals("Task should be dragged into another channel, so should be re-allocated", true, taskCard.IsDisposed);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals(resource2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
				AssertEquals("Task should still be in its original component", sneakyBucket.PK, loadedTask.ProcessHeader.FH_FC_CurrentComponent);

				taskCard = control.FindAll<TaskCardControl>().Single();
				AssertEquals("Should place card in correct cell for its age, not the one actually dragged into", 0, taskCard.Cell.TimeIndex);
			}
		}

		public void TestTryAllocateTask_ForReleaseScheduler_ShouldNotAllowTransfer()
		{
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(config.System, Factory.NewWithValidTestData<GlbGroup>());
			var buffer = config.Buffer;
			var board = CreateBoard(config.System);
			var section1 = CreateReleaseSchedulerBoardSection(buffer, releaseGroup.Group, board);
			section1.Column = 1;
			section1.ColWidthPercent = 50;
			var section2 = CreateBoardSection(buffer, board);
			section2.Column = 2;
			section2.ColWidthPercent = 50;
			section2.SectionConfiguration.CellsPerSubsection = 13;
			section2.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var section1Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section2.PK);

				var taskCard = section2Control.FindAll<TaskCardControl>().Single();
				taskCard.Parent = form;
				taskCard.Left = section1Control.Left + 10;
				taskCard.BringToFront();

				AssertEquals(false, form.PushToNewSection(taskCard));
				AssertEquals("Task cards cannot be dragged into Release Scheduler board sections.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestTryAllocateTask_SameChannel_ShouldDragDropBetweenTimeSlot()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var section = config.BucketSection;
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.ChannelBy = ZString.Empty;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			section.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.AgreedDeliveryDate;
			section.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Booo", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource1.GS_Code);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = control.FindSingle<TaskCardControl>();
				var destinationCell = control.ViewModel.ComponentGrid[taskCard.Cell.Row - 1, taskCard.Cell.Column];

				AssertEquals(true, DragTaskTicketToCell(taskCard, control, destinationCell));
				AssertEquals("Task should be dragged into another cell of the same channel, so it should be destroyed and a new task ticket created", true, taskCard.IsDisposed);

				var newTicket = control.FindSingle<TaskCardControl>();

				AssertNotEquals(taskCard, newTicket);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals(resource1.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ZDateTime.UtcNow.AddDays(1).AddHours(4), loadedTask.GetProcessHeader().FH_AgreedDeliveryDate);
			}
		}

		public void TestTryAllocateTask_ToOtherChannel_ShouldMove()
		{
			var section = config.BucketSection;
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();
			section.SectionConfiguration.ChannelBy = ZString.Empty;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			resource2.GS_FullName = "Samwise the Brave";

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = control.FindSingle<TaskCardControl>();
				var destinationCell = control.ViewModel.ComponentGrid[taskCard.Cell.Row, taskCard.Cell.Column + 1];

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				AssertEquals(true, DragTaskTicketToCell(taskCard, control, destinationCell));

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals(resource2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);

				taskCard = control.FindAll<TaskCardControl>().Single();
				AssertEquals("Should place card in correct cell for its age, not the one actually dragged into", 0, taskCard.Cell.TimeIndex);
			}
		}

		public void TestTryAllocateTask_ToOtherChannelWithTasksThereAlready_ShouldPreserveExistingTasks()
		{
			var section = config.BucketSection;
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();
			section.SectionConfiguration.ChannelBy = ZString.Empty;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			var task1 = workflow.Parent.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			var task2 = workflow.Parent.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var task1Card = control.FindAll<TaskCardControl>().First(c => c.CardContent.TaskIdentifier == task1.PK);
				var destinationCell = control.ViewModel.ComponentGrid[task1Card.Cell.Row, task1Card.Cell.Column + 1];

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				AssertEquals(true, DragTaskTicketToCell(task1Card, control, destinationCell));

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task1.PK);
				AssertEquals(resource2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);

				task1Card = control.FindAll<TaskCardControl>().First(c => c.CardContent.TaskIdentifier == task1.PK);
				AssertEquals("Should place card in correct cell for its age, not the one actually dragged into", 0, task1Card.Cell.TimeIndex);

				AssertEquals("Existing card should still be displayed", 2, task1Card.Parent.FindAll<TaskCardControl>().Count());
			}
		}

		public void TestTryAllocateTask_SameComponent_NotChanneled()
		{
			var board = CreateBoard(config.System);
			var section = CreateBoardSection(config.Bucket, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();

				var control = form.FindAll<BMComponentControl>().Single();
				var taskCard = control.FindAll<TaskCardControl>().Single();

				AssertEquals(false, control.TryReallocateTask(taskCard));
			}
		}

		public void TestTryAllocateTask_DifferentComponent_NotChanneled()
		{
			var system = config.System;
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var board = CreateBoard(system);
			var section1 = CreateBoardSection(bucket1, board, col: 0);
			var section2 = CreateBoardSection(bucket2, board, col: 1);

			section1.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section2.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = CreateWorkflow(jobHeader, "workflow", bucket1);
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60); // Two tasks makes things fail.

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().First();
				var section1Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section2.PK);

				AssertSamePK(bucket1, workflow.CurrentComponent);

				taskCard.OnDragDropStarting();

				taskCard.Left = section2Control.Left + 10;
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				Application.DoEvents();

				taskCard.OnDragDropFinished();

				AssertSamePK(bucket2, workflow.CurrentComponent);

				var newFactory = Factory.CreateNewFactory();
				var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

				AssertSamePK(bucket2, loadedWorkflow.CurrentComponent);
			}
		}

		public void TestDragDrop()
		{
			var staff1 = CreateStaffInCurrentBranchDept("FOO", "FooBar");
			var staff2 = CreateStaffInCurrentBranchDept("BAR", "BarFoo");

			var section = config.BucketSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK, true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);

			CreateTask(workflow, staff1.GS_Code, 60);
			CreateTask(workflow, staff1.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.WindowState = FormWindowState.Maximized;
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().First();
				var section1Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section.PK);

				var emptyTaskPanel = form.FindAll<TaskPanel>().Single(p => !p.TaskCards.Any());

				taskCard.OnDragDropStarting();

				taskCard.Left = emptyTaskPanel.Left + 20;
				taskCard.Top = emptyTaskPanel.Top + 60;
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				Application.DoEvents();

				section1Control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				taskCard.OnDragDropFinished();

				var newFactory = Factory.CreateNewFactory();
				var loadedTask = newFactory.Load<ProcessTask>(taskCard.CardContent.TaskIdentifier);

				AssertEquals(staff2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
			}
		}

		#endregion

		#region Tickets in Cells

		public void TestTaskContents_ReleaseSchedulerBoardSection()
		{
			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var buffer = CreateBuffer(system, "booff");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			LinkComponents(entryBucket, bucket1);
			LinkComponents(entryBucket, bucket2);

			LinkComponents(bucket1, buffer);
			LinkComponents(bucket2, buffer);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.DesignateAsCCR(buffer);

			group.Staff.AddRange(resource1, resource2);

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 90);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 90);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket1.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);
			var task2_1 = CreateTask(workflow2, resource2.GS_Code, 90);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = bucket2.PK;
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-1);
			var task3_1 = CreateTask(workflow3, resource2.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(2, viewModel.PrimaryChannels.Count());
			AssertEquals(2, viewModel.SecondaryAxisChannels.Count());
			AssertEquals("The unioned set of channels shouldn't have fewer channels", 4, viewModel.AllChannels.Count());

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(4, taskPanels.Length);

				var unreleasedChannel1Panel = taskPanels[2];
				var unreleasedChannel2Panel = taskPanels[3];

				AssertEquals(1, unreleasedChannel1Panel.TaskCards.Count());
				AssertEquals(workflow1.PK, unreleasedChannel1Panel.TaskCards.ElementAt(0).CardContent.WorkflowIdentifier);

				AssertEquals(2, unreleasedChannel2Panel.TaskCards.Count());
				AssertEquals(workflow2.PK, unreleasedChannel2Panel.TaskCards.ElementAt(0).CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, unreleasedChannel2Panel.TaskCards.ElementAt(1).CardContent.WorkflowIdentifier);
			}
		}

		public void TestTaskContents_SectionWithMultipleComponents_Buckets()
		{
			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");

			LinkComponents(entryBucket, bucket1);
			LinkComponents(entryBucket, bucket2);

			var section = CreateBoardSection(bucket1);
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = bucket2.PK;

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			workflow1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 90);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 90);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = entryBucket.PK;
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(1, taskPanels.Length);

				var taskPanel = taskPanels[0];

				AssertEquals(2, taskPanel.TaskCards.Count());
				AssertCollectionContains(taskPanel.TaskCards, t => t.CardContent.WorkflowIdentifier == workflow1.PK);
				AssertCollectionContains(taskPanel.TaskCards, t => t.CardContent.WorkflowIdentifier == workflow2.PK);
			}
		}

		[TestDate(2013, 12, 13)]
		public void TestTaskContents_SectionWithMultipleComponents_BuffersWithSameTimespan_TimeCells()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var buffer1 = CreateBuffer(system, "buffer1");
			var buffer2 = CreateBuffer(system, "buffer2");

			LinkComponents(entryBucket, buffer1);
			LinkComponents(entryBucket, buffer2);

			var section = CreateBoardSection(buffer1);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer2.PK;

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task1 = CreateTask(workflow1, resource.GS_Code, 90);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer2.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task2 = CreateTask(workflow2, resource.GS_Code, 90);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = entryBucket.PK;
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task3 = CreateTask(workflow3, resource.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(13, taskPanels.Length);

				var taskPanel = taskPanels[10];

				AssertEquals(2, taskPanel.TaskCards.Count());
				AssertCollectionContains(taskPanel.TaskCards, t => t.CardContent.WorkflowIdentifier == workflow1.PK);
				AssertCollectionContains(taskPanel.TaskCards, t => t.CardContent.WorkflowIdentifier == workflow2.PK);
			}
		}

		[TestDate(2014, 1, 9, 15, 50, 0)]
		public void TestTaskContents_Buffer()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Saturday, WorkingDaysTestHelper.NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Sunday, WorkingDaysTestHelper.NineToFive);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var buffer = CreateBuffer(system, "boofer");
			LinkComponents(entryBucket, buffer);

			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			for (int i = 0; i < section.SectionConfiguration.CellsPerSubsection; i++)
			{
				var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow.FH_FC_CurrentComponent = buffer.PK;
				workflow.FH_ReleaseDateTime = workflow.FH_ReleaseDateTime.AddDays(-i);
				CreateTask(workflow, resource.GS_Code, 60, description: "task" + i);
			}

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 800, Height = 800 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(section.SectionConfiguration.CellsPerSubsection, taskPanels.Length);

				for (int i = 0; i < taskPanels.Length; i++)
				{
					var panel = taskPanels[i];
					AssertEquals("Should be one card in panel " + i, 1, panel.TaskCards.Count());
					AssertEquals("task" + i, panel.TaskCards.ElementAt(0).CardContent.GetTask(Factory).P9_Description);
				}
			}
		}

		[TestDate(2013, 12, 13)]
		public void TestTaskContents_SectionWithMultipleComponents_BuffersWithSameTimespan()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var buffer1 = CreateBuffer(system, "buffer1");
			var buffer2 = CreateBuffer(system, "buffer2");

			LinkComponents(entryBucket, buffer1);
			LinkComponents(entryBucket, buffer2);

			var section = CreateBoardSection(buffer1);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer2.PK;

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task1 = CreateTask(workflow1, resource.GS_Code, 90);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer2.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task2 = CreateTask(workflow2, resource.GS_Code, 90);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = entryBucket.PK;
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task3 = CreateTask(workflow3, resource.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(13, taskPanels.Length);

				var taskPanel = taskPanels[10];

				AssertEquals(2, taskPanel.TaskCards.Count());
				AssertCollectionContains(taskPanel.TaskCards, t => t.CardContent.WorkflowIdentifier == workflow1.PK);
				AssertCollectionContains(taskPanel.TaskCards, t => t.CardContent.WorkflowIdentifier == workflow2.PK);
			}
		}

		[TestDate(2013, 12, 13)]
		public void TestTaskContents_SectionWithMultipleComponents_BuffersWithDifferentTimespans()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var buffer1 = CreateBuffer(system, "buffer1");
			var buffer2 = CreateBuffer(system, "buffer2");
			buffer2.FC_BufferTimespanInMinutes = 64 * 60;

			LinkComponents(entryBucket, buffer1);
			LinkComponents(entryBucket, buffer2);

			var section = CreateBoardSection(buffer1);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer2.PK;

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task1 = CreateTask(workflow1, resource.GS_Code, 90);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer2.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task2 = CreateTask(workflow2, resource.GS_Code, 90);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = entryBucket.PK;
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-2);
			var task3 = CreateTask(workflow3, resource.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(13, taskPanels.Length);

				var thirdTaskPanel = taskPanels[10];
				var fourthTaskPanel = taskPanels[9];

				AssertEquals(1, thirdTaskPanel.TaskCards.Count());
				AssertEquals(workflow1.PK, thirdTaskPanel.TaskCards.ElementAt(0).CardContent.WorkflowIdentifier);

				AssertEquals(1, fourthTaskPanel.TaskCards.Count());
				AssertEquals(workflow2.PK, fourthTaskPanel.TaskCards.ElementAt(0).CardContent.WorkflowIdentifier);
			}
		}

		[TestDate(2013, 12, 13)]
		public void TestTaskContents_SectionWithMultipleComponents_BuffersWithDifferentTimespans_ZoneZeroInAdditionalComponent()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = config.System;
			var entryBucket = CreateBucket(system, "Entry to BMS");
			var buffer1 = CreateBuffer(system, "buffer1");
			var buffer2 = CreateBuffer(system, "buffer2");
			buffer2.FC_BufferTimespanInMinutes = 64 * 60;

			LinkComponents(entryBucket, buffer1);
			LinkComponents(entryBucket, buffer2);

			var section = CreateBoardSection(buffer1);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer2.PK;

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-11);
			var task1 = CreateTask(workflow1, resource.GS_Code, 90);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer2.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-11);
			var task2 = CreateTask(workflow2, resource.GS_Code, 90);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = entryBucket.PK;
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-11);
			var task3 = CreateTask(workflow3, resource.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				var taskPanels = control.Table.FindAll<TaskPanel>().ToArray();
				AssertEquals(13, taskPanels.Length);

				var day10TaskPanel = taskPanels[3];
				var day13TaskPanel = taskPanels[0];

				AssertEquals(1, day10TaskPanel.TaskCards.Count());
				AssertEquals(workflow1.PK, day10TaskPanel.TaskCards.ElementAt(0).CardContent.WorkflowIdentifier);

				AssertEquals(1, day13TaskPanel.TaskCards.Count());
				AssertEquals(workflow2.PK, day13TaskPanel.TaskCards.ElementAt(0).CardContent.WorkflowIdentifier);
			}
		}

		public void TestCellContent_Buffer_ReleaseScheduler_Vertical()
		{
			var system = config.System;
			var bucket = config.Bucket;
			var buffer = config.Buffer;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "resource1";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "resource2";
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			resource3.GS_FullName = "resource3";
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();
			resource4.GS_FullName = "resource4";

			group.Staff.AddRange(resource1, resource2, resource3, resource4);

			resource1.DesignateAsCCR(buffer);
			resource2.DesignateAsCCR(buffer);

			AssertNoErrors(section);

			for (int i = 0; i < 10; i++)
			{
				var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow1.FH_CompletionStatement = string.Format("workflow{0}_1", i);
				var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow2.FH_CompletionStatement = string.Format("workflow{0}_2", i);
				var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow3.FH_CompletionStatement = string.Format("workflow{0}_3", i);

				var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow4.FH_CompletionStatement = string.Format("workflow{0}_4", i);
				var workflow5 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow5.FH_CompletionStatement = string.Format("workflow{0}_5", i);

				workflow1.FH_FC_CurrentComponent = workflow2.FH_FC_CurrentComponent = buffer.PK;
				workflow3.FH_FC_CurrentComponent = workflow4.FH_FC_CurrentComponent = workflow5.FH_FC_CurrentComponent = bucket.PK;

				for (int j = 0; j < 2; j++)
				{
					CreateTask(workflow1, resource1.GS_Code, 60);
					CreateTask(workflow2, resource2.GS_Code, 60);
					CreateTask(workflow3, resource2.GS_Code, 60);
					CreateTask(workflow4, resource3.GS_Code, 60);
					CreateTask(workflow5, resource4.GS_Code, 60);
				}
			}

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			using (var form = new ZForm { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Application.DoEvents();

				BMComponentControlTest.AssertCellContents(control, 0, 0, CellContentType.Label);
				BMComponentControlTest.AssertCellContents(control, 1, 0, CellContentType.ChannelHeading);
				BMComponentControlTest.AssertCellContents(control, 2, 0, CellContentType.ChannelHeading);
				BMComponentControlTest.AssertCellContents(control, 3, 0, CellContentType.ChannelHeading);

				BMComponentControlTest.AssertCellContents(control, 0, 1, CellContentType.ChannelHeading);
				BMComponentControlTest.AssertCellContents(control, 1, 1, CellContentType.Cards, taskCardCount: 10);
				BMComponentControlTest.AssertCellContents(control, 2, 1, CellContentType.Cards, taskCardCount: 10);
				BMComponentControlTest.AssertCellContents(control, 3, 1, CellContentType.Cards);

				BMComponentControlTest.AssertCellContents(control, 0, 2, CellContentType.ChannelHeading);
				BMComponentControlTest.AssertCellContents(control, 1, 2, CellContentType.Cards);
				BMComponentControlTest.AssertCellContents(control, 2, 2, CellContentType.Cards, taskCardCount: 10);
				BMComponentControlTest.AssertCellContents(control, 3, 2, CellContentType.Cards, taskCardCount: 20);

				var releasedChannel1TaskPanel = (TaskPanel)control.Table.GetControlFromPosition(1, 1);
				var taskCards = releasedChannel1TaskPanel.TaskCards.ToArray();

				var defaultTicketHeight = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard).Height + 4;
				var startingHeight = 3;
				var y = startingHeight;

				AssertEquals("Did this fail because you resized default cards?", 10, taskCards.Length);

				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(3, y), taskCards[0].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(3, y += defaultTicketHeight), taskCards[1].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(3, y += defaultTicketHeight), taskCards[2].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(3, y += defaultTicketHeight), taskCards[3].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(3, y += defaultTicketHeight), taskCards[4].Location);

				y = startingHeight;

				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(129, y), taskCards[5].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(129, y += defaultTicketHeight), taskCards[6].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(129, y += defaultTicketHeight), taskCards[7].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(129, y += defaultTicketHeight), taskCards[8].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(129, y += defaultTicketHeight), taskCards[9].Location);
			}
		}

		#endregion

		#region Implementation

		VisualBoardTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	#endregion

	#region NonTransactionedTestCase

	class BMComponentControlSnapshotTest : NonTransactionedTestCase
	{
		[TestDate(2015, 8, 3)]
		public void TestSetupTasksWhenTooManyItemsOnBoard()
		{
			var factory = new BusinessObjectFactory();
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var config = TestConfigsHelper.CreateSchematicTestConfig(factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(factory).ProcessHeaders[0];

			for (int i = 0; i <= 100; i++)
			{
				BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			}
			factory.Save();

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm { Width = 1024, Height = 768 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel) { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				control.SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
				Thread.Sleep(TimeSpan.FromSeconds(1));
				Application.DoEvents();
				Thread.Sleep(TimeSpan.FromSeconds(1));
				Application.DoEvents();

				var taskCardsShown = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("We show a label instead of the too many items", 0, taskCardsShown.Length);
			}
		}
	}

	#endregion

	#region BoardSectionControlTestCase

	[TestedType(typeof(BMComponentControl))]
	class BMComponentControlIBoardSectionControlTest : BoardSectionControlTestCase<BMComponentControl>
	{
		protected override BMComponentControl GetControl()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			Factory.Save();

			return new BMComponentControl(config.BufferSection.SectionConfiguration, viewModel);
		}
	}

	#endregion

	#region Test Classes

	class BMComponentControlForTest : BMComponentControl
	{
		public BMComponentControlForTest(BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionViewModel viewModel)
			: base(sectionConfiguration, viewModel)
		{
		}

		protected override Control GetHeaderControlCore(CellContent cell, BMComponentControl componentControl, BMBoardSectionViewModel viewModel, Func<Control> controlToWrapGetter)
		{
			return new HeaderWrapperControlForTest(controlToWrapGetter(), cell, componentControl, viewModel);
		}
	}

	class HeaderWrapperControlForTest : HeaderWrapperControl
	{
		internal HeaderWrapperControlForTest(Control controlToWrap, CellContent cell, BMComponentControl componentControl, BMBoardSectionViewModel viewModel)
			: base(controlToWrap, cell, componentControl, viewModel)
		{
		}

		public CellContent Cell => cell;
	}

	#endregion
}
