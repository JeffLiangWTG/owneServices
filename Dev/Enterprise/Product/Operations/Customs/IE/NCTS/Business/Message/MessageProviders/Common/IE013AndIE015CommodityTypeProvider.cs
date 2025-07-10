using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015CommodityTypeProvider : IIE013AndIE015CommodityType
	{
		public IE013AndIE015CommodityTypeProvider(NctsDepartureCargoDesc cargoDesc)
		{
			this.cargoDesc = cargoDesc;
		}
		readonly NctsDepartureCargoDesc cargoDesc;

		public string GoodsDescription => cargoDesc.BY_Description;

		public string CUSCode => cargoDesc.BY_CusC4Number;

		public string HarmonizedSystemSubHeadingCode => cargoDesc.BY_HarmonisedTariff.Left(6);

		public string CombinedNomenclatureCode => cargoDesc.BY_HarmonisedTariff.SubstringSafe(6, 2);

		public IReadOnlyCollection<string> DangerousGoods => dangerousGoods ?? (dangerousGoods = cargoDesc.UNDGs.Select(p => p.Substance?.DG_Code.ToString()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray());
		IReadOnlyCollection<string> dangerousGoods;

		public decimal GrossMass => cargoDesc.BY_GrossWeight;

		public decimal NetMass => cargoDesc.BY_NetWeight;

		public decimal SupplementaryUnits => cargoDesc.BY_CustomsSecondQuantity;
	}
}
