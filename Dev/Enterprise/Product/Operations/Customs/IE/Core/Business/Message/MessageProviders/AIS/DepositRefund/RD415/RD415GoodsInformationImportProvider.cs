using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415GoodsInformationImportProvider : IRD415GoodsInformationImport
	{
		public RD415GoodsInformationImportProvider(CusEntryLine cusEntryLine)
		{
			entryLine = cusEntryLine;
		}
		readonly CusEntryLine entryLine;

		public IGoodsInformationOtherTypeCommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => new GoodsInformationOtherTypeCommodityCodeProvider(entryLine.RandomLine.JI_Tariff));
		CachedValue<GoodsInformationOtherTypeCommodityCodeProvider> commodityCode;

		public IGoodsQuantity GoodsQuantity => null;

		public decimal ValueOfGoods => valueOfGoods ??= entryLine.TotalLinePriceInLocalCurrency;
		decimal? valueOfGoods;

		public string GoodsDescription => entryLine.CL_Description;
	}
}
