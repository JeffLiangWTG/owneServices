using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;
using RefCusRateTypes = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GoodsItemDutyCalculator
	{
		public GoodsItemDutyCalculator(NctsCommonCargoDesc goodsItem)
		{
			GoodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		protected NctsCommonCargoDesc GoodsItem { get; }

		public IDutyCalculationResult Calculate(RateView rateForCalculation)
		{
			Argument.NotNull(rateForCalculation, nameof(rateForCalculation));

			var rateFormula = CleanFormula(rateForCalculation);
			var universalRateData = GetGoodsItemRateCalcData();

			var calculationResult = IsAddOrCvdDeparture(rateForCalculation)
				? AddOrCvdDepartureRateCalculationVisitor.CalculateParticipatingFees(universalRateData, rateFormula)
				: RateCalculationVisitor.CalculateParticipatingFees(universalRateData, rateFormula);

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

		protected virtual GoodsItemRateCalcData GetGoodsItemRateCalcData() =>
			new GoodsItemRateCalcData(GoodsItem);

		bool IsAddOrCvdDeparture(RateView rateForCalculation) =>
			GoodsItem.Header.IsDepartureMovement && IsAddOrCvdRateType(rateForCalculation.ZZ2_ZZR_RateTypeCode);

		static bool IsAddOrCvdRateType(string rateTypeCode) =>
			rateTypeCode == RefCusRateTypes.AntiDumpingDuty || rateTypeCode == RefCusRateTypes.CountervailingDuty;
	}
}
