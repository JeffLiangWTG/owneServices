using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemTypePartiesProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemTypePartiesProvider>
	{
		public void TestExporter()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var orgAddress = orgHeader.MainAddress;
			invoiceLine.JI_OA_ExporterAddress = orgAddress.PK;
			AssertEquals("Exporter", "IEE007", Provider.Exporter.ID);
		}

		public void TestSeller()
		{
			SetUpTestData();
			invoiceLine.SellerDocAddress.E2_AddressOverride = true;
			invoiceLine.SellerDocAddress.E2_GovRegNum = "IETEST";
			AssertEquals("Seller", "IETEST", Provider.Seller.ID);
		}

		public void TestBuyer()
		{
			SetUpTestData();
			invoiceLine.BuyerDocAddress.E2_AddressOverride = true;
			invoiceLine.BuyerDocAddress.E2_GovRegNum = "IETEST";
			AssertEquals("Buyer", "IETEST", Provider.Buyer.ID);
		}

		public void TestSupplyChainActor()
		{
			SetUpTestData();
			var supplyChainActorReference = Factory.New<EU.Business.Declaration.CusSupplyChainActorReference>();
			supplyChainActorReference.CFR_Code = "CS";
			supplyChainActorReference.CFR_Reference = "123";
			invoiceLine.CusSupplyChainActorReferences.Add(supplyChainActorReference);
			AssertEquals("Code", "CS", Provider.SupplyChainActor.SingleOrDefault().Role);
			AssertEquals("Reference", "123", Provider.SupplyChainActor.SingleOrDefault().ID);
		}

		public void TestAdditionalFiscalReference()
		{
			SetUpTestData();
			var fiscalReference = Factory.New<CusFiscalReference>();
			fiscalReference.CFR_Code = "CS";
			fiscalReference.CFR_Reference = "123";
			invoiceLine.FiscalReferences.Add(fiscalReference);
			AssertEquals("Code", "CS", Provider.AdditionalFiscalReference.SingleOrDefault().Role);
			AssertEquals("Reference", "123", Provider.AdditionalFiscalReference.SingleOrDefault().ID);
		}

		protected override IM413AndIM415GoodsShipmentItemTypePartiesProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemTypePartiesProvider(invoiceLine);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
