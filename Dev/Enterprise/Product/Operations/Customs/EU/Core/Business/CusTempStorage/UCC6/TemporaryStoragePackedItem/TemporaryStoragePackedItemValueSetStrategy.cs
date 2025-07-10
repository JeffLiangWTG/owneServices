using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class TemporaryStoragePackedItemValueSetStrategy : IValueSetStrategy
{
	public TemporaryStoragePackedItemValueSetStrategy(TemporaryStoragePackedItem temporaryStoragePackedItem)
	{
		this.temporaryStoragePackedItem = temporaryStoragePackedItem;
	}
	readonly TemporaryStoragePackedItem temporaryStoragePackedItem;

	public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
	{
		switch (valueThatHasChanged.Name)
		{
			case TemporaryStoragePackedItem.Schema.API_Tariff:
			case TemporaryStoragePackedItem.Schema.API_RN_NKGoodsOrigin:
			case TemporaryStoragePackedItem.Schema.API_GoodsValue:
			case TemporaryStoragePackedItem.Schema.API_CustomsQty:
			case TemporaryStoragePackedItem.Schema.API_CustomsUQ:
			case TemporaryStoragePackedItem.Schema.API_CustomsQty2:
			case TemporaryStoragePackedItem.Schema.API_CustomsUQ2:
			case TemporaryStoragePackedItem.Schema.API_CustomsQty3:
			case TemporaryStoragePackedItem.Schema.API_CustomsUQ3:
				temporaryStoragePackedItem.RefreshAllDutyAmountsFromTariffRates();
				break;
		}
	}
}
