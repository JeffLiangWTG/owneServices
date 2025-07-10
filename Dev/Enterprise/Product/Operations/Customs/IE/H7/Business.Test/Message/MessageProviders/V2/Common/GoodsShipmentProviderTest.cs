using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class GoodsShipmentProviderTest : DataProviderTestCase<GoodsShipmentProvider>
	{
		public void TestNatureOfTransaction()
		{
			SetUpTestData();
			AssertNull("Nature Of Transaction", goodsShipmentProvider.NatureOfTransaction);
		}

		public void TestTotalAmountInvoiced()
		{
			SetUpTestData();
			AssertEquals("Total Amount Invoiced", 100m, goodsShipmentProvider.TotalAmountInvoiced);
		}

		public void TestInvoiceCurrency()
		{
			SetUpTestData();
			AssertEquals("Invoice Currency", "AUD", goodsShipmentProvider.InvoiceCurrency);
		}

		public void TestDateOfAcceptance()
		{
			SetUpTestData();
			AssertEquals("Date Of Acceptance", DateTime.MinValue, goodsShipmentProvider.DateOfAcceptance);
		}

		public void TestExchangeRate()
		{
			SetUpTestData();
			AssertEquals("Exchange Rate", 0m, goodsShipmentProvider.ExchangeRate);
		}

		public void TestAdditionalSupplyChainActors()
		{
			SetUpTestData();
			AssertEquals("Additional Supply Chain Actors", 0, goodsShipmentProvider.AdditionalSupplyChainActors.Count);
		}

		public void TestBuyer()
		{
			SetUpTestData();
			AssertNull("Buyer", goodsShipmentProvider.Buyer);
		}

		public void TestSeller()
		{
			SetUpTestData();
			AssertNull("Seller", goodsShipmentProvider.Seller);
		}

		public void TestExporter()
		{
			SetUpTestData();
			bill.ABL_ShipperName = "Exporter1";
			goodsShipmentProvider = new GoodsShipmentProvider(bill);

			AssertEquals("Name", "Exporter1", goodsShipmentProvider.Exporter.Name);

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "Exporter2";

			bill.ABL_OA_Shipper = orgAddress.PK;
			goodsShipmentProvider = new GoodsShipmentProvider(bill);

			AssertType<BillPartyProvider>(goodsShipmentProvider.Exporter);
			AssertEquals("Name", "Exporter2", goodsShipmentProvider.Exporter.Name);
		}

		public void TestDeliveryTerms()
		{
			SetUpTestData();
			AssertNull("Delivery Terms", goodsShipmentProvider.DeliveryTerms);
		}

		public void TestCountryOfDispatch()
		{
			SetUpTestData();
			AssertNull("Country Of Dispatch", goodsShipmentProvider.CountryOfDispatch);
		}

		public void TestMemberStateTerritory()
		{
			SetUpTestData();
			AssertNull("Member State Territory", goodsShipmentProvider.MemberStateTerritory);
		}

		public void TestDestination()
		{
			SetUpTestData();
			AssertNull("Destination", goodsShipmentProvider.Destination);
		}

		public void TestWarehouse()
		{
			SetUpTestData();
			AssertNull("Warehouse", goodsShipmentProvider.Warehouse);
		}

		public void TestPreviousDocuments()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentProvider.PreviousDocuments);
			AssertEquals(1, goodsShipmentProvider.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentProvider.SupportingDocuments);
			AssertEquals(1, goodsShipmentProvider.SupportingDocuments.Count);
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			AssertNotNull("PreviousDocuments", goodsShipmentProvider.AdditionalInformations);
			AssertEquals(1, goodsShipmentProvider.AdditionalInformations.Count);
		}

		public void TestAdditionalReferences()
		{
			SetUpTestData();
			AssertNotNull("AdditionalReferences", goodsShipmentProvider.AdditionalReferences);
			AssertEquals("Additional References", 1, goodsShipmentProvider.AdditionalReferences.Count);
		}

		public void TestAdditionsAndDeductions()
		{
			SetUpTestData();
			AssertEquals("Additions And Deductions", 0, goodsShipmentProvider.AdditionsAndDeductions.Count);
		}

		public void TestAdditionalFiscalReferences()
		{
			SetUpTestData();
			AssertEquals("Additional Fiscal References", 1, goodsShipmentProvider.AdditionalFiscalReferences.Count);
			var additionalFiscalReference = goodsShipmentProvider.AdditionalFiscalReferences.ElementAt(0);
			AssertEquals("1", additionalFiscalReference.SequenceNumber);
			AssertEquals("FR5", additionalFiscalReference.Role);
			AssertEquals("TestRegNo", additionalFiscalReference.VatIdentificationNumber);
		}

		public void TestPostalCharges()
		{
			SetUpTestData();
			AssertNull("Postal Charges", goodsShipmentProvider.PostalCharges);
		}

		public void TestConsignment()
		{
			SetUpTestData();
			AssertNotNull("Consignment", goodsShipmentProvider.Consignment);
		}

		public void TestGoodsShipmentItems()
		{
			SetUpTestData();
			AssertNotNull("Goods Shipment Items Not Null", goodsShipmentProvider.GoodsShipmentItems);
			AssertEquals("Goods Shipment Items Count", 1, goodsShipmentProvider.GoodsShipmentItems.Count);
		}

		void SetUpTestData()
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_GoodsValue = 100;
			bill.ABL_RX_NKCustomsValueCurrency = "AUD";
			bill.ABL_SellerRegNo = "TestRegNo";

			bill.PreviousDocuments.AddNew();
			bill.SupportingDocuments.AddNew();
			bill.Packs.AddNew();
			bill.PackedItems.AddNew();

			var additionalDocument1 = bill.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var additionalDocument2 = bill.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			goodsShipmentProvider = new GoodsShipmentProvider(bill);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		GoodsShipmentProvider goodsShipmentProvider;

		protected override GoodsShipmentProvider GetProvider()
		{
			SetUpTestData();
			return goodsShipmentProvider;
		}
	}
}
