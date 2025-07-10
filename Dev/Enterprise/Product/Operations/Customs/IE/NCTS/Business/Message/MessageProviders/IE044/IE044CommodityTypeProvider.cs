using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;

using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044CommodityTypeProvider : IIE044CommodityType
	{
		public IE044CommodityTypeProvider(NctsArrivalCargoDesc goodsItem)
		{
			Argument.NotNull(goodsItem, nameof(goodsItem));
			if (goodsItem.UnloadedGoodsItem is null)
			{
				this.goodsItem = goodsItem;
			}
			else
			{
				this.goodsItem = goodsItem.UnloadedGoodsItem;
			}
		}
		readonly NctsCommonCargoDesc goodsItem;

		public string GoodsDescription => goodsItem.BY_Description;

		public string CUSCode => goodsItem.BY_CusC4Number;

		public string CombinedNomenclatureCode => goodsItem.Factory.GetValue(ref combinedNomenclatureCodeCached, () =>
		{
			NctsEuOfficeCodeCollectionForDepartureGrid offices;
			if (goodsItem is NctsUnloadedCargoDesc unloadedCargoDesc)
			{
				offices = unloadedCargoDesc.ArrivalCargoDescParent?.Bill?.Header?.ArrivalMovementHeader?.CustomsOfficesForDeparture;
			}
			else
			{
				offices = goodsItem.Bill?.Header?.ArrivalMovementHeader?.CustomsOfficesForDeparture;
			}
			if (offices.Cast<NctsEuOfficeCode>().Any(x => x.IsOfficeDeparture && x.IsInCL112CountryList))
			{
				return null;
			}
			else
			{
				return goodsItem.BY_FormattedHarmonisedTariff.Replace(".", string.Empty).SubstringSafe(6, 2);
			}
		});
		CachedProperty<string> combinedNomenclatureCodeCached;

		public string HarmonizedSystemSubHeadingCode => goodsItem.BY_FormattedHarmonisedTariff.Replace(".", string.Empty).Left(6);

		public decimal GrossMass => goodsItem.GrossMassInKilograms;

		public decimal NetMass => goodsItem.NetMassInKilograms;

		public decimal SupplementaryUnits => 0;
	}
}
