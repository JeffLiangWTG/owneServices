using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.PAVE.MENT.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test;

public class MENTAgedScoreExtractorNonTransactionTest : NonTransactionedTestCase
{
	[TestDate(1995, 12, 23, 12, 0, 0)]
	public void TestSeriesFilter_DateTime()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "DNT";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "PAK";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component1 = Factory.NewWithValidTestData<BMComponent>();
		component1.FC_FS_System = system.PK;
		component1.FC_Name = "AAA";
		var component2 = Factory.NewWithValidTestData<BMComponent>();
		component2.FC_FS_System = system.PK;
		component2.FC_Name = "BBB";
		var component3 = Factory.NewWithValidTestData<BMComponent>();
		component3.FC_FS_System = system.PK;
		component3.FC_Name = "CCC";

		var query = MENTTestHelper.CreateQuery(Factory, "KURT");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Time recorded",
			FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days,
		});

		Factory.Save();

		var viewFilter = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);
		Assert("viewFilter not empty", !viewFilter.IsEmpty);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Sequence = 1;

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(-1), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddDays(-2), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(-3), group1.PK, component2.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(10), group1.PK, component2.PK, "2");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(20), group1.PK, component2.PK, "3");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69, ZDateTime.Now.AddDays(100), group1.PK, component3.PK, "3");

		var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);

		AssertEquals(7m, extractedData.First().YValue);
		AssertEquals(5m, extractedData.Last().YValue);
	}

	[TestDate(1995, 12, 23, 0, 0, 0)]
	public void TestSeriesFilterInstantaneous()
	{
		var group = Factory.NewWithValidTestData<GlbGroup>();
		group.GG_Code = "NEX";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "NOT";

		var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
		var buffer = VisualBoardsTestHelper.CreateBuffer(system);

		var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

		var workflow1 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Grapeman is a beast. He has a pencil to write with", ZDateTime.Now);
		workflow1.FH_GG_ReleaseGroup = group.PK;
		var workflow2 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Grapeman is a beast. He has a pencil to write with", ZDateTime.Now);
		workflow2.FH_GG_ReleaseGroup = group2.PK;

		Factory.Save();

		var query = MENTTestHelper.CreateQuery(Factory, "Mousecop");
		query.MAQ_SqlText = "select 64.98 as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, '' as AttributeValue, '' as Staff from dbo.ProcessHeader where FH_FH_ParentHeader is not null";

		var extraction = MENTTestHelper.CreateExtraction(Factory, "The player formerly known as", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Release Group",
			FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = group.PK,
		});

		var rgColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.ReleaseGroup);
		rgColumn.Selected = true;
		rgColumn.Sequence = 1;

		Factory.Save();

		var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

		AssertEquals(64.98m, rows[0].YValue);
		AssertEquals("NEX", rows[0].XSeriesString);
	}

	public void TestExtractor_SeriesFilterOnComponent()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "DNT";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "PAK";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component1 = Factory.NewWithValidTestData<BMComponent>();
		component1.FC_FS_System = system.PK;
		component1.FC_Name = "AAA";
		var component2 = Factory.NewWithValidTestData<BMComponent>();
		component2.FC_FS_System = system.PK;
		component2.FC_Name = "BBB";
		var component3 = Factory.NewWithValidTestData<BMComponent>();
		component3.FC_FS_System = system.PK;
		component3.FC_Name = "CCC";

		var query = MENTTestHelper.CreateQuery(Factory, "KURT");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Current Component",
			FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = component2.PK,
		});

		Factory.Save();

		var viewFilter = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);
		Assert("viewFilter not empty", !viewFilter.IsEmpty);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Selected = true;
		componentColumn.Sequence = 1;

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(1), group1.PK, component2.PK, "2");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(-1), group1.PK, component2.PK, "3");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69, ZDateTime.Now.AddDays(100), group1.PK, component3.PK, "3");

		var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

		AssertEquals(15m, extractedData.First().YValue);
	}

	public void TestExtractor_SeriesFilterOnComponent_InvalidPK()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "DNT";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "PAK";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component1 = Factory.NewWithValidTestData<BMComponent>();
		component1.FC_FS_System = system.PK;
		component1.FC_Name = "AAA";
		var component2 = Factory.NewWithValidTestData<BMComponent>();
		component2.FC_FS_System = system.PK;
		component2.FC_Name = "BBB";
		var component3 = Factory.NewWithValidTestData<BMComponent>();
		component3.FC_FS_System = system.PK;
		component3.FC_Name = "CCC";

		var query = MENTTestHelper.CreateQuery(Factory, "KURT");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Current Component",
			FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = ZGuid.Invalid,
		});

		Factory.Save();

		var viewFilter = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);
		Assert("viewFilter empty", viewFilter.IsEmpty);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Selected = true;
		componentColumn.Sequence = 1;

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(1), group1.PK, component2.PK, "2");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(-1), group1.PK, component2.PK, "3");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69, ZDateTime.Now.AddDays(100), group1.PK, component3.PK, "3");

		var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 3 }), 0);

		AssertEquals(7m, extractedData[0].YValue);
		AssertEquals(15m, extractedData[1].YValue);
		AssertEquals(69m, extractedData[2].YValue);
	}

	public void TestExtractor_SeriesFilterOnGroup()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "DNT";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "PAK";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component1 = Factory.NewWithValidTestData<BMComponent>();
		component1.FC_FS_System = system.PK;
		component1.FC_Name = "AAA";
		var component2 = Factory.NewWithValidTestData<BMComponent>();
		component2.FC_FS_System = system.PK;
		component2.FC_Name = "BBB";
		var component3 = Factory.NewWithValidTestData<BMComponent>();
		component3.FC_FS_System = system.PK;
		component3.FC_Name = "CCC";

		var query = MENTTestHelper.CreateQuery(Factory, "KURT");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Release Group",
			FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = group2.PK,
		});

		Factory.Save();

		var viewFilter = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);
		Assert("viewFilter not empty", !viewFilter.IsEmpty);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Selected = true;
		componentColumn.Sequence = 1;

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group2.PK, component2.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(1), group2.PK, component2.PK, "2");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(-1), group2.PK, component3.PK, "3");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69, ZDateTime.Now.AddDays(100), group1.PK, component3.PK, "3");

		var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);

		AssertEquals(10m, extractedData.First().YValue);
		AssertEquals(5m, extractedData.Last().YValue);
	}

	public void TestExtractor_SeriesFilterOnGroup_Contains()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "SOMERG";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "OTHRG";
		var group3 = Factory.NewWithValidTestData<GlbGroup>();
		group3.GG_Code = "BLAH";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component = Factory.NewWithValidTestData<BMComponent>();
		component.FC_FS_System = system.PK;
		component.FC_Name = "AAA";
		var query = MENTTestHelper.CreateQuery(Factory, "COVID19");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Quarantine", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Release Group",
			FilterStripValueSetter = f =>
			{
				((ModuleGuidFilter)f).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
				((ModuleGuidFilter)f).SelectedFilters.AddTextFilterStrip("Code", "RG").SqlComparisonOperator = SQLComparisonOperator.Contains;
			}
		});

		Factory.Save();

		var viewFilter = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);
		Assert("viewFilter not empty", !viewFilter.IsEmpty);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Selected = true;
		componentColumn.Sequence = 1;

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 10, ZDateTime.Now, group1.PK, component.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 20, ZDateTime.Now, group2.PK, component.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 30, ZDateTime.Now, group3.PK, component.PK, "1");

		var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

		AssertEquals(30m, extractedData.First().YValue);
	}

	public void TestExtractor_SeriesFilterStaff()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "DNT";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "PAK";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component1 = Factory.NewWithValidTestData<BMComponent>();
		component1.FC_FS_System = system.PK;
		component1.FC_Name = "AAA";
		var component2 = Factory.NewWithValidTestData<BMComponent>();
		component2.FC_FS_System = system.PK;
		component2.FC_Name = "BBB";
		var component3 = Factory.NewWithValidTestData<BMComponent>();
		component3.FC_FS_System = system.PK;
		component3.FC_Name = "CCC";
		var staff1 = Factory.NewWithValidTestData<GlbStaff>();
		staff1.GS_Code = "AAA";
		var staff2 = Factory.NewWithValidTestData<GlbStaff>();
		staff2.GS_Code = "BBB";

		var query = MENTTestHelper.CreateQuery(Factory, "KURT");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Staff Code",
			FilterStripValueSetter = f => ((ModuleNkFilter)f).Property = "AAA",
		});

		Factory.Save();

		var viewFilter = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);
		Assert("viewFilter not empty", !viewFilter.IsEmpty);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Selected = true;
		componentColumn.Sequence = 1;

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1", "AAA");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group2.PK, component2.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(1), group2.PK, component2.PK, "2");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddDays(-1), group2.PK, component3.PK, "3", "AAA");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69, ZDateTime.Now.AddDays(100), group1.PK, component3.PK, "3");
		var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);

		AssertEquals(2m, extractedData.First().YValue);
		AssertEquals(5m, extractedData.Last().YValue);
	}
	public void TestInstantaneousExtractorQueries_ShouldProperlyUseParameterisedQuery()
	{
		var group = Factory.NewWithValidTestData<GlbGroup>();
		group.GG_Code = "NEX";

		var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
		var buffer = VisualBoardsTestHelper.CreateBuffer(system);
		var releaseGroup = VisualBoardsTestHelper.CreateReleaseGroup(system, group);

		var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

		var workflow1 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Grapeman is a beast. He has a pencil to write with", ZDateTime.Now);
		workflow1.FH_GG_ReleaseGroup = group.PK;

		Factory.Save();

		var query = MENTTestHelper.CreateQuery(Factory, "Mousecop");
		query.MAQ_SqlText = "select 64.98 as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, '' as AttributeValue, '' as Staff from dbo.ProcessHeader where FH_FH_ParentHeader is not null";

		var extraction = MENTTestHelper.CreateExtraction(Factory, "The player formerly known as", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

		var rgColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.ReleaseGroup);
		rgColumn.Selected = true;
		rgColumn.Sequence = 1;

		AddSeriesFilterStrips(extraction.SeriesFilter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Release Group",
			FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = group.PK,
		});

		AddSeriesFilterStrips(extraction.SeriesFilter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Time recorded",
			FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days,
		});

		Factory.Save();

		var extractorResult = MENTTestHelper.CreateExtractor(extraction).Extract();

		var command = SqlEventTracker.Instance.LastSqlQuery;

		AssertNotNull("We should succesfully execute our extractor query, however...", command);

		AssertContains("The query should be parameterised, and yet...", ExtractionSqlGenerator.SectionFilterParameterName + 1, command);
		AssertContains("The query should be parameterised, and yet...", ExtractionSqlGenerator.SectionFilterParameterName + 2, command);
		AssertNotContains("We should customise all parameters, and not include those stinky autogenerated parameters, and yet...", "@CWO", command);
	}

	public void TestHistoricalExtractorQueries_ShouldProperlyUseParameterisedQuery()
	{
		var group1 = Factory.NewWithValidTestData<GlbGroup>();
		group1.GG_Code = "DNT";
		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		group2.GG_Code = "PAK";

		var system = Factory.NewWithValidTestData<BMSystem>();
		var component1 = Factory.NewWithValidTestData<BMComponent>();
		component1.FC_FS_System = system.PK;
		component1.FC_Name = "AAA";
		var component2 = Factory.NewWithValidTestData<BMComponent>();
		component2.FC_FS_System = system.PK;
		component2.FC_Name = "BBB";

		var query = MENTTestHelper.CreateQuery(Factory, "KURT");
		var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Sequence = 1;
		var rgColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
		rgColumn.Sequence = 2;

		var additionalExtraction = MENTTestHelper.CreateExtraction(Factory, "Flying Mango", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);
		var additionalComponentColumn = MENTTestHelper.GetSqlColumnFromCollection(additionalExtraction.SeriesColumns, MENTColumns.Codes.Component, true);
		additionalComponentColumn.Sequence = 1;

		MENTTestHelper.CreateAdditionalExtractionLink(extraction, query, additionalExtraction);

		AddSeriesFilterStrips(extraction.SeriesFilter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Current Component",
			FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = component2.PK,
		});

		Factory.Save();

		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group2.PK, component1.PK, "1");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 23, ZDateTime.Now.AddMinutes(5), group2.PK, component1.PK, "2");
		MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(6), group2.PK, component2.PK, "1");

		var extractorResult = MENTTestHelper.CreateExtractor(extraction).Extract();

		var command = SqlEventTracker.Instance.LastSqlQuery;

		AssertNotNull("We should succesfully execute our extractor query, however...", command);

		AssertContains("The query should be parameterised, and yet...", ExtractionSqlGenerator.MAQCodeParameterName, command);
		AssertNotContains("We should customise all parameters, and not include those stinky autogenerated parameters, and yet...", "@CWO", command);
	}

	static void AddSeriesFilterStrips(StmModuleFilter stmFilter, params FilterStripsTestHelper.FilterStripDefinition[] filterStripDefs)
	{
		var filters = new SeriesFilterBusinessObject();

		foreach (var filterStripDef in filterStripDefs)
		{
			var filterStrip = filters.FilterStrips.AddNew(filterStripDef.FilterStripName);
			filterStrip.OrCategory = filterStripDef.OrCategory;

			var filter = filterStrip.CurrentModuleFilter;
			filterStripDef.FilterStripValueSetter(filter);
		}

		filters.WriteFilterStripsToXml(stmFilter, filters.FilterStrips, new EmptyLayoutsHelper());
	}

	#region Implementation

	protected override void SetUp()
	{
		MENTTestHelper.ClearMENTTables();
		base.SetUp();
		BMSTestHelper.EnableBMSInRegistry();
	}

	#endregion
}

