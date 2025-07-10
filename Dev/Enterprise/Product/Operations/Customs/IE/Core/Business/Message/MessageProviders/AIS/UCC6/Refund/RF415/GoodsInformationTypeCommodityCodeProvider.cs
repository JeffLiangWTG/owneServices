using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class GoodsInformationTypeCommodityCodeProvider : GoodsInformationOtherTypeCommodityCodeProvider, IGoodsInformationTypeCommodityCode
	{
		public GoodsInformationTypeCommodityCodeProvider(EntryLineWrapper entryLineWrapper) : base(entryLineWrapper.RandomInvoiceLine.JI_Tariff) { }

		public string TaricAdditionalCode => null;

		public string NationalAdditionalCommodityCode => null;

		public string HarmonizedSystemSubheadingCode => null;

		public IReadOnlyCollection<string> NationalAdditionalCode => Array.Empty<string>();

		public string TypeGoods => null;
	}
}
