using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer.Test
{
	sealed class EUH7HVLVAsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestEORINumberMapping()
		{
			var factory = NewFactory();
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.WayBillNumber = "WAYBILL1";

			var additionalReferenceEOE = SetupAdditionalReference(null, null, "1234", new EntryType
			{
				Code = "EOE",
				Description = "Exporter EORI Number"
			});

			var additionalReferenceEOI = SetupAdditionalReference(null, null, "9876", new EntryType
			{
				Code = "EOI",
				Description = "Importer EORI Number"
			});

			hvlvShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { additionalReferenceEOE, additionalReferenceEOI });

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", factory);

			var bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var billPK = bill.PK;

			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_ShipperRegNoType", "EOR", bill.ABL_ShipperRegNoType);
				AssertEquals("bill.ABL_ShipperRegNo", "1234", bill.ABL_ShipperRegNo);
				AssertEquals("bill.ABL_ConsigneeRegNoType", "EOR", bill.ABL_ConsigneeRegNoType);
				AssertEquals("bill.ABL_ConsigneeRegNo", "9876", bill.ABL_ConsigneeRegNo);
			});

			hvlvShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { additionalReferenceEOI });
			bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("same bill", billPK, bill.PK);
				AssertEquals("bill.ABL_ShipperRegNoType", string.Empty, bill.ABL_ShipperRegNoType);
				AssertEquals("bill.ABL_ShipperRegNo", string.Empty, bill.ABL_ShipperRegNo);
				AssertEquals("bill.ABL_ConsigneeRegNoType", "EOR", bill.ABL_ConsigneeRegNoType);
				AssertEquals("bill.ABL_ConsigneeRegNo", "9876", bill.ABL_ConsigneeRegNo);
			});

			hvlvShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { additionalReferenceEOE });
			bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("same bill", billPK, bill.PK);
				AssertEquals("bill.ABL_ShipperRegNoType", "EOR", bill.ABL_ShipperRegNoType);
				AssertEquals("bill.ABL_ShipperRegNo", "1234", bill.ABL_ShipperRegNo);
				AssertEquals("bill.ABL_ConsigneeRegNoType", string.Empty, bill.ABL_ConsigneeRegNoType);
				AssertEquals("bill.ABL_ConsigneeRegNo", string.Empty, bill.ABL_ConsigneeRegNo);
			});
		}

		public void TestPackedItemsAreNotDuplicatedWhenImportingBills()
		{
			var factory = NewFactory();
			var hvlvShipment = PrepareDataObject();
			hvlvShipment.WayBillNumber = "WAYBILL1";

			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.SuspendCheckBusinessObjectType();
			manifestHeader.AMA_RN_NKCountry = "IE";

			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", factory);
			var bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var billPK = bill.PK;
			AssertEquals("Precondition", 2, bill.PackedItems.Count);

			bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("Same bill", billPK, bill.PK);
				AssertEquals("Still 2 packed items on Bill", 2, bill.PackedItems.Count);
			});
		}

		public void TestTransportValueAndInsuranceValueapping()
		{
			var factory = NewFactory();
			var hvlvShipment = PrepareDataObject();
			hvlvShipment.TransportValue = 12.00;
			hvlvShipment.InsuranceValue = 11.00;
			hvlvShipment.GoodsValueCurrency = new Currency() { Code = "AUD" };

			var manifestHeader = factory.New<AsycudaManifestHeader>();
			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", factory);
			var bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_TransportValue", (ZDecimal)12, bill.ABL_TransportValue);
				AssertEquals("bill.ABL_TransportValueCurrency", "AUD", bill.ABL_RX_NKTransportValueCurrency);
				AssertEquals("bill.ABL_InsuranceValue", (ZDecimal)11, bill.ABL_InsuranceValue);
				AssertEquals("bill.ABL_InsuranceValueCurrency", "AUD", bill.ABL_RX_NKInsuranceValueCurrency);
			});
		}

		public void TestIncotermValueapping()
		{
			var factory = NewFactory();
			var hvlvShipment = PrepareDataObject();
			hvlvShipment.ShipmentIncoTerm = new IncoTerm();
			hvlvShipment.ShipmentIncoTerm.Code = "CFR";

			var manifestHeader = factory.New<AsycudaManifestHeader>();
			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", factory);
			var bill = new EUH7HVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, helper, true).ReadIntoBusinessObject();

			AssertEquals("bill.ABL_Incoterm", "CFR", bill.ABL_Incoterm);
		}

		public void TestDoNotLoadAsycudaBillWhenHeaderNotInDatabase()
		{
			var factory = NewFactory();
			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", factory);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN00001";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "OLDBIL00002";
			bill.MatchingReference = "TESTMATCHREF";

			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var portOfLoading = new UNLOCO() { Code = "AUSYD" };
			var portOfDischarge = new UNLOCO() { Code = "SGSIN" };
			var billShipment = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 10m, "Goods Descrption", 1m, "Carrier Reference", "STD", "PRE");
			billShipment.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New(ASYCUDA.Business.AddInfoConstants.Bill.MatchingReference, "TESTMATCHREF") });

			var billReader = new EUH7HVLVAsycudaBillDataObjectReader(billShipment, Logger, Factory, header, helper, false);

			AssertNull("Should not find matching because the header is not in DB", billReader.TryGetExistingBusinessObject());

			Factory.SaveForTesting();

			var matchingBill = billReader.TryGetExistingBusinessObject();
			AssertEquals("Existing matching bill is found", bill, matchingBill);
		}

		public void TestUCRNumberAndVendorID()
		{
			var factory = NewFactory();
			var shipment = PrepareDataObject();
			shipment.VendorIdentifier = "12345";
			var ucr = new AdditionalReference();
			ucr.Type = new EntryType();
			ucr.Type.Code = "UCR";
			ucr.ReferenceNumber = "2345";
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { ucr });

			var header = factory.New<AsycudaManifestHeader>();
			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", factory);

			var bill = new EUH7HVLVAsycudaBillDataObjectReader(shipment, Logger, Factory, header, helper, true).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("UCR Number", "2345", bill.ABL_UCRNumber);
				AssertEquals("Vendor ID", "12345", bill.ABL_SellerRegNo);
			});
		}

		Shipment PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.WayBillNumber = "WAYBILL1";

			var commercialInvoiceLine1 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine1.Link = 1;
			commercialInvoiceLine1.LinePrice = 1;

			var commercialInvoiceLine2 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine2.Link = 2;
			commercialInvoiceLine2.LinePrice = 10;

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine1, commercialInvoiceLine2 });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;
			hvlvShipment.GoodsValueCurrency = new Currency() { Code = "ABC" };

			var item1 = new PackedItem();
			item1.Description = "PackedItemDesc";
			item1.PackedQuantity = 1;
			item1.GrossWeight = 2m;
			item1.CommercialInvoiceLineLink = 1;
			item1.Product = new Product() { Code = "DEF" };

			var item2 = new PackedItem();
			item2.Description = "PackedItemDesc2";
			item2.PackedQuantity = 2;
			item2.GrossWeight = 1m;
			item2.GrossWeightUnit = new UnitOfWeight() { Code = "T" };
			item2.CommercialInvoiceLineLink = 2;
			item2.Product = new Product() { Code = "GHI" };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);

			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item1, item2 });
			hvlvShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			return hvlvShipment;
		}
	}
}
