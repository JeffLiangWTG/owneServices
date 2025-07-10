using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MCommodityCodeProvider : IMCommodityCode
	{
		MCommodityCodeProvider(AsycudaPackedItem packItem)
		{
			this.packItem = packItem;
		}

		readonly AsycudaPackedItem packItem;

		public static MCommodityCodeProvider NewOrNull(AsycudaPackedItem packItem)
		{
			return packItem == null ? null : new MCommodityCodeProvider(packItem);
		}

		public string HarmonizedSystemSubheadingCode => packItem.API_Tariff.Left(6);

		public string CombinedNomenclatureCode => null;

		public string TaricCode => null;

		public IReadOnlyCollection<ITaricAdditionalCode> TaricAdditionalCode => Array.Empty<ITaricAdditionalCode>();

		public IReadOnlyCollection<ICcQualifierNationalAdditionalCode> NationalAdditionalCode => Array.Empty<ICcQualifierNationalAdditionalCode>();

		public string TypeGoods => null;
	}
}
