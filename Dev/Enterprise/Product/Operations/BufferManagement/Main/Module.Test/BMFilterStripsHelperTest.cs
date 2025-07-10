using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module.Test
{
	class BMFilterStripsHelperTest : BMSTestCaseWithFactory
	{
		public void TestBufferManagementComponent()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = AddSystemWithWorkflowType();
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component-1";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component-2";

			var dummyBO1 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader1 = ProcessJobHeader.GetForParent(dummyBO1, Factory);
			var processHeader1 = processJobHeader1.ProcessHeaders[0];
			processHeader1.FH_FC_CurrentComponent = component1.PK;

			var dummyBO2 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader2 = ProcessJobHeader.GetForParent(dummyBO2, Factory);
			var processHeader2 = processJobHeader2.ProcessHeaders[0];
			processHeader2.FH_FC_CurrentComponent = component1.PK;

			var dummyBO3 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader3 = ProcessJobHeader.GetForParent(dummyBO3, Factory);
			var processHeader3 = processJobHeader3.ProcessHeaders[0];
			processHeader3.FH_FC_CurrentComponent = component2.PK;

			var dummyBO4 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader4 = ProcessJobHeader.GetForParent(dummyBO4, Factory);
			var processHeader4 = processJobHeader4.ProcessHeaders[0];
			processHeader4.FH_FC_CurrentComponent = ZGuid.Empty;

			Factory.Save();

			var filters = new DummyFilterStripBizOWithWorkflowFilters("DUM");
			var componentFilter = ((ModuleGuidFilter)filters["Buffer Management Component"]);
			var list = componentFilter.List;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.Cast<BMComponent>().Any(x => x.FC_Name == "Component-1"));
			AssertEquals(true, list.Cast<BMComponent>().Any(x => x.FC_Name == "Component-2"));

			componentFilter.IsActive = true;
			componentFilter.Property = component1.PK;

			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			var resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertContainsExactElementsInAnyOrder("Operator = Exact, should return BO1 & BO2", new[] { dummyBO1, dummyBO2 }, resultList);

			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertContainsExactElementsInAnyOrder("Operator = NotEqual, should return BO3", new[] { dummyBO3 }, resultList);

			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertContainsExactElementsInAnyOrder("Operator = IsBlank, should return BO4", new[] { dummyBO4 }, resultList);

			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertContainsExactElementsInAnyOrder("Operator = IsNotBlank, should return BO1, BO2, BO3", new[] { dummyBO1, dummyBO2, dummyBO3 }, resultList);
		}

		public void TestBufferManagementComponent_OnlyExistForSpecificWorkflowTypes()
		{
			BMSTestHelper.EnableBMSInRegistry();
			AddSystemWithWorkflowType("DUM");
			AddSystemWithWorkflowType("WKI");

			var filters = new ModuleFilterCollection();
			var helper = new BMFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), ZString.Empty, Factory);
			helper.Initialise(typeof(DummyBusinessObjectWithWorkflow), Factory);
			helper.AddFilterStrips(filters);
			var componentFilter = filters["Buffer Management Component"];
			AssertNotNull("BM filters should have been automatically added because the helper determines the template code based on business object type if no alternate template code is provided. And yet...", componentFilter);

			filters = new ModuleFilterCollection();
			helper = new BMFilterStripsHelper(typeof(StmData), ZString.Empty, Factory);
			helper.Initialise(typeof(StmData), Factory);
			helper.AddFilterStrips(filters);
			componentFilter = filters["Buffer Management Component"];
			AssertNull("Should only exist if a supported type is passed in to the filter helper, even with an empty template code, and yet...", componentFilter);

			filters = new ModuleFilterCollection();
			helper = new BMFilterStripsHelper(typeof(StmData), "WKI", Factory);
			helper.Initialise(typeof(StmData), Factory);
			helper.AddFilterStrips(filters);
			componentFilter = filters["Buffer Management Component"];
			AssertNull("Should only exist if a supported type is passed in to the filter helper, regardless of the code supplied, and yet...", componentFilter);

			filters = new ModuleFilterCollection();
			helper = new BMFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "WKI", Factory);
			helper.Initialise(typeof(DummyBusinessObjectWithWorkflow), Factory);
			helper.AddFilterStrips(filters);
			componentFilter = filters["Buffer Management Component"];
			AssertNotNull("BM filters should have been automatically added because a supported workflow type was specified, and yet...", componentFilter);
		}

		public void TestBufferManagementTagFiltersWithComboFilters()
		{
			AddSystemWithWorkflowType();
			BMSTestHelper.EnableBMSInRegistry();

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "DEF", "DEF");
			var aaa = BMSTestHelper.CreateTagMagnitude(tagDef, "AAA", "AAA");

			Factory.Save();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var jobWorkflowFilterBusinessObject = module.FilterBusinessObject;

				var jobOrWorkflowFilter = jobWorkflowFilterBusinessObject.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				var jobOrWorkflowFilterStrip = (JobOrWorkflowFilter)jobOrWorkflowFilter.CurrentModuleFilter;
				jobOrWorkflowFilterStrip.SetJobOnly();

				var shownOnDiagramFilter = jobWorkflowFilterBusinessObject.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram);
				var shownOnDiagramFilterStrip = (ShownOnDiagramFilter)shownOnDiagramFilter.CurrentModuleFilter;
				shownOnDiagramFilterStrip.ComparisonOperator = ShownOnDiagramFilter.ComparisonConstants.FiltersMatch;

				var linkedEntityFilter = shownOnDiagramFilterStrip.SelectedFilters.AddFilterStrip<ModuleGuidFilter>(BMNCNShape.ModuleFilterConstants.LinkedEntity);
				linkedEntityFilter.ComparisonOperator = ShownOnDiagramFilter.ComparisonConstants.FiltersMatch;

				var tagFilter = linkedEntityFilter.SelectedFilters.AddFilterStrip<ModuleGuidAppliedToSubCollectionFilter>(ProcessHeader.ModuleFilterConstants.TagMagnitude);
				tagFilter.ComparisonOperator = TagWithJobOrWorkflowFilter.IsAppliedComparisonOperator;
				tagFilter.Property = aaa.PK;

				AssertNoExceptionThrown(() => Factory.Load<ProcessHeader>(jobWorkflowFilterBusinessObject.Filter));
			}
		}

		public void TestBufferManagementTagFilters()
		{
			AddSystemWithWorkflowType();
			BMSTestHelper.EnableBMSInRegistry();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = definition.Magnitudes.AddNew();
			magnitude1.TGM_Code = "WOW";
			var magnitude2 = definition.Magnitudes.AddNew();
			magnitude2.TGM_Code = "WOO";

			var dummyBO1 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader1 = ProcessJobHeader.GetForParent(dummyBO1, Factory);
			var processHeader1 = processJobHeader1.ProcessHeaders[0];
			processHeader1.AddTag(magnitude1);

			var dummyBO2 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader2 = ProcessJobHeader.GetForParent(dummyBO2, Factory);
			var processHeader2 = processJobHeader2.ProcessHeaders[0];
			processHeader2.AddTag(magnitude1);

			var dummyBO3 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader3 = ProcessJobHeader.GetForParent(dummyBO3, Factory);
			var processHeader3 = processJobHeader3.ProcessHeaders[0];
			processJobHeader3.AddTag(magnitude1);
			processHeader3.AddTag(magnitude1);

			var dummyBO4 = (IWorkflowProvider)Factory.New<DummyBusinessObjectWithWorkflow>();
			var processJobHeader4 = ProcessJobHeader.GetForParent(dummyBO4, Factory);
			var processHeader4 = processJobHeader4.ProcessHeaders[0];

			Factory.Save();

			var filters = new DummyFilterStripBizOWithWorkflowFilters("DUM");
			filters.QueryObjectType = typeof(DummyBusinessObjectWithWorkflow);
			var tagMagnitudeStrip = (TagWithJobOrWorkflowFilter)filters[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeStrip.IsActive = true;
			tagMagnitudeStrip.Property = magnitude1.PK;
			tagMagnitudeStrip.SqlComparisonOperator = SQLComparisonOperator.Contains;
			tagMagnitudeStrip.DropDownTypeName = "ALL";

			var resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(3, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(1, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(3, resultList.Length);

			tagMagnitudeStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			tagMagnitudeStrip.DropDownTypeName = "ALL";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(1, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(3, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(1, resultList.Length);

			processHeader4.AddTag(magnitude2);
			Factory.Save();

			filters = new DummyFilterStripBizOWithWorkflowFilters("DUM");
			var tagDefinitionFilterStrip = (TagWithJobOrWorkflowFilter)filters[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];
			tagDefinitionFilterStrip.IsActive = true;
			tagDefinitionFilterStrip.Property = definition.PK;
			tagDefinitionFilterStrip.SqlComparisonOperator = SQLComparisonOperator.Contains;
			tagDefinitionFilterStrip.DropDownTypeName = "ALL";

			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(4, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(1, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(4, resultList.Length);

			tagDefinitionFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			tagDefinitionFilterStrip.DropDownTypeName = "ALL";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(0, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(3, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load<DummyBusinessObjectWithWorkflow>(filters.Filter);
			AssertEquals(0, resultList.Length);
		}

		public void TestBMFilterStrips_ShouldBeAddedWithAutoWorkflowFilterStrips()
		{
			BMSTestHelper.EnableBMSInRegistry();
			AddSystemWithWorkflowType();

			var bizo1 = new DummyFilterStripBusinessObject(Factory) { QueryObjectType = typeof(DummyBusinessObject) };
			var bizo2 = GetFilterBusinessObjectForWorkflowBizo();

			AssertNull("Buffer management filter strips should not have been added because the target type is not a workflow provider, and yet...", bizo1[ProcessHeader.ModuleFilterConstants.TagMagnitude]);
			AssertNotNull("Buffer management filter strips should have been added because the target type is a workflow provider, and yet...", bizo2[ProcessHeader.ModuleFilterConstants.TagMagnitude]);
		}

		public void TestBMFilterStirps_ShouldBeAddedWhenBMIsEnabledInRegistry()
		{
			BMSTestHelper.DisableBMSInRegistry();
			AddSystemWithWorkflowType();

			var bizo = GetFilterBusinessObjectForWorkflowBizo();

			AssertNull("Buffer management filter strips should not have been added because buffer management is disabled in the registry, and yet...", bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude]);

			BMSTestHelper.EnableBMSInRegistry();

			bizo = GetFilterBusinessObjectForWorkflowBizo();
			AssertNotNull("Buffer management filter strips should have been added because buffer management is enabled in the registry, and yet...", bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude]);
		}

		public void TestBMFilterStrips_ShouldBeAddedWhenWorkflowTypeIsAssociatedWithBMSystem()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var bizo = GetFilterBusinessObjectForWorkflowBizo();
			AssertNull("Buffer management filter strips should not have been added because there is no BM system associated with its workflow type, and yet...", bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude]);

			AddSystemWithWorkflowType();

			bizo = GetFilterBusinessObjectForWorkflowBizo();

			AssertNotNull("Buffer management filter strips should have been added because the target type's workflow type is associated with a BM system, and yet...", bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude]);
		}

		public void TestTagFilterInGroup_ShouldNotResultInWrongParameterPlaceholders()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "DEF", "DEF");
			var aaa = BMSTestHelper.CreateTagMagnitude(tagDef, "AAA", "AAA");
			var bbb = BMSTestHelper.CreateTagMagnitude(tagDef, "BBB", "BBB");
			var ccc = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC", "CCC");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			workflow2.AddTag(aaa, showSecurityDialog: false);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			workflow3.AddTag(bbb, showSecurityDialog: false);

			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			workflow4.AddTag(ccc, showSecurityDialog: false);

			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");
			workflow5.AddTag(aaa, showSecurityDialog: false);
			workflow5.AddTag(bbb, showSecurityDialog: false);

			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow6");
			workflow6.AddTag(aaa, showSecurityDialog: false);
			workflow6.AddTag(ccc, showSecurityDialog: false);

			var workflow7 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow7");
			workflow7.AddTag(bbb, showSecurityDialog: false);
			workflow7.AddTag(ccc, showSecurityDialog: false);

			var workflow8 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow8");
			workflow8.AddTag(aaa, showSecurityDialog: false);
			workflow8.AddTag(bbb, showSecurityDialog: false);
			workflow8.AddTag(ccc, showSecurityDialog: false);

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var strip = filterBizo.FilterStrips.AddNew("Completion Statement");
				var completionStatementFilter = (ModuleTextFilter)strip.CurrentModuleFilter;
				completionStatementFilter.Property = "workflow";

				const string group = "LeGroup";

				strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagMagnitude);
				var filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
				filter.Property = aaa.PK;
				filter.ComparisonOperator = TagWithJobOrWorkflowFilter.IsAppliedComparisonOperator;
				filter.GroupName = group;
				filter.OrCategory = FilterOrCategory.BurlyWood;

				strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagMagnitude);
				filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
				filter.Property = bbb.PK;
				filter.ComparisonOperator = TagWithJobOrWorkflowFilter.NotAppliedComparisonOperator;
				filter.GroupName = group;
				filter.OrCategory = FilterOrCategory.None;

				strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagMagnitude);
				filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
				filter.Property = ccc.PK;
				filter.ComparisonOperator = TagWithJobOrWorkflowFilter.IsAppliedComparisonOperator;
				filter.GroupName = group;
				filter.OrCategory = FilterOrCategory.BurlyWood;

				var query = filterBizo.Filter;
				var results = Factory.Load<ProcessHeader>(query);
				var failMessage = "The wrong workflows were matched, which might indicate that the query is being built with incorrectly reused parameter names. All three tags should be mentioned in the query.\n\nQUERY WITH PARAMETER NAMES:\n\n";
				failMessage += query.ParameterisedText.ParameterisedQueryText + "\n\n\nQUERY WITH PARAMETER VALUES:\n\n" + query.ParameterisedText.LiteralTextSql + "\n\nPARAMETERS:\n\n" + string.Join("\n", query.Params.Select(x => x.ParameterName + ": " + x.ValueForSql));
				AssertContainsExactElementsInAnyOrder(failMessage, new[] { "workflow2", "workflow4", "workflow6" }, results.Select(x => x.FH_CompletionStatement));
			}
		}

		public void TestBMFilterStripsHelper_IndexSearch()
		{
			BMSTestHelper.EnableBMSInRegistry();
			AddSystemWithWorkflowType("SHP");

			var filters = new ModuleFilterCollection();
			var helper = new BMFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), "SHP", Factory);
			helper.Initialise(typeof(DummyBusinessObjectWithWorkflow), Factory);
			helper.AddFilterStripsForIndexSearch(filters, GetTestSearchFields());

			AssertNotNull((IndexSearchModuleGuidFilter)filters[IndexSearchFilterHelper.DefaultHiddenPrefix + "BUFFERMANAGEMENTCOMPONENT"]);
			AssertNotNull((IndexSearchTagWithJobOrWorkflowFilter)filters[IndexSearchFilterHelper.DefaultHiddenPrefix + "TAG"]);
			AssertNotNull((IndexSearchTagWithJobOrWorkflowFilter)filters[IndexSearchFilterHelper.DefaultHiddenPrefix + "TAGGROUP"]);
		}

		SearchField[] GetTestSearchFields()
		{
			var searchField1 = SearchField.Create(IndexSearchFilterHelper.DefaultHiddenPrefix + "BUFFERMANAGEMENTCOMPONENT", IndexSearchFilterHelper.DefaultHiddenPrefix + "BUFFERMANAGEMENTCOMPONENT");
			var searchField2 = SearchField.Create(IndexSearchFilterHelper.DefaultHiddenPrefix + "TAG", IndexSearchFilterHelper.DefaultHiddenPrefix + "TAG");
			var searchField3 = SearchField.Create(IndexSearchFilterHelper.DefaultHiddenPrefix + "TAGGROUP", IndexSearchFilterHelper.DefaultHiddenPrefix + "TAGGROUP");

			return new SearchField[] { searchField1 , searchField2 , searchField3 };
		}

		public void TestTagFilter_ShouldResetParameterNamesEachTimeFilterBizoQueryIsGenerated()
		{
			AssertFilter_ShouldResetParameterNamesEachTimeFilterBizoQueryIsGenerated(ProcessHeader.ModuleFilterConstants.TagMagnitude);
		}

		public void TestTagDefinitionFilter_ShouldResetParameterNamesEachTimeFilterBizoQueryIsGenerated()
		{
			AssertFilter_ShouldResetParameterNamesEachTimeFilterBizoQueryIsGenerated(ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
		}

		static void AssertFilter_ShouldResetParameterNamesEachTimeFilterBizoQueryIsGenerated(string filterName)
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var strip = filterBizo.FilterStrips.AddNew(filterName);
			var filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			filter.Property = ZGuid.NewZGuid();
			filter.ComparisonOperator = TagWithJobOrWorkflowFilter.IsAppliedComparisonOperator;

			var query = filterBizo.Filter;
			var parameterNames = query.Params.Select(x => x.ParameterName).ToArray();
			var sql = query.LiteralTextSqlFormatted;

			query = filterBizo.Filter;
			var actualParams = query.Params.Select(x => x.ParameterName).ToArray();
			AssertContainsExactElementsInAnyOrder(parameterNames, actualParams);
			AssertEquals(sql, query.LiteralTextSqlFormatted);
		}

		DummyFilterStripBusinessObject GetFilterBusinessObjectForWorkflowBizo()
		{
			return new DummyFilterStripBusinessObject(Factory) { QueryObjectType = typeof(DummyBusinessObjectWithWorkflow) };
		}

		BMSystem AddSystemWithWorkflowType(string workflowCode = "DUM")
		{
			return BMSTestHelper.CreateSystem(Factory, workflowCode);
		}
	}

	public class BMFilterStripsHelperAutomaticFilterTest : AutomaticFilterTest
	{
		public override void SetUpForHelperFiltersWorkTests(Type businessObjectType)
		{
			var workflowType = WorkflowFilterStripsHelper.GetTemplateCodeForBusinessObjectType(businessObjectType);

			if (string.IsNullOrWhiteSpace(workflowType))
			{
				return;
			}

			BMSTestHelper.EnableBMSInRegistry();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var workflowDeterminer = system.RelatedWorkflowTypes.AddNew();

			workflowDeterminer.FSW_WorkflowType = workflowType;

			Factory.Save();
		}

		public void AssertBufferManagementComponent(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var componentFilter = ((ModuleGuidFilter)filterBusinessObject["Buffer Management Component"]);

			if (componentFilter != null)
			{
				var system = BMSystem.GetSystemForWorkflowType(WorkflowFilterStripsHelper.GetTemplateCodeForBusinessObjectType(businessObjectType), Factory);
				var component1 = system.Components.AddNew();
				component1.FC_Name = "Component-1";
				var component2 = system.Components.AddNew();
				component2.FC_Name = "Component-2";

				var bizo1 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var processJobHeader1 = ProcessJobHeader.GetForParent(bizo1, Factory);
				var processHeader1 = processJobHeader1.ProcessHeaders[0];
				processHeader1.FH_FC_CurrentComponent = component1.PK;

				var bizo2 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var processJobHeader2 = ProcessJobHeader.GetForParent(bizo2, Factory);
				var processHeader2 = processJobHeader2.ProcessHeaders[0];
				processHeader2.FH_FC_CurrentComponent = component1.PK;

				var bizo3 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var processJobHeader3 = ProcessJobHeader.GetForParent(bizo3, Factory);
				var processHeader3 = processJobHeader3.ProcessHeaders[0];
				processHeader3.FH_FC_CurrentComponent = component2.PK;

				var bizo4 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var processJobHeader4 = ProcessJobHeader.GetForParent(bizo4, Factory);
				var processHeader4 = processJobHeader4.ProcessHeaders[0];
				processHeader4.FH_FC_CurrentComponent = ZGuid.Empty;

				Factory.Save();

				var list = componentFilter.List;
				AssertEquals(2, list.Count);
				AssertEquals(true, list.Cast<BMComponent>().Any(x => x.FC_Name == "Component-1"));
				AssertEquals(true, list.Cast<BMComponent>().Any(x => x.FC_Name == "Component-2"));

				componentFilter.IsActive = true;
				componentFilter.Property = component1.PK;

				componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
				var resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("Operator = Exact, should return BO1 & BO2", new[] { bizo1, bizo2 }, resultList);

				componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
				resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("Operator = NotEqual, should return BO3", new[] { bizo3 }, resultList);

				componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
				resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("Operator = IsBlank, should return BO4", new[] { bizo4 }, resultList);

				componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
				resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("Operator = IsNotBlank, should return BO1, BO2, BO3", new[] { bizo1, bizo2, bizo3 }, resultList);
			}
		}

		public void AssertTagFilters(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = definition.Magnitudes.AddNew();
			magnitude1.TGM_Code = "WOW";
			var magnitude2 = definition.Magnitudes.AddNew();
			magnitude2.TGM_Code = "WOO";

			var dummyBO1 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var processJobHeader1 = ProcessJobHeader.GetForParent(dummyBO1, Factory);
			var processHeader1 = processJobHeader1.ProcessHeaders[0];
			processHeader1.AddTag(magnitude1);

			var dummyBO2 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var processJobHeader2 = ProcessJobHeader.GetForParent(dummyBO2, Factory);
			var processHeader2 = processJobHeader2.ProcessHeaders[0];
			processHeader2.AddTag(magnitude1);

			var dummyBO3 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var processJobHeader3 = ProcessJobHeader.GetForParent(dummyBO3, Factory);
			var processHeader3 = processJobHeader3.ProcessHeaders[0];
			processJobHeader3.AddTag(magnitude1);
			var tagLink4 = processHeader3.AddTag(magnitude1);

			var dummyBO4 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var processJobHeader4 = ProcessJobHeader.GetForParent(dummyBO4, Factory);
			var processHeader4 = processJobHeader4.ProcessHeaders[0];

			Factory.Save();

			var tagMagnitudeStrip = (TagWithJobOrWorkflowFilter)filterBusinessObject[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeStrip.IsActive = true;
			tagMagnitudeStrip.Property = magnitude1.PK;
			tagMagnitudeStrip.SqlComparisonOperator = SQLComparisonOperator.Contains;
			tagMagnitudeStrip.DropDownTypeName = "ALL";

			var resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(3, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(1, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(3, resultList.Length);

			tagMagnitudeStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			tagMagnitudeStrip.DropDownTypeName = "ALL";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(1, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(3, resultList.Length);

			tagMagnitudeStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(1, resultList.Length);

			processHeader4.AddTag(magnitude2);

			Factory.Save();

			tagMagnitudeStrip.IsActive = false;

			var tagDefinitionFilterStrip = (TagWithJobOrWorkflowFilter)filterBusinessObject[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];
			tagDefinitionFilterStrip.IsActive = true;
			tagDefinitionFilterStrip.Property = definition.PK;
			tagDefinitionFilterStrip.SqlComparisonOperator = SQLComparisonOperator.Contains;
			tagDefinitionFilterStrip.DropDownTypeName = "ALL";

			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(4, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(1, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(4, resultList.Length);

			tagDefinitionFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			tagDefinitionFilterStrip.DropDownTypeName = "ALL";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(0, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "JOB";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(3, resultList.Length);

			tagDefinitionFilterStrip.DropDownTypeName = "WFL";
			resultList = Factory.Load(businessObjectType, filterBusinessObject.Filter);
			AssertEquals(0, resultList.Length);
		}

		public void AssertWorkflowsFilter(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (ModuleGuidForeignCollectionFilter)filterBusinessObject[ProcessHeader.ModuleFilterConstants.WorkflowsForJob];

			if (filter != null)
			{
				var jobWithOpenWorkflow = getNewBusinessObject(Factory, businessObjectType) as IWorkflowProvider;
				var jobWithClosedWorkflow = getNewBusinessObject(Factory, businessObjectType) as IWorkflowProvider;
				var jobHeaderWithOpenWorkflow = ProcessJobHeader.GetForParent(jobWithOpenWorkflow, Factory, addDefaultProcessHeaderIfNone: false);
				var jobHeaderWithClosedWorkflow = ProcessJobHeader.GetForParent(jobWithClosedWorkflow, Factory, addDefaultProcessHeaderIfNone: false);
				var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
				var openWorkflow = jobHeaderWithOpenWorkflow.ProcessHeaders.AddNew();
				var closedWorkflow = jobHeaderWithClosedWorkflow.ProcessHeaders.AddNew();
				var openTask = Factory.NewWithValidTestData<ProcessTask>();
				var closedTask = Factory.NewWithValidTestData<ProcessTask>();

				openTask.P9_FH_ProcessHeader = openWorkflow.PK;
				closedTask.P9_FH_ProcessHeader = closedWorkflow.PK;
				releaseGroup.GG_Code = "ZAZ";

				Factory.Save();

				openWorkflow.FH_GG_ReleaseGroup = releaseGroup.PK;
				openTask.P9_Status = "ASN";
				closedWorkflow.FH_GG_ReleaseGroup = releaseGroup.PK;
				closedTask.P9_Status = "CLS";

				Factory.Save();

				var workflowFilter = filter.SelectedFilters.AddFilterStrip<TasksModuleFilter>("Tasks");
				workflowFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
				workflowFilter.SelectedFilters.AddTextFilterStrip("Status", "CLS");

				var taskResult = Factory.Load<ProcessTask>(workflowFilter.SelectedFilters.Filter);
				AssertEquals(1, taskResult.Length);
				AssertEquals(closedTask.PK, taskResult.Single().PK);

				filter.SelectedFilters.AddGuidFilterStrip("Release Group", releaseGroup.PK);

				var subFilterResult = Factory.Load<ProcessHeader>(filter.SelectedFilters.Filter);
				AssertEquals("Sub filter: " + filter.SelectedFilters.Filter.LiteralTextSqlFormatted, 1, subFilterResult.Length);
				AssertContainsExactElementsInAnyOrder("Sub filter: " + filter.SelectedFilters.Filter.LiteralTextSqlFormatted, new[] { closedWorkflow.PK }, subFilterResult.Select(x => x.PK));

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

				var pkQuery = new ZQuery();
				var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
				pkQuery.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithOpenWorkflow.PK);
				pkQuery.AddToFilter(JoinCondition.Or, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithClosedWorkflow.PK);
				var query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				var result = Factory.Load(businessObjectType, query);
				AssertEquals("Any match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("Any match: " + query.LiteralTextSqlFormatted, jobWithClosedWorkflow.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("None match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("None match: " + query.LiteralTextSqlFormatted, jobWithOpenWorkflow.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				var allWorkflows = Factory.Load<ProcessHeader>(new ZQuery());

				var expectedCount = allWorkflows.Length == 2 ? 1 : 0; // If job headers exist then no results will be found
				AssertEquals("All match: " + query.LiteralTextSqlFormatted, expectedCount, result.Length);

				if (expectedCount == 1)
				{
					AssertEquals("All match: " + query.LiteralTextSqlFormatted, jobWithClosedWorkflow.PK, result.Single().PK);
				}

				CombineAssertions(() =>
				{
					AssertWorkflowsForJobFilter_AnyMatch(filterBusinessObject, businessObjectType, getNewBusinessObject);
					AssertWorkflowsForJobFilter_NoneMatch(filterBusinessObject, businessObjectType, getNewBusinessObject);
					AssertWorkflowsForJobFilter_AllMatch(filterBusinessObject, businessObjectType, getNewBusinessObject);
				});
			}
		}

		public void AssertWorkflowsForJobFilter_AnyMatch(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			filterBusinessObject.ResetModuleFilters();
			var job = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false, description: "Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "B");

			Factory.Save();

			var workflowsFilter = (ModuleGuidForeignCollectionFilter)filterBusinessObject["Workflows for Job"];
			AssertEquals(ModuleTextFilter.ComparisonConstants.AnyMatch, workflowsFilter.ComparisonOperator);
			workflowsFilter.IsActive = true;

			workflowsFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "A");

			var query = filterBusinessObject.Filter;
			var results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("The job should have been returned because one of the workflows in the job meets the ANY MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, new[] { job }, results);

			workflow1.FH_CompletionStatement = "C";
			Factory.Save();

			query = filterBusinessObject.Filter;
			var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
			query.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, job.PK);
			results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("There should be an empty result because none of the workflows (including job-level) match the ANY MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<BusinessObject>(), results);

			jobHeader.FH_CompletionStatement = "A Job";
			Factory.Save();

			results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("The job should have been returned because the job header meets the ANY MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, new[] { job }, results);
		}

		public void AssertWorkflowsForJobFilter_NoneMatch(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			filterBusinessObject.ResetModuleFilters();
			var job = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false, description: "Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "B");

			Factory.Save();

			var workflowsFilter = (ModuleGuidForeignCollectionFilter)filterBusinessObject["Workflows for Job"];
			workflowsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			workflowsFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "A");
			workflowsFilter.IsActive = true;

			var query = filterBusinessObject.Filter;
			var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
			query.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, job.PK);
			var results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("There should be an empty result because one of the workflows matches the NONE MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<BusinessObject>(), results);

			workflow1.FH_CompletionStatement = "C";
			Factory.Save();

			results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("The job and workflows should have been returned because none of the workflows in the job (including job-level) meet the NONE MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, new[] { job }, results);

			jobHeader.FH_CompletionStatement = "A Job";
			Factory.Save();

			results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("There should be an empty result because the job-level workflow matches the NONE MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<BusinessObject>(), results);
		}

		public void AssertWorkflowsForJobFilter_AllMatch(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			filterBusinessObject.ResetModuleFilters();
			var job = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false, description: "Spaceship Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Spaceship");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Scorpion");

			Factory.Save();

			var workflowsFilter = (ModuleGuidForeignCollectionFilter)filterBusinessObject["Workflows for Job"];
			workflowsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			workflowsFilter.IsActive = true;
			workflowsFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "Spaceship");

			var query = filterBusinessObject.Filter;
			var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
			query.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, job.PK);
			var results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("No results should be returned because not all of the workflows in the job meet the ALL MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<BusinessObject>(), results);

			workflow2.FH_CompletionStatement = "Spaceship 2";
			Factory.Save();

			results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("Now all of the workflows in the job meet the ALL MATCH criteria, so the job's workflows should have been returned, and yet... " + query.LiteralTextSqlFormatted, new[] { job }, results);

			jobHeader.FH_CompletionStatement = "Job";
			Factory.Save();

			results = Factory.Load(businessObjectType, query);
			AssertContainsExactElementsInAnyOrder("No results should be returned because the job-level workflow doesn't meet the ALL MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<BusinessObject>(), results);
		}
	}
}
