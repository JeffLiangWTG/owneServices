using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class RF415GoodsInformationProvider : IRF415GoodsInformationType
	{
		public RF415GoodsInformationProvider(AsycudaPackedItem item)
		{
			this.item = item;
		}
		readonly AsycudaPackedItem item;

		public string CommodityCode => item.API_Tariff;

		public string GoodsDescription => item.API_GoodsDescription;

		public IGoodsQuantityRF415Type GoodsQuantity => null;

		public IMoney CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => new MoneyProvider(Utilities.Round(item.API_GoodsValue, 2), item.API_RX_NKGoodsValueCurrency));
		CachedValue<MoneyProvider> customsValue;

		public IReadOnlyCollection<ITypeOfDuty> TypeOfDuty => Array.Empty<ITypeOfDuty>();
	}
}
