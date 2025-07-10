using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2HVLVAsycudaPackedItemObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaPackedItemFromHVLVPackingLine()
		{
			var (hvlvShipment, packingLineDataObject, packedItemDataObject) = PrepareDataObject();
			var pack = Factory.NewWithValidTestData<ASYCUDA.Business.AsycudaBill>().Packs.AddNew();
			packingLineDataObject.GoodsDescription = "Goods";
			var packedItemBO = new ICS2HVLVAsycudaPackedItemObjectReader(packingLineDataObject, packedItemDataObject, null, logger, Factory, pack, new ICS2AsycudaManifestDataObjectReaderHelper("DE", Factory.BOFactory), hvlvShipment).ReadIntoBusinessObject();

			AssertNotNull(packedItemBO);
			AssertEquals("packedItemBO.API_RN_NKGoodsOrigin", "FR", packedItemBO.API_RN_NKGoodsOrigin);
			AssertEquals("packedItemBO.API_FormattedTariff", "9403.20.00 30", packedItemBO.API_FormattedTariff);
			AssertEquals("packedItemBO.API_GoodsDescription", "Goods", packedItemBO.API_GoodsDescription);
		}

		(Shipment, PackingLine, PackedItem) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine.Link = 1;
			commercialInvoiceLine.HarmonisedCode = "9403.2000.30";
			commercialInvoiceLine.CountryOfOrigin = new Country() { Code = "FR" };

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;

			var item = new PackedItem();
			item.PackedQuantity = 1;
			item.GrossWeight = 1;
			item.GrossWeightUnit = new UnitOfWeight() { Code = "KG" };
			item.NetWeight = 1;
			item.NetWeightUnit = new UnitOfWeight { Code = "KG" };
			item.CIFValue = 1;
			item.CommercialInvoiceLineLink = 1;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item, });
			return (hvlvShipment, packingLine, item);
		}
	}
}
