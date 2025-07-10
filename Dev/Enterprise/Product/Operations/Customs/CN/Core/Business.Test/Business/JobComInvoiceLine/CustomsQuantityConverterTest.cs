using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsQuantityConverterTest : BaseCustomsQuantityConverterTest
	{
		public void TestCalculateFromNetWeightToCustomsQtyCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var converter = new CustomsQuantityConverter(invoiceLine, invoiceLine.JI_CustomsQuantityInfo, invoiceLine.JI_CustomsUnitQtyInfo);
			invoiceLine.JI_NetWeight = 1.01;
			invoiceLine.JI_NetWeightUQ = "T";
			invoiceLine.JI_CustomsUnitQty = "035";
			AssertEquals(1010m, converter.CalculateFromNetWeightToCustomsQtyCore());
		}

		public override void TestCalculateCustomsQuantityWhenNetWeightIsPresent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "056";
			invoiceLine.JI_NetWeightUQ = "T";
			invoiceLine.JI_NetWeight = 1m;
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_CustomsUnitQty = "035";
			AssertEquals(1000m, invoiceLine.JI_CustomsQuantity);
		}
	}
}
