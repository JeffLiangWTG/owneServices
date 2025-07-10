using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class MCommodityType04Provider : IMCommodity
	{
		public MCommodityType04Provider(EntryLineWrapper entryLineWrapper)
		{
			this.entryLineWrapper = entryLineWrapper;
			this.entryLine = entryLineWrapper.EntryLine;
			randomInvoiceLine = entryLineWrapper.RandomInvoiceLine;
		}

		readonly EntryLineWrapper entryLineWrapper;
		readonly JobComInvoiceLine randomInvoiceLine;
		readonly CusEntryLine entryLine;

		public string DescriptionOfGoods => entryLine.CL_Description;

		public string CusCode => randomInvoiceLine.ZG_CusNumber;

		public string QuotaOrderNumber => randomInvoiceLine.JI_ConcessionOrder;

		public IMCommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => new MCommodityCodeType03Provider(entryLineWrapper));
		CachedValue<IMCommodityCode> commodityCode;

		public IGoodsMeasure GoodsMeasure => CachedValueHelper.GetValue(ref goodsMeasure, () => new GoodsMeasureProvider(entryLine));
		CachedValue<IGoodsMeasure> goodsMeasure;

		public IMoney InvoiceLine => CachedValueHelper.GetValue(ref invoiceLine, () => new MInvoiceLineTypeProvider(entryLineWrapper));
		CachedValue<IMoney> invoiceLine;

		public ICalculationOfTaxes CalculationOfTaxes => CachedValueHelper.GetValue(ref calculationOfTaxes, () => new CalculationOfTaxesProvider(entryLineWrapper));
		CachedValue<ICalculationOfTaxes> calculationOfTaxes;
	}
}
