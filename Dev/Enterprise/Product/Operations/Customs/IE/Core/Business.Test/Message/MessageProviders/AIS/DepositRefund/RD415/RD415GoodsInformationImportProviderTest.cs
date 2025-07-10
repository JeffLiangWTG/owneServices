using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415GoodsInformationImportProviderTest : DataProviderTestCase<RD415GoodsInformationImportProvider>
	{
		public void TestIRD415GoodsInformationImport()
		{
			Assert("Should implement IRD415GoodsInformationImport", Provider is IRD415GoodsInformationImport);
		}

		public void TestCommodityCode()
		{
			var commodityCode = Provider.CommodityCode;
			AssertType<GoodsInformationOtherTypeCommodityCodeProvider>(commodityCode);
			AssertSame("Cached", commodityCode, Provider.CommodityCode);
		}

		public void TestGoodsQuantity()
		{
			AssertNull(Provider.GoodsQuantity);
		}

		public void TestValueOfGoods()
		{
			AssertEquals(0m, Provider.ValueOfGoods);

			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_LinePrice = 25000m;
			AssertEquals(25000m, GetProvider().ValueOfGoods);

			invoiceLine2.JI_LinePrice = 752.25m;
			AssertEquals(25752.25m, GetProvider().ValueOfGoods);
		}

		public void TestGoodsDescription()
		{
			AssertEquals(string.Empty, Provider.GoodsDescription);

			entryLine.CL_Description = "Test Goods";
			AssertEquals("Test Goods", Provider.GoodsDescription);
		}

		protected override RD415GoodsInformationImportProvider GetProvider()
		{
			SetUpTestData();
			return new RD415GoodsInformationImportProvider(entryLine);
		}

		void SetUpTestData()
		{
			if (entryLine == null)
			{
				declaration = Factory.New<JobDeclaration>();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2 = invoice.InvoiceLines.AddNew();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine2.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine invoiceLine2;
	}
}
