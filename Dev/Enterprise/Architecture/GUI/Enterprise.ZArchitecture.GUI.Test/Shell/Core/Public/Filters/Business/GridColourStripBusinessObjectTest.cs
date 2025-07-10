using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.StmModuleFilter;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(GridColourStripBusinessObject))]
	public class GridColourStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestShouldAddSystemDefinedStatusFilterProperty()
		{
			var scheme = Factory.NewWithValidTestData<GridColourScheme>();
			var parentFilterBizo = new FilterStripBusinessObjectForTest();
			parentFilterBizo.ShouldAddSystemDefinedStatusFilterForTest = true;
			var filterBizo = new GridColourStripBusinessObject(parentFilterBizo, scheme, typeof(DummyBusinessObject));
			Assert(filterBizo.ShouldAddSystemDefinedStatusFilter);

			parentFilterBizo.ShouldAddSystemDefinedStatusFilterForTest = false;
			Assert(!filterBizo.ShouldAddSystemDefinedStatusFilter);
		}

		public void TestValidateRuleName_QueryPerformance()
		{
			var scheme = Factory.NewWithValidTestData<GridColourScheme>();
			var parentFilterBizo = new DummyFilterBusinessObject();
			var filterBizo = new GridColourStripBusinessObject(parentFilterBizo, scheme, typeof(DummyBusinessObject));

			using (Db.Connection.TrackExecutedCommands())
			{
				filterBizo.RuleName = "Squanch";
				var matchingCommands = Db.Connection.ExecutedCommands.Where(x => x.Contains("FROM dbo.StmModuleFilter")).ToArray();
				AssertEquals(1, matchingCommands.Length);
				AssertContains("It's important to add the filter type clause so that the correct index is used. SAD!", "S9_FilterType <> 'FRU'", matchingCommands.Single());
			}
		}

		public void TestClone_ShouldCopyFilterData()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "module1";

			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var scheme = Factory.NewWithValidTestData<GridColourScheme>();
				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, scheme, typeof(DummyLogged));
				var clone = (GridColourStripBusinessObject)gridColorStrip.Clone();
				AssertEquals("colourScheme", gridColorStrip.colourScheme, clone.colourScheme);
				AssertSame("ParentFilterStrip", gridColorStrip.ParentFilterStrip, clone.ParentFilterStrip);
				AssertEquals("LayoutContext", ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext, ((IFilterStripBusinessObjectInternals)clone).LayoutContext);
				AssertEquals("QueryObjectType", gridColorStrip.QueryObjectType, clone.QueryObjectType);
			}
		}

		public void TestResetModuleFiltersHasFilterModuleStrategies()
		{
			var scheme = Factory.NewWithValidTestData<GridColourScheme>();
			var parentFilterBizo = new DummyFilterBusinessObject();

			var strategy = new FilterModuleStrategyForTest();
			var strategyList = new List<FilterModuleStrategy>();
			strategyList.Add(strategy);

			using (ObjectFactory.Substitute("FilterModuleStrategies", strategyList))
			{
				var filterBizo = new GridColourStripBusinessObject(parentFilterBizo, scheme, typeof(DummyBusinessObject));

				AssertEquals("Strategy contained initially", "Z0_Description (System)", filterBizo.ModuleFilters["Z0_Description" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);
				filterBizo.ResetModuleFilters();
				AssertEquals("Strategy not added after reset", "Z0_Description (System)", filterBizo.ModuleFilters["Z0_Description" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GridColourStripBusinessObject(new FilterStripBusinessObjectForTest(), null, typeof(StmData));
		}

		#region Test Internals

		public void TestCtorWithNoMissingMethodException()
		{
			var filterStrip = new FilterStripBusinessObjectForTestWithParamlessConstructor(null);
			AssertExceptionThrown<ArgumentException>(() => new GridColourStripBusinessObject(filterStrip, null, null, true));
		}

		public void TestHasCustomSql()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "module1";

			using (var filterControl1 = new FilterControlForTest(filterStrip))
			{
				var gridColorStrip1 = new GridColourStripBusinessObject(filterControl1.FilterBusinessObject, null, typeof(DummyLogged), true);
				gridColorStrip1.RuleName = "rule1";
				Assert(gridColorStrip1.HasCustomSqlFilter);
			}
		}

		public void TestStripUsesANotherInstanceOfParentFilterStripSoTheDelegateDoesntCallIntoTheParentsStrip()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "module1";

			using (var filterControl1 = new FilterControlForTest(filterStrip))
			{
				var gridColorStrip1 = new GridColourStripBusinessObject(filterControl1.FilterBusinessObject, null, typeof(DummyLogged), true);
				gridColorStrip1.RuleName = "rule1";

				AssertEquals("module1" + GridColourFactory.ColorStripCode, ((IFilterStripBusinessObjectInternals)gridColorStrip1).LayoutContext);
				AssertEquals("should use another instance so the delegates don't mix", true, filterStrip != gridColorStrip1.ParentFilterStrip);
				AssertEquals("Should use same factory as parent", filterStrip.Factory, gridColorStrip1.Factory);

				var gridColorStrip2 = new GridColourStripBusinessObject(filterControl1.FilterBusinessObject, null, typeof(DummyLogged), false);
				gridColorStrip2.RuleName = "rule2";

				AssertEquals("module1" + GridColourFactory.ColorStripCode, ((IFilterStripBusinessObjectInternals)gridColorStrip2).LayoutContext);
				AssertEquals("should use same instance for optimization", true, filterStrip == gridColorStrip2.ParentFilterStrip);
				AssertEquals("Should use same factory as parent", filterStrip.Factory, gridColorStrip2.Factory);
			}
		}

		public void TestFindLayout()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "module1";

			using (var filterControl1 = new FilterControlForTest(filterStrip))
			{
				var gridColorStrip1 = new GridColourStripBusinessObject(filterControl1.FilterBusinessObject, null, typeof(DummyLogged));
				gridColorStrip1.RuleName = "rule1";

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "rule1";
				filter1.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip1).LayoutContext;
				filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;

				var filter1a = Factory.New<StmModuleFilter>();
				filter1a.S9_FilterName = "rule1a";
				filter1a.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip1).LayoutContext;
				filter1a.S9_RelatedEntityID = gridColorStrip1.LayoutsHelper.CurrentUserPk;

				var filter2 = Factory.New<StmModuleFilter>();
				filter2.S9_FilterName = "rule2";
				filter2.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip1).LayoutContext;
				filter2.S9_IsPublished = true;

				var filter3 = Factory.New<StmModuleFilter>();
				filter3.S9_FilterName = "rule3";
				filter3.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip1).LayoutContext;
				filter3.S9_IsSystem = true;

				Factory.Save();

				AssertNull(gridColorStrip1.FindLayout("rule1", false));
				AssertNotNull(gridColorStrip1.FindLayout(filter1.PK.ToString(), false));

				AssertNotNull(gridColorStrip1.FindLayout("rule1a", false));
				AssertNotNull(gridColorStrip1.FindLayout(filter1a.PK.ToString(), false));

				AssertNotNull(gridColorStrip1.FindLayout("rule2", true));
				AssertNotNull(gridColorStrip1.FindLayout(filter2.PK.ToString(), true));

				AssertNotNull(gridColorStrip1.FindLayout("rule3", true));
				AssertNotNull(gridColorStrip1.FindLayout(filter3.PK.ToString(), true));
			}
		}

		public void TestFindLayout_Multilingual()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.Constants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				var filterStrip = new FilterStripBusinessObjectForTest();
				((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "module";

				using (var filterControl = new FilterControlForTest(filterStrip))
				{
					var ruleName = "rule";
					var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, null, typeof(DummyLogged));
					gridColorStrip.RuleName = ruleName;

					var filter = Factory.New<StmModuleFilter>();
					filter.S9_FilterName = ruleName;
					filter.S9_IsSystem = true;
					filter.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;

					Factory.Save();

					var key = ((ResourceString)filter.S9_FilterNameMultilingual).ResourceKey;
					var ruleNameInChineseSimplified = "rule (Chinese - Simplified)";
					mockRes.Put(key, new ResourceStringData(key, ruleNameInChineseSimplified));

					gridColorStrip.FindLayout(ruleName, false);
					AssertEquals(ruleNameInChineseSimplified, gridColorStrip.RuleName);
				}
			}
		}

		public void TestFindLayoutWithColourScheme()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "module1";

			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var colourScheme = Factory.New<GridColourScheme>();
				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, colourScheme, typeof(DummyLogged));
				gridColorStrip.RuleName = "rule1";

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "rule1";
				filter1.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;
				filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;

				var filter1a = Factory.New<StmModuleFilter>();
				filter1a.S9_FilterName = "rule1a";
				filter1a.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;
				filter1a.S9_RelatedEntityID = ZGuid.Empty;

				var filter1b = Factory.New<StmModuleFilter>();
				filter1b.S9_FilterName = "rule1b";
				filter1b.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;
				filter1b.S9_RelatedEntityID = gridColorStrip.LayoutsHelper.CurrentUserPk;

				var filter2 = Factory.New<StmModuleFilter>();
				filter2.S9_FilterName = "rule2";
				filter2.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;
				filter2.S9_IsPublished = true;

				var filter3 = Factory.New<StmModuleFilter>();
				filter3.S9_FilterName = "rule3";
				filter3.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;
				filter3.S9_IsSystem = true;

				Factory.Save();

				AssertNull(gridColorStrip.FindLayout("rule1", false));
				AssertNotNull(gridColorStrip.FindLayout(filter1.PK.ToString(), false));

				AssertNull(gridColorStrip.FindLayout("rule1a", false));
				AssertNotNull(gridColorStrip.FindLayout(filter1a.PK.ToString(), false));

				AssertNotNull(gridColorStrip.FindLayout("rule1b", false));
				AssertNotNull(gridColorStrip.FindLayout(filter1b.PK.ToString(), false));

				AssertNull(gridColorStrip.FindLayout("rule2", true));
				AssertNotNull(gridColorStrip.FindLayout(filter2.PK.ToString(), true));

				AssertNull(gridColorStrip.FindLayout("rule3", false));
				AssertNotNull(gridColorStrip.FindLayout(filter3.PK.ToString(), false));
			}
		}

		public void TestFindLayoutForRuleWithSameNameAsScheme()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "TestModule";
			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var colourScheme = Factory.New<GridColourScheme>();
				colourScheme.S9_FilterName = "TestName";

				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, colourScheme, typeof(DummyLogged)) { RuleName = "TestName" };

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "TestName";
				filter1.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip).LayoutContext;
				filter1.S9_RelatedEntityID = colourScheme.PK;

				Factory.Save();

				var filter2 = gridColorStrip.FindLayout("TestName", false);
				AssertNotNull(filter2);
				AssertEquals(filter1.PK, filter2.PK);
			}
		}

		public void TestGetCorrectLayoutForRuleAfterSaved()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "TestModule";
			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var colourScheme = filterStrip.Factory.New<GridColourScheme>();
				colourScheme.S9_FilterName = "TestScheme";
				colourScheme.S9_ModuleID = ModuleIdSuffix.GridColorScheme;
				colourScheme.S9_IsPublished = true;
				colourScheme.PublishAcrossAllCompanies = true;
				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, colourScheme, typeof(DummyLogged)) { RuleName = "TestName" };
				var strip = gridColorStrip.FilterStrips.AddNew();
				strip.FilterDescription = "firstFilter";
				colourScheme.ColourStrips.Add(gridColorStrip);

				gridColorStrip.SaveLayout(gridColorStrip.RuleName, true);

				var factory2 = new BusinessObjectFactory();
				var colourScheme2 = factory2.Load<GridColourScheme>(colourScheme.PK);
				colourScheme2.S9_IsPublished = false;
				colourScheme2.PublishAcrossAllCompanies = false;
				gridColorStrip.FilterStrips[0].FilterDescription = "filterThatShouldBeAdded";
				gridColorStrip.SaveLayout(gridColorStrip.RuleName, false);
				factory2.Save();

				var factory3 = new BusinessObjectFactory();
				var colourScheme3 = factory3.Load<GridColourScheme>(colourScheme.PK);
				colourScheme3.SetStripsFromFilter(filterStrip, typeof(BusinessObject));

				AssertEquals("Get changed layout", "filterThatShouldBeAdded", colourScheme3.ColourStrips[0].FilterStrips[0].FilterDescription);
			}
		}

		public void TestValidateRuleName()
		{
			var filterStrip1 = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip1).LayoutContext = "module1";

			var filterStrip2 = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip2).LayoutContext = "module2";

			var filterStrip3 = new FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)filterStrip3).LayoutContext = "module1";

			using (var filterControl1 = new FilterControlForTest(filterStrip1))
			{
				var gridColorStrip1 = new GridColourStripBusinessObject(filterControl1.FilterBusinessObject, null, typeof(DummyLogged));
				gridColorStrip1.RuleName = "rule1";

				using (var filterControl2 = new FilterControlForTest(filterStrip2))
				{
					var filter = Factory.New<StmModuleFilter>();
					filter.S9_FilterName = "rule1";
					filter.S9_ModuleID = ((IFilterStripBusinessObjectInternals)gridColorStrip1).LayoutContext;
					Factory.Save();

					var gridColorStrip2 = new GridColourStripBusinessObject(filterControl2.FilterBusinessObject, null, typeof(DummyLogged));
					gridColorStrip2.RuleName = "rule1";

					using (var filterControl3 = new FilterControlForTest(filterStrip3))
					{
						var gridColorStrip3 = new GridColourStripBusinessObject(filterControl3.FilterBusinessObject, null, typeof(DummyLogged));
						gridColorStrip3.RuleName = "rule1";

						AssertNoErrors(gridColorStrip1.RuleNameInfo);
						AssertNoErrors(gridColorStrip2.RuleNameInfo);
						AssertHasErrors(gridColorStrip3.RuleNameInfo);

						filter.S9_RelatedEntityID = ZGuid.NewZGuid();

						gridColorStrip3 = new GridColourStripBusinessObject(filterControl3.FilterBusinessObject, null, typeof(DummyLogged));
						gridColorStrip3.RuleName = "rule1";
						AssertHasErrors(gridColorStrip3.RuleNameInfo);

						gridColorStrip3.RuleName = "rule3";
						AssertNoErrors(gridColorStrip3.RuleNameInfo);

						filter.S9_FilterName = "late";
						filter.S9_IsPublished = true;
						Factory.Save();
						gridColorStrip3.RuleName = "";
						gridColorStrip3.RuleName = "late";
						AssertHasErrors(gridColorStrip3.RuleNameInfo);
					}
				}
			}
		}

		public void TestGetModuleFiltersNonEditable()
		{
			TestGetModuleFilters(false);
		}

		public void TestGetModuleFiltersEditable()
		{
			TestGetModuleFilters(true);
		}

		void TestGetModuleFilters(bool editable)
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, null, typeof(DummyLogged), editable);
				var filters = gridColorStrip.GetModuleFilters();

				AssertNull(filters["filterThatOverrides"]);
				AssertNotNull(filters["firstFilter"]);
				AssertNotNull(filters["Active Status"]);
				AssertNotNull(filters["filterThatShouldBeAdded"]);
				AssertNotNull(filters["Creating User"]);
				AssertNotNull(filters["Last Edit User"]);
				AssertNotNull(filters["Created Time"]);
				AssertNotNull(filters["Last Edit Time"]);
				AssertNotNull(filters["Created On Web/Internal"]);

				AssertEquals(FilterVisibility.Visible, filters["firstFilter"].Visibility);
			}
		}

		public void TestGetFiltersForParentModuleWithHelperFilters_ShouldNotDoubleAddHelperFilters()
		{
			var parentFilterBizo = new DummyFilterStripBusinessObjectWithInterface();

			using (var control = new FilterControlForTest(parentFilterBizo))
			{
				var gridColorStrip = new GridColourStripBusinessObjectWithInterface(control.FilterBusinessObject, null, typeof(DummyBusinessObjectWithInterface), true);
				AssertNotNull("Automatic adding of filters using IFilterStripsHelpers is disabled on GridColourStripBusinessObjects, but the strip from the helper should be grabbed from the parent filter bizo (rather than attempting to add it twice, triggering an exception), and yet...", gridColorStrip.GetModuleFilters()["Awesomeness Factor"]);
			}
		}

		public void TestQueryObjectTypeInitialized()
		{
			var parentFilterBizo = new DummyFilterStripBusinessObjectWithInterface();

			using (var control = new FilterControlForTest(parentFilterBizo))
			{
				var gridColorStrip = new GridColourStripBusinessObjectWithInterface(control.FilterBusinessObject, null, typeof(DummyBusinessObjectWithInterface), true);
				AssertNotNull("Wrapped FilterStripBusinessObject needs to be initialized properly", gridColorStrip.ParentFilterStrip.QueryObjectType);
			}
		}

		#region Helper Classes

		#region ModuleFilterForTest

		class ModuleFilterForTest : ModuleFilter
		{
			public ModuleFilterForTest(string desc) : base(desc) { }

			protected override void ClearCore() { }

			protected override bool IsEmptyCore => false;

			protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				var filter = new ModuleTextFilter("filterThatShouldBeAdded", delegate { return new ZQuery(); });
				filter.MultilingualDescription = (NoResString)"filterThatShouldBeAdded";
				return filter;
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom) { }

			public override bool IsExpensiveQuery
			{
				get { return false; }
			}

			protected override FilterCategory DefaultCategory
			{
				get { return null; }
			}

			protected override ModuleFilterValidation GetNewValidation()
			{
				return null;
			}

			protected override object[] QueryDelegateParameters
			{
				get { return null; }
			}

			protected override ZQuery GetQueryUsingFilterColumns()
			{
				return null;
			}

			protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer) { }

			protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader) { }

			protected override void FillWithValidTestFilterValueCore() { }
		}

		#endregion

		class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			public FilterStripBusinessObjectForTest()
				: base()
			{
				category = new FilterCategory((NoResString)"Common Numbers");
			}

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var result = new ModuleFilterCollection();

				var firstFilter = result.AddTextFilter("firstFilter", delegate { return new ZQuery(); });
				firstFilter.MultilingualDescription = (NoResString)"firstFilter";
				firstFilter.Visibility = FilterVisibility.AlwaysVisible;

				SetActiveStatusFilter(StmDataSchema.SD_IsCancelled, false);

				return result;
			}

			protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
			{
				var filterThatOverrides = new ModuleFilterForTest("filterThatOverrides");
				filterThatOverrides.MultilingualDescription = (NoResString)"filterThatOverrides";
				filterThatOverrides.Category = category;
				filterThatOverrides.IsCommon = true;

				return filterThatOverrides;
			}

			readonly FilterCategory category;

			public bool ShouldAddSystemDefinedStatusFilterForTest { get;set; }

			public override bool ShouldAddSystemDefinedStatusFilter => ShouldAddSystemDefinedStatusFilterForTest;
		}

		class FilterControlForTest : ZFilterStripControl
		{
			public FilterControlForTest(FilterStripBusinessObject filter) : base(null, filter) { }
		}

		class GridColourStripBusinessObjectWithInterface : GridColourStripBusinessObject, IDummyInterface
		{
			public GridColourStripBusinessObjectWithInterface(FilterStripBusinessObject parentFilterStrip, GridColourScheme colourScheme, Type businessEntityType, bool editable = false)
				: base(parentFilterStrip, colourScheme, businessEntityType, editable)
			{
			}

			public void DoNothing()
			{
			}
		}

		class DummyFilterStripBusinessObjectWithInterface : FilterStripBusinessObject, IDummyInterface
		{
			public DummyFilterStripBusinessObjectWithInterface()
			{
				QueryObjectType = typeof(DummyBusinessObjectWithInterface);
			}

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				return new ModuleFilterCollection();
			}

			protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
			{
				var result = base.GetCustomFilterStripsHelpersCore();
				result.Add(new DummyFilterStripsHelper());

				return result;
			}

			public void DoNothing()
			{
			}
		}

		#endregion

		#endregion
	}
}
