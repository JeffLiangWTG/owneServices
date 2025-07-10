namespace Enterprise.Customs.CH.Business.Testing;

class HouseConsignmentDataProviderTest : BasePassarDataProviderTest<HouseConsignmentDataProvider>
{
	public void TestConstructorNullArgument()
	{
		AssertNull(HouseConsignmentDataProvider.New(null));
	}

	public void TestConsignmentItems() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.ConsignmentItems);
		AssertSame("cached", DataProvider.ConsignmentItems, DataProvider.ConsignmentItems);
	});

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertNull("SequenceNumber", DataProvider.SequenceNumber);
		AssertNull("ReferenceNumberUCR", DataProvider.ReferenceNumberUCR);
		AssertNull("CountryOfDispatch", DataProvider.CountryOfDispatch);
		AssertNull("CountryOfDestination", DataProvider.CountryOfDestination);
		AssertNull("Consignor", DataProvider.Consignor);
		AssertNull("Consignee", DataProvider.Consignee);
		AssertNull("PreviousDocument", DataProvider.PreviousDocuments);
		AssertNull("SupportingDocument", DataProvider.SupportingDocuments);
		AssertNull("TransportDocument", DataProvider.TransportDocuments);
		AssertNull("AdditionalInformation", DataProvider.AdditionalInformations);
		AssertNull("AdditionalReference", DataProvider.AdditionalReferences);
		AssertNull("Packagings", DataProvider.Packagings);
	});

	protected override HouseConsignmentDataProvider CreateDataProvider() => HouseConsignmentDataProvider.New(EntryHeader);
}
