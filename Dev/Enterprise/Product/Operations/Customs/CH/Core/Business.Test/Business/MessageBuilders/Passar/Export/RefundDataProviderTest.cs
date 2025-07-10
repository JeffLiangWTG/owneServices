namespace Enterprise.Customs.CH.Business.Testing;

sealed class RefundDataProviderTest : BasePassarDataProviderTest<RefundDataProvider>
{
	public void TestNewNull() => AssertNull(RefundDataProvider.New(null));

	public void TestNewInstance() => AssertNotNull(RefundDataProvider.New(InvoiceLine));

	public void TestProperties() => CombineAssertions(() =>
	{
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;
		InvoiceLine.JI_RefundReferenceNumber = "24CH12345678901238";
		InvoiceLine.JI_RefundGoodsItemNumber = 1;
		InvoiceLine.JI_RefundReason = "Reason Text";

		AssertEquals("GDRN", "24CH12345678901238", DataProvider.GoodsDeclarationReferenceNumber);
		AssertEquals("Goods Item Number", 1, DataProvider.GoodsItemNumber);
		AssertEquals("Reason", "Reason Text", DataProvider.Reason);
	});

	protected override RefundDataProvider CreateDataProvider()	=> RefundDataProvider.New(InvoiceLine);
}
