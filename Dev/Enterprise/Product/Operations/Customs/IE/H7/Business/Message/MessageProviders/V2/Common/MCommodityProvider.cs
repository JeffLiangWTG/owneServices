using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MCommodityProvider : IMCommodity
	{
		public MCommodityProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public string DescriptionOfGoods => packedItem?.API_GoodsDescription;

		public string CusCode => null;

		public string QuotaOrderNumber => null;

		public IMCommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => MCommodityCodeProvider.NewOrNull(packedItem));
		CachedValue<IMCommodityCode> commodityCode;

		public IGoodsMeasure GoodsMeasure => CachedValueHelper.GetValue(ref goodsMeasureCached, () => GoodsMeasureProvider.NewOrNull(packedItem));
		CachedValue<IGoodsMeasure> goodsMeasureCached;

		public IMoney InvoiceLine => CachedValueHelper.GetValue(ref invoiceLineCached, () => InvoiceLineProvider.NewOrNull(packedItem));
		CachedValue<IMoney> invoiceLineCached;

		public ICalculationOfTaxes CalculationOfTaxes => null;
	}
}
