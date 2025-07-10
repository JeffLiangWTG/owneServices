using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class GoodsItemDutyCalculator
	{
		public GoodsItemDutyCalculator(TemporaryStoragePackedItem goodsItem)
		{
			this.GoodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		protected TemporaryStoragePackedItem GoodsItem { get; }

		public IDutyCalculationResult Calculate(RateView rateForCalculation)
		{
			Argument.NotNull(rateForCalculation, nameof(rateForCalculation));

			var rateFormula = CleanFormula(rateForCalculation);
			var universalRateData = GetGoodsItemRateCalcData();
			var calculationResult = RateCalculationVisitor.CalculateParticipatingFees(universalRateData, rateFormula);

			return calculationResult;
		}

		static ZString CleanFormula(RateView rateForCalculation)
		{
			var result = rateForCalculation.ZZ2_RateFormula;

			if (result == "0")
			{
				result = FormattableString.Invariant($"{Customs.Business.UniversalReferenceConstants.ValueForDuty}*{result}");
			}

			return result;
		}

		protected virtual GoodsItemRateCalcData GetGoodsItemRateCalcData() => new GoodsItemRateCalcData(GoodsItem);
	}
}
