using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentCodeSubTypeListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When combinationsProvider is null", () => new PreviousDocumentCodeSubTypeListProvider(combinationsProvider: null));
	}

	public void TestGetPreviousDocumentCodeList()
	{
		var combinationItems = GetCombinationsWithMoreThanOneTemplate();
		var combinationsProvider = GetCombinationsProvider(combinationItems);
		var documentBehaviourExtender = new PreviousDocumentCodeSubTypeListProvider(combinationsProvider);
		var previousDocumentCodeList = documentBehaviourExtender.GetPreviousDocumentCodeList();
		AssertEquals("CodesAsString", "270, 820, ZZZ", previousDocumentCodeList.CodesAsString);
	}

	public void TestGetPreviousDocumentSubTypeList()
	{
		var combinationItems = GetCombinationsWithMoreThanOneTemplate();
		var combinationsProvider = GetCombinationsProvider(combinationItems);
		var documentBehaviourExtender = new PreviousDocumentCodeSubTypeListProvider(combinationsProvider);
		var previousDocumentSubTypeList = documentBehaviourExtender.GetPreviousDocumentSubTypeList();
		AssertEquals("CodesAsString", "X, Z", previousDocumentSubTypeList.CodesAsString);
	}

	IPreviousDocumentCombinationsProvider GetCombinationsProvider(IReadOnlyCollection<PreviousDocumentCombinationItem> combinationItems)
	{
		var providerMock = new Mock<IPreviousDocumentCombinationsProvider>();
		providerMock.Setup(x => x.GetAllowedPreviousDocumentCombinations()).Returns(() => combinationItems);
		return providerMock.Object;
	}

	IReadOnlyCollection<PreviousDocumentCombinationItem> GetCombinationsWithMoreThanOneTemplate()
	{
		var combinations = new List<PreviousDocumentCombinationItem>();
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC1", "270", "X", PreviousDocumentCombinationTemplate._1));
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC2", "820", "Z", PreviousDocumentCombinationTemplate._2));
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC3", "ZZZ", "Z", PreviousDocumentCombinationTemplate._4));
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC4", "ZZZ", "Z", PreviousDocumentCombinationTemplate._5));
		return combinations.AsReadOnly();
	}
}
