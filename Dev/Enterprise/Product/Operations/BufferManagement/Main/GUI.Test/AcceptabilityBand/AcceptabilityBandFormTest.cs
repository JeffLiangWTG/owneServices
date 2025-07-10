using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(AcceptabilityBandForm))]
	class AcceptabilityBandFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "ThisPenIsExcellent";

			using (var form = new AcceptabilityBandForm(band))
			{
				AssertEquals("Acceptability Band - ThisPenIsExcellent", form.FormCaption);
			}
		}

		public void TestFormMakesFilterStripsInvisible()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "TwoHundredBillionTubas";
			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();

				var filterStripSet = form.FindAll<FilterRuleFilterStripControl>().ToArray();
				var sqlTextBox = form.Controls.Find("SQLTextBox", true)[0];

				var filterStrip1 = filterStripSet.Single(x => x.Parent.Name == "filterCustomisationControl");
				var filterStrip2 = filterStripSet.Single(x => x.Parent.Name == "supersetFilterCustomisationControl");

				AssertEquals(false, filterStrip1.Visible);
				AssertEquals(false, filterStrip2.Visible);
				AssertEquals(true, sqlTextBox.Visible);

				band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
				Application.DoEvents();
				AssertEquals(true, filterStrip1.Visible);
				AssertEquals(false, filterStrip2.Visible);
				AssertEquals(false, sqlTextBox.Visible);

				band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;
				Application.DoEvents();
				AssertEquals(true, filterStrip1.Visible);
				AssertEquals(false, filterStrip2.Visible);
				AssertEquals(true, sqlTextBox.Visible);
			}
		}

		public void TestFilterStripControlPreview_CanContainCompanyFilter()
		{
			BMSRegistry.Instance.AllowCompanyFiltersInTagRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			TagRuleRunnerCompanyFilterTest.SetFilterStrips(band.FilterRule, moduleName: ModuleIDs.Customs.JobDeclaration.Name);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow N", bucket);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();

				var previewResult = new List<ProcessHeader>();
				EmbeddedModuleTestHelper.SetListToStoreSearchResultsWhenPopupShown(previewResult);

				var previewButton = GetPreviewButton(form, parentControlName: "filterCustomisationControl");

				using (TestConnection.TrackExecutedCommands())
				{
					previewButton.PerformClick();
					AssertEquals("Preview should show 1 result", 1, previewResult.Count);
					AssertContainsExactElementsInAnyOrder("Preview should has workflows", previewResult.Select(x => x.PK).ToArray(), new[] { workflow.PK });

					TagRuleRunnerCompanyFilterTest.AssertCompanyFilter(
						"WHEN AllowCompanyFiltersInTagRules = false, previewing band with JobDeclaration filterstrip should have company filter",
						TestConnection.ExecutedCommands.Where(sql => !sql.Contains("SELECT TOP 1 * FROM (SELECT TOP 1 * FROM dbo.ProcessHeader)")),
						expectCompanyFilter: true);
				}
			}
		}

		public static ToolStripItem GetPreviewButton(Form form, string parentControlName)
		{
			var toolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip" && x.GetParent<BMFilterStripWrapperControl>().Name == "filterCustomisationControl");
			return toolStrip.Items["ToolStripPreviewDropButton"];
		}

		public void TestFilterStripControlPreview()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket1, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader2 = BMSTestHelper.CreateJobHeader(orgHeader2);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp", bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Shalala", bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "Uslurp", bucket2);

			Factory.Save();

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				var filterControlSet = form.FindAll<BMFilterStripWrapperControl>().ToArray();
				var filterControl1 = filterControlSet.Single(x => (((ZGroupBox)(x.Parent)).CaptionResourceString.Caption == "Value Calculation Filters"));
				AssertEquals(true, filterControl1.IsPreviewAllowed);

				var filterControl2 = filterControlSet.Single(x => (((ZGroupBox)(x.Parent)).CaptionResourceString.Caption == "Superset Filters"));
				AssertEquals(true, filterControl2.IsPreviewAllowed);

				var stripControl1 = filterControl1.FindAll<FilterRuleFilterStripControl>().Single();
				var strip1 = stripControl1.AddNewFilterStrip();
				strip1.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)strip1.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				var stripControl2 = filterControl2.FindAll<FilterRuleFilterStripControl>().Single();
				var strip2 = stripControl2.AddNewFilterStrip();
				strip2.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)strip2.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip" && x.GetParent<BMFilterStripWrapperControl>().Name == "filterCustomisationControl");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("The result should reflect the added filter strip AND the auto added component-related filter strip, and yet...", new[] { workflow1.PK }, result.Select(x => x.PK));

				var supToolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip" && x.GetParent<BMFilterStripWrapperControl>().Name == "supersetFilterCustomisationControl");
				var supPreviewButton = supToolStrip.Items["ToolStripPreviewDropButton"];
				supPreviewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("The result should reflect the added filter strip AND the auto added component-related filter strip, and yet...", new[] { workflow1.PK }, result.Select(x => x.PK));

				band.BAB_FC_Component = ZGuid.Empty;
				Factory.Save();

				previewButton.PerformClick();
				AssertContainsExactElementsInAnyOrder("The result should include matching workflows from all components since one was not specified on the band, and yet...", new[] { workflow1.PK, workflow3.PK }, result.Select(x => x.PK));

				supPreviewButton.PerformClick();
				AssertContainsExactElementsInAnyOrder("The result should include matching workflows from all components since one was not specified on the band, and yet...", new[] { workflow1.PK, workflow3.PK }, result.Select(x => x.PK));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		[TestDate(2016, 4, 20)]
		public void TestFilterStripControlPreview_Superset()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var band = BMSTestHelper.CreateAcceptabilityBand(bucket1, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var jobHeader2 = BMSTestHelper.CreateJobHeader(orgHeader2);
			var jobHeader3 = BMSTestHelper.CreateJobHeader(orgHeader3);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala", bucket1);
			workflow1.FH_AgreedDeliveryDate = new ZDateTime(2016, 4, 19);

			var workflow11 = BMSTestHelper.CreateWorkflow(jobHeader2, "Shalala", bucket1);
			workflow11.FH_AgreedDeliveryDate = new ZDateTime(2016, 4, 20);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp", bucket1);
			workflow2.FH_AgreedDeliveryDate = new ZDateTime(2016, 4, 20);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Shalala", bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "Uslurp", bucket2);

			Factory.Save();

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				var filterControlSet = form.FindAll<BMFilterStripWrapperControl>().ToArray();

				var filterControl1 = filterControlSet.Single(x => (((ZGroupBox)(x.Parent)).CaptionResourceString.Caption == "Value Calculation Filters"));
				AssertEquals(true, filterControl1.IsPreviewAllowed);
				var stripControl1 = filterControl1.FindAll<FilterRuleFilterStripControl>().Single();
				var strip1 = stripControl1.AddNewFilterStrip();
				strip1.CurrentDataItem.FilterDescription = "Agreed Delivery Date";
				((ModuleDateFilter)strip1.CurrentDataItem.CurrentModuleFilter).PropertySearch = "Today";

				var filterControl2 = filterControlSet.Single(x => (((ZGroupBox)(x.Parent)).CaptionResourceString.Caption == "Superset Filters"));
				AssertEquals(true, filterControl2.IsPreviewAllowed);
				var stripControl2 = filterControl2.FindAll<FilterRuleFilterStripControl>().Single();
				var strip2 = stripControl2.AddNewFilterStrip();
				strip2.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)strip2.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip" && x.GetParent<BMFilterStripWrapperControl>().Name == "filterCustomisationControl");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new[] { workflow11.PK }, result.Select(x => x.PK));

				var supToolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip" && x.GetParent<BMFilterStripWrapperControl>().Name == "supersetFilterCustomisationControl");
				var supPreviewButton = supToolStrip.Items["ToolStripPreviewDropButton"];
				supPreviewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new[] { workflow1.PK, workflow11.PK }, result.Select(x => x.PK));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert("1024 wide is too small. RJW approved 1080p for BMS.", true);
		}

		public void TestGraphConfigurationReadOnly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var boardA = BMSTestHelper.CreateBoard(system, name: "First board");
			var sectionA = BMSTestHelper.CreateBoardSection(bucket, boardA);

			var boardB = BMSTestHelper.CreateBoard(system, name: "Second board");
			var sectionB = BMSTestHelper.CreateBoardSection(bucket, boardB);

			var bandA = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var bandB = BMSTestHelper.CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);

			sectionA.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandA.PK;
			sectionA.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandB.PK;
			sectionB.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandA.PK;

			Factory.Save();

			using (var form = new AcceptabilityBandForm_ForTest(bandA))
			{
				form.Show();
				Application.DoEvents();

				var mainTabControl = form.FindAll<ZTemplateTabControl>().First();
				var measurementTabPage = mainTabControl.FindAll<ZTabPage>().Single(t => t.Text == "Measurement");
				mainTabControl.SelectedTab = measurementTabPage;
				Application.DoEvents();

				var secondaryTabControl = form.FindAll<ZTabControl>().Single(t => t.Name == "zTabControl1" && t.FindAll<ZTabPage>().Any(p => p.Text == "Visualization"));
				var visualisationTabPage = secondaryTabControl.FindAll<ZTabPage>().Single(t => t.Text == "Visualization");
				secondaryTabControl.SelectedTab = visualisationTabPage;
				Application.DoEvents();

				var thirdTabControl = visualisationTabPage.FindAll<ZTabControl>().Single(t => t.Name == "zTabControl1");

				var graphTabPage = thirdTabControl.FindAll<ZTabPage>().Single(t => t.Name == "graphConfigurationTabPage");
				thirdTabControl.SelectedTab = graphTabPage;
				Application.DoEvents();

				var graphTitleTextBox = form.FindAll<ZTextBox>().Single(t => t.Name == "graphTitleTextBox");
				AssertEquals("GIVEN initially no query-extraction-visualisation, WHEN visualisation-graph-title-textbox is shown THEN it should be READONLY", true, graphTitleTextBox.GetReadOnly());
			}
		}

		public void TestShowPreDeleteDialogs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var boardA = BMSTestHelper.CreateBoard(system, name: "First board");
			var sectionA = BMSTestHelper.CreateBoardSection(bucket, boardA);

			var boardB = BMSTestHelper.CreateBoard(system, name: "Second board");
			var sectionB = BMSTestHelper.CreateBoardSection(bucket, boardB);

			var bandA = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var bandB = BMSTestHelper.CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);

			sectionA.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandA.PK;
			sectionA.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandB.PK;

			sectionB.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandA.PK;

			Factory.Save();

			using (var form = new AcceptabilityBandForm_ForTest(bandA))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				var result1 = form.ShowPreDeleteDialogs_Exposed();
				AssertMultilineASCIIEquals("", @"You are about to delete this record permanently from the section config in following boards. Do you want to proceed?
First board
Second board", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.No, result1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // One for the "are you sure you want to delete" notification.
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // One for the "boards use this acceptability band" notification.

				var result2 = form.ShowPreDeleteDialogs_Exposed();
				AssertMultilineASCIIEquals("", @"You are about to delete this record permanently from the section config in following boards. Do you want to proceed?
First board
Second board", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.Yes, result2);
			}
		}

		#region Test Implementation

		class AcceptabilityBandForm_ForTest : AcceptabilityBandForm
		{
			public AcceptabilityBandForm_ForTest(BMComponentAcceptabilityBand acceptabilityBand)
				: base(acceptabilityBand)
			{
			}

			public ContinueWithDelete ShowPreDeleteDialogs_Exposed()
			{
				return ShowPreDeleteDialogs();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			var rule = band.FilterRule;
			Factory.Save();
			return new AcceptabilityBandForm(band);
		}

		#endregion
	}
}
