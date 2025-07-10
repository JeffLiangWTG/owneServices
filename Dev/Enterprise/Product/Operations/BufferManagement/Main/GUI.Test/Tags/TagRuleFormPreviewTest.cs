using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TagRuleFormPreviewTest : BMSTestCaseWithFactory
	{
		#region Preview - Can Contain Company Filter

		public void TestPreview_CanContainCompanyFilter_AddTag()
		{
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule N", TagRuleActionTypeList.Codes.AddTag);
			TagRuleRunnerCompanyFilterTest.SetFilterStrips(tagRule.Filter, moduleName: ModuleIDs.Customs.JobDeclaration.Name);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow N");

			Factory.Save();

			AssertPreview_CanContainCompanyFilter(tagRule);
		}

		public void TestPreview_CanContainCompanyFilter_RemoveTag()
		{
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule N", TagRuleActionTypeList.Codes.RemoveTag);
			TagRuleRunnerCompanyFilterTest.SetFilterStrips(tagRule.Filter, moduleName: ModuleIDs.Customs.JobDeclaration.Name);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow N");
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertPreview_CanContainCompanyFilter(tagRule);
		}

		public void TestPreview_CanContainCompanyFilter_MaintainMagnitude()
		{
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule N", TagRuleActionTypeList.Codes.MaintainMagnitude, 100m);
			TagRuleRunnerCompanyFilterTest.SetFilterStrips(tagRule.Filter, moduleName: ModuleIDs.Customs.JobDeclaration.Name);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow N");
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertPreview_CanContainCompanyFilter(tagRule);
		}

		public void TestPreview_CanContainCompanyFilter_AddAndRemoveTag_PreviewAdd()
		{
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule N", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			TagRuleRunnerCompanyFilterTest.SetFilterStrips(tagRule.Filter, moduleName: ModuleIDs.Customs.JobDeclaration.Name);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow N");

			Factory.Save();

			AssertPreview_CanContainCompanyFilter(tagRule, AddRemoveTagRulePreviewOptionsList.Codes.Add);
		}

		public void TestPreview_CanContainCompanyFilter_AddAndRemoveTag_PreviewRemove()
		{
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule N", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			TagRuleRunnerCompanyFilterTest.SetFilterStrips(tagRule.Filter, moduleName: ModuleIDs.Customs.JobDeclaration.Name, completionStatementFilter: "Workflow N (not exist)");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow N");
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertPreview_CanContainCompanyFilter(tagRule, AddRemoveTagRulePreviewOptionsList.Codes.Remove);
		}

		#endregion

		#region Preview - Can Contain Tag DefinitionCodeFilter

		public void TestPreview_CanContainTagDefinitionCodeFilter()
		{
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule", TagRuleActionTypeList.Codes.MaintainMagnitude);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "DF1", "DF1");
			var parameters = new ZSqlParameterCollection();

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidAppliedToSubCollectionFilter>(tagRule.Filter, ProcessHeader.ModuleFilterConstants.TagDefinitionCode, (filter) =>
			{
				filter.Property = tagMagnitude.Definition.PK;
			});

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow N");
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertPreview(tagRule, executedCommands =>
			{
				var tagDefinitionCodeJoinSQL =
@"					SELECT FH_PK
					FROM dbo.ProcessHeader
					JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId
					JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK";

				Assert("Should contains TagDefinitionCodeFilter SQL", TestConnection.ExecutedCommands.Any(sql => sql.Contains(tagDefinitionCodeJoinSQL)));
			});
		}

		#endregion

		#region Validation

		public void TestPreview_WhenFilterWithCountrySpecificModuleConifugred_AndModuleNotAvailableInRuleBranchContext_ShouldShowValidationErrorWindow_AndNotSearch_AndNotThrowException()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "DUM");

			Factory.Save();

			var rule = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "Bad Rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TGR_GB_Branch = branch.PK;

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(rule.Filter, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			using (var form = new TagRuleForm(rule))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
				{
					Fail("No dialogs should be shown because the exception should have been thrown/caught when building the filter. SAD!");
				});

				AssertNoExceptionThrown("Clicking preview when filters are invalid shouldn't throw uncaught exceptions. SAD!", () => ClickPreviewButton(form));
			}

			AssertEquals(@"Unable to build a database query with the provided filter strips in the context of the Branch and Department specified. Please ensure that any filter strip modules are available in the specified context.
Filter: Parent Job
Module: AirCargoOutturnBills",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Implementation

		void AssertPreview(TagRule tagRule, Action<IEnumerable<string>> assertExecutedCommands, string buttonType = null)
		{
			BMSRegistry.Instance.AllowCompanyFiltersInTagRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ZFormModaliser.ShowDialogsInTest = true;

			using (var form = new TagRuleForm(tagRule))
			{
				form.Show();

				var previewResult = new List<ProcessHeader>();
				EmbeddedModuleTestHelper.SetListToStoreSearchResultsWhenPopupShown(previewResult);

				using (TestConnection.TrackExecutedCommands())
				{
					ClickPreviewButton(form, buttonType);

					AssertEquals("Preview should show 1 result", 1, previewResult.Count);
					AssertEquals("Preview should show 'workflow N'", "Workflow N", previewResult.First().FH_CompletionStatement);
					assertExecutedCommands(TestConnection.ExecutedCommands);
				}
			}
		}

		void AssertPreview_CanContainCompanyFilter(TagRule tagRule, string buttonType = null)
		{
			BMSRegistry.Instance.AllowCompanyFiltersInTagRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertPreview(tagRule, executedCommands =>
			{
				TagRuleRunnerCompanyFilterTest.AssertCompanyFilter(
						"WHEN AllowCompanyFiltersInTagRules = false, previewing tag-rule with JobDeclaration filterstrip should have company filter",
						TestConnection.ExecutedCommands.Where(sql => !sql.Contains("SELECT TOP 1 * FROM (SELECT TOP 1 * FROM dbo.ProcessHeader)")),
						expectCompanyFilter: true);
			}, buttonType);
		}

		static void ClickPreviewButton(Form form, string buttonType = null)
		{
			var previewButton = (ZToolStripDropDownButton)GetPreviewButton(form);

			switch (buttonType)
			{
				case AddRemoveTagRulePreviewOptionsList.Codes.Add:
					var addButton = previewButton.DropDownItems[0];
					AssertEquals("Add Tags - Items to which tags will be added", addButton.Text);
					addButton.PerformClick();
					break;
				case AddRemoveTagRulePreviewOptionsList.Codes.Remove:
					var removeButton = previewButton.DropDownItems[1];
					AssertEquals("Remove Tags - Items from which tags will be removed", removeButton.Text);
					removeButton.PerformClick();
					break;
				default:
					previewButton.PerformClick();
					break;
			}
		}

		static ToolStripItem GetPreviewButton(Form form)
		{
			var taskFilterControl = form.FindAll<BMFilterStripWrapperControl>().Single();
			var toolStrip = taskFilterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");

			return toolStrip.Items["ToolStripPreviewDropButton"];
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "TAG");
			tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "TMG");

			Factory.Save();
		}

		TagMagnitude tagMagnitude;

		#endregion
	}
}
