using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CommodityType03Provider : ICommodityType03
	{
		public CommodityType03Provider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
			nctsHeader = Argument.NotNull(item.Header, nameof(item.Header));
		}

		public string DescriptionOfGoods => item.BY_Description;

		public string CusCode => item.BY_CusC4Number;
		public string HarmonizedSystemSubHeadingCode => item.BY_HarmonisedTariff.SubstringSafe(0, 6);
		public string CombinedNomenclatureCode => item.BY_HarmonisedTariff.Length == 8 ? item.BY_HarmonisedTariff.SubstringSafe(6, 2) : null;

		public decimal GrossMass => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, item.GrossMassInKilograms);

		public decimal NetMass => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, item.NetMassInKilograms);

		readonly NctsCommonCargoDesc item;
		readonly EU.NCTS.Business.NctsHeader nctsHeader;
	}
}
