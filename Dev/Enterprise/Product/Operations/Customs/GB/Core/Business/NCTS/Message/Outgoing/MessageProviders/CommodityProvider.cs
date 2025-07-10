using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CommodityProvider : ICommodity
	{
		readonly NctsCommonCargoDesc item;
		readonly EU.NCTS.Business.NctsHeader nctsHeader;

		public CommodityProvider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
			nctsHeader = Argument.NotNull(item.Header, nameof(item.Header));
		}

		public string DescriptionOfGoods => item.BY_Description;

		public string CusCode => item.BY_CusC4Number;

		public string HarmonizedSystemSubHeadingCode => harmonizedSystemSubHeadingCode;
		string harmonizedSystemSubHeadingCode => item.BY_HarmonisedTariff.Left(6);

		bool IsDepartureOfficeCountryGB => DepartureOfficeCountry == Core.Constants.CountryCodes.UnitedKingdom;

		bool IsDepartureOfficeCountryXI => DepartureOfficeCountry == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;

		public string CombinedNomenclatureCode => string.IsNullOrEmpty(harmonizedSystemSubHeadingCode) || IsDepartureOfficeCountryGB ? null : !string.IsNullOrEmpty(combinedNomenclatureCode) && IsDepartureOfficeCountryXI ? combinedNomenclatureCode : null;

		string combinedNomenclatureCode => item.BY_HarmonisedTariff.SubstringSafe(6, 2);

		string DepartureOfficeCountry => item.Header.IsPhase5 ? item.Header.CommonMovementHeader.DepartureCustomsOfficeCodeCountry : item.Header.DepartureCustomsOfficeCodeCountry;

		public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = NctsDataRetrieveMethods.GetUNDGDataItems(item).Select((undg, index) => new DangerousGoodsProvider(undg, index + 1)).ToArray<IDangerousGoods>());
		IReadOnlyCollection<IDangerousGoods> dangerousGoods;

		public decimal GrossMass => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, item.GrossMassInKilograms);

		public decimal NetMass => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, item.NetMassInKilograms);

		public decimal SupplementaryQty => item.BY_CustomsSecondQuantity.Normalize();
	}
}
