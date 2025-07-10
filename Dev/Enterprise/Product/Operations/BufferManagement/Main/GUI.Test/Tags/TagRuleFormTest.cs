using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TagRuleForm))]
	class TagRuleFormTest : ZFormBasherTest
	{
		#region Splitter Index

		protected override void TestSplitterIndexCore()
		{
			var strategy = new TaskTrackingAsyncStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				base.TestSplitterIndexCore();
				strategy.AwaitAll(taskToIgnore: null);
			}
		}

		#endregion

		#region FormCaption

		public void TestFormCaption()
		{
			var rule = Factory.New<TagRule>();
			rule.TGR_Name = "TestEstuary";

			using (var form = new TagRuleForm(rule))
			{
				AssertEquals("Tag Rule - TestEstuary", form.FormCaption);
			}
		}

		#endregion

		#region System-defined Tag Rules

		public void TestSystemFilterStripControlReadOnly()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");

			var rule = Factory.NewWithValidTestData<TagRule>();
			var template = rule.TagTemplate;
			template.TGL_TGM_Magnitude = tagMagnitude1.PK;
			rule.TGR_IsSystem = true;
			var filter = rule.Filter;

			Factory.Save();

			var loadedRule = Factory.CreateNewFactory().Load<TagRule>(rule.PK);

			using (var form = new TagRuleForm(loadedRule))
			{
				form.Show();
				Application.DoEvents();

				var wrapperControl = form.FindAll<FilterRuleFilterStripControl>().Single();
				AssertEquals(true, wrapperControl.ReadOnly);
			}

			loadedRule.Filter.ReadOnly = false;

			using (var form = new TagRuleForm(loadedRule))
			{
				form.Show();
				Application.DoEvents();

				var wrapperControl = form.FindAll<FilterRuleFilterStripControl>().Single();
				AssertEquals(false, wrapperControl.ReadOnly);
			}
		}

		public void TestSystemDefinedTagRule_EditForm_ShouldNotMakeScheduleReadOnly()
		{
			var tagRule = Factory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.TGR_IsSystem, true));
			AssertScheduleControlReadOnly(tagRule, c => c.ShowEditForm(tagRule), false);
		}

		public void TestSystemDefinedTagRule_ViewForm_ShouldMakeScheduleReadOnly()
		{
			var tagRule = Factory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.TGR_IsSystem, true));
			AssertScheduleControlReadOnly(tagRule, c => c.ShowViewForm(tagRule), true);
		}

		public void TestNonSystemDefinedTagRule_EditForm_ShouldHaveNonReadOnlySchedule()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(definition, "Ma1");
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			Factory.Save();

			AssertScheduleControlReadOnly(tagRule, c => c.ShowEditForm(tagRule), false);
		}

		public void TestNonSystemDefinedTagRule_ViewForm_ShouldMakeScheduleReadOnly()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(definition, "Ma1");
			var tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			Factory.Save();

			AssertScheduleControlReadOnly(tagRule, c => c.ShowViewForm(tagRule), true);
		}

		static void AssertScheduleControlReadOnly(TagRule tagRule, Func<ZController, IZForm> showFormFunc, bool expectReadOnly)
		{
			var controller = ZControllerFactory.Instance.GetControllerForBizo(tagRule);
			using (var form = (TagRuleForm)showFormFunc(controller))
			{
				var tabControl = form.FindAll<ZTemplateTabControl>().Single();
				var scheduleTab = tabControl.TabPages.OfType<ZTabPage>().Single(t => t.Text == "Schedule");
				tabControl.SelectedTab = scheduleTab;

				var scheduleControl = scheduleTab.FindSingle<ScheduleTaskRecurrenceControl>();
				AssertEquals(expectReadOnly, scheduleControl.GetReadOnly());
			}
		}

		#endregion

		#region Filters

		public void TestFilterSameBizoOnSave()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMagAndTemplate(Factory);
			var filter = rule.Filter;
			Factory.Save();

			using (var form = new TagRuleForm(rule))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				AssertEquals(filter.PK, rule.Filter.PK);
			}
		}

		public void TestFilter_ReferencesPersistedFilter_ChangesAsPersistedFilterIsChanged()
		{
			var tagRule = BMSTestHelper.CreateTagRuleWithDefAndMagAndTemplate(Factory);
			var jobWorkflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobWorkflow1");
			var jobWorkflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobWorkflow2");
			var babbyWorkflow = BMSTestHelper.CreateWorkflow(jobWorkflow1, "basically I'm very smol");
			var ruleFilter = tagRule.Filter;

			Factory.Save();

			var nestedLayoutName = "filterForJobWorkflow1";
			var outerLayoutName = "layoutWithFilterForJobWorkflow1";

			StmModuleFilter nestedLayout;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var strip = module.FilterBusinessObject.AddFilterStrip<JobCodeFilter>(ProcessHeader.ModuleFilterConstants.JobCode);
				strip.IsActive = true;
				strip.WorkflowTypeCode = "ORG";
				strip.Property = CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)jobWorkflow1.Parent);
				nestedLayout = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, nestedLayoutName, true, true, true);
			}

			StmModuleFilter outerLayout;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var innerLayout = module.FilterBusinessObject.AddFilterStrip<ModuleUserDefinedFilter>("[USR]" + nestedLayout.S9_FilterName);

				var uniqueStrip = module.FilterBusinessObject.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				uniqueStrip.SetJobOnly();

				outerLayout = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, outerLayoutName, true, true, true);
			}

			AssertNoErrors(nestedLayout);
			AssertNoErrors(outerLayout);

			Factory.Save();

			FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(ruleFilter, "[USR]" + outerLayoutName);

			Factory.Save();

			AssertPreviewWindowContents(new[] { jobWorkflow1.PK }, tagRule);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var strip = module.FilterBusinessObject.AddFilterStrip<JobCodeFilter>(ProcessHeader.ModuleFilterConstants.JobCode);
				strip.IsActive = true;
				strip.WorkflowTypeCode = "ORG";
				strip.Property = CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)jobWorkflow2.Parent);
				nestedLayout = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, nestedLayoutName, true, true, true);
			}

			Factory.Save();

			AssertPreviewWindowContents(new[] { jobWorkflow2.PK }, tagRule);
		}

		public void TestPreviewBeforeTagDefined_ShouldNotThrowException()
		{
			var rule = Factory.New<TagRule>();

			using (var form = new TagRuleForm(rule))
			{
				form.Show();

				var taskFilterControl = form.FindAll<BMFilterStripWrapperControl>().Single();

				ZFormModaliser.ShowDialogsInTest = true;
				var toolStrip = taskFilterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 11, 15, 0, 0, 0)] // DST in Sydney, not Brisbane
		public void TestPreviewWhenBranchDepartmentSpecifiedOnTag_Brisbane()
		{
			TestDateAttribute.UseUNLOCO = true;

			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Desc = "Jony";
			dept.GE_Code = "DEP";

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Won", null);
			workflow1.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 23, 0, 0); // 15th NOV 9am BNE/15th NOV 10am SYD -- 14th NOV 11pm UTC
			var w1Tag = workflow1.AddTag(tagMag).Link;
			w1Tag.TGL_Magnitude = 1m;

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Too", null);
			workflow2.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 13, 0, 0); // 14th NOV 11pm BNE/15th NOV 12am SYD -- 14th NOV 1pm UTC
			var w2Tag = workflow2.AddTag(tagMag).Link;
			w2Tag.TGL_Magnitude = 1m;

			var rule = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 2m;
			rule.TGR_GB_Branch = branchBNE.PK;
			rule.TGR_GE_Department = dept.PK;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today,
			});

			Factory.Save();

			AssertPreviewWindowContents(new[] { workflow1.PK }, rule);
		}

		[TestDate(2019, 11, 15, 0, 0, 0)] // DST in Sydney, not Brisbane
		public void TestPreviewWhenBranchDepartmentSpecifiedOnTag_Sydney()
		{
			TestDateAttribute.UseUNLOCO = true;

			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Desc = "Jony";
			dept.GE_Code = "DEP";

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Won", null);
			workflow1.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 23, 0, 0); // 15th NOV 9am BNE/15th NOV 10am SYD -- 14th NOV 11pm UTC
			var w1Tag = workflow1.AddTag(tagMag).Link;
			w1Tag.TGL_Magnitude = 1m;

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Too", null);
			workflow2.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 13, 0, 0); // 14th NOV 11pm BNE/15th NOV 12am SYD -- 14th NOV 1pm UTC
			var w2Tag = workflow2.AddTag(tagMag).Link;
			w2Tag.TGL_Magnitude = 1m;

			var rule = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 2m;
			rule.TGR_GB_Branch = branchSYD.PK;
			rule.TGR_GE_Department = dept.PK;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today,
			});

			Factory.Save();

			AssertPreviewWindowContents(new[] { workflow1.PK, workflow2.PK }, rule);
		}

		public void TestPreview_ForAddAndRemoveRule_ShouldShowResultsDependingOnWhichPreviewWasClicked()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "BBB");
			var rule = BMSTestHelper.CreateTagRule(mag, "Sweet sweet can", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, "Completion Statement", "A");

			var workflowA = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "A");
			var workflowB = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "B");
			var tagB = workflowB.AddTag(mag).Link;

			Factory.Save();
			var controller = ZControllerFactory.Instance.GetControllerForBizo(rule);

			using (var form = (Form)controller.ShowEditForm(rule))
			{
				Application.DoEvents();

				var control = form.FindSingle<FilterRuleFilterStripControl>();
				var toolStrip = control.FindSingle<ZToolStrip>(x => x.Name == "ToolStrip");
				var previewButton = (ZToolStripDropDownButton)toolStrip.Items["ToolStripPreviewDropButton"];
				AssertEquals(true, previewButton.ShowDropDownArrow);

				var addButton = previewButton.DropDownItems[0];
				AssertEquals("Add Tags - Items to which tags will be added", addButton.Text);

				var result = new List<ProcessHeader>();
				ZFormModaliser.ShowDialogsInTest = true;
				EmbeddedModuleTestHelper.SetListToStoreSearchResultsWhenPopupShown(result);

				addButton.PerformClick();
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder(new[] { "A" }, result.Select(x => x.FH_CompletionStatement));

				var removeButton = previewButton.DropDownItems[1];
				AssertEquals("Remove Tags - Items from which tags will be removed", removeButton.Text);

				removeButton.PerformClick();
				Application.DoEvents();
				AssertContainsExactElementsInAnyOrder(new[] { "B" }, result.Select(x => x.FH_CompletionStatement));
			}
		}

		public void TestPreview_ForNonAddAndRemoveRules_ShouldHaveNonDropDownPreviewButton()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.AddTag);
			Factory.Save();

			using (var form = new TagRuleForm(rule))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<FilterRuleFilterStripControl>();
				var toolStrip = control.FindSingle<ZToolStrip>(x => x.Name == "ToolStrip");
				var previewButton = (ZToolStripDropDownButton)toolStrip.Items["ToolStripPreviewDropButton"];
				AssertEquals("The preview button should only be a drop down for the AddAndRemove action type, and yet...", false, previewButton.ShowDropDownArrow);

				rule.TGR_ActionType = TagRuleActionTypeList.Codes.RemoveTag;
				Application.DoEvents();
				AssertEquals("The preview button should only be a drop down for the AddAndRemove action type, and yet...", false, previewButton.ShowDropDownArrow);

				rule.TGR_ActionType = TagRuleActionTypeList.Codes.AddAndRemoveTag;
				Application.DoEvents();
				AssertEquals("The preview button should only be a drop down for the AddAndRemove action type, and yet...", true, previewButton.ShowDropDownArrow);
				AssertEquals(2, previewButton.DropDownItems.Count);

				rule.TGR_ActionType = TagRuleActionTypeList.Codes.MaintainMagnitude;
				Application.DoEvents();
				AssertEquals("The preview button should only be a drop down for the AddAndRemove action type, and yet...", false, previewButton.ShowDropDownArrow);

				rule.TGR_ActionType = TagRuleActionTypeList.Codes.AddAndRemoveTag;
				Application.DoEvents();
				AssertEquals("The preview button should only be a drop down for the AddAndRemove action type, and yet...", true, previewButton.ShowDropDownArrow);
				AssertEquals("The menu items should not be duplicated, and yet...", 2, previewButton.DropDownItems.Count);
			}
		}

		public void TestFormResizedToMinimumSize_ShouldDisplayControlsCorrectly()
		{
			var rule = Factory.New<TagRule>();

			FilterStripsTestHelper.AddFilterStrips(rule.Filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Parent Job",
				FilterStripValueSetter = (f) =>
				{
					var filter = (ModuleGuidModuleSpecifiedFilter)f;
					filter.SelectedModule = ModuleIDs.WorkItem.Name;
					filter.SelectedFilters.AddTextFilterStrip("Summary", "Dr Zaius");
				},
				ComparisonOperatorSetter = (f) => ((ModuleGuidFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch
			});

			Size minimumSize;

			using (var form = new TagRuleForm(rule))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush(); // the description binding does not work without this or works intermittently; we need to do this before assert

				var filterStripControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				var findBox = filterStripControl.FindAll<ZGuidFindBox>().Single();
				var collectionBox = filterStripControl.FindAll<ZFilterCollectionFindBox>().Single();

				AssertEquals(false, findBox.Visible);
				AssertEquals(true, collectionBox.Visible);
				AssertEquals("1 filter applied", collectionBox.DescriptionBox.Text);

				minimumSize = form.MinimumSize;
				form.Size = minimumSize;
				Application.DoEvents();
				UserIdleWorker.Flush(); // the description binding does not work without this or works intermittently; we need to do this before assert

				filterStripControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				findBox = filterStripControl.FindAll<ZGuidFindBox>().Single();
				collectionBox = filterStripControl.FindAll<ZFilterCollectionFindBox>().Single();

				AssertEquals(false, findBox.Visible);
				AssertEquals(true, collectionBox.Visible);
				AssertEquals("1 filter applied", collectionBox.DescriptionBox.Text);

				form.Close();
			}

			using (var form = new TagRuleForm(rule) { Size = minimumSize })
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush(); // the description binding does not work without this or works intermittently; we need to do this before assert

				var filterStripControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				var findBox = filterStripControl.FindAll<ZGuidFindBox>().Single();
				var collectionBox = filterStripControl.FindAll<ZFilterCollectionFindBox>().Single();

				AssertEquals(false, findBox.Visible);
				AssertEquals(true, collectionBox.Visible);
				AssertEquals("1 filter applied", collectionBox.DescriptionBox.Text);
			}
		}

		public void TestShouldReloadParentJobFiltersWithFiltersMatchStripCorrectly()
		{
			var rule = Factory.New<TagRule>();

			FilterStripsTestHelper.AddFilterStrips(rule.Filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Parent Job",
				FilterStripValueSetter = (f) =>
				{
					var filter = (ModuleGuidModuleSpecifiedFilter)f;
					filter.SelectedModule = ModuleIDs.WorkItem.Name;
					filter.SelectedFilters.AddTextFilterStrip("Summary", "Dr Zaius");
				},
				ComparisonOperatorSetter = (f) => ((ModuleGuidFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch
			});

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedRule = newFactory.Load<TagRule>(rule.PK);

			using (var form = new TagRuleForm(loadedRule))
			{
				form.Show();
				Application.DoEvents();

				var filterStripControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				var collectionBox = filterStripControl.FindAll<ZFilterCollectionFindBox>().Single();

				UserIdleWorker.Flush(); // the description binding does not work without this or works intermittently; we need to do this before assert
				AssertEquals(1, filterStripControl.FilterStripsCount);
				AssertEquals("1 filter applied", collectionBox.DescriptionBox.Text);
			}
		}

		public void TestTagRuleUserDefinedFilter()
		{
			var userDefinedFilterName = "UDF for Tag Rule";
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.ProcessHeader, userDefinedFilterName, isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);

			var rule = Factory.New<TagRule>();

			using (var form = new TagRuleForm(rule))
			{
				form.Show();
				Application.DoEvents();

				var filterStripControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				var control = filterStripControl.FindAll<FilterRuleFilterStripControl>().Single();
				var moduleFilters = control.FilterBusinessObject.ModuleFilters;

				var userDefinedFilterInFiltersList = moduleFilters.FirstOrDefault(f => f.Code.Contains(userDefinedFilterName));
				AssertNotNull("User defined filter should appear in the list of filters", userDefinedFilterInFiltersList);
			}
		}

		public void TestFilterRuleValidation_ShouldWorkOnFilterSelectionPopups()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(rule.Filter, "Parent Job", filter =>
				{
					filter.SelectedModule = ModuleIDs.JobShipment.Name;
					var milestoneFilter = filter.SelectedFilters.AddFilterStrip<WorkflowModuleTextFilter>("Milestone Completed");
					milestoneFilter.EventReference = "A";
				},
				filter =>
				{
					filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				});

			Factory.Save();

			using (var form = new TagRuleForm(rule))
			{
				form.Show();
				Application.DoEvents();

				var filterStripControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				var collectionBox = filterStripControl.FindAll<ZFilterCollectionFindBox>().Single();
				var wasFormVisibleAfterClickingOk = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (FilterCollectionEmbeddedModulePopup)dialog;

					popup.Shown += (_, x_) =>
					{
						var filter = (WorkflowModuleTextFilter)popup.Module_ForTest.FilterBusinessObject.ActiveModuleFilters.Single();
						AssertHasError(filter.EventReferenceInfo, "This option cannot be used on filter rules for performance reasons.");

						popup.ExposedOKButtonForTesting.PerformClick();
						Application.DoEvents();

						wasFormVisibleAfterClickingOk = popup.Visible;
					};
				});

				collectionBox.PopupButton.PerformClick();
				Application.DoEvents();

				AssertEquals("There are errors. Please correct these before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not have closed because of the error.", true, wasFormVisibleAfterClickingOk);
			}
		}

		#endregion

		#region Implementation

		void AssertPreviewWindowContents(ZGuid[] expectedContent, TagRule tagRule)
		{
			using (var form = new TagRuleForm(tagRule))
			{
				form.Show();

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					if (o is EmbeddedModulePopup popupForm)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var taskFilterControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				var toolStrip = taskFilterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				form.Show();
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder(expectedContent, result.Select(x => x.PK).ToArray());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			return new TagRuleForm(rule);
		}

		#endregion
	}
}
