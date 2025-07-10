using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer.Test
{
	sealed class EUH7HVLVAsycudaPackedItemObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaPackedItemFromHVLVPackingLine()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packBO1 = bill.Packs.AddNew();
			var packBO2 = bill.Packs.AddNew();

			var packedItemBO1 = new EUH7HVLVAsycudaPackedItemObjectReader(packingLineDataObject.PackedItemCollection[0], logger, Factory, bill, packBO1, hvlvShipment).ReadIntoBusinessObject();
			var packedItemBO2 = new EUH7HVLVAsycudaPackedItemObjectReader(packingLineDataObject.PackedItemCollection[1], logger, Factory, bill, packBO2, hvlvShipment).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull(packedItemBO1);
				AssertEquals("packedItemBO.API_Tariff", "1234.56.78", packedItemBO1.API_FormattedTariff);
				AssertEquals("packedItemBO.API_RN_NKGoodsOrigin", "AA", packedItemBO1.API_RN_NKGoodsOrigin);
				AssertEquals("packedItemBO.API_GoodsDescription", "PackedItemDesc", packedItemBO1.API_GoodsDescription);
				AssertEquals("packedItemBO.API_GoodsValue", 10M, packedItemBO1.API_GoodsValue);
				AssertEquals("packedItemBO.API_RX_NKGoodsValueCurrency", "ABC", packedItemBO1.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItemBO.API_GrossWeight", 1000M, packedItemBO1.API_GrossWeight);
				AssertEquals("packedItemBO.API_NetWeight", 2000M, packedItemBO1.API_NetWeight);
				AssertEquals("packedItemBO.API_CustomsValue", 20.29M, packedItemBO1.API_CustomsValue);
				AssertEquals("packedItemBO.API_CustomsQty", 20M, packedItemBO1.API_CustomsQty);

				AssertNotNull(packedItemBO2);
				AssertEquals("packedItemBO.API_Tariff", "8765.43.21", packedItemBO2.API_FormattedTariff);
				AssertEquals("packedItemBO.API_RN_NKGoodsOrigin", "BB", packedItemBO2.API_RN_NKGoodsOrigin);
				AssertEquals("packedItemBO.API_GoodsDescription", "PackedItemDesc2", packedItemBO2.API_GoodsDescription);
				AssertEquals("packedItemBO.API_GoodsValue", 20M, packedItemBO2.API_GoodsValue);
				AssertEquals("packedItemBO.API_RX_NKGoodsValueCurrency", "ABC", packedItemBO2.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItemBO.API_GrossWeight", 1M, packedItemBO2.API_GrossWeight);
				AssertEquals("packedItemBO.API_NetWeight", 2M, packedItemBO2.API_NetWeight);
				AssertEquals("packedItemBO.API_CustomsValue", 20.299M, packedItemBO2.API_CustomsValue);
				AssertEquals("packedItemBO.API_CustomsQty", 10M, packedItemBO2.API_CustomsQty);
			});
		}

		(Shipment, PackingLine) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine1 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine1.Link = 1;
			commercialInvoiceLine1.HarmonisedCode = "1234.56.78";
			commercialInvoiceLine1.CountryOfOrigin = new Country() { Code = "AA" };
			commercialInvoiceLine1.Weight = 1000;
			commercialInvoiceLine1.NetWeight = 2000;
			commercialInvoiceLine1.WeightUnit = new UnitOfWeight { Code = "KG" };
			commercialInvoiceLine1.CustomsValue = 20.29;
			commercialInvoiceLine1.CustomsQuantity = 20;

			var commercialInvoiceLine2 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine2.Link = 2;
			commercialInvoiceLine2.HarmonisedCode = "8765.43.21";
			commercialInvoiceLine2.CountryOfOrigin = new Country() { Code = "BB" };
			commercialInvoiceLine2.Weight = 1000;
			commercialInvoiceLine2.NetWeight = 2000;
			commercialInvoiceLine2.WeightUnit = new UnitOfWeight { Code = "G" };
			commercialInvoiceLine2.CustomsValue = 20.299;
			commercialInvoiceLine2.CustomsQuantity = 10;

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine1, commercialInvoiceLine2 });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;
			hvlvShipment.GoodsValueCurrency = new Currency() { Code = "ABC" };

			var item1 = new PackedItem();
			item1.Description = "PackedItemDesc";
			item1.CommercialInvoiceLineLink = 1;
			item1.GoodsValue = 10;

			var item2 = new PackedItem();
			item2.Description = "PackedItemDesc2";
			item2.CommercialInvoiceLineLink = 2;
			item2.GoodsValue = 20;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);

			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item1, item2 });

			return (hvlvShipment, packingLine);
		}
	}
}
