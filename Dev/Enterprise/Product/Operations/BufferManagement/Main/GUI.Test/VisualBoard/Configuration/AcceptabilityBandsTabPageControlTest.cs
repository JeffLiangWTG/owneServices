using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AcceptabilityBandsTabPageControlTest : BMSGUITestCase
	{
		public void TestShouldShowAcceptabilityBandsNotAvailableLabel_WhenWorkflowManagementModeIsEWForBWF()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "Values Must Change!");
			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);

			Factory.Save();

			void TestCase(string workflowManagementMode, bool shouldGridBeVisibleAndNotAvailableLabelNotVisible)
			{
				BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowManagementMode);

				using (var form = new BMBoardForm(config.BufferBoard))
				{
					form.Show();
					Application.DoEvents();

					var tabControl = form.FindSingle<ZTabControl>("SectionConfigTabControl");
					tabControl.SelectTab("AcceptabilityBandsTabPageControl");
					Application.DoEvents();

					var control = form.FindSingle<AcceptabilityBandsTabPageControl>();
					var grid = control.FindSingle<ZGrid>();
					var bandsHintLabel = control.FindSingle<Label>("BandsHintLabel");
					var notAvailableLabel = control.FindSingle<Label>("NotAvailableLabel");

					AssertEquals(shouldGridBeVisibleAndNotAvailableLabelNotVisible, grid.Visible);
					AssertEquals(shouldGridBeVisibleAndNotAvailableLabelNotVisible, bandsHintLabel.Visible);
					AssertEquals(!shouldGridBeVisibleAndNotAvailableLabelNotVisible, notAvailableLabel.Visible);
				}
			}

			TestCase(WorkflowManagementModes.Codes.BasicWorkflow, shouldGridBeVisibleAndNotAvailableLabelNotVisible: false);
			TestCase(WorkflowManagementModes.Codes.EnhancedWorkflow, shouldGridBeVisibleAndNotAvailableLabelNotVisible: false);
			TestCase(WorkflowManagementModes.Codes.IncludesBufferManagement, shouldGridBeVisibleAndNotAvailableLabelNotVisible: true);
			TestCase(WorkflowManagementModes.Codes.PlanningManagement, shouldGridBeVisibleAndNotAvailableLabelNotVisible: true);
		}

		public void TestOverridenBoundaryValues()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "Values Must Change!");
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);

			Factory.Save();

			using (var form = new BMBoardForm(config.BufferBoard))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>("SectionConfigTabControl");
				tabControl.SelectTab("AcceptabilityBandsTabPageControl");
				Application.DoEvents();

				var control = form.FindSingle<AcceptabilityBandsTabPageControl>();
				var grid = control.FindSingle<ZGrid>();

				AssertEquals(1, grid.List.Count);

				var cautionMinColumn = grid.Columns.IndexOf(x => x.ColumnName == "CautionMinEffectiveValue");
				var goodMinColumn = grid.Columns.IndexOf(x => x.ColumnName == "GoodMinEffectiveValue");
				var excellentMinColumn = grid.Columns.IndexOf(x => x.ColumnName == "ExcellentMinEffectiveValue");
				var excellentMaxColumn = grid.Columns.IndexOf(x => x.ColumnName == "ExcellentMaxEffectiveValue");
				var goodMaxColumn = grid.Columns.IndexOf(x => x.ColumnName == "GoodMaxEffectiveValue");
				var cautionMaxColumn = grid.Columns.IndexOf(x => x.ColumnName == "CautionMaxEffectiveValue");
				var isOverriddenColumn = grid.Columns.IndexOf(x => x.ColumnName == "AreBoundaryValuesOverridden");

				AssertEquals(1, grid[0, cautionMinColumn]);
				AssertEquals(2, grid[0, goodMinColumn]);
				AssertEquals(3, grid[0, excellentMinColumn]);
				AssertEquals(4, grid[0, excellentMaxColumn]);
				AssertEquals(5, grid[0, goodMaxColumn]);
				AssertEquals(6, grid[0, cautionMaxColumn]);

				band.BAB_CautionLowerBound = 7;
				band.BAB_GoodLowerBound = 8;
				band.BAB_ExcellentLowerBound = 9;
				band.BAB_ExcellentUpperBound = 10;
				band.BAB_GoodUpperBound = 11;
				band.BAB_CautionUpperBound = 12;

				Factory.Save();
				Application.DoEvents();

				AssertEquals(7, grid[0, cautionMinColumn]);
				AssertEquals(8, grid[0, goodMinColumn]);
				AssertEquals(9, grid[0, excellentMinColumn]);
				AssertEquals(10, grid[0, excellentMaxColumn]);
				AssertEquals(11, grid[0, goodMaxColumn]);
				AssertEquals(12, grid[0, cautionMaxColumn]);

				grid[0, isOverriddenColumn] = true;
				Application.DoEvents();

				AssertEquals(7, grid[0, cautionMinColumn]);
				AssertEquals(8, grid[0, goodMinColumn]);
				AssertEquals(9, grid[0, excellentMinColumn]);
				AssertEquals(10, grid[0, excellentMaxColumn]);
				AssertEquals(11, grid[0, goodMaxColumn]);
				AssertEquals(12, grid[0, cautionMaxColumn]);

				grid[0, cautionMinColumn] = new ZInt(13);
				grid[0, goodMinColumn] = new ZInt(14);
				grid[0, excellentMinColumn] = new ZInt(15);
				grid[0, excellentMaxColumn] = new ZInt(16);
				grid[0, goodMaxColumn] = new ZInt(17);
				grid[0, cautionMaxColumn] = new ZInt(18);

				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals(13, sectionBand.CautionMinOverride);
				AssertEquals(14, sectionBand.GoodMinOverride);
				AssertEquals(15, sectionBand.ExcellentMinOverride);
				AssertEquals(16, sectionBand.ExcellentMaxOverride);
				AssertEquals(17, sectionBand.GoodMaxOverride);
				AssertEquals(18, sectionBand.CautionMaxOverride);

				grid[0, isOverriddenColumn] = false;
				Application.DoEvents();

				AssertEquals(7, grid[0, cautionMinColumn]);
				AssertEquals(8, grid[0, goodMinColumn]);
				AssertEquals(9, grid[0, excellentMinColumn]);
				AssertEquals(10, grid[0, excellentMaxColumn]);
				AssertEquals(11, grid[0, goodMaxColumn]);
				AssertEquals(12, grid[0, cautionMaxColumn]);
			}
		}
	}
}
