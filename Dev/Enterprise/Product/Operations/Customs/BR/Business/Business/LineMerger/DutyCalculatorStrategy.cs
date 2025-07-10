using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business
{
	public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)declaration;

		protected override bool ShouldCalculateDuties => Declaration.IsImportExcludingLicense;

		protected override ZString GetEntryLineFeeType(RateView rate) => rate.ZZ2_ZZR_RateTypeCode;

		public override void CalculateDuties()
		{
			base.CalculateDuties();

			if (Declaration.IsImportExcludingLicense)
			{
				CalculateDutyAndTaxBySpecialRate();
				CalculateSiscomexUsageEntryFee();
				CalculateAfrmmEntryFee();
				CalculateOtherExpensesICMSEntryFee();
				CalculateICMSEntryFee();
				CalculateImportLicenseFineEntryFee();
			}
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersForCalculation() => Declaration.ActiveEntryHeaders.FormalEntries;

		protected override void CalculateDutiesForRates(Customs.Business.CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, IEnumerable<RateView> applicableRates)
		{
			var invLine = entryLine.RandomLine as JobComInvoiceLine;
			var specialCaseList = invLine.SpecialCaseTaxes.Cast<SpecialCaseTax>();
			foreach (var rateCode in LoadRateCodesInOrderOfCalculation(entryLine.Factory))
			{
				var rate = applicableRates.FirstOrDefault(x => x.RateCode == rateCode.ZY1_RateCode);
				if (rate != null && !specialCaseList.Any(x => x.TaxGroup == rateCode.ZY1_RateCode))
				{
					CalculateByCustomsRate(entryLine as CusEntryLine, entryLineUniversalData, rate);
				}
				else
				{
					CalculateByOverriddenRate(entryLine as CusEntryLine, entryLineUniversalData, rateCode);
				}
			}
		}

		protected override IEnumerable<IZZRateSelectionCriteria> GetRateSelectionCriteria(BaseJobComInvoiceLine invLine)
		{
			var invoiceLine = (JobComInvoiceLine)invLine;
			if (invoiceLine.JI_PrimaryPreference == RatePreferenceType.Normal)
			{
				yield return invoiceLine.DutyRateSelectionCriteria;
			}
			if (!invoiceLine.PisRateIsOverridden)
			{
				yield return invoiceLine.PISVigentRateSelectionCriteria;
			}
			if (!invoiceLine.CofinsRateIsOverridden)
			{
				yield return invoiceLine.CofinsVigentRateSelectionCriteria;
			}
			if (!invoiceLine.IPIRateIsOverridden)
			{
				yield return invoiceLine.IPIVigentRateSelectionCriteria;
			}
			if (!invoiceLine.AntidumpingRateIsOverridden)
			{
				yield return invoiceLine.AntidumpingRateSelectionCriteria;
			}
		}

		protected override IEnumerable<RateLoadTariffCriteriaSet> GetTariffAndRateCriteriaSetsForInvoiceLine(BaseJobComInvoiceLine randomLine)
		{
			var rateCriteriaSets = base.GetTariffAndRateCriteriaSetsForInvoiceLine(randomLine).ToList();

			var invLine = randomLine as JobComInvoiceLine;
			if (invLine.JI_PrimaryPreference == RatePreferenceType.ExTariff && invLine.ExDutyTariff is TariffView exDutyTariff)
			{
				rateCriteriaSets.Insert(0, new RateLoadTariffCriteriaSet(exDutyTariff, invLine.DutyRateSelectionCriteria));
			}
			return rateCriteriaSets;
		}

		CusRefRateCodeView[] LoadRateCodesInOrderOfCalculation(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RateCodesInOrderOfCalculation", () =>
			{
				return CusRefRateCodeView.Loader.Load(factory, Core.Constants.CountryCodes.Brazil).OrderBy(x => x, new CusRefRateCodeViewComparer()).ToArray();
			});
		}

		void CalculateByOverriddenRate(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, CusRefRateCodeView rateCode)
		{
			var tax = entryLine.RandomLine.Taxes.FindByType(rateCode.ZY1_RateCode);
			var methodOfCalculation = tax?.JLT_MethodOfCalculation ?? ZString.Empty;
			if (methodOfCalculation.IsEmpty || methodOfCalculation == SpecialCaseTaxTypeList.Codes.AdValoremRate || methodOfCalculation == SpecialCaseTaxTypeList.Codes.Reduced ||
				(entryLine.Declaration.IsImportSiscomex && methodOfCalculation != SpecialCaseTaxTypeList.Codes.QuantityPerUnit))
			{
				var rate = tax?.JLT_Rate ?? 0m;
				CalculateDuty(entryLine, entryLineUniversalData, rateCode.RateType, $"VFD * {rate / 100m}", rate);
			}
		}

		void CalculateByCustomsRate(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, RateView rateView)
		{
			var rate = ZDecimal.TryParse(rateView.ZZ2_RateFormulaDerivedFrom, out var result) ? result : (decimal?)null;
			CalculateDuty(entryLine, entryLineUniversalData, rateView?.CusRateType, rateView.ZZ2_RateFormula, rate);
		}

		void CalculateDuty(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, RefCusRateType cusRateType, ZString rateFormula, decimal? rate)
		{
			if (cusRateType != null)
			{
				var dutyAmount = 0m;
				var rateType = cusRateType.ZZR_RateType;

				entryLineUniversalData.CustomsValueFormula = cusRateType.ZZR_CustomsValueFormula;

				if (ShouldCalculateDuty(rateType, entryLine.RandomLine, entryLineUniversalData.DateOfValuation))
				{
					dutyAmount = Calculate(entryLine, entryLineUniversalData, rateFormula);
				}

				if (rateType == Universal.Constants.RateTypes.Duty && entryLine.RandomLine.DutyTaxRegime == TaxRegimeList.Codes.Suspension && RatePreferenceRequiresCalculateBaseAmount(entryLine.RandomLine.JI_PrimaryPreference))
				{
					var dutyRate = (entryLine.RandomLine.DutyRateIsOverridden ? entryLine.RandomLine.OverriddenDutyRateValue : entryLine.RandomLine.NormalDutyRateValue) / 100;
					entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(rateType, entryLineUniversalData.CustomsValue * dutyRate);
				}
				else
				{
					entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(rateType, dutyAmount);
				}

				UpdateEntryLineFee(entryLine, entryLineUniversalData, rateType, dutyAmount, rate);
			}
		}

		ZBool RatePreferenceRequiresCalculateBaseAmount(ZString ratePreference)
		{
			switch (ratePreference)
			{
				case Constants.RatePreferenceType.Normal:
				case Constants.RatePreferenceType.FreeTradeAgreement:
				case Constants.RatePreferenceType.ExTariff:
				case Constants.RatePreferenceType.ReducedRate:
					return true;
				default:
					return false;
			}
		}

		ZBool ShouldCalculateDuty(ZString rateType, JobComInvoiceLine invoiceLine, ZDateTime date)
		{
			switch (rateType)
			{
				case Universal.Constants.RateTypes.Duty:
					return BRRefCusProcedure.GetRefCusProcedure(invoiceLine.Factory, ProcedureCategories.Duty, declaration.JE_MessageType, declaration.JE_MessageSubType, invoiceLine.DutyTaxRegime, date)?.ZZ6_CalculateDuty ?? true;
				case RateTypes.PIS:
				case RateTypes.Cofins:
					return BRRefCusProcedure.GetRefCusProcedure(invoiceLine.Factory, ProcedureCategories.PisCofins, declaration.JE_MessageType, declaration.JE_MessageSubType, invoiceLine.PisCofinsTaxRegime, date)?.ZZ6_CalculateDuty ?? true;
				case RateTypes.IPI:
					return !new[] { IPITaxRegimeList.Codes.Exemption, IPITaxRegimeList.Codes.NonTaxable, IPITaxRegimeList.Codes.Suspension }.Contains(invoiceLine.IPITaxRegime.ToString());
				default:
					return true;
			}
		}

		void UpdateEntryLineFee(Customs.Business.CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, ZString feeType, decimal dutyAmount, decimal? rate)
		{
			var fee = entryLine.Fees.AddOrUpdate(feeType, dutyAmount);
			fee.CF_BaseValue = Utilities.Round(entryLineUniversalData.ValueForDuty, fee.CF_BaseValueDecimalPlaces);
			fee.CF_Rate = rate ?? (fee.CF_BaseValue != 0m ? (fee.CF_ChargeAmount / fee.CF_BaseValue) * 100m : 0m);
			fee.CF_MethodOfCalculation = Constants.MethodOfCalculation.Percentage;
		}

		protected override void UpdateEntryLineFees(Customs.Business.CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
		}

		void CalculateSiscomexUsageEntryFee()
		{
			var entryHeaders = Declaration.IsImportOnly ? Declaration.ActiveEntryHeaders.SiscomexUsageFeeEntries : Declaration.ActiveEntryHeaders.FormalEntries;
			foreach (var entryHeader in entryHeaders)
			{
				new SiscomexUsageEntryFeeCalculator(entryHeader).UpdateFeeOnEntryLines();
			}
		}

		void CalculateICMSEntryFee()
		{
			var calculator = new ICMSFCPFeeCalculator();
			foreach (var entryHeader in Declaration.ActiveEntryHeaders.FormalEntries)
			{
				foreach (var entryLine in entryHeader.MergedLines)
				{
					calculator.UpdateFeesOnEntryLine(entryLine);
				}
			}
		}

		void CalculateAfrmmEntryFee()
		{
			if (Declaration.ShouldCalculateAfrmm)
			{
				foreach (var entryHeader in Declaration.ActiveEntryHeaders.FormalEntries)
				{
					new AFRMMEntryFeeCalculator(entryHeader).UpdateFeeOnEntryLines();
				}
			}
		}

		void CalculateImportLicenseFineEntryFee()
		{
			foreach (var entryHeader in Declaration.ActiveEntryHeaders.FormalEntries)
			{
				foreach (var entryLine in entryHeader.MergedLines)
				{
					new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();
				}
			}
		}

		void CalculateOtherExpensesICMSEntryFee()
		{
			foreach (var entryHeader in Declaration.ActiveEntryHeaders.FormalEntries)
			{
				foreach (var entryLine in entryHeader.MergedLines)
				{
					new OtherExpensesICMSFCPFeeCalculator(entryLine).UpdateOtherExpensesICMSFeeOnEntryLine();
				}
			}
		}

		void CalculateDutyAndTaxBySpecialRate()
		{
			foreach (var entryHeader in Declaration.ActiveEntryHeaders.FormalEntries)
			{
				foreach (var entryLine in entryHeader.MergedLines)
				{
					new SpecialRateEntryFeeCalculator(entryLine).UpdateFeesOnEntryLine();
				}
			}
		}
	}
}
