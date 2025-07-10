using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCommonCargoDescValueSetStrategy : IValueSetStrategy
	{
		public NctsCommonCargoDescValueSetStrategy(NctsCommonCargoDesc goodsItem)
		{
			GoodsItem = goodsItem;
		}
		protected NctsCommonCargoDesc GoodsItem { get; }

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case NctsCommonCargoDesc.Schema.BY_MonetaryValue:
				case NctsCommonCargoDesc.Schema.BY_RN_NKCountryOfOrigin:
				case NctsCommonCargoDesc.Schema.BY_HarmonisedTariff:
				case NctsCommonCargoDesc.Schema.BY_ZZF_NKTaxType:
				case NctsCommonCargoDesc.Schema.BY_CustomsQuantity:
				case NctsCommonCargoDesc.Schema.BY_CustomsUnitQty:
				case NctsCommonCargoDesc.Schema.BY_CustomsSecondQuantity:
				case NctsCommonCargoDesc.Schema.BY_CustomsSecondUnitQty:
				case NctsCommonCargoDesc.Schema.BY_CustomsThirdQuantity:
				case NctsCommonCargoDesc.Schema.BY_CustomsThirdUnitQty:
				case NctsCommonCargoDesc.Schema.BY_CustomsFourthQuantity:
				case NctsCommonCargoDesc.Schema.BY_CustomsFourthUnitQty:
					GoodsItem.UpdateAllFeesFromTariffRates();
					break;
			}

			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
		}
	}
}
