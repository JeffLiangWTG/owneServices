using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer.Test
{
	sealed class EUH7HVLVAsycudaPackDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaPackFromHVLVPackingLine()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packBO = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("packBO.APA_PackQty", 1, packBO.APA_PackQty);
				AssertEquals("packBO.APA_PackUQ", ZString.Empty, packBO.APA_PackUQ);
				AssertEquals("packBO.APA_GoodsDescription", "PackDesc", packBO.APA_GoodsDescription);
				AssertEquals("packBO.APA_Weight", (ZDecimal)10.00, packBO.APA_Weight);
				AssertEquals("packBO.APA_WeightUQ", "KG", packBO.APA_WeightUQ);
				AssertEquals("packBO.LinePrice", (ZDecimal)11.00, packBO.LinePrice);
				AssertEquals("packBO.LinePriceCurrency", "ABC", packBO.LinePriceCurrency);
				AssertEquals("packBO.APA_LineNo", (ZShort)1, packBO.APA_LineNo);
				AssertEquals("packBO.PackedItems", 2, packBO.PackedItems.Count);
			});

			packingLineDataObject.Weight = null;
			packBO = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("packBO.APA_Weight", (ZDecimal)20.00, packBO.APA_Weight);
		}

		public void TestPackedItemIsNotImportedIfFieldsAreEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.GoodsValueCurrency.Code = ZString.Empty;
			packingLineDataObject.PackedItemCollection[0].GoodsValue = 0;
			packingLineDataObject.PackedItemCollection[0].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].NetWeight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].Weight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = ZString.Empty;

			packingLineDataObject.PackedItemCollection[1].GoodsValue = 0;
			packingLineDataObject.PackedItemCollection[1].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].NetWeight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].Weight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = ZString.Empty;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("No packed items are created if description/harmonised code/country of origin is empty", 0, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfGoodsValueIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.GoodsValueCurrency.Code = ZString.Empty;
			packingLineDataObject.PackedItemCollection[0].GrossWeight = 0;
			packingLineDataObject.PackedItemCollection[0].NetWeight = 0;
			packingLineDataObject.PackedItemCollection[0].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = ZString.Empty;

			packingLineDataObject.PackedItemCollection[1].GrossWeight = 0;
			packingLineDataObject.PackedItemCollection[1].NetWeight = 0;
			packingLineDataObject.PackedItemCollection[1].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = ZString.Empty;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Goods Value are not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfNetWeightIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.GoodsValueCurrency.Code = ZString.Empty;
			packingLineDataObject.PackedItemCollection[0].GoodsValue = 0;
			packingLineDataObject.PackedItemCollection[0].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].NetWeight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = ZString.Empty;

			packingLineDataObject.PackedItemCollection[1].GoodsValue = 0;
			packingLineDataObject.PackedItemCollection[1].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].NetWeight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = ZString.Empty;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Net Weight are not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfCustomsValueIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsValue = 1;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CustomsValue = 2;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Customs Value is not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfCustomsQuantityIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsQuantity = 1;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CustomsQuantity = 2;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Customs Quantity is not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfGrossWeightIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.GoodsValueCurrency.Code = ZString.Empty;
			packingLineDataObject.PackedItemCollection[0].GoodsValue = 0;
			packingLineDataObject.PackedItemCollection[0].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].NetWeight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = ZString.Empty;

			packingLineDataObject.PackedItemCollection[1].GoodsValue = 0;
			packingLineDataObject.PackedItemCollection[1].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].NetWeight = 0;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = ZString.Empty;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Gross Weight is not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfDescriptionIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = ZString.Empty;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Descriptions are not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfHarmonisedCodeIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			packingLineDataObject.PackedItemCollection[0].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = "123.456";
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = ZString.Empty;

			packingLineDataObject.PackedItemCollection[1].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = "1.234";
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = ZString.Empty;

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Harmonised Codes are not empty", 2, bill.PackedItems.Count);
		}

		public void TestPackedItemIsCreatedIfCountryOfOriginIsNotEmpty()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			packingLineDataObject.PackedItemCollection[0].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CountryOfOrigin.Code = "AU";

			packingLineDataObject.PackedItemCollection[1].Description = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].HarmonisedCode = ZString.Empty;
			hvlvShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1].CountryOfOrigin.Code = "US";

			_ = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Packed items are created because Country of Origin are not empty", 2, bill.PackedItems.Count);
		}

		(Shipment, PackingLine) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine1 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine1.Link = 1;
			commercialInvoiceLine1.LinePrice = 1;
			commercialInvoiceLine1.NetWeight = 2m;
			commercialInvoiceLine1.Weight = 1m;
			commercialInvoiceLine1.HarmonisedCode = "1234.56.78";
			commercialInvoiceLine1.CountryOfOrigin = new Country() { Code = "AA" };

			var commercialInvoiceLine2 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine2.Link = 2;
			commercialInvoiceLine2.LinePrice = 10;
			commercialInvoiceLine2.NetWeight = 1m;
			commercialInvoiceLine2.Weight = 2m;
			commercialInvoiceLine2.CountryOfOrigin = new Country() { Code = "BB" };

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine1, commercialInvoiceLine2 });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;
			hvlvShipment.GoodsValueCurrency = new Currency() { Code = "ABC" };
			hvlvShipment.TotalWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };

			var item1 = new PackedItem();
			item1.Description = "PackedItemDesc1";
			item1.PackedQuantity = 1;
			item1.GrossWeight = 2m;
			item1.GoodsValue = 10;
			item1.CommercialInvoiceLineLink = 1;
			item1.Product = new Product() { Code = "DEF" };

			var item2 = new PackedItem();
			item2.Description = "PackedItemDesc2";
			item2.PackedQuantity = 2;
			item2.GrossWeight = 1m;
			item2.GoodsValue = 20;
			item2.GrossWeightUnit = new UnitOfWeight() { Code = "T" };
			item2.CommercialInvoiceLineLink = 2;
			item2.Product = new Product() { Code = "GHI" };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item1, item2 });
			packingLine.GoodsDescription = "PackDesc";
			packingLine.Weight = 10;
			packingLine.ManifestedWeight = 20;

			return (hvlvShipment, packingLine);
		}
	}
}
