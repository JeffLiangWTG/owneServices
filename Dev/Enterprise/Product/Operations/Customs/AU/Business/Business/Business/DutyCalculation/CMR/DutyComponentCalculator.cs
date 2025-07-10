using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DutyComponentCalculator
	{
		public DutyResult CalculateDuty(ICMRDutyData dutyData, ICMRDutyRate firstDutyRate, ICMRDutyRate additionalDutyRate)
		{
			DutyResult result = new DutyResult();
			if (firstDutyRate != null)
			{
				if (firstDutyRate.CalculationType == Constants.DutyCalcTypes.InCalc
					|| firstDutyRate.CalculationType == Constants.DutyCalcTypes.Free
					|| firstDutyRate.CalculationType == Constants.DutyCalcTypes.Info)
				{
					result.Amount = Money.Empty;
				}
				else
				{
					result = GetDutyResult(dutyData, firstDutyRate);
					DutyResult resultFromAdditional = GetDutyResult(dutyData, additionalDutyRate);

					if (firstDutyRate.CalculationType == Constants.DutyCalcTypes.Lower && resultFromAdditional.Amount.Amount < result.Amount.Amount
						|| firstDutyRate.CalculationType == Constants.DutyCalcTypes.Higher && resultFromAdditional.Amount.Amount > result.Amount.Amount)
					{
						result = resultFromAdditional;
					}
				}
			}
			return result;
		}

		internal DutyResult GetDutyResult(ICMRDutyData dutyData, ICMRDutyRate dutyRate)
		{
			DutyDataFromInvoiceLine randomLineDutyData = dutyData.RandomLineDutyData;

			DutyResult result = new DutyResult();
			if (dutyRate != null)
			{
				RateInfo[] ratesApplicable = dutyRate.RatesApplicable;

				bool hasFlatRate = false;
				ZDecimal percentage = 0m;

				ZDecimal totalDuty = 0m;

				foreach (RateInfo rate in ratesApplicable)
				{
					ZDecimal baseValue = 0m;
					ZDecimal adjustedRate = 0m;

					if (rate.DutyRateField == DutyRateField.CustomsValue)
					{
						baseValue = dutyData.CustomsValue;
						percentage = rate.Rate;
						adjustedRate = rate.Rate / 100;
					}
					else if (rate.DutyRateField == DutyRateField.FirstQty)
					{
						baseValue = ConvertToCustomsQuantity(randomLineDutyData.FirstUQ, dutyData.FirstQty, rate.Unit);
						adjustedRate = rate.Rate;
						hasFlatRate = true;
					}
					else if (rate.DutyRateField == DutyRateField.SecondQty)
					{
						baseValue = ConvertToCustomsQuantity(randomLineDutyData.SecondUQ, dutyData.SecondQty, rate.Unit);
						adjustedRate = rate.Rate;
						hasFlatRate = true;
					}
					else if (rate.DutyRateField == DutyRateField.OtherDutyFactor)
					{
						baseValue = dutyData.OtherDutyFactor;
						adjustedRate = rate.Rate;
					}

					if (hasFlatRate)
					{
						result.FlatRateUQ = rate.Unit;
						result.FlatRateAmount = rate.Rate;
					}
					ZDecimal unRounded = baseValue * adjustedRate;
					totalDuty += ZArchitecture.Core.Utilities.Round(unRounded, 5);
				}

				result.Percent = percentage;
				totalDuty = totalDuty.Truncate(2);
				result.Amount = new Money(totalDuty, JobDeclaration.GetLocalCurrency());
			}
			return result;
		}

		ZDecimal ConvertToCustomsQuantity(ZString sourceUnit, ZDecimal sourceQty, ZString destinationUnit)
		{
			return destinationUnit == sourceUnit ? sourceQty : ZDecimal.Zero;//TODO : More sophisticated conversion
		}
	}
}
