using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	public class ChartSectionConfigurationControlTest : TestCaseWithFactory
	{
		public void TestEnableMENTSectionsDefault()
		{
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				form.Show();
				var label = control.FindSingle<ZLabel>();
				AssertEquals("MENT sections are no longer supported.", label.Text);
			}
		}

		public void TestCheckBox_EnableAndReadOnly()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Extraction Default Visualisation";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);
			configuration.OverrideDefaultVisualisation = true;

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				AssertEquals("GIVEN OverrideDefaultVisualisation = TRUE", true, configuration.OverrideDefaultVisualisation);
				var isNormalisedCheckBox = form.Controls.Find("isNormalisedCheckBox", true)[0] as ZCheckBox;
				AssertEquals("THEN configuration-controls should be not readonly", false, isNormalisedCheckBox.ReadOnly);

				configuration.ExtractionPK = ZGuid.NewZGuid();
				AssertNull("WHEN setting invalid extraction", configuration.Extraction);
				AssertEquals("THEN configuration-controls should be readonly", true, isNormalisedCheckBox.ReadOnly);

				configuration.ExtractionPK = extraction.PK;
				AssertNotNull("WHEN setting valid extraction", configuration.Extraction);
				AssertEquals("THEN OverrideDefaultVisualisation should automaticlaly set to FALSE", false, configuration.OverrideDefaultVisualisation);
				AssertEquals("THEN checkbox should be readonly", true, isNormalisedCheckBox.ReadOnly);

				configuration.OverrideDefaultVisualisation = true;
				AssertEquals("WHEN setting OverrideDefaultVisualisation = TRUE", true, configuration.OverrideDefaultVisualisation);
				AssertEquals("THEN controls should be not readonly", false, isNormalisedCheckBox.ReadOnly);
			}
		}

		#region Extraction and Override Visualisation relationship

		public void TestOnLoad_ExtractionEmpty_OverrideVisualisationShouldBeDisable()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Extraction Default Visualisation";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory);

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				AssertNull("GIVEN Extraction is empty", configuration.Extraction);

				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				AssertGraphVisualisationConfigurationIsReadOnly(
					"WHEN loading, THEN override-visualisation should be READONLY because setting it before setting extraction cause RelatedVisualisation to be NULL hence inconsistent data",
					form,
					true);

				configuration.ExtractionPK = extraction.PK;
				AssertEquals("WHEN setting extraction", extraction.PK, configuration.ExtractionPK);
				AssertEquals("THEN override-visualisation should be false", false, configuration.OverrideDefaultVisualisation);

				var overrideVisualisationCheckBox = form.Controls.Find("overrideVisualisationCheckBox", true)[0];
				AssertEquals("THEN override-visualisation should be enabled", true, overrideVisualisationCheckBox.Enabled);

				AssertGraphVisualisationConfigurationIsReadOnly(
					"THEN configurations should be READONLY",
					form,
					true);
			}
		}

		public void TestOnLoad_ExtractionNotEmpty_OverrideVisualisationShouldBeEnable()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Extraction Default Visualisation";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				AssertNotNull("GIVEN Extraction exist", configuration.Extraction);

				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				var overrideVisualisationCheckBox = form.Controls.Find("overrideVisualisationCheckBox", true)[0];
				AssertEquals("WHEN loading THEN override-visualisation should be enabled", true, overrideVisualisationCheckBox.Enabled);
			}
		}

		public void TestModifyExtractionPK_ToEmpty_OverrideVisualisationShouldBeFalseAndDisable()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Extraction Default Visualisation";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				AssertNotNull("GIVEN Extraction exist", configuration.Extraction);

				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				configuration.ExtractionPK = ZGuid.Empty;
				AssertEquals("WHEN modifying Extraction to empty", ZGuid.Empty, configuration.ExtractionPK);
				AssertEquals("THEN override-visualisation should be false", false, configuration.OverrideDefaultVisualisation);

				var overrideVisualisationCheckBox = form.Controls.Find("overrideVisualisationCheckBox", true)[0];
				AssertEquals("THEN override-visualisation should be disable", false, overrideVisualisationCheckBox.Enabled);

				AssertGraphVisualisationConfigurationIsReadOnly(
					"THEN configurations should be READONLY",
					form,
					true);
			}
		}

		public void TestModifyExtractionPK_ToNotEmpty_OverrideVisualisationShouldBeFalseAndEnable()
		{
			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true, queryCode: "TESTQUERY1");
			extraction1.DefaultVisualisation.GraphTitle = "Extraction 1";

			var extraction2 = MENTTestHelper.CreateExtraction(Factory, "Extraction 2", isInstantaneous: true, queryCode: "TESTQUERY2");
			extraction2.DefaultVisualisation.GraphTitle = "Extraction 2";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction1);

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				AssertNotNull("GIVEN Extraction exist", configuration.Extraction);

				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				configuration.ExtractionPK = extraction2.PK;
				AssertEquals("WHEN modifying Extraction", extraction2.PK, configuration.ExtractionPK);
				AssertEquals("THEN override-visualisation should be untick because otherwise it got values from previous extraction", false, configuration.OverrideDefaultVisualisation);

				var overrideVisualisationCheckBox = form.Controls.Find("overrideVisualisationCheckBox", true)[0];
				AssertEquals("THEN override-visualisation should be enabled", true, overrideVisualisationCheckBox.Enabled);

				AssertGraphVisualisationConfigurationIsReadOnly(
					"THEN configurations should be READONLY",
					form,
					true);
			}
		}

		public void TestModifyExtractionPK_ToInvalid_OverrideVisualisationShouldBeFalseAndDisable()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Extraction Default Visualisation";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				AssertNotNull("GIVEN Extraction exist", configuration.Extraction);

				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				var invalidExtractinPK = ZGuid.NewZGuid();
				configuration.ExtractionPK = invalidExtractinPK;
				AssertEquals("WHEN modifying Extraction to invalid", invalidExtractinPK, configuration.ExtractionPK);
				AssertEquals("THEN override-visualisation should be false", false, configuration.OverrideDefaultVisualisation);

				var overrideVisualisationCheckBox = form.Controls.Find("overrideVisualisationCheckBox", true)[0];
				AssertEquals("THEN override-visualisation should be disable", false, overrideVisualisationCheckBox.Enabled);

				AssertGraphVisualisationConfigurationIsReadOnly("THEN configurations should be READONLY", form, true);
			}
		}

		public void TestExtractionField_WhenKeyPressed_ShowMENTAgedScoreQueryForm_ShouldOpenTheVisualizationTab_AndSelectTheExtraction()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				AssertNotNull("GIVEN Extraction exist", configuration.Extraction);

				control.SetDataBinding(configuration, string.Empty);
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var extractionField = control.FindSingleOrDefault<ZGuidFindBox>(o => o.Name == "extractionZGuidFindBox");
				extractionField.CodeBox.Focus();
				KeySender.SendKeyDownToProcessCmdKey(extractionField.CodeBox, (int)Keys.F3);

				AssertNotNull("Form cannot be null", ZFormModaliser.LastFormShownForTest);
				AssertEquals("Should bring up query form", "Edit MENT Aged Score Query", ZFormModaliser.LastFormShownForTest.Text);

				Application.DoEvents();

				var queryForm = ZFormModaliser.LastFormShownForTest as MENTAgedScoreQueryForm;
				var mentTabPage = queryForm.FindSingle<MENTNavigationTabPage>();
				var zTabControl = mentTabPage.Parent as ZTabControl;

				Assert("The visualization tab should be selected", zTabControl.SelectedTab == mentTabPage);

				var visualizationControl = mentTabPage.FindSingle<VisualisationConfigurationControl>();

				var extractionSelected = visualizationControl.ExtractionsGrid_ForTest.GetCurrent() as MENTAgedScoreExtraction;
				Assert("The correct extraction should be selected in the grid", extractionSelected.PK == configuration.ExtractionPK);
			}
		}

		#endregion

		public void TestOverrideDefaultVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Extraction Default Visualisation";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				control.SetDataBinding(configuration, string.Empty);
				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				AssertEquals("Pre-Condition OverrideDefaultVisualisation = FALSE", false, configuration.OverrideDefaultVisualisation);
				AssertNotNull("Extraction Default Visualisation should never be NULL", configuration.Extraction.DefaultVisualisation);

				AssertGraphVisualisationConfigurationIsReadOnly("WHEN OverrideDefaultVisualisation is FALSE THEN configuration should be READONLY.", form, true);

				configuration.OverrideDefaultVisualisation = true;
				Application.DoEvents();

				AssertGraphVisualisationConfigurationIsReadOnly("WHEN OverrideDefaultVisualisation is TRUE THEN configuration should be NOT READONLY.", form, false);

				AssertEquals("WHEN OverrideDefaultVisualisation is TRUE THEN visualisation should have default values from extraction.DefaultVisualisation",
					"Extraction Default Visualisation",
					configuration.RelatedVisualisation.GraphTitle);

				configuration.RelatedVisualisation.GraphTitle = "Updated Extraction Default Visualisation";
				Factory.Save();
				AssertEquals("WHEN modify related-visualisation, the original visualisation should not change",
					"Extraction Default Visualisation",
					extraction.DefaultVisualisation.GraphTitle);
			}
		}

		public void TestOverrideDefaultVisualisation_AlternatingValue()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			configuration.OverrideDefaultVisualisation = true;

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionConfigurationControl())
			{
				control.SetDataBinding(configuration, string.Empty);

				form.Show();
				form.Controls.Add(control);

				Application.DoEvents();

				AssertEquals("Initially OverrideDefaultVisualisation should be true", true, configuration.OverrideDefaultVisualisation);

				AssertGraphVisualisationConfigurationIsReadOnly("WHEN OverrideDefaultVisualisation is TRUE THEN configuration should be NOT READONLY.", form, false);

				configuration.OverrideDefaultVisualisation = false;
				Application.DoEvents();

				AssertGraphVisualisationConfigurationIsReadOnly("WHEN OverrideDefaultVisualisation is FALSE THEN configuration should be READONLY.", form, true);

				configuration.OverrideDefaultVisualisation = true;
				Application.DoEvents();

				AssertGraphVisualisationConfigurationIsReadOnly("WHEN OverrideDefaultVisualisation is TRUE (alternating) THEN configuration should be NOT READONLY.", form, false);
			}
		}

		public void TestOverrideDefaultVisualisation_WithMultipleConfigurations()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, new string[] { "ORG" });
			var board = system.Boards.AddNew();
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true);
			extraction.DefaultVisualisation.GraphTitle = "Template Visualisation";

			var section1 = MENTTestHelper.CreateMENTBoardSection(Factory, board, extraction.PK);
			var section2 = MENTTestHelper.CreateMENTBoardSection(Factory, board, extraction.PK);
			var configuration1 = (ChartSectionConfiguration)section1.Configuration;
			var configuration2 = (ChartSectionConfiguration)section2.Configuration;

			configuration1.OverrideDefaultVisualisation = false;
			configuration1.RelatedVisualisation.GraphTitle = "Configuration1";
			configuration2.OverrideDefaultVisualisation = true;
			configuration2.RelatedVisualisation.GraphTitle = "Configuration2";

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				Application.DoEvents();

				var boardSectionsGrid = form.Controls.Find("BoardSectionsGrid", true)[0] as ZGrid;

				boardSectionsGrid.SelectSingleElement(section1);
				form.Refresh();

				var graphTitleTextBox = form.Controls.Find("graphTitleTextBox", true)[0];

				AssertEquals("Pre-condition: grid shows 3 rows i.e. section1, section2 and empty row", 3, boardSectionsGrid.VisibleRowCount);
				AssertEquals("Pre-condition: section1 should have 1 display-sequence", 1, section1.DisplaySequence);
				AssertEquals("Pre-condition: section2 should have 2 display-sequence", 2, section2.DisplaySequence);

				AssertEquals("WHEN section1 is selected in grid, THEN visualisation should show section1 values.", "Configuration1", graphTitleTextBox.Text);
				AssertEquals("WHEN OverrideDefaultVisualisation is FALSE", false, configuration1.OverrideDefaultVisualisation);
				AssertGraphVisualisationConfigurationIsReadOnly("THEN configuration should be READONLY", form, true);

				boardSectionsGrid.SelectSingleElement(section2);
				Application.DoEvents();
				AssertEquals("WHEN section2 is selected in grid, THEN visualisation should shows section2 values.", "Configuration2", graphTitleTextBox.Text);
				AssertEquals("WHEN OverrideDefaultVisualisation is TRUE", true, configuration2.OverrideDefaultVisualisation);
				AssertGraphVisualisationConfigurationIsReadOnly("WHEN OverrideDefaultVisualisation is TRUE THEN configuration should be NOT READONLY", form, false);
			}
		}

		#region Implementation

		void AssertGraphVisualisationConfigurationIsReadOnly(string message, ZForm form, bool readOnly)
		{
			var graphTitleTextBox = form.Controls.Find("graphTitleTextBox", true)[0];
			var isNormalisedCheckBox = form.Controls.Find("isNormalisedCheckBox", true)[0];
			var graphTypeDropEdit = form.Controls.Find("graphTypeDropEdit", true)[0];
			var grid = (ZGrid)form.Controls.Find("zGrid1", true)[0];

			graphTypeDropEdit.Focus(); // move away from grid focus then grid.focus(), so it triggers VisualisationColumnSpecification.GetPropertyReadOnly
			grid.Focus();

			CombineAssertions(message, () =>
			{
				AssertEquals("graphTitleTextBox", readOnly, graphTitleTextBox.GetReadOnly());
				AssertEquals("isNormalisedCheckBox", readOnly, isNormalisedCheckBox.GetReadOnly());
				AssertEquals("graphTypeDropEdit", readOnly, graphTypeDropEdit.GetReadOnly());
				AssertEquals("grid", readOnly, grid.GetReadOnly() || !grid.Enabled);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		#endregion
	}
}
