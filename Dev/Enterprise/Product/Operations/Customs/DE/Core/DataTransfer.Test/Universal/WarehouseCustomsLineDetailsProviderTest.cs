using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestGetNewLineDetail()
		{
			var provider = new WarehouseCustomsLineDetailsProviderForTest(WarehouseCustomsTestHelper.Shipment);
			var fallbackDetail = (WarehouseCustomsFallbackDetailWithEntryInstruction)provider.GetFallbackDetail(WarehouseCustomsTestHelper.Invoice);
			AssertType<WarehouseCustomsLineDetails>(new WarehouseCustomsLineDetailsProviderForTest(WarehouseCustomsTestHelper.Shipment).GetNewLineDetail(Factory, WarehouseCustomsTestHelper.InvoiceLine, fallbackDetail));
		}

		public void TestGetFallBackDetail()
		{
			var provider = new WarehouseCustomsLineDetailsProviderForTest(WarehouseCustomsTestHelper.Shipment);
			var fallbackDetail = (WarehouseCustomsFallbackDetailWithEntryInstruction)provider.GetFallbackDetail(WarehouseCustomsTestHelper.Invoice);
			CombineAssertions(() =>
			{
				AssertEquals("InvoiceNumber", "INV123", fallbackDetail.InvoiceNumber);
				AssertEquals("InvoiceDate", new ZDateTime(2023, 6, 21), fallbackDetail.InvoiceDate);
				AssertEquals("IncotermCode", "FOB", fallbackDetail.IncotermCode);
				AssertEquals("IncotermPlace", "Frankfurt", fallbackDetail.IncotermPlace);
				AssertEquals("ValuationCode", "21", fallbackDetail.ValuationCode);
				AssertEquals("ImporterAddress", "Importer ORG", fallbackDetail.ImporterAddress.CompanyName);
				AssertEquals("SupplierAddress", "Supplier ORG", fallbackDetail.SupplierAddress.CompanyName);
				AssertEquals("BuyerAddress", "Buyer ORG", fallbackDetail.BuyerAddress.CompanyName);
				AssertEquals("SellerAddress", "Seller ORG", fallbackDetail.SellerAddress.CompanyName);
				AssertEquals("PortOfLoading", "DEWIB", fallbackDetail.PortOfLoading);
				AssertEquals("FirstEUArrival", "DEHAM", fallbackDetail.PortOfFirstEUArrival);
				AssertEquals("Transport", "SEA", fallbackDetail.TransportMode);
				AssertEquals("InvoiceNumber", 1, fallbackDetail.supportingInfos.Count);
				AssertEquals("header-support1", fallbackDetail.supportingInfos[0].ReferenceNumber);
				AssertEquals("LinePriceCurrency", "EUR", fallbackDetail.LinePriceCurrency);
				AssertEquals("CountryCode", "DE", fallbackDetail.CountryCode);
			});
		}
	}

	sealed class WarehouseCustomsLineDetailsProviderForTest : WarehouseCustomsLineDetailsProvider
	{
		public WarehouseCustomsLineDetailsProviderForTest(Shipment shipment)
			: base(shipment)
		{
		}

		public new WarehouseCustomsFallbackDetail GetFallbackDetail(CommercialInvoiceHeader invoice) => base.GetFallbackDetail(invoice);

		public new WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
			=> base.GetNewLineDetail(factory, invoiceLine, fallbackDetail);
	}
}
