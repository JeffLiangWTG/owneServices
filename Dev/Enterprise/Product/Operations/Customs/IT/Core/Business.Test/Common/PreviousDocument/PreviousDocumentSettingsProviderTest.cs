using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentSettingsProviderTest : TestCaseWithFactory
{
	public void TestGetFieldsInfoWhenOnlyOneTemplateIsFound()
	{
		var combinationItems = GetCombinationsWithOnlyOneTemplate();
		var combinationsProvider = GetCombinationsProvider(combinationItems);
		var documentBehaviourExtender = new PreviousDocumentSettingsProvider(combinationsProvider);
		var settings = documentBehaviourExtender.GetSettings();

		CombineAssertions(() =>
		{
			AssertEquals("IsReferenceNumberEditable", true, settings.IsReferenceNumberEditable);
			AssertEquals("IsReferenceNumber2Editable", false, settings.IsReferenceNumber2Editable);
			AssertEquals("IsDateOfIssueEditable", true, settings.IsDateOfIssueEditable);
			AssertEquals("IsLineNoEditable", false, settings.IsLineNoEditable);
			AssertEquals("IsStatusEditable", false, settings.IsStatusEditable);
			AssertEquals("IsCustomsOfficeEditable", false, settings.IsCustomsOfficeEditable);
		});
	}

	public void TestGetFieldsInfoWhenMoreThanOneTemplatesAreFound()
	{
		var combinationItems = GetCombinationsWithMoreThanOneTemplate();
		var combinationsProvider = GetCombinationsProvider(combinationItems);
		var documentBehaviourExtender = new PreviousDocumentSettingsProvider(combinationsProvider);
		var settings = documentBehaviourExtender.GetSettings();

		CombineAssertions(() =>
		{
			AssertEquals("IsReferenceNumberEditable", false, settings.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", false, settings.IsDateOfIssueEditable);
			AssertEquals("IsReferenceNumber2Editable", false, settings.IsReferenceNumber2Editable);
			AssertEquals("IsLineNoEditable", false, settings.IsLineNoEditable);
			AssertEquals("IsStatusEditable", false, settings.IsStatusEditable);
			AssertEquals("IsCustomsOfficeEditable", false, settings.IsCustomsOfficeEditable);
		});
	}

	IPreviousDocumentCombinationsProvider GetCombinationsProvider(IReadOnlyCollection<PreviousDocumentCombinationItem> combinationItems)
	{
		var providerMock = new Mock<IPreviousDocumentCombinationsProvider>();
		providerMock.Setup(x => x.GetAllowedPreviousDocumentCombinations()).Returns(() => combinationItems);
		providerMock.Setup(x => x.GetNewSettings(It.IsAny<PreviousDocumentCombinationTemplate?>())).Returns((PreviousDocumentCombinationTemplate? selectedTemplate) => new PreviousDocumentFieldsInfo(selectedTemplate));
		return providerMock.Object;
	}

	IReadOnlyCollection<PreviousDocumentCombinationItem> GetCombinationsWithOnlyOneTemplate()
	{
		var combinations = new List<PreviousDocumentCombinationItem>();
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC1", "270", "X", PreviousDocumentCombinationTemplate._1));
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC2", "820", "Z", PreviousDocumentCombinationTemplate._1));
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC3", "ZZZ", "Z", PreviousDocumentCombinationTemplate._1));
		combinations.Add(new PreviousDocumentCombinationItem(Factory, "PROC4", "ZZZ", "Z", PreviousDocumentCombinationTemplate._1));
		return combinations.AsReadOnly();
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
