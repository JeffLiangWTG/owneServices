using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	public class GraphVisualisationConfigurationControlTest : BMSTestCaseWithFactory
	{
		#region Populate Button

		public void TestPopulate_WithNoOverrideDefaultVisualisation()
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

				configuration.OverrideDefaultVisualisation = false;
				AssertEquals("GIVEN OverrideDefaultVisualisation=FALSE", false, configuration.OverrideDefaultVisualisation);

				var button = control.Controls.Find("refreshCategorySequenceZButton", true).Cast<ZButton>().SingleOrDefault();
				button.ReadOnly = false;
				button.PerformClick();
				AssertEquals("WHEN click populate, SHOULD show warning message", "'Override Default Visualization' should be enabled in order to populate the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideCategorySequencePopulateEnabled()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Alabama Horses");

			using (var control = new GraphVisualisationConfigurationControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(visualisation, string.Empty);

				var refreshCategorySequenceZButton = control.Controls.Find("refreshCategorySequenceZButton", true)[0];

				AssertEquals(true, refreshCategorySequenceZButton.Enabled);

				visualisation.UseOverriddenCategorySequence = true;

				Application.DoEvents();

				AssertEquals(false, visualisation.AllowPopulateCategorySequenceCollection);
				AssertEquals(false, refreshCategorySequenceZButton.Enabled);
			}
		}

		#endregion

		#region Preview

		public void TestPreviewForm()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Modal or not");

			using (var control = new GraphVisualisationConfigurationControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				control.SetDataBinding(visualisation, string.Empty);
				form.Show();

				var button = (ZButton)form.Controls.Find("previewButton", true).Single();

				var oldFormCount = Application.OpenForms.Count;

				button.PerformClick();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(oldFormCount + 1, Application.OpenForms.Count);
				Application.OpenForms.OfType<PreviewVisualisationForm>().ToArray().ForEach(f => f.Close());
			}
		}

		public void TestPreviewButton_Click_NullRef()
		{
			using (var control = new GraphVisualisationConfigurationControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var previewButton = (ZButton)form.Controls.Find("previewButton", true).Single();

				CombineAssertions("Precondition", () =>
				{
					AssertNull("No error message", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Preview button should be disabled", false, previewButton.Enabled);
				});

				var oldFormCount = Application.OpenForms.Count;

				AssertNoExceptionThrown("WHEN click preview, THEN no exception should be thrown", previewButton.PerformClick);

				AssertNull("No new form shown", ZFormModaliser.LastFormShownForTest);
				AssertEquals("Total form shown same as before clicking preview", oldFormCount, Application.OpenForms.Count);

				AssertNull("should not have error message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreviewButton_DeletedBackingObject()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Modal or not");

			using (var control = new GraphVisualisationConfigurationControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				control.SetDataBinding(visualisation, string.Empty);
				form.Show();

				Application.DoEvents();

				visualisation.Delete();

				var button = (ZButton)form.Controls.Find("previewButton", true).Single();

				var oldFormCount = Application.OpenForms.Count;

				AssertNoExceptionThrown(button.PerformClick);
				AssertEquals(true, button.Enabled);

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(oldFormCount, Application.OpenForms.Count);

				AssertEquals("Please create an extraction before previewing", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestVisualisationReadOnly()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Visualisation 1");

			visualisation.MVI_IsCustomised = false;

			using (var control = new GraphVisualisationConfigurationControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				control.SetDataBinding(visualisation, string.Empty);
				form.Show();

				Application.DoEvents();

				var graphTitleTextBox = form.Controls.Find("graphTitleTextBox", true)[0];
				var isNormalisedCheckBox = form.Controls.Find("isNormalisedCheckBox", true)[0];
				var graphTypeDropEdit = form.Controls.Find("graphTypeDropEdit", true)[0];
				var grid = (ZGrid)form.Controls.Find("zGrid1", true)[0];
				grid.Focus();

				CombineAssertions("WHEN showing visualisation @ GraphConfiguration, should not be ReadOnly", () =>
				{
					AssertEquals("graphTitleTextBox", false, graphTitleTextBox.GetReadOnly());
					AssertEquals("isNormalisedCheckBox", false, isNormalisedCheckBox.GetReadOnly());
					AssertEquals("graphTypeDropEdit", false, graphTypeDropEdit.GetReadOnly());
					AssertEquals("grid", false, grid.GetReadOnly());
				});
			}
		}

		#region Implementation

		protected override bool ShouldDisableAsyncBehaviour => true;

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		#endregion
	}
}
