using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class GoodsShipmentItemProviderTest : DataProviderTestCase<GoodsShipmentItemProvider>
	{
		public void TestDeclarationGoodsItemNumber()
		{
			SetUpTestData();
			AssertNull("Declaration Goods Item Number null", goodsShipmentItemProvider.DeclarationGoodsItemNumber);
		}

		public void TestStatisticalValue()
		{
			SetUpTestData();
			AssertEquals("Statistical Value", 0m, goodsShipmentItemProvider.StatisticalValue);
		}

		public void TestNatureOfTransaction()
		{
			SetUpTestData();
			AssertNull("Nature Of Transaction", goodsShipmentItemProvider.NatureOfTransaction);
		}

		public void TestReferenceNumberUCR()
		{
			SetUpTestData();
			AssertNull("Reference Number UCR", goodsShipmentItemProvider.ReferenceNumberUCR);
		}

		public void TestDateOfAcceptance()
		{
			SetUpTestData();
			AssertEquals("Date Of Acceptance", DateTime.MinValue, goodsShipmentItemProvider.DateOfAcceptance);
		}

		public void TestAuthorisations()
		{
			SetUpTestData();
			AssertEquals("Authorisations", 0, goodsShipmentItemProvider.Authorisations.Count);
		}

		public void TestProcedure()
		{
			SetUpTestData();
			AssertEquals("Count of Additional Procedures", 1, goodsShipmentItemProvider.Procedure.AdditionalProcedure.Count);
			Assert("An Empty Additional Procedure exists", goodsShipmentItemProvider.Procedure.AdditionalProcedure.Any(x => string.IsNullOrEmpty(x.AdditionalProcedure)));

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C08;
			goodsShipmentItemProvider = new GoodsShipmentItemProvider(packedItem);

			CombineAssertions(() =>
			{
				AssertEquals("Count of Additional Procedures", 1, goodsShipmentItemProvider.Procedure.AdditionalProcedure.Count);
				Assert("Additional Procedure exists", goodsShipmentItemProvider.Procedure.AdditionalProcedure.Any(x => x.AdditionalProcedure == "C08"));
			});

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C07F48;
			goodsShipmentItemProvider = new GoodsShipmentItemProvider(packedItem);

			CombineAssertions(() =>
			{
				AssertEquals("Count of Additional Procedures", 2, goodsShipmentItemProvider.Procedure.AdditionalProcedure.Count);
				Assert("Additional Procedure exists", goodsShipmentItemProvider.Procedure.AdditionalProcedure.Any(x => x.AdditionalProcedure == "C07"));
				Assert("Additional Procedure exists", goodsShipmentItemProvider.Procedure.AdditionalProcedure.Any(x => x.AdditionalProcedure == "F48"));
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			SetUpTestData();
			AssertEquals("Additional Supply Chain Actors", 0, goodsShipmentItemProvider.AdditionalSupplyChainActors.Count);
		}

		public void TestBuyer()
		{
			SetUpTestData();
			AssertNull("Buyer", goodsShipmentItemProvider.Buyer);
		}

		public void TestSeller()
		{
			SetUpTestData();
			AssertNull("Seller", goodsShipmentItemProvider.Seller);
		}

		public void TestExporter()
		{
			SetUpTestData();
			bill.ABL_ShipperName = "Exporter1";
			goodsShipmentItemProvider = new GoodsShipmentItemProvider(packedItem);

			AssertEquals("Name", "Exporter1", goodsShipmentItemProvider.Exporter.Name);

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "Exporter2";

			bill.ABL_OA_Shipper = orgAddress.PK;
			goodsShipmentItemProvider = new GoodsShipmentItemProvider(packedItem);

			AssertType<BillPartyProvider>(goodsShipmentItemProvider.Exporter);
			AssertEquals("Name", "Exporter2", goodsShipmentItemProvider.Exporter.Name);

			AssertNull("Name", GetProviderWithNullPack().Exporter);
		}

		public void TestOrigin()
		{
			SetUpTestData();
			AssertNull("Origin", goodsShipmentItemProvider.Origin);
		}

		public void TestCountryOfDispatch()
		{
			SetUpTestData();
			AssertNull("CountryOfDispatch", goodsShipmentItemProvider.CountryOfDispatch);
		}

		public void TestMemberStateTerritory()
		{
			SetUpTestData();
			AssertNull("MemberStateTerritory", goodsShipmentItemProvider.MemberStateTerritory);
		}

		public void TestDestination()
		{
			SetUpTestData();
			AssertNull("Destination", goodsShipmentItemProvider.Destination);
		}

		public void TestCommodity()
		{
			SetUpTestData();
			AssertNull("Destination", goodsShipmentItemProvider.Destination);
		}

		public void TestPackages()
		{
			SetUpTestData();
			AssertNotNull("Packages", goodsShipmentItemProvider.Packages);
			AssertEquals("Packages Count", 1, goodsShipmentItemProvider.Packages.Count);
		}

		public void TestPreviousDocuments()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentItemProvider.PreviousDocuments);
			AssertEquals(1, goodsShipmentItemProvider.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentItemProvider.SupportingDocuments);
			AssertEquals(1, goodsShipmentItemProvider.SupportingDocuments.Count);
		}

		public void TestTransportDocuments()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentItemProvider.TransportDocuments);
			AssertEquals(1, goodsShipmentItemProvider.TransportDocuments.Count);
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentItemProvider.AdditionalInformations);
			AssertEquals(1, goodsShipmentItemProvider.AdditionalInformations.Count);
		}

		public void TestAdditionalReferences()
		{
			SetUpTestData();
			AssertNotNull("AdditionalReferences", goodsShipmentItemProvider.AdditionalFiscalReferences);
			AssertEquals("Additional References", 1, goodsShipmentItemProvider.AdditionalReferences.Count);
		}

		public void TestCustomsValuation()
		{
			SetUpTestData();
			AssertNull("Customs Valuation", goodsShipmentItemProvider.CustomsValuation);
		}

		public void TestValuationAdjustment()
		{
			SetUpTestData();
			AssertNull("Valuation Adjustment", goodsShipmentItemProvider.ValuationAdjustment);
		}

		public void TestAdditionalFiscalReferences()
		{
			SetUpTestData();
			AssertEquals("Additional Fiscal References", 1, goodsShipmentItemProvider.AdditionalFiscalReferences.Count);
			var additionalFiscalReference = goodsShipmentItemProvider.AdditionalFiscalReferences.ElementAt(0);
			AssertEquals("1", additionalFiscalReference.SequenceNumber);
			AssertEquals("FR5", additionalFiscalReference.Role);
			AssertEquals("TestRegNo", additionalFiscalReference.VatIdentificationNumber);
		}

		public void TestTransportAndInsuranceCostsToTheDestination()
		{
			SetUpTestData();
			var transportAndInsuranceCostsToTheDestination = goodsShipmentItemProvider.TransportAndInsuranceCostsToTheDestination;
			CombineAssertions(() =>
			{
				AssertNotNull("Transport And Insurance Costs To The Destination section exists", transportAndInsuranceCostsToTheDestination);
				AssertEquals(0m, transportAndInsuranceCostsToTheDestination.Amount);
			});

			bill.ABL_TransportValue = 10;
			bill.ABL_InsuranceValue = 10;
			bill.ABL_RX_NKTransportValueCurrency = "USD";
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			packedItem.Bill.PackedItems.AddNew();

			goodsShipmentItemProvider = new GoodsShipmentItemProvider(packedItem);
			CombineAssertions(() =>
			{
				AssertNotNull("Transport And Insurance Costs To The Destination section exists", goodsShipmentItemProvider.TransportAndInsuranceCostsToTheDestination);
				AssertEquals("Transport And Insurance Costs To The Destination Amount", 10m, goodsShipmentItemProvider.TransportAndInsuranceCostsToTheDestination.Amount);
				AssertEquals("Transport And Insurance Costs To The Destination Currency", "USD", goodsShipmentItemProvider.TransportAndInsuranceCostsToTheDestination.Currency);
			});
		}

		public void TestAdditionalFiscalReferences_PackedItemIsNull()
		{
			SetUpTestData();
			var newPackedItem = bill.PackedItems.AddNew();
			var newProvider = new GoodsShipmentItemProvider(newPackedItem);

			CombineAssertions("Case when PackedItem.Pack is null", () =>
			{
				AssertNull("To make sure Pack is null.", newPackedItem.Pack);
				AssertNoExceptionThrown("AdditionalFiscalReferences should not throw exception with null Pack.", () => _ = newProvider.AdditionalFiscalReferences);
			});
		}

		public void TestContainerIds()
		{
			SetUpTestData();
			AssertEquals("Container Ids", 0, goodsShipmentItemProvider.ContainerIds.Count);
		}

		void SetUpTestData()
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_SellerRegNo = "TestRegNo";

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 100;

			packedItem = bill.PackedItems.AddNew();
			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);

			packedItem.PreviousDocuments.AddNew();
			packedItem.SupportingDocuments.AddNew();
			var additionalDocument1 = packedItem.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var additionalDocument2 = packedItem.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var additionalDocument3 = packedItem.AdditionalDocuments.AddNew();
			additionalDocument3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;

			goodsShipmentItemProvider = new GoodsShipmentItemProvider(packedItem);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		AsycudaPack pack;
		AsycudaPackedItem packedItem;
		GoodsShipmentItemProvider goodsShipmentItemProvider;

		protected override GoodsShipmentItemProvider GetProvider()
		{
			SetUpTestData();
			return goodsShipmentItemProvider;
		}

		GoodsShipmentItemProvider GetProviderWithNullPack() => new GoodsShipmentItemProvider(Factory.New<AsycudaPackedItem>());
	}
}
