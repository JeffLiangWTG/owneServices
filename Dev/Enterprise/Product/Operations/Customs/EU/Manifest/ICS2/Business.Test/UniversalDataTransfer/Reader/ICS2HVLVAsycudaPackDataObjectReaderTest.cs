using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2HVLVAsycudaPackDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaPackFromHVLVPackingLine()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var bill = Factory.NewWithValidTestData<ASYCUDA.Business.AsycudaBill>();
			var packBO1 = new ICS2HVLVAsycudaPackDataObjectReader(packingLineDataObject, packingLineDataObject.PackedItemCollection[0], logger, Factory, bill, new ICS2AsycudaManifestDataObjectReaderHelper("DE", Factory.BOFactory), true, hvlvShipment).ReadIntoBusinessObject();
			var packBO2 = new ICS2HVLVAsycudaPackDataObjectReader(packingLineDataObject, packingLineDataObject.PackedItemCollection[1], logger, Factory, bill, new ICS2AsycudaManifestDataObjectReaderHelper("DE", Factory.BOFactory), true, hvlvShipment).ReadIntoBusinessObject();

			AssertNotNull(packBO1);
			AssertEquals("packBO.APA_CommodityCode", "DEF", packBO1.APA_CommodityCode);
			AssertEquals("packBO.LinePrice", (ZDecimal)1.00, packBO1.LinePrice);
			AssertEquals("packBO.LinePriceCurrency", "ABC", packBO1.LinePriceCurrency);
			AssertEquals("packBO.APA_PackUQ", "PCE", packBO1.APA_PackUQ);
			AssertEquals("packBO.APA_Weight", (ZDecimal)2.00, packBO1.APA_Weight);
			AssertEquals("packBO.APA_WeightUQ", "KG", packBO1.APA_WeightUQ);

			AssertNotNull(packBO2);
			AssertEquals("packBO.APA_CommodityCode", "GHI", packBO2.APA_CommodityCode);
			AssertEquals("packBO.LinePrice", (ZDecimal)1.00, packBO2.LinePrice);
			AssertEquals("packBO.LinePriceCurrency", "ABC", packBO2.LinePriceCurrency);
			AssertEquals("packBO.APA_PackUQ", "ABC", packBO2.APA_PackUQ);
			AssertEquals("packBO.APA_Weight", (ZDecimal)1.00, packBO2.APA_Weight);
			AssertEquals("packBO.APA_WeightUQ", "T", packBO2.APA_WeightUQ);
		}

		(Shipment, PackingLine) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine.Link = 1;
			commercialInvoiceLine.HarmonisedCode = "1234.56.78";

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;
			hvlvShipment.GoodsValueCurrency = new Currency();
			hvlvShipment.GoodsValueCurrency.Code = "ABC";

			var item1 = new PackedItem();
			item1.Description = "PackedItemDesc";
			item1.PackedQuantity = 1;
			item1.GrossWeight = 2m;
			item1.NetWeight = 1m;
			item1.NetWeightUnit = new UnitOfWeight { Code = "KG" };
			item1.CommercialInvoiceLineLink = 1;
			item1.Product = new Product() { Code = "DEF" };

			var item2 = new PackedItem();
			item2.Description = "PackedItemDesc";
			item2.PackedQuantity = 1;
			item2.GrossWeight = 1m;
			item2.GrossWeightUnit = new UnitOfWeight() { Code = "T" };
			item2.NetWeight = 1m;
			item2.NetWeightUnit = new UnitOfWeight { Code = "KG" };
			item2.CommercialInvoiceLineLink = 1;
			item2.Product = new Product() { Code = "GHI" };
			item2.UnitOfQuantity = new PackageType() { Code = "ABC" };

			item1.FindMatchingCommercialInvoiceLine(hvlvShipment).LinePrice = 1;
			item2.FindMatchingCommercialInvoiceLine(hvlvShipment).LinePrice = 1;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item1, item2 });
			return (hvlvShipment, packingLine);
		}
	}
}
