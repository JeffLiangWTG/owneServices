using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using GlowIndexQueryService.Tests.Common;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMFilterRuleFilterBusinessObject))]
	class BMFilterRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCustomSQLFilter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var filterBizo = new BMFilterRuleFilterBusinessObject();
			var customSqlFilter = (ModuleSQLFilter)filterBizo["Custom SQL Filter"];
			var strip = filterBizo.FilterStrips.AddNew("Custom SQL Filter");
			var filter = (ModuleSQLFilter)strip.CurrentModuleFilter;
			filter.Property1 = "1=1";
			filter.IsActive = true;

			var layout = Factory.New<StmModuleFilter>();
			layout.S9_IsPublished = true;

			filterBizo.FillLayoutValues(layout, ModuleIDs.BMFilterRule);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("GIVEN user do not have permission to create custom-sql-filter", false, EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed);

				var loadedFilterBizO = new BMFilterRuleFilterBusinessObject();
				loadedFilterBizO.LoadFilterRuleLayout(layout);

				var moduleSQLFilter = loadedFilterBizO.ModuleFilters.OfType<ModuleSQLFilter>().First();
				AssertEquals("WHEN loading existing custom-sql-filter", "1=1", moduleSQLFilter.Property1);
				AssertEquals("THEN custom-sql-filter should be readonly", true, moduleSQLFilter.ReadOnly);
			}
		}

		public void TestCanSaveTwice()
		{
			var processHeader = Factory.NewWithValidTestData<ProcessHeader>();
			processHeader.FH_CompletionStatement = "Phyllis Willis Jillingham Spencer";
			Factory.Save();

			var bizo = new BMFilterRuleFilterBusinessObjectForTest();
			var customSqlFilter = (ModuleSQLFilter)bizo["Custom SQL Filter"];
			customSqlFilter.IsActive = true;
			customSqlFilter.Property1 = string.Format("{0} = 'Phyllis Willis Jillingham Spencer'", ProcessHeaderSchema.FH_CompletionStatement.Name);

			bizo.SavePreconfiguredLayout("Swag");
			AssertNoExceptionThrown(() => bizo.SavePreconfiguredLayout("Swag"));
		}

		public void TestModuleFilters_ShouldShowAllWorkflowsWhenActiveStatusIsAll()
		{
			var filterBizo = new BMFilterRuleFilterBusinessObject();
			AssertNotNull(filterBizo["Active Status"]);

			var activeHeader = BMSTestHelper.CreateWorkflow(Factory, "Active");
			var inactiveHeader = BMSTestHelper.CreateWorkflow(Factory, "Inactive");
			inactiveHeader.FH_IsActive = false;

			var filter = (ModuleTextFilter)filterBizo["Active Status"];
			filter.Property = "All";
			filter.IsActive = true;

			Factory.Save();

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(activeHeader, result);
			AssertCollectionContains(inactiveHeader, result);
		}

		public void TestModuleFilters_ShouldShowOnlyActiveWorkflowsWhenActiveStatusIsActive()
		{
			var filterBizo = new BMFilterRuleFilterBusinessObject();
			AssertNotNull(filterBizo["Active Status"]);

			var activeHeader = BMSTestHelper.CreateWorkflow(Factory, "Active");
			var inactiveHeader = BMSTestHelper.CreateWorkflow(Factory, "Inactive");
			inactiveHeader.FH_IsActive = false;

			var filter = (ModuleTextFilter)filterBizo["Active Status"];
			filter.Property = "Active";
			filter.IsActive = true;

			Factory.Save();

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(activeHeader, result);
			AssertCollectionNotContains(inactiveHeader, result);
		}

		public void TestModuleFilters_ShouldShowOnlyInactiveWorkflowsWhenActiveStatusIsInactive()
		{
			var filterBizo = new BMFilterRuleFilterBusinessObject();
			AssertNotNull(filterBizo["Active Status"]);

			var activeHeader = BMSTestHelper.CreateWorkflow(Factory, "Active");
			var inactiveHeader = BMSTestHelper.CreateWorkflow(Factory, "Inactive");
			inactiveHeader.FH_IsActive = false;

			var filter = (ModuleTextFilter)filterBizo["Active Status"];
			filter.Property = "Inactive";
			filter.IsActive = true;

			Factory.Save();

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(activeHeader, result);
			AssertCollectionContains(inactiveHeader, result);
		}

		public void TestModuleFilters_ActiveStatusFilterShouldNotBeAppliedByDefault()
		{
			var filterBizo = new BMFilterRuleFilterBusinessObject();
			AssertNotNull(filterBizo["Active Status"]);

			var activeHeader = BMSTestHelper.CreateWorkflow(Factory, "Active");
			var inactiveHeader = BMSTestHelper.CreateWorkflow(Factory, "Inactive");
			inactiveHeader.FH_IsActive = false;

			Factory.Save();

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(activeHeader, result);
			AssertCollectionContains(inactiveHeader, result);
		}

		public void TestFilterWithOrGroups_ShouldCreateUniqueParameterNamesForFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, true, "Job Header");
			var workflow = jobHeader.ProcessHeaders.Single();
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagAdded = BMSTestHelper.CreateTagMagnitude(tagDef, "ADD");
			var tagOptional = BMSTestHelper.CreateTagMagnitude(tagDef, "OPT");
			var tagNotAdded = BMSTestHelper.CreateTagMagnitude(tagDef, "NOT");
			jobHeader.AddTag(tagAdded);

			Factory.Save();

			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var strip = processHeaderBizo.FilterStrips.AddNew("Tag Magnitude");
			strip.OrCategory = FilterOrCategory.Red;
			var filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			filter.Property = tagAdded.PK;
			filter.OrCategory = FilterOrCategory.Red;
			filter.IsActive = true;

			strip = processHeaderBizo.FilterStrips.AddNew("Tag Magnitude");
			strip.OrCategory = FilterOrCategory.None;
			filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			filter.Property = tagNotAdded.PK;
			filter.IsActive = true;

			strip = processHeaderBizo.FilterStrips.AddNew("Tag Magnitude");
			strip.OrCategory = FilterOrCategory.Red;
			filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			filter.Property = tagOptional.PK;
			filter.OrCategory = FilterOrCategory.Red;
			filter.IsActive = true;

			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;

			var ruleBizo = new BMFilterRuleFilterBusinessObject();
			ruleBizo.LoadLayout(layout);
			var ruleFilter = ruleBizo.Filter;

			var processHeaderSql = processHeaderFilter.LiteralTextSqlFormatted;
			AssertEquals("tagAdded should have been included in the query, and yet... " + processHeaderSql.Replace("\t", "    "), true, processHeaderSql.Contains(tagAdded.PK.ToString()));
			AssertEquals("tagOptional should have been included in the query, and yet... " + processHeaderSql.Replace("\t", "    "), true, processHeaderSql.Contains(tagOptional.PK.ToString()));
			AssertEquals("tagNotAdded should have been included in the query, and yet... " + processHeaderSql.Replace("\t", "    "), true, processHeaderSql.Contains(tagNotAdded.PK.ToString()));

			var ruleBizoSql = ruleFilter.LiteralTextSqlFormatted;
			AssertEquals("tagAdded should have been included in the query, and yet... " + ruleBizoSql.Replace("\t", "    "), true, ruleBizoSql.Contains(tagAdded.PK.ToString()));
			AssertEquals("tagOptional should have been included in the query, and yet... " + ruleBizoSql.Replace("\t", "    "), true, ruleBizoSql.Contains(tagOptional.PK.ToString()));
			AssertEquals("tagNotAdded should have been included in the query, and yet... " + ruleBizoSql.Replace("\t", "    "), true, ruleBizoSql.Contains(tagNotAdded.PK.ToString()));

			var processHeaderResult = Factory.Load<ProcessHeader>(processHeaderFilter);
			var ruleResult = Factory.Load<ProcessHeader>(ruleFilter);

			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, processHeaderResult);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, ruleResult);
		}

		public void TestFormWithMultipleFilterControls_ShouldAddPreviewFiltersForCorrectControl()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "Bizo 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(Factory, "Bizo 2");
			Factory.Save();

			var dataSource = Factory.New<BusinesObjectForMultipleFilterControlForm>();
			using (var form = new FormWithMultipleFilterControls(dataSource))
			{
				form.Show();
				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(o =>
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

				var toolStrip = form.Control1.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Filter from Control1 should have been used because its preview button was pressed, and yet...",
					new[] { workflow1.FH_CompletionStatement }, result.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

				toolStrip = form.Control2.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Filter from Control2 should have been used because its preview button was pressed, and yet...",
					new[] { workflow2.FH_CompletionStatement }, result.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		#region Filter Business Object For Test

		class BMFilterRuleFilterBusinessObjectForTest : BMFilterRuleFilterBusinessObject
		{
			public StmModuleFilter SavePreconfiguredLayout(ZString layoutName, bool global = false)
			{
				return new DataGridLayoutManager().SavePreconfiguredLayout(this, layoutName, false, global, SaveColumnLayout.Ignore);
			}
		}

		#endregion

		public void TestFilterHasSqlFilter()
		{
			var processHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			processHeader.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			Factory.Save();

			var bizo = new BMFilterRuleFilterBusinessObject();
			var customSqlFilter = (ModuleSQLFilter)bizo["Custom SQL Filter"];
			customSqlFilter.IsActive = true;

			customSqlFilter.Property1 = string.Format("{0} = '{1}'", ProcessHeaderSchema.FH_DateAcceptability.Name, DateAcceptabilityList.Codes.ExtendedStartExtendedFinish);
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
		}

		public void TestUserDefinedFilters_ContainsProcessHeaderUserDefinedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.ProcessHeader, "From dbo.ProcessHeader user-defined", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.ProcessHeader, "From dbo.ProcessHeader normal layout", true, false, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.BMFilterRule))
			{
				var filter = module.FilterBusinessObject["[USR]From dbo.ProcessHeader user-defined"];
				AssertNotNull("User-defined filters should be shared between ProcessHeader and BMFilterRule modules, and yet...", filter);

				filter = module.FilterBusinessObject["[USR]From dbo.ProcessHeader normal layout"];
				AssertNull("Normal layouts from dbo.ProcessHeader should not be added as user-defined filters, and yet...", filter);

				filter = module.FilterBusinessObject["From dbo.ProcessHeader normal layout"];
				AssertNull("Normal layouts from dbo.ProcessHeader should not be added as user-defined filters, and yet...", filter);
			}
		}

		#region Index Filters

		public void Test_IndexFilter()
		{
			BMSRegistry.Instance.UseGlowIndexingForTagRuleFilters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var header = BMSTestHelper.CreateWorkflow(Factory, "lalala");

			var mock = new MockGlowIndexQuerySearchEngine();
			mock.GlowEntityTypes = new HashSet<string> { "IProcessHeader" };
			mock.SearchField = new SearchFieldCollection("IProcessHeader", new[] { new SearchField("ACTIVESTATUS", "Active Status", typeof(bool)) });
			mock.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{
					new GlowIndexQueryResult(header.PK.ToString(), "IProcessHeader")
				},
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(mock))
			{
				var filterBizo = new BMFilterRuleFilterBusinessObject(true);
				AssertNotNull(filterBizo["ACTIVESTATUS"]);

				var filter = (IndexSearchModuleFlagsFilter)filterBizo["ACTIVESTATUS"];
				filter.Property0 = true;
				filter.IsActive = true;

				Factory.Save();

				var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
				AssertCollectionContains(header, result);
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMFilterRuleFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.UseGlowIndexingForTagRuleFilters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		class FormWithMultipleFilterControls : ZForm, IFilterPreviewableWithSubObject
		{
			public BMFilterStripWrapperControl Control1;
			public BMFilterStripWrapperControl Control2;

			readonly BusinesObjectForMultipleFilterControlForm bizo1;
			readonly BusinesObjectForMultipleFilterControlForm bizo2;

			public FormWithMultipleFilterControls(BusinesObjectForMultipleFilterControlForm dataSource)
				: base(dataSource)
			{
				InitializeComponent();
				Size = ControlDpiScalingHelper.NewScaledSize(1000, 800);
				bizo1 = dataSource.Factory.New<BusinesObjectForMultipleFilterControlForm>();
				bizo1.Name = "Bizo 1";
				bizo2 = dataSource.Factory.New<BusinesObjectForMultipleFilterControlForm>();
				bizo2.Name = "Bizo 2";
				PerformLayout();
			}

			new void InitializeComponent()
			{
				((System.ComponentModel.ISupportInitialize)(MessageStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(ErrorStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(BindingSource)).BeginInit();
				SuspendLayout();
				MainStatusBar.Location = ControlDpiScalingHelper.NewScaledPoint(0, 255);
				MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(300, 24);
				Control1 = new BMFilterStripWrapperControl()
				{
					FilterControlIdentifier = "Control 1",
					Name = "Control 1",
					IsPreviewAllowed = true,
					Location = ControlDpiScalingHelper.NewScaledPoint(3, 3)
				};
				Controls.Add(Control1);
				BindingSource.SetBindingMember(Control1, "FilterRule");
				Control2 = new BMFilterStripWrapperControl()
				{
					FilterControlIdentifier = "Control 2",
					Name = "Control 2",
					IsPreviewAllowed = true,
					Location = ControlDpiScalingHelper.NewScaledPoint(3, 100)
				};
				Controls.Add(Control2);
				BindingSource.SetBindingMember(Control2, "FilterRule");
				AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
				ClientSize = ControlDpiScalingHelper.NewScaledSize(300, 279);
				Name = "Form1";
				Text = "Form1";
				((System.ComponentModel.ISupportInitialize)(MessageStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(ErrorStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(BindingSource)).EndInit();
				ResumeLayout(false);
				PerformLayout();
			}

			public BusinessObject GetObjectForPreview(string filterControlIdentifier)
			{
				switch (filterControlIdentifier)
				{
					case "Control 1":
						return bizo1;
					case "Control 2":
						return bizo2;
					default:
						return null;
				}
			}
		}

		class BusinesObjectForMultipleFilterControlForm : DummyBusinessObject, IFilterPreviewable
		{
			public string Name { get; set; }

			public StmModuleFilter FilterRule
			{
				get
				{
					if (filter == null || filter.IsDeleted)
					{
						filter = Factory.NewWithValidTestData<StmModuleFilter>();
						filter.S9_ModuleID = ModuleIDs.BMFilterRule.Name;

						RegisterEditableChildObject(filter);
					}

					return filter;
				}
			}
			StmModuleFilter filter;

			public ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode)
			{
				var query = new ZQuery();
				query.AddToFilter(ProcessHeaderSchema.FH_CompletionStatement, Name);
				return query;
			}

			public BusinesObjectForMultipleFilterControlForm(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion
	}
}
