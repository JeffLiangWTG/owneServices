using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class ArrivalAndUnloadingGoodsItemDataObjectWriter : GoodsItemDataObjectWriter<NctsCommonCargoDesc>
	{
		public ArrivalAndUnloadingGoodsItemDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader commercialInvoiceHeaderData)
			: base(manager, helper, headerData, commercialInvoiceHeaderData)
		{
		}

		protected override UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine PopulateCommercialInvoiceLineData(NctsCommonCargoDesc goodsIteBO)
		{
			var commercialInvoiceLineData = base.PopulateCommercialInvoiceLineData(goodsIteBO);
			commercialInvoiceLineData.AddInfoGroupCollection = GetGoodsItemCusAddInfos(goodsIteBO); // Results of Control
			return commercialInvoiceLineData;
		}

		List<UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup> GetGoodsItemCusAddInfos(NctsCommonCargoDesc goodItemBO) => AddInfoGroupCollectionCreator.CreateCollection(helper, goodItemBO, writeManager);
	}
}
