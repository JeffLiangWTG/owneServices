using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public static class CUSTAXEntryLineConfirmedFeesHelper
	{
		public static void CreateEntryLineConfirmedFeesIfLineIsFinal(this CusEntryLine entryLine, ICUSTAXLine line)
		{
			if (line.LineCompletionFlag.In(lineCompletionFlagsForCreation))
			{
				foreach (var duty in line.Duties)
				{
					var newFee = CreateNewFee(entryLine, duty);
					newFee.CF_IsLandedCostOnly = false;
					newFee.CF_BaseValue = duty.BaseValue;
					newFee.CF_ChargeAmount = duty.ChargeAmount;

					var firstRate = duty.DutyRates.FirstOrDefault();
					if (firstRate != null)
					{
						newFee.CF_MethodOfCalculation = BuildMethodOfCalculation(firstRate);
						newFee.CF_Rate = firstRate.Rate;

						foreach (var dutyRate in duty.DutyRates.Skip(1))
						{
							var newChildFee = CreateNewFee(entryLine, duty);
							newChildFee.CF_MethodOfCalculation = BuildMethodOfCalculation(dutyRate);
							newChildFee.CF_Rate = dutyRate.Rate;
							newChildFee.CF_IsLandedCostOnly = true;
						}
					}
					else
					{
						newFee.CF_MethodOfCalculation = duty.MethodOfCalculation;
					}
				}
			}

			static EU.Business.Declaration.CusEntryLineFee CreateNewFee(CusEntryLine entryLine, ICUSTAXLineDuty duty)
			{
				var newFee = entryLine.ConfirmedFees.AddNew() as EU.Business.Declaration.CusEntryLineFee;
				newFee.CF_ChargeType = ((ZString)duty.ChargeType).Left(3);
				newFee.NationalFeeTypeCode = duty.ChargeType;
				return newFee;
			}
		}

		static string BuildMethodOfCalculation(ICUSTAXLineDutyRate dutyRate) => $"{dutyRate.CriteriaType} {dutyRate.AssessmentScale}";

		public static void DeleteEntryLineConfirmedFees(this CusEntryLine entryLine)
		{
			entryLine.ConfirmedFees.RemoveAndDeleteAll();
		}

		public static bool RequiresProcessingOfEntryLineConfirmedFees(this ICUSTAXLine line)
		{
			return line.LineCompletionFlag.In(lineCompletionFlagsForDeletion);
		}

		static readonly ImmutableArray<string> lineCompletionFlagsForDeletion = ImmutableArray.Create(
			ImportCompletionFlagList.Codes._1, ImportCompletionFlagList.Codes._2, ImportCompletionFlagList.Codes._3,
			ImportCompletionFlagList.Codes._4, ImportCompletionFlagList.Codes._5, ImportCompletionFlagList.Codes._6
		);

		static readonly ImmutableArray<string> lineCompletionFlagsForCreation = ImmutableArray.Create(ImportCompletionFlagList.Codes._4, ImportCompletionFlagList.Codes._5, ImportCompletionFlagList.Codes._6);
	}
}
