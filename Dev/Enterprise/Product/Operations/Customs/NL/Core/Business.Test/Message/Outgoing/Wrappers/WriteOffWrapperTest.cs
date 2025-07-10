using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class WriteOffWrapperTest : DataProviderTestCase<WriteOffWrapper>
{
	public void TestQuantityQuantity()
	{
		supportingDocument.CSI_Quantity = 20;
		AssertEquals(20m, supportingDocumentWriteOffWrapper.QuantityQuantity);
		AssertEquals(0m, previousDocumentWriteOffWrapper.QuantityQuantity);
	}
	public void TestUnitCode()
	{
		supportingDocument.CSI_UnitOfQuantity = "UNT1";
		AssertEquals("UNT1", supportingDocumentWriteOffWrapper.UnitCode);
		AssertNull(previousDocumentWriteOffWrapper.UnitCode);
	}
	public void TestAmount()
	{
		supportingDocument.CSI_Value = 30;
		AssertEquals(30m, supportingDocumentWriteOffWrapper.Amount);
		AssertEquals(0m, previousDocumentWriteOffWrapper.Amount);
	}
	public void TestCurrencyId()
	{
		supportingDocument.CSI_RX_NKCurrency = "EUR";
		AssertEquals("EUR", supportingDocumentWriteOffWrapper.CurrencyId);
		AssertNull(previousDocumentWriteOffWrapper.CurrencyId);
	}
	public void TestPackaging()
	{
		AssertNull(supportingDocumentWriteOffWrapper.Packaging);
		AssertNull(previousDocumentWriteOffWrapper.Packaging);
	}
	protected override void SetUp()
	{
		base.SetUp();
		supportingDocument = Factory.New<SupportingDocument>();
		previousDocumentWriteOffWrapper = new WriteOffWrapper(null);
		supportingDocumentWriteOffWrapper = new WriteOffWrapper(supportingDocument);
	}
	SupportingDocument supportingDocument;
	WriteOffWrapper previousDocumentWriteOffWrapper;
	WriteOffWrapper supportingDocumentWriteOffWrapper;

	protected override WriteOffWrapper GetProvider() => previousDocumentWriteOffWrapper;
}
