using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		readonly ZDecimal minimumThreshold = 50;

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

		protected override bool CanBeNegative => true;

		public override void CalculateDuties()
		{
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.Fees.Cast<CusEntryLineFee>().ForEach(fee => fee.CF_ChargeAmount = 0);
					CalculateDutyAndTax(entryLine, new EntryLineUniversalRate(entryLine));
				}

				ClearDutyAndTaxUnderMinimumThreshold(entryHeader);

				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.CL_DutyPercent = entryLine.CL_CustomsValue.IsEmpty ? decimal.Zero : (entryLine.DutyAmount / entryLine.CL_CustomsValue) * 100;
				}
			}
		}

		void CalculateDutyAndTax(CusEntryLine entryLine, EntryLineUniversalRate universalRateData)
		{
			var invoiceLine = entryLine.RandomLine;
			var universalTariff = invoiceLine?.UniversalTariff;
			if (universalTariff != null)
			{
				var declaration = entryLine.Declaration;
				var entryHeader = entryLine.Header;

				var dutyFraction = DutyModeList.GetDutyFraction(entryLine.DutyModeCode);
				var criteria = invoiceLine.IsImport ? invoiceLine.DutyRateSelectionCriteria : invoiceLine.ExportDutyRateSelectionCriteria;
				foreach (var dutyRate in universalTariff.GetApplicableRates(criteria))
				{
					CalculateDutyAndTax(entryLine, universalRateData, dutyRate, dutyFraction, invoiceLine.UniversalTariffRateType);
				}

				if (declaration.IsImport && entryHeader.IsEntering)
				{
					var exciseFraction = DutyModeList.GetExciseFraction(entryLine.DutyModeCode);
					foreach (var taxRate in universalTariff.GetApplicableRates(invoiceLine.ExciseRateSelectionCriteria))
					{
						CalculateDutyAndTax(entryLine, universalRateData, taxRate, exciseFraction, taxRate.ZZ2_ZZR_RateTypeCode);
					}

					CalculateAdditionalRate(entryLine, universalRateData, AntiDumpingDutyRateStrategy.AdditionalElementCode, Universal.Constants.RateTypes.AntiDumping);
					CalculateAdditionalRate(entryLine, universalRateData, CountervailingDutyRateStrategy.AdditionalElementCode, Universal.Constants.RateTypes.Countervailing);

					CalculateVAT(entryLine, universalRateData, invoiceLine.AppliedTaxAndFee);
				}
			}
		}

		static void CalculateAdditionalRate(CusEntryLine entryLine, EntryLineUniversalRate universalRateData, string additionalElementCode, string rateType)
		{
			var rateString = entryLine.RandomLine.AdditionalInformationHelper.GetAdditionalElementValue(additionalElementCode);
			if (ZDecimal.TryParse(rateString, out var rate) && !rate.IsEmpty)
			{
				var amount = universalRateData.CustomsValue * rate;
				if (amount != 0)
				{
					entryLine.Fees.AddOrUpdate(rateType, Utilities.Round(amount, 2));
				}
			}
		}

		void CalculateDutyAndTax(CusEntryLine entryLine, EntryLineUniversalRate universalRateData, RateView rate, decimal fraction, string rateType)
		{
			universalRateData.CustomsValueFormula = rate?.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
			if (rate?.ZZ2_RateFormula.Contains(Constants.UniversalReferenceConstants.RateFormulaCountrySpecificValue.CVInUSD, StringComparison.Ordinal) ?? false)
			{
				if (!universalRateData.CountrySpecificValueList.ContainsKey(Constants.UniversalReferenceConstants.RateFormulaCountrySpecificValue.CVInUSD))
				{
					universalRateData.CountrySpecificValueList.Add(Constants.UniversalReferenceConstants.RateFormulaCountrySpecificValue.CVInUSD, entryLine.CustomsValueInUSD);
				}
			}

			var amount = rate == null ? 0m : CalculateDutyAndTax(entryLine, universalRateData, rate.ZZ2_RateFormula, rate.RateCode, fraction);

			if (!string.IsNullOrEmpty(rateType))
			{
				universalRateData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(rateType, amount);
			}
		}

		static void CalculateVAT(CusEntryLine entryLine, EntryLineUniversalRate universalRateData, RefCusTaxOrFee appliedTaxAndFee)
		{
			var fraction = DutyModeList.GetVATFraction(entryLine.DutyModeCode);
			if (fraction != 0)
			{
				var rate = appliedTaxAndFee?.ZZF_Value ?? ZDecimal.Zero;
				if (!rate.IsEmpty)
				{
					var dutyAmount = universalRateData.CountrySpecificValueList.GetValue(entryLine.RandomLine.UniversalTariffRateType) * DutyModeList.GetDutyFractionForVAT(entryLine.DutyModeCode);
					var exciseAmount = universalRateData.CountrySpecificValueList.GetValue(Universal.Constants.RateTypes.Excise);
					var antiDumpingAmount = entryLine.Fees.GetAmount(Universal.Constants.RateTypes.AntiDumping);
					var countervailingAmount = entryLine.Fees.GetAmount(Universal.Constants.RateTypes.Countervailing);

					var vatAmount = Utilities.Round(universalRateData.CustomsValue + dutyAmount + exciseAmount + antiDumpingAmount + countervailingAmount, 2) * rate;
					if (vatAmount != 0)
					{
						entryLine.Fees.AddOrUpdate(appliedTaxAndFee?.ZZF_Code ?? ZString.Empty, Utilities.Round(vatAmount * fraction, 2));
					}
				}
			}
		}

		decimal CalculateDutyAndTax(CusEntryLine entryLine, EntryLineUniversalRate universalRateData, ZString rateFormula, ZString rateCode, decimal fraction)
		{
			// For Duty,				ZZ2_RateFormula should be 'VFD * rate'
			// For Excise,			ZZ2_RateFormula should be 'VFD / (1 - rate) * rate'
			// For Export Duty, ZZ2_RateFormula should be 'ROUND(VFD / (1 + rate), 0) * rate'
			var amount = Calculate(entryLine, universalRateData, rateFormula);

			if (amount != 0 && fraction != 0 && !rateCode.IsEmpty)
			{
				entryLine.Fees.AddOrUpdate(rateCode, Utilities.Round(amount * fraction, 2));
			}
			return amount;
		}

		void ClearDutyAndTaxUnderMinimumThreshold(CusEntryHeader entryHeader)
		{
			var totalDutyAmount = entryHeader.TotalDutyAmount;
			if (totalDutyAmount > 0 && totalDutyAmount < minimumThreshold)
			{
				if (entryHeader.IsEntering || entryHeader.IsExiting)
				{
					ClearDutyAndTax(entryHeader, entryHeader.MergedLines[0].UniversalDutyRateType);
				}
			}

			var totalVATAmount = entryHeader.TotalGSTVATAmount;
			if (totalVATAmount > 0 && totalVATAmount < minimumThreshold)
			{
				ClearDutyAndTax(entryHeader, Constants.UniversalReferenceConstants.RefCusRateTypes.VAT);
			}

			var totalExciseAmount = entryHeader.TotalExciseAmount;
			if (totalExciseAmount > 0 && totalExciseAmount < minimumThreshold)
			{
				ClearDutyAndTax(entryHeader, Universal.Constants.RateTypes.Excise);
			}

			var totalAntiDumpingAmount = entryHeader.TotalAntiDumpingAmount;
			if (totalAntiDumpingAmount > 0 && totalAntiDumpingAmount < minimumThreshold)
			{
				ClearDutyAndTax(entryHeader, Universal.Constants.RateTypes.AntiDumping);
			}

			var totalCountervailingAmount = entryHeader.TotalCountervailingAmount;
			if (totalCountervailingAmount > 0 && totalCountervailingAmount < minimumThreshold)
			{
				ClearDutyAndTax(entryHeader, Universal.Constants.RateTypes.Countervailing);
			}
		}

		static void ClearDutyAndTax(CusEntryHeader entryHeader, ZString rateType)
		{
			entryHeader.MergedLines.SelectMany(l => l.GetFees(rateType)).ForEach(x => x.CF_ChargeAmount = 0);
		}
	}
}
