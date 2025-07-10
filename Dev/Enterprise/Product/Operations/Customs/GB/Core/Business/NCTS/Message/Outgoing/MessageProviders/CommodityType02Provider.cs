using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CommodityType02Provider : ICommodityType02
	{
		public CommodityType02Provider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
			nctsHeader = Argument.NotNull(item.Header, nameof(item.Header));
		}

		public string DescriptionOfGoods => item.BY_Description;

		public string CusCode => item.BY_CusC4Number;
		public string HarmonizedSystemSubHeadingCode => item.BY_HarmonisedTariff.SubstringSafe(0, 6);
		public string CombinedNomenclatureCode => item.BY_HarmonisedTariff.Length == 8 ? item.BY_HarmonisedTariff.SubstringSafe(6, 2) : null;
		public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = NctsDataRetrieveMethods.GetUNDGDataItems(item).Select((undg, index) => new DangerousGoodsProvider(undg, index + 1)).ToArray<IDangerousGoods>());
		IReadOnlyCollection<IDangerousGoods> dangerousGoods;

		public decimal GrossMass => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, item.GrossMassInKilograms);

		public decimal NetMass => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, item.NetMassInKilograms);

		readonly NctsCommonCargoDesc item;
		readonly EU.NCTS.Business.NctsHeader nctsHeader;
	}
}
