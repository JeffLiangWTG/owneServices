using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CommodityType03Provider : ICommodityType03
	{
		readonly NctsCommonCargoDesc item;
		public CommodityType03Provider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		public string DescriptionOfGoods => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent.BY_Description != unloadedItem.BY_Description ? unloadedItem.BY_Description : (StatusIsNewItem ? item.BY_Description : null);

		public string CusCode => item.BY_CusC4Number;

		public string HarmonizedSystemSubHeadingCode => IsHarmonisedTariffChanged(item) || StatusIsNewItem ? item.BY_HarmonisedTariff.SubstringSafe(0, 6) : null;

		public string CombinedNomenclatureCode => (StatusIsNewItem || IsHarmonisedTariffChanged(item)) && item.BY_HarmonisedTariff.Length == 8 ? item.BY_HarmonisedTariff.SubstringSafe(6, 2) : null;

		public decimal? GrossMass => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent != null && unloadedItem.ArrivalCargoDescParent.GrossMassInKilograms != unloadedItem.GrossMassInKilograms
			? unloadedItem.GrossMassInKilograms
			: (StatusIsNewItem ? item.GrossMassInKilograms : null);

		public decimal? NetMass => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent != null && unloadedItem.ArrivalCargoDescParent.NetMassInKilograms != unloadedItem.NetMassInKilograms
			? NctsDataRetrieveMethods.NetMassInKilogramsNullableByPreviousDocument(unloadedItem)
			: (StatusIsNewItem ? item.NetMassInKilograms : null);

		public bool IsHarmonisedTariffChanged(NctsCommonCargoDesc item) => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent != null && unloadedItem.ArrivalCargoDescParent.BY_HarmonisedTariff != unloadedItem.BY_HarmonisedTariff;

		bool StatusIsNewItem => item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW;
	}
}
