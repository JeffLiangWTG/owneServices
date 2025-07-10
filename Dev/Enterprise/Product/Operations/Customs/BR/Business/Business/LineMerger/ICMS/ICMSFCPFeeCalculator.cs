using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public interface IICMSCalculationParameters
	{
		ZDecimal ICMSRate { get; }
		ZDecimal ICMSFCPRate { get; }
		ZDecimal ICMSBaseValueReductionPercentage { get; }
		ZDecimal ICMSTotalAmountReductionPercentage { get; }
		ZString ICMSFormula { get; }
		ZString ICMSTaxRegime { get; }
	}

	public interface IICMSCalculationValues
	{
		ZDecimal CustomsValue { get; }
		ZDecimal DutyAmount { get; }
		ZDecimal IPIAmount { get; }
		ZDecimal PISAmount { get; }
		ZDecimal CofinsAmount { get; }
		ZDecimal SiscomexUsageAmount { get; }
		ZDecimal AfrmmTaxAmount { get; }
		ZDecimal AntidumpingAmount { get; }
		ZDecimal EICAmount { get; }
	}

	public class ICMSFCPFeeCalculator
	{
		const string FeeType = Constants.RateTypes.ICMS;

		public ICMSFCPFee Calculate(IICMSCalculationParameters parameters, IICMSCalculationValues values)
		{
			ZDecimal? icmsBaseAmount = null, icmsTotalAmount = null, icmsFCPTotalAmount = null;

			var icmsRate = parameters.ICMSRate / 100;
			var icmsFCPRate = parameters.ICMSFCPRate / 100;
			var baseValueReduction = parameters.ICMSBaseValueReductionPercentage / 100;
			var totalAmountReduction = parameters.ICMSTotalAmountReductionPercentage / 100;

			if (icmsRate > 0 && icmsRate < 1 && !(baseValueReduction > 0 && parameters.ICMSFormula.IsEmpty))
			{
				var totalBaseAmount = values.CustomsValue
					+ values.DutyAmount
					+ values.IPIAmount
					+ values.PISAmount
					+ values.CofinsAmount
					+ values.SiscomexUsageAmount
					+ values.AfrmmTaxAmount
					+ values.AntidumpingAmount
					+ values.EICAmount;

				var icmsRateBaseAmountCalculation = icmsRate + icmsFCPRate;

				if (parameters.ICMSFormula == ICMSFormulaList.Codes.BCR && baseValueReduction > 0)
				{
					icmsRateBaseAmountCalculation -= icmsRateBaseAmountCalculation * baseValueReduction;
				}

				icmsBaseAmount = totalBaseAmount / (1 - icmsRateBaseAmountCalculation);

				if (!parameters.ICMSFormula.IsEmpty && baseValueReduction > 0)
				{
					icmsBaseAmount *= 1 - baseValueReduction;
				}

				icmsTotalAmount = 0m;
				icmsFCPTotalAmount = 0m;

				if (parameters.ICMSTaxRegime == ICMSTaxRegimeList.Codes.FullCollection || parameters.ICMSTaxRegime == ICMSTaxRegimeList.Codes.Reduction)
				{
					icmsTotalAmount = icmsBaseAmount * icmsRate;
					icmsFCPTotalAmount = icmsBaseAmount * icmsFCPRate;

					if ((!parameters.ICMSFormula.IsEmpty && totalAmountReduction > 0) || (baseValueReduction == 0 && totalAmountReduction > 0))
					{
						icmsTotalAmount -= icmsTotalAmount * totalAmountReduction;
					}
				}
			}

			return new ICMSFCPFee() { BaseAmount = icmsBaseAmount, ICMSAmount = icmsTotalAmount, FCPAmount = icmsFCPTotalAmount };
		}

		public void UpdateFeesOnEntryLine(CusEntryLine entryLine)
		{
			var parameters = entryLine.RandomLine as IICMSCalculationParameters;
			var result = Calculate(parameters, entryLine);

			if (result.ICMSAmount.HasValue && result.BaseAmount.HasValue)
			{
				var icmsFee = entryLine.Fees.AddOrUpdate(FeeType, result.ICMSAmount.Value.Round(2));
				icmsFee.CF_BaseValue = result.BaseAmount.Value.Round(2);
				icmsFee.CF_Rate = parameters.ICMSRate;
				icmsFee.CF_MethodOfCalculation = Constants.MethodOfCalculation.Percentage;
			}

			if (parameters.ICMSFCPRate > 0 && result.FCPAmount.HasValue && result.BaseAmount.HasValue)
			{
				var icmsFCPFee = entryLine.Fees.AddOrUpdate(Constants.RateTypes.ICMSFCP, result.FCPAmount.Value.Round(2));
				icmsFCPFee.CF_BaseValue = result.BaseAmount.Value.Round(2);
				icmsFCPFee.CF_Rate = parameters.ICMSFCPRate;
				icmsFCPFee.CF_MethodOfCalculation = Constants.MethodOfCalculation.Percentage;
			}
		}

		public class ICMSFCPFee
		{
			public ZDecimal? BaseAmount { get; set; }
			public ZDecimal? ICMSAmount { get; set; }
			public ZDecimal? FCPAmount { get; set; }
		}
	}
}
