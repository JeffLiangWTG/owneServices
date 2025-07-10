using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescValueSetStrategy : NctsCommonCargoDescValueSetStrategy
	{
		public NctsDepartureCargoDescValueSetStrategy(NctsDepartureCargoDesc goodsItem) : base(goodsItem) { }

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case NctsCommonCargoDesc.Schema.BY_MonetaryValue:
				case NctsCommonCargoDesc.Schema.BY_RN_NKCountryOfOrigin:
				case NctsCommonCargoDesc.Schema.BY_HarmonisedTariff:
				case NctsCommonCargoDesc.Schema.BY_ZZF_NKTaxType:
					ApportionedAmountToGuaranteesLiabilityAmount();
					break;
			}
		}

		void ApportionedAmountToGuaranteesLiabilityAmount()
		{
			GoodsItem.Header?.ApportionedAmountToGuaranteesLiabilityAmount();
		}
	}
}
