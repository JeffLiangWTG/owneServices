using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend43ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend43ConsignmentHouseLevelProvider>
	{
		public void TestNewOrNull()
		{
			CombineAssertions(() =>
			{
				AssertNull("Bill missing", SendAndAmend43ConsignmentHouseLevelProvider.NewOrNull(null));
				AssertNotNull(SendAndAmend43ConsignmentHouseLevelProvider.NewOrNull(bill));
			});
		}

		public void TestTotalGrossMass()
		{
			bill.ABL_GrossWeight = 1987m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 1.987m, Provider.TotalGrossMass);
		}

		public void TestAdditionalFiscalReferences()
		{
			var fiscalReference = bill.AdditionalFiscalReferences.AddNew();
			fiscalReference.CFR_Reference = "FR5Reference";
			fiscalReference.CFR_Code = "FR5";

			AssertType<AdditionalFiscalReferenceProvider>("AdditionalFiscalReference", Provider.AdditionalFiscalReferences);
			AssertEquals("Reference", "FR5Reference", Provider.AdditionalFiscalReferences.Identifier);
			AssertEquals("Code", "FR5", Provider.AdditionalFiscalReferences.Type);
		}
		public void TestSupportingDocumentsHouseLevel()
		{
			bill.SupportingDocuments.AddNew();
			AssertEquals("SupportingDocumentsHouseLevel Count", 1, Provider.SupportingDocumentsHouseLevel.Count);
		}

		public void TestAdditionalInformationCollection()
		{
			bill.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestAdditionalSupplyChainActors()
		{
			bill.CusSupplyChainActorReferences.AddNew();
			AssertEquals("AdditionalSupplyChainActors", 1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestConsignee()
		{
			bill.ABL_ConsigneeName = "TestConsignee";
			AssertType<BillPartyProvider>("Consignee", Provider.Consignee);
			AssertEquals("Consignee", "TestConsignee", Provider.Consignee.Name);
		}

		public void TestGoodsItems()
		{
			bill.Packs.AddNew();
			AssertEquals("GoodsItem", 1, Provider.GoodsItems.Count);
		}

		public void TestConsignor()
		{
			bill.ABL_ShipperName = "TestConsignor";
			AssertType<BillPartyProvider>("Consignor", Provider.Consignor);
			AssertEquals("Consignor", "TestConsignor", Provider.Consignor.Name);
		}

		public void TestPostalCharges()
		{
			bill.ABL_FreightValue = 10.00m;
			bill.ABL_RX_NKFreightValueCurrency = "EUR";

			AssertEquals("PostalCharges Value", 10.00m, Provider.PostalCharges.Value);
			AssertEquals("PostalCharges Currency", "EUR", Provider.PostalCharges.Currency);
		}

		public void TestTransportDocumentHouseLevel()
		{
			bill.ABL_BillNumber = "TestBillNumber";
			AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
		}

		public void TestUCRNumber()
		{
			const string expected = "1234";
			bill.ABL_UCRNumber = expected;

			AssertEquals(expected, Provider.UCRNumber);
		}

		SendAndAmend43ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend43ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend43ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
