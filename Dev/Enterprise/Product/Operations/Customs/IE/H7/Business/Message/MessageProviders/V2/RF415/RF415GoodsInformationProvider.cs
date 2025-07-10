using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415GoodsInformationProvider : IRF415GoodsInformation
	{
		public RF415GoodsInformationProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public IGoodsInformationTypeCommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => new CommodityCodeProvider(packedItem));
		CachedValue<CommodityCodeProvider> commodityCode;

		public string GoodsDescription => packedItem.API_GoodsDescription;

		public IGoodsQuantity GoodsQuantity => null;

		public IMoney CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => new CustomsValueProvider(packedItem));
		CachedValue<CustomsValueProvider> customsValue;

		public IReadOnlyCollection<ITypeOfDuty> TypeOfDuty => Array.Empty<TypeOfDutyProvider>();
	}
}
