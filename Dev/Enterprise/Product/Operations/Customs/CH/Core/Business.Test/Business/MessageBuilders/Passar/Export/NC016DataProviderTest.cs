namespace Enterprise.Customs.CH.Business.Testing;

sealed class NC016DataProviderTest : BasePassarMessageDataProviderTest<NC016DataProvider>
{
	public void TestExportOperationGDRN()
	{
		Declaration.ActiveEntryHeaders[0].MovementReferenceNumberSetter("23BE14785692745688.2");
		AssertEquals("GDRN", "23BE14785692745688.2", DataProvider.ExportOperationGDRN);
	}

	public void TestTransitOperationMRN() => AssertEquals("not mapped", string.Empty, DataProvider.TransitOperationMRN);

	public void TransportOperationJRN() => AssertEquals("not mapped", string.Empty, DataProvider.TransportOperationJRN);

	protected override NC016DataProvider CreateDataProvider() => new NC016DataProvider(SendingObject);
}
