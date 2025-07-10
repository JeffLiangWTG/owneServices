using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
		: base(declaration)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

		protected override bool ShouldTruncate => true;

		protected override bool CanBeNegative => true;

		protected override bool ShouldCalculateDuties => Declaration.IsImport;

		protected override ZString GetEntryLineFeeType(RateView rate) => ChargeTypeList.Codes.Duty;

		public override void CalculateDuties()
		{
			if (ShouldCalculateDuties)
			{
				CacheRatesForAllEntries();
				CalculateDutiesForAllEntries();
			}
		}

		protected override void CalculateDutiesForAllEntries()
		{
			foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
			{
				foreach (var entryLine in entry.MergedLines)
				{
					var universalData = new EntryLineUniversalRate(entryLine);
					var dutyAmount = CalculateDutiesForEntryLine(entryLine, universalData);
					var dutyReductionTaxUniversalData = new EntryLineUniversalRate(entryLine);
					dutyReductionTaxUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(ChargeTypeList.Codes.Duty, dutyAmount);
					CalculateDutyReductionOrExemption(entryLine, dutyReductionTaxUniversalData);
					CalculateDomesticTax(entryLine, dutyReductionTaxUniversalData);
					CalculateVAT(entryLine, dutyReductionTaxUniversalData);
				}
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.Duty);
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.LiquorTax);
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.AgricultureTax);
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.SpecialConsumptionTax);
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.TransportationTax);
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.EducationTax);
				UpdateEntryHeaderCharge(entry, ChargeTypeList.Codes.VAT);
				CalculatePenaltyForLateDeclaration(entry);
				CalculatePenaltyForMissedDeclaration(entry);
			}
		}

		void UpdateEntryHeaderCharge(CusEntryHeader entry, string chargeType)
		{
			ZDecimal chargeAmount = 0;
			foreach (var entryLine in entry.MergedLines)
			{
				chargeAmount += entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == chargeType && x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			}

			chargeAmount = Math.Truncate(chargeAmount / 10) * 10;

			if (chargeAmount != 0)
			{
				entry.SetChargeAmount(chargeType, chargeAmount);
			}
		}

		void CalculatePenaltyForMissedDeclaration(CusEntryHeader entry)
		{
			var previousAmount = entry.Charges.Where(x => x.C1_ChargeType == ChargeTypeList.Codes.PenaltyForMissedDeclaration && !x.C1_RateOverrideReasonCode.IsEmpty)?.Sum(x => x.C1_ChargeAmount) ?? ZDecimal.Zero;
			if (Declaration.JE_MissedDecPenaltyRate > 0 || previousAmount > 0)
			{
				entry.UpdateChargeAmount(ChargeTypeList.Codes.PenaltyForMissedDeclaration, Math.Truncate(entry.TotalAmountPayable * (Declaration.JE_MissedDecPenaltyRate / 100m) / 10) * 10);
			}
		}

		void CalculateVAT(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
			var isCVandDTYexcluded = entryLine.RandomLine.JI_ProductTypeCode == ProductOrMaterialCodeList.Codes.A &&
									(entryLine.RandomLine.JI_ZZF_NKTaxType == Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA ||
									entryLine.RandomLine.JI_ZZF_NKTaxType == Constants.ZZ.RefCusTaxOrFeeCodes.VATRateB);

			var valueForVATorVATExemption = isCVandDTYexcluded ? 0m : entryLineUniversalData.CustomsValue;
			valueForVATorVATExemption += entryLineUniversalData.CountrySpecificValueList.TryGetValue(ChargeTypeList.Codes.Duty, out var dutyAmount) && !isCVandDTYexcluded ? dutyAmount : 0m;
			valueForVATorVATExemption += entryLineUniversalData.CountrySpecificValueList.TryGetValue(CountrySpecificValueListKey.DomesticTax, out var domesticTaxAmount) ? domesticTaxAmount : 0m;
			valueForVATorVATExemption += entryLineUniversalData.CountrySpecificValueList.TryGetValue(ChargeTypeList.Codes.AgricultureTax, out var agricultureTax) ? agricultureTax : 0m;
			valueForVATorVATExemption += entryLineUniversalData.CountrySpecificValueList.TryGetValue(ChargeTypeList.Codes.EducationTax, out var educationTax) ? educationTax : 0m;

			switch (entryLine.RandomLine.JI_ZZF_NKTaxType)
			{
				case Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA:
					entryLine.CL_ValueForVAT = valueForVATorVATExemption;
					entryLine.CL_ValueExemptForVAT = 0m;
					break;
				case Constants.ZZ.RefCusTaxOrFeeCodes.VATRateB:
					entryLine.CL_ValueForVAT = 0m;
					entryLine.CL_ValueExemptForVAT = valueForVATorVATExemption;
					break;
				case Constants.ZZ.RefCusTaxOrFeeCodes.VATRateC:
					valueForVATorVATExemption += entryLineUniversalData.CountrySpecificValueList.TryGetValue(CountrySpecificValueListKey.DutyReduction, out var dutyReduction) ? dutyReduction : 0m;
					entryLine.CL_ValueExemptForVAT = new ZDecimal(valueForVATorVATExemption * entryLine.RandomLine.DutyReductionRate / 100m).Truncate();
					entryLine.CL_ValueForVAT = valueForVATorVATExemption - entryLine.CL_ValueExemptForVAT;
					break;
			}

			if (!entryLine.CL_ValueForVAT.IsEmpty)
			{
				var effectiveAssessmentDate = entryLine.RandomLine.EffectiveAssessmentDate;
				var vat = new RefCusTaxOrFee.Loader(Declaration.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, entryLine.RandomLine.JI_ZZF_NKTaxType, effectiveAssessmentDate);
				var vatRate = vat?.ZZF_Value ?? ZDecimal.Zero;
				if (vatRate > 0)
				{
					var vatAmount = new ZDecimal(entryLine.CL_ValueForVAT * vatRate).Truncate();
					entryLine.SetFeeAmount(ChargeTypeList.Codes.VAT, vatAmount);
				}
			}
		}

		protected override void UpdateEntryLineFees(Customs.Business.CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
		}

		void UpdateEntryLineFee(CusEntryLine entryLine, ZString dutyTaxType, decimal dutyTaxAmount, decimal? rate = null)
		{
			entryLine.SetFeeAmount(dutyTaxType, dutyTaxAmount, rate);
		}

		ZDecimal CalculateDutiesForEntryLine(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
			var rateNumeric = ZDecimal.Zero;
			var dutyAmountToUpdate = decimal.Zero;
			var invoiceLine = entryLine.RandomLine;
			var universalTariff = invoiceLine.UniversalTariff;
			if (universalTariff != null)
			{
				var applicableRates = universalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);
				if (applicableRates.Count() == 1)
				{
					var rate = applicableRates.Single();
					dutyAmountToUpdate = Calculate(entryLine, entryLineUniversalData, rate.ZZ2_RateFormula);
					if (dutyAmountToUpdate > 0)
					{
						rateNumeric = GetRateNumeric(rate);
					}
				}
				else if (applicableRates.Count() > 1)
				{
					foreach (var rate in applicableRates)
					{
						var duty = Calculate(entryLine, entryLineUniversalData, rate.ZZ2_RateFormula);
						switch (invoiceLine.JI_DutyRateSelection)
						{
							case Constants.ZZ.ApplicabilityAdditionalCodes.Max:
								if (duty > dutyAmountToUpdate)
								{
									dutyAmountToUpdate = duty;
									if (dutyAmountToUpdate > 0)
									{
										rateNumeric = GetRateNumeric(rate);
									}
								}
								break;
							case Constants.ZZ.ApplicabilityAdditionalCodes.Min:
								if (dutyAmountToUpdate == 0 || dutyAmountToUpdate > duty)
								{
									dutyAmountToUpdate = duty;
									if (dutyAmountToUpdate > 0)
									{
										rateNumeric = GetRateNumeric(rate);
									}
								}
								break;
						}
					}
				}
				entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(ChargeTypeList.Codes.Duty, dutyAmountToUpdate);
				UpdateEntryLineFee(entryLine, ChargeTypeList.Codes.Duty, dutyAmountToUpdate, rateNumeric);
			}
			return dutyAmountToUpdate;
		}

		static public ZDecimal GetRateNumeric(RateView rate)
		{
			return rate.ZZ2_RateFormulaDerivedFrom.IsEmpty ? 0 : Convert.ToDecimal(rate.ZZ2_RateFormulaDerivedFrom);
		}

		void CalculateDutyReductionOrExemption(CusEntryLine entryLine, EntryLineUniversalRate dutyReductionTaxUniversalData)
		{
			var dutyReductionExemptionTariff = entryLine.RandomLine.DutyReductionExemptionTariff;
			if (dutyReductionExemptionTariff != null)
			{
				var rate = dutyReductionExemptionTariff.Rates.SingleOrDefault();
				if (rate != null)
				{
					dutyReductionTaxUniversalData.CustomsValueFormula = rate.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
					var dutyReductionAmount = Calculate(entryLine, dutyReductionTaxUniversalData, rate.ZZ2_RateFormula);
					if (dutyReductionAmount > 0)
					{
						dutyReductionTaxUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(ChargeTypeList.Codes.Duty, -dutyReductionAmount);
						dutyReductionTaxUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(CountrySpecificValueListKey.DutyReduction, dutyReductionAmount);
						entryLine.CL_DutyReductionAmount = dutyReductionAmount;
						if (dutyReductionTaxUniversalData.CountrySpecificValueList.TryGetValue(ChargeTypeList.Codes.Duty, out var dutyAmount))
						{
							entryLine.SetFeeAmount(ChargeTypeList.Codes.Duty, dutyAmount);
						}

						if (dutyReductionExemptionTariff.GetAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies) != null)
						{
							CalculateAgricultureTax(entryLine, dutyReductionTaxUniversalData, dutyReductionAmount, Constants.ZZ.RefCusTaxOrFeeCodes.AgricultureTaxA);
						}
					}
				}
			}
		}

		void CalculateDomesticTax(CusEntryLine entryLine, EntryLineUniversalRate dutyReductionTaxUniversalData)
		{
			var invoiceLine = entryLine.RandomLine;
			var domesticTaxTariff = invoiceLine.DomesticTax;
			if (domesticTaxTariff != null)
			{
				var domesticRate = domesticTaxTariff.Rates.SingleOrDefault(x => x.CusRateType.ZZR_RateType == Constants.ZZ.RateTypes.DomesticTax
																			&& x.ZZ2_StartDate <= invoiceLine.EffectiveAssessmentDate
																			&& x.ZZ2_EndDate >= invoiceLine.EffectiveAssessmentDate);
				if (domesticRate != null)
				{
					dutyReductionTaxUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(CountrySpecificValueListKey.InstallationCost, invoiceLine.JI_InstallationCost);
					dutyReductionTaxUniversalData.CustomsValueFormula = domesticRate.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
					var domesticTaxAmount = Calculate(entryLine, dutyReductionTaxUniversalData, domesticRate.ZZ2_RateFormula);
					if (domesticTaxAmount > 0)
					{
						var taxClassification = domesticTaxTariff.GetAttribute(Constants.ZZ.TariffAttributes.TaxClassification1)?.ZZ3_Value ?? ZString.Empty;
						dutyReductionTaxUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(CountrySpecificValueListKey.DomesticTax, domesticTaxAmount);
						CalculateDomesticTaxReductionOrExemption(entryLine, dutyReductionTaxUniversalData);

						if (dutyReductionTaxUniversalData.CountrySpecificValueList.TryGetValue(CountrySpecificValueListKey.DomesticTax, out var finalDomesticTaxAmountAfterReductionOrExemption))
						{
							if (finalDomesticTaxAmountAfterReductionOrExemption > 0)
							{
								if (domesticTaxTariff.GetAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxBApplies) != null)
								{
									CalculateAgricultureTax(entryLine, dutyReductionTaxUniversalData, finalDomesticTaxAmountAfterReductionOrExemption, Constants.ZZ.RefCusTaxOrFeeCodes.AgricultureTaxB);
								}
								CalculateEducationTax(entryLine, dutyReductionTaxUniversalData);
								UpdateEntryLineFee(entryLine, taxClassification, finalDomesticTaxAmountAfterReductionOrExemption, GetRateNumeric(domesticRate));
							}
						}
					}
				}
			}
		}

		void CalculatePenaltyForLateDeclaration(CusEntryHeader entry)
		{
			var declarationDueDate = ZDateTime.Empty;
			switch (Declaration.JE_LateDecPenaltyDateCode)
			{
				case LateDecPenaltyDateCodeList.Codes.D:
					declarationDueDate = Constants.PenaltyCalculationConstants.GetDeclarationDueDate(Declaration.JE_DateOfArrival);
					break;
				case LateDecPenaltyDateCodeList.Codes.W:
					declarationDueDate = Constants.PenaltyCalculationConstants.GetDeclarationDueDate(Declaration.UnderbondMovementArrivalDate);
					break;
			}
			if (!declarationDueDate.IsEmpty)
			{
				var effectiveAssessmentDate = ((JobDeclaration)declaration).GetEntryIssueDate(entry.PK);
				var overdueDays = (effectiveAssessmentDate - declarationDueDate).TotalDays;
				var penaltyAmount = ZDecimal.Zero;
				if (overdueDays <= ZDecimal.Zero)
				{
					penaltyAmount = ZDecimal.Zero;
				}
				else if (overdueDays <= Constants.PenaltyCalculationConstants.TwentyDaysAfterDueDate)
				{
					penaltyAmount = GetPenaltyForLateDeclaration(entry.CustomsValue, Constants.ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.FirstLevel, effectiveAssessmentDate);
				}
				else if (overdueDays <= Constants.PenaltyCalculationConstants.FiftyDaysAfterDueDate)
				{
					penaltyAmount = GetPenaltyForLateDeclaration(entry.CustomsValue, Constants.ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.SecondLevel, effectiveAssessmentDate);
				}
				else if (overdueDays <= Constants.PenaltyCalculationConstants.EightyDaysAfterDueDate)
				{
					penaltyAmount = GetPenaltyForLateDeclaration(entry.CustomsValue, Constants.ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.ThirdLevel, effectiveAssessmentDate);
				}
				else
				{
					penaltyAmount = GetPenaltyForLateDeclaration(entry.CustomsValue, Constants.ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.FourthLevel, effectiveAssessmentDate);
				}
				penaltyAmount = Math.Truncate(penaltyAmount / 10) * 10;
				entry.UpdateChargeAmount(ChargeTypeList.Codes.PenaltyForLateDeclaration, penaltyAmount);
			}
		}

		ZDecimal GetPenaltyForLateDeclaration(ZDecimal customsValue, ZString taxOrFeeCode, ZDateTime effectiveAssessmentDate)
		{
			var result = ZDecimal.Zero;
			var fee = new RefCusTaxOrFee.Loader(Declaration.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, taxOrFeeCode, effectiveAssessmentDate);
			if (fee != null)
			{
				result = customsValue * fee.ZZF_Value;
				if (result > fee.ZZF_Maximum)
				{
					result = fee.ZZF_Maximum;
				}
			}
			return result;
		}

		class CountrySpecificValueListKey
		{
			public const string InstallationCost = "InstallationCost";
			public const string DomesticTax = "DMT";
			public const string DutyReduction = "DRE";
		}

		void CalculateDomesticTaxReductionOrExemption(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
			var domesticTaxExemptionTariff = entryLine.RandomLine.DomesticTaxExemptionTariff;
			if (domesticTaxExemptionTariff != null)
			{
				var rate = domesticTaxExemptionTariff.Rates.SingleOrDefault();
				if (rate != null)
				{
					entryLineUniversalData.CustomsValueFormula = rate.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
					var domesticTaxReductionAmount = Calculate(entryLine, entryLineUniversalData, rate.ZZ2_RateFormula);
					if (domesticTaxReductionAmount > 0)
					{
						entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(CountrySpecificValueListKey.DomesticTax, -domesticTaxReductionAmount);
					}
				}
			}
		}

		void CalculateEducationTax(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
			var domesticTaxTariff = entryLine.RandomLine.DomesticTax;
			if (domesticTaxTariff != null)
			{
				var educationTaxRate = domesticTaxTariff.Rates.SingleOrDefault(x => x.RateCode == Constants.ZZ.RateCodes.EducationTaxRate);
				if (educationTaxRate != null)
				{
					entryLineUniversalData.CustomsValueFormula = educationTaxRate.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
					var educationTaxAmount = Calculate(entryLine, entryLineUniversalData, educationTaxRate.ZZ2_RateFormula);
					if (educationTaxAmount > 0)
					{
						entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(ChargeTypeList.Codes.EducationTax, educationTaxAmount);
						UpdateEntryLineFee(entryLine, ChargeTypeList.Codes.EducationTax, educationTaxAmount);
					}
				}
			}
		}

		void CalculateAgricultureTax(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, decimal valueForTax, string taxCode)
		{
			var effectiveAssessmentDate = entryLine.RandomLine.EffectiveAssessmentDate;
			var agricultureTax = new RefCusTaxOrFee.Loader(Declaration.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, taxCode, effectiveAssessmentDate);
			var agricultureTaxRate = agricultureTax?.ZZF_Value ?? ZDecimal.Zero;
			if (agricultureTaxRate > 0)
			{
				var agricultureTaxAmount = new ZDecimal(valueForTax * agricultureTaxRate).Truncate();
				entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(ChargeTypeList.Codes.AgricultureTax, agricultureTaxAmount);
				if (entryLineUniversalData.CountrySpecificValueList.TryGetValue(ChargeTypeList.Codes.AgricultureTax, out var taxAmount))
				{
					entryLine.SetFeeAmount(ChargeTypeList.Codes.AgricultureTax, taxAmount);
				}
			}
		}
	}
}
