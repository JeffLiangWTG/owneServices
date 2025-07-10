namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class WUVCalculatedMergeTest : MergeOnAddInfoValueTestCase
	{
		protected override void SetPropertyWithValue1(JobComInvoiceLine line)
		{
			AssertNotNull("Currency", line.InvoiceHeader.Invoice_Currency);
			line.JI_IsPackToBondForLine = true;
			line.JI_CustomsUnitQty = "KG";
			line.JI_CustomsQuantity = 10;
			line.JI_LinePrice = 100m;
			AssertEquals("CustomsValue", 100m, line.JI_CustomsValue);
			AssertEquals("WUV", 10m, line.WUV);
		}

		protected override void SetPropertyWithValue2(JobComInvoiceLine line)
		{
			AssertNotNull("Currency", line.InvoiceHeader.Invoice_Currency);
			line.JI_IsPackToBondForLine = true;
			line.JI_CustomsUnitQty = "KG";
			line.JI_CustomsQuantity = 10;
			line.JI_LinePrice = 200m;
			AssertEquals("CustomsValue", 200m, line.JI_CustomsValue);
			AssertEquals("WUV", 20m, line.WUV);
		}
	}
}
