using System.Linq;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(RestrictionAdditionalInformationDataProvider))]
sealed class RestrictionAdditionalInformationDataProviderTest : BasePassarDataProviderTest<RestrictionAdditionalInformationDataProvider>
{
	public void TestNewCollectionEmtpy() => AssertEquals("empty collection", Enumerable.Empty<RestrictionAdditionalInformationDataProvider>(), RestrictionAdditionalInformationDataProvider.NewCollection(null));

	public void TestNewCollectionWithRestrictionAdditionalInformation()
	{
		var additionalInformation1 = Restriction.AdditionalInformations.AddNew();
		additionalInformation1.CY_Code = "ABCD";
		additionalInformation1.CY_Data = "Sampe Text";
		additionalInformation1.CY_Order = 1;

		var additionalInformation2 = Restriction.AdditionalInformations.AddNew();
		additionalInformation2.CY_Code = "EFGH";
		additionalInformation2.CY_Data = "Sampe Text";
		additionalInformation2.CY_Order = 2;

		var dataProviders = RestrictionAdditionalInformationDataProvider.NewCollection(Restriction.AdditionalInformations);

		CombineAssertions(() =>
		{
			AssertEquals(2, dataProviders.Count());
			AssertEquals("Sequence at 1 index", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, dataProviders.ElementAt(1).SequenceNumber);
		});
	}

	public void TestProvider()
	{
		var restriction = Restriction;

		var additionalInformation = restriction.AdditionalInformations.AddNew();
		additionalInformation.CY_Code = "ABCD";
		additionalInformation.CY_Data = "Sampe Text";
		additionalInformation.CY_Order = 1;

		CombineAssertions(() =>
		{
			AssertEquals("Code", "ABCD", DataProvider.Code);
			AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
			AssertEquals("Text", "Sampe Text", DataProvider.Text);

			additionalInformation.CY_Data = string.Empty;
			AssertNull("Empty Text should be null", DataProvider.Text);
		});
	}

	Restriction Restriction => restriction ??= CreateRestriction();
	Restriction restriction;

	Restriction CreateRestriction()
	{
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		return invoiceLine.Restrictions.AddNew();
	}

	protected override RestrictionAdditionalInformationDataProvider CreateDataProvider() => RestrictionAdditionalInformationDataProvider.NewCollection(Restriction.AdditionalInformations).First();
}
