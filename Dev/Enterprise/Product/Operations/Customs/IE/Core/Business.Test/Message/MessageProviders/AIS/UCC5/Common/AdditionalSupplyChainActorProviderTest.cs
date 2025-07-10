using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class AdditionalSupplyChainActorProviderTest : DataProviderTestCase<AdditionalSupplyChainActorProvider>
	{
		public void TestRole()
		{
			SetUpTestData();
			supplyChainActorReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			supplyChainActorReference.CFR_Reference = "REF001";
			AssertEquals("AdditionalSupplyChainActorProvider Role", FiscalReferenceCodeList.Codes.FR1_Importer, Provider.Role);
		}

		public void TestID()
		{
			SetUpTestData();
			supplyChainActorReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			supplyChainActorReference.CFR_Reference = "REF001";
			AssertEquals("AdditionalSupplyChainActorProvider ID", "REF001", Provider.ID);
		}

		protected override AdditionalSupplyChainActorProvider GetProvider()
		{
			SetUpTestData();
			return new AdditionalSupplyChainActorProvider(supplyChainActorReference);
		}

		void SetUpTestData()
		{
			if (supplyChainActorReference == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				supplyChainActorReference = invoiceLine.CusSupplyChainActorReferences.AddNew();
			}
		}

		EU.Business.Declaration.CusSupplyChainActorReference supplyChainActorReference;
	}
}
