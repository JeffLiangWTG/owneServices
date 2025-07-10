using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class TaxInvoiceTradeLineItemTypeBuilderTest : TestCaseWithFactory
	{
		public void TestBuildXML()
		{
			var builder = new TaxInvoiceTradeLineItemTypeBuilder("TestNameSpace", new TaxInvoiceTradeLineItem
			{
				ReverseDate = new ZDateTime(2022, 03, 02),
				Sequence = 1,
				DescriptionText = "AAA",
				InvoiceAmount = "1001",
				CalculatedAmount = "1",
			});
			var expectedXmlResult = @"<TaxInvoiceTradeLineItem xmlns=""TestNameSpace"">
  <SequenceNumeric>1</SequenceNumeric>
  <InvoiceAmount>1001</InvoiceAmount>
  <NameText>AAA</NameText>
  <PurchaseExpiryDateTime>20220302</PurchaseExpiryDateTime>
  <TotalTax>
    <CalculatedAmount>1</CalculatedAmount>
  </TotalTax>
</TaxInvoiceTradeLineItem>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceTradeLineItem").ToString());
		}
	}
}
