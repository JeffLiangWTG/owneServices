using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415GoodsInformationExportProviderTest : DataProviderTestCase<RD415GoodsInformationExportProvider>
	{
		public void TestIRD415GoodsInformationExport()
		{
			Assert("Should implement IRD415GoodsInformationExport", Provider is IRD415GoodsInformationExport);
		}

		public void TestGoodsDescription()
		{
			SetUpTestData();
			invoiceLine.JI_Description = "Description";
			AssertEquals("Description", Provider.GoodsDescription);

			invoiceLine.JI_Description = "Goods Description";
			AssertEquals("Goods Description", Provider.GoodsDescription);
		}

		public void TestCommodityCode()
		{
			var commodityCode = Provider.CommodityCode;
			AssertType<GoodsInformationOtherTypeCommodityCodeProvider>(commodityCode);
			AssertSame("Cached", commodityCode, Provider.CommodityCode);
		}

		public void TestNetMass()
		{
			SetUpTestData();
			invoiceLine.JI_CustomsQuantity = 100;
			AssertEquals(100m, Provider.NetMass);

			invoiceLine2.JI_CustomsQuantity = 0.75;
			AssertEquals(100.75m, GetProvider().NetMass);
		}

		public void TestSupplementaryUnits()
		{
			SetUpTestData();
			invoiceLine.JI_CustomsSecondQuantity = 50;
			AssertEquals(50m, Provider.SupplementaryUnits);

			invoiceLine2.JI_CustomsSecondQuantity = 12.5;
			AssertEquals(62.5m, GetProvider().SupplementaryUnits);
		}

		protected override RD415GoodsInformationExportProvider GetProvider()
		{
			SetUpTestData();
			return new RD415GoodsInformationExportProvider(entryLine);
		}

		void SetUpTestData()
		{
			if (entryLine == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine2.JI_CL = entryLine.PK;
			}
		}

		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine invoiceLine2;
	}
}
