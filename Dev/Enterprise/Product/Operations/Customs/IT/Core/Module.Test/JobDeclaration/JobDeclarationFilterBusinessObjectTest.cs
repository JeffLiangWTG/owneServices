using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(JobDeclarationFilterBusinessObject))]
sealed class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestLookups()
	{
		var filterBizObj = new JobDeclarationFilterBusinessObject();
		AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
	}

	public void TestMessageVersionFilter()
	{
		var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
		var filter = filterBizObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.MessageVersion];

		AssertNotNull("Message Version Filter", filter);
		CombineAssertions("Message Version Filter", () =>
		{
			AssertType<ModuleTextFilter>("Type", filter);
			AssertEquals("Category", FilterCategories.ModesAndTypes, filter.Category);
			AssertEquals("Description", "Message Version", filter.Description);
			AssertEquals("MaxLength", 3, filter.MaxLength);
			AssertSame("MessageVersionList", filterBizObj.Lookups.MessageVersionList, ((ModuleTextFilter)filter).List);
		});
	}

	public void TestMessageVersionFilterComparisonOperators()
	{
		var expectedComparisonOperators = new[]
		{
			ModuleTextFilter.ComparisonConstants.Exact,
			ModuleTextFilter.ComparisonConstants.NotEqual,
			ModuleTextFilter.ComparisonConstants.IsBlank,
			ModuleTextFilter.ComparisonConstants.IsNotBlank,
		};

		var filterBizObj = GetNewFilterStripBusinessObject();
		var filter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.MessageVersion];
		AssertContainsExactElementsInExactOrder("ComparisonOperator_List", expectedComparisonOperators, filter.ComparisonOperator_List.GetAllCodesZString());
	}

	public void TestMessageVersionFilterSearchResult()
	{
		var declarationImp = Factory.New<JobDeclaration>();
		declarationImp.JE_MessageType = "IMP";

		var declarationExpTxt = Factory.New<JobDeclaration>();
		declarationExpTxt.JE_MessageType = "EXP";
		declarationExpTxt.MessageVersion = "TXT";

		var declarationExpXml = Factory.New<JobDeclaration>();
		declarationExpXml.JE_MessageType = "EXP";
		declarationExpXml.MessageVersion = "XML";

		var declarationExpXml2 = Factory.New<JobDeclaration>();
		declarationExpXml2.JE_MessageType = "EXP";
		declarationExpXml2.MessageVersion = "XML";
		Factory.Save();

		var filterBizObj = GetNewFilterStripBusinessObject();
		var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		CombineAssertions("When filter not applied", () =>
		{
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 4, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationImp, declarationExpTxt, declarationExpXml, declarationExpXml2 }, declarationCollection);
		});

		var filter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.DeclarationFilterConstants.MessageVersion];
		filter.IsActive = true;

		CombineAssertions("When filter applied, Comparison Exact - TXT", () =>
		{
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "TXT";
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 1, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationExpTxt }, declarationCollection);
		});

		CombineAssertions("When filter applied, Comparison Exact - XML", () =>
		{
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "XML";
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 2, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationExpXml, declarationExpXml2 }, declarationCollection);
		});

		CombineAssertions("When filter applied, Comparison NotEqual - TXT", () =>
		{
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = "TXT";
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 3, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationImp, declarationExpXml, declarationExpXml2 }, declarationCollection);
		});

		CombineAssertions("When filter applied, Comparison NotEqual - XML", () =>
		{
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = "XML";
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 2, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationImp, declarationExpTxt }, declarationCollection);
		});

		CombineAssertions("When filter applied, Comparison IsBlank", () =>
		{
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 1, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationImp }, declarationCollection);
		});

		CombineAssertions("When filter applied, Comparison IsNotBlank", () =>
		{
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			declarationCollection.Load(filterBizObj.Filter);
			AssertEquals("Count", 3, declarationCollection.Count);
			AssertContainsExactElementsInAnyOrder("Result", new[] { declarationExpTxt, declarationExpXml, declarationExpXml2 }, declarationCollection);
		});
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new JobDeclarationFilterBusinessObject();
	}
}
