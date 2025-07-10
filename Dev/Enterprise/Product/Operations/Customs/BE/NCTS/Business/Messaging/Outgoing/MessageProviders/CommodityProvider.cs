using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CommodityProvider : ICommodity
	{
		readonly NctsCommonCargoDesc item;
		public CommodityProvider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		public string DescriptionOfGoods => item.BY_Description;

		public string CusCode => item.BY_CusC4Number;

		public string HarmonizedSystemSubHeadingCode => item.BY_HarmonisedTariff.Left(6);

		public string CombinedNomenclatureCode => item.BY_HarmonisedTariff.SubstringSafe(6, 2);

		public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = NctsDataRetrieveMethods.GetUNDGDataItems(item).Select((undg, index) => new DangerousGoodsProvider(undg, index + 1)).ToArray<IDangerousGoods>());
		IReadOnlyCollection<IDangerousGoods> dangerousGoods;

		public decimal GrossMass => item.GrossMassInKilograms;

		public decimal? NetMass => NctsDataRetrieveMethods.NetMassInKilogramsNullableByPreviousDocument(item);

		public decimal SupplementaryQty => item.BY_CustomsSecondQuantity.Normalize();

		public ICalculationOfTaxes CalculationOfTaxes => null;

		public ICommodityCode CommodityCode => null;

		public IGoodsMeasure GoodsMeasure => null;

		public decimal InvoiceLine => 0m;

		public string QuotaOrderNumber => null;

		public string TypeOfGoods => null;
	}
}
