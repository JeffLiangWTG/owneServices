using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.PAVE.MENT.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test;

[TestedType(typeof(MENTAgedScoreExtraction))]
class MENTAgedScoreExtractionTest : EnterpriseBusinessObjectTestCase
{
	public void TestCreateLinkForMENTAgedScoreExtraction()
	{
		var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();

		Assert("Pre condition", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

		var url = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.MENTAgedScoreExtraction, extraction.PK);

		Assert(!string.IsNullOrEmpty(url));
	}

	public void TestCloneExtraction()
	{
		var extraction = MENTTestHelper.CreateExtraction(Factory, "CarPark");
		var additionalExtractionLink = MENTTestHelper.CreateAdditionalExtractionLink(extraction, extraction.RelatedQuery, extraction);
		extraction.AggregationType = ExtractionTypes.Codes.Count;
		extraction.CollectionColumn = MENTColumns.Codes.Score;

		var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
		componentColumn.Sequence = 99;

		var scoreColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);
		scoreColumn.Sequence = 99;

		var filter = extraction.SeriesFilter;
		AddSeriesFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
		{
			FilterStripName = "Time recorded",
			FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days,
		});

		Factory.Save();

		var copy = (MENTAgedScoreExtraction)extraction.Clone();
		AssertNotEquals(extraction.SeriesFilter.PK, copy.SeriesFilter.PK);
		AssertEquals(extraction.MEX_Name + " Copy", copy.MEX_Name);
		AssertEquals(extraction.AggregationType, copy.AggregationType);
		AssertEquals(extraction.CollectionColumn, copy.CollectionColumn);

		var copyComponentColumn = copy.SeriesColumns.Cast<SQLColumnSpecification>().Single(c => c.Selected);
		var copyScoreColumn = copy.CategoryColumns.Cast<SQLColumnSpecification>().Single(c => c.Selected);

		AssertEquals(scoreColumn.Sequence, copyScoreColumn.Sequence);
		AssertEquals(componentColumn.Sequence, copyComponentColumn.Sequence);
		var copyAdditionalExtractionLink = copy.AdditionalExtractions.Cast<AdditionalExtractionLink>().Single();
		AssertEquals(extraction.PK, copyAdditionalExtractionLink.ExtractionPK);
		AssertEquals(additionalExtractionLink.QueryPK, copyAdditionalExtractionLink.QueryPK);
		AssertEquals(copy, copyAdditionalExtractionLink.BaseExtraction);

		AssertEquals(extraction.SeriesFilter.S9_FilterData, copy.SeriesFilter.S9_FilterData);
		AssertEquals(extraction.SeriesFilter.S9_ColumnLayoutData, copy.SeriesFilter.S9_ColumnLayoutData);
		AssertEquals(extraction.SeriesFilter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper()).S0_FilterDataValues, copy.SeriesFilter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper()).S0_FilterDataValues);
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

	#region BusinessObjectTestCaseOverrides

	protected override BusinessObject GetNewBusinessObject()
	{
		var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
		var query = extraction.RelatedQuery;
		query.MAQ_Code = "CHARLIE";

		Factory.Save();

		return extraction;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var extraction = factory.NewWithValidTestData<MENTAgedScoreExtraction>();
		var query = extraction.RelatedQuery;
		query.MAQ_Code = "SNOOPY";

		return extraction;
	}

	protected override IEnumerable<string> XmlMemberNames
	{
		get
		{
			yield return "AdditionalExtractions";
			yield return "AggregationType";
			yield return "CategoryColumns";
			yield return "CollectionColumn";
			yield return "IsInstantaneous";
			yield return "SeriesColumns";
		}
	}

	#endregion
}

[TestedType(typeof(MENTAgedScoreExtraction))]
public class MENTAgedScoreExtractionRelatedFilterTest : RelatedModuleFilterSupportableTestCase<MENTAgedScoreExtraction>
{
	protected override IEnumerable<FilterRuleTestSet> GetFilterRules(MENTAgedScoreExtraction businessObject)
	{
		return new[] { new FilterRuleTestSet(null, () => businessObject.SeriesFilter, "SeriesFilterBusinessObject") };
	}

	protected override void ValidateBusinessObject(MENTAgedScoreExtraction businessObject)
	{
		businessObject.Validation.ValidateAll();
	}

	protected override MENTAgedScoreExtraction GetNewBusinessObject()
	{
		var extraction = base.GetNewBusinessObject();
		var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
		extraction.MEX_MAQ = query.PK;
		extraction.MEX_Name = "Filter Test";

		return extraction;
	}

	protected override string FilterDescriptionForValidationTest => "Staff Code";
}
