using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class CommodityCodeProvider : IGoodsInformationTypeCommodityCode
	{
		public CommodityCodeProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public string CombinedNomenclatureCode => packedItem.API_Tariff.SubstringSafe(0, 6);

		public string TaricCode => packedItem.API_Tariff.SubstringSafe(4, 2);

		public string TaricAdditionalCode => null;

		public string NationalAdditionalCommodityCode => null;

		public string HarmonizedSystemSubheadingCode => null;

		public IReadOnlyCollection<string> NationalAdditionalCode => Array.Empty<string>();

		public string TypeGoods => null;
	}
}
