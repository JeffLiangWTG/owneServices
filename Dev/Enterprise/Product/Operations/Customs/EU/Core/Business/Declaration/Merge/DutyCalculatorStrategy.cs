using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)base.declaration;
		readonly UserEnteredStashManager userEnteredStashManager = new UserEnteredStashManager();

		protected override bool ShouldCalculateDuties => UseUniversalFeeCalculation;

		public override void CalculateDuties()
		{
			CalculateValueForVAT();
			if (ShouldCalculateDuties)
			{
				CacheRatesForAllEntries();
				CalculateDutiesForAllEntries();
			}
		}

		protected override IEnumerable<IZZRateSelectionCriteria> GetRateSelectionCriteria(BaseJobComInvoiceLine invoiceLine)
		{
			var result = new List<IZZRateSelectionCriteria>();
			var invLine = (JobComInvoiceLine)invoiceLine;
			result.Add(invLine.DutyRateSelectionCriteria);
			result.Add(invLine.AntiDumpingRateSelectionCriteria);
			result.Add(invLine.CountervailingRateSelectionCriteria);
			result.AddRange(invLine.NationalRateSelectionCriteria);
			return result;
		}

		protected override void CalculateDutiesForAllEntries()
		{
			foreach (CusEntryLine entryLine in Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>()
						.SelectMany(entryHeader => entryHeader.MergedLines))
			{
				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					ClearEntryLineSystemCalculatedStashingUserEnteredValue(entryLine);
					CalculateAllEntryLineFees(entryLine);
				}
			}
		}

		void CalculateAllEntryLineFees(CusEntryLine entryLine)
		{
			if (ShouldCalculateDutiesForEntryLine(entryLine))
			{
				CalculateEntryLineFees(entryLine);
			}

			if (ShouldCalculateTaxesForEntryLine(entryLine))
			{
				CalculateEntryLineExtraFees(entryLine);
				CalculateEntryLineVatFee(entryLine);
				CalculateEntryLineNonVatableExtraFees(entryLine);
			}
		}

		protected virtual bool ShouldCalculateDutiesForEntryLine(CusEntryLine entryLine) => entryLine.RandomLine.CusProcedure?.ZZ6_CalculateDuty ?? true;

		protected virtual bool ShouldCalculateTaxesForEntryLine(CusEntryLine entryLine) => true;

		void ClearEntryLineSystemCalculatedStashingUserEnteredValue(CusEntryLine entryLine)
		{
			foreach (var systemLineFee in entryLine.Fees.Cast<CusEntryLineFee>().Where(f => f.IsActionBlank).ToList())
			{
				if (ShouldStash(entryLine, systemLineFee))
				{
					userEnteredStashManager.Stash(systemLineFee.UserEnteredStashSource);
				}
				entryLine.Fees.RemoveAndDelete(systemLineFee);
			}
		}

		protected virtual ZBool ShouldStash(CusEntryLine entryLine, CusEntryLineFee entryLineFee) => true;

		protected virtual void CalculateEntryLineFees(CusEntryLine entryLine)
		{
			var entryLineTariffAndCriteriaSets = GetTariffAndRateCriteriaSetsForInvoiceLine(entryLine.RandomLine);
			var rates = RateLoader.LoadRatesForMultipleCriteriaSets(entryLineTariffAndCriteriaSets);

			PopulateEntryLineFees(entryLine, rates.Where(rate => rate != null && !ShouldCalculateSystemFeeForThisCode(entryLine, rate.RateCode)));
		}

		protected virtual void PopulateEntryLineFees(CusEntryLine entryLine, IEnumerable<RateView> rates) => rates.ForEach(rate => CalculateUniversalRateFees(entryLine, rate));

		protected virtual bool ShouldCalculateSystemFeeForThisCode(CusEntryLine entryLine, string rateCode) => entryLine.Fees.HasOverrideFeeOfGivenCode(rateCode);

		protected virtual IUniversalDutyCalculator GetNewEntryLineDutyCalculator(CusEntryLine entryLine) => new EntryLineDutyCalculator(entryLine, RateCalculationVisitorMode);

		void CalculateUniversalRateFees(CusEntryLine entryLine, RateView rateForCalculation)
		{
			var dutyCalculator = GetNewEntryLineDutyCalculator(entryLine);
			var calculatedDuty = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);
			var rateCode = entryLine.GetFeeCodeFromRateCode(rateForCalculation.RateCode);
			var overrideReasonCode = GetOverrideReasonCode(rateForCalculation);

			calculatedDuty.IntermediateResults.ForEach(calculatedFee
				=> AddNewEntryLineFee(entryLine, rateCode, overrideReasonCode, calculatedFee, rateForCalculation));
		}

		protected virtual bool ShouldDutyCalculationIncludeNonParticipatingMinMaxResults => false;

		protected virtual void AddNewEntryLineFee(CusEntryLine entryLine, ZString rateCode, ZString overrideReasonCode, IDutyCalculationIntermediateResult calculatedFee, RateView rateForCalculation = null)
		{
			if (calculatedFee != null)
			{
				var newEntryLineFee = entryLine.Fees.AddNew();
				newEntryLineFee.CF_ChargeType = rateCode;
				newEntryLineFee.CF_RateOverrideReasonCode = overrideReasonCode;
				newEntryLineFee.CF_MethodOfCalculation = calculatedFee.MethodOfCalculation;
				newEntryLineFee.CF_Rate = calculatedFee.AdjustedRate;
				SetBaseValue(entryLine, newEntryLineFee, calculatedFee);
				SetChargeAmount(entryLine, newEntryLineFee, calculatedFee);
				SetUserEditableValues(newEntryLineFee, calculatedFee);
				SetNationalType(entryLine, newEntryLineFee, rateForCalculation);
			}
		}

		protected virtual void SetNationalType(CusEntryLine entryLine, CusEntryLineFee entryLineFee, RateView rateForCalculation)
		{
		}

		protected virtual ZString GetOverrideReasonCode(RateView rateView) => ZString.Empty;

		protected virtual void SetBaseValue(CusEntryLine entryLine, CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		{
			entryLineFee.CF_BaseValue = intermediateResult.BaseValue;
		}

		protected virtual void SetChargeAmount(CusEntryLine entryLine, CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		{
			entryLineFee.CF_ChargeAmount = intermediateResult.ParticipatingAmount;
		}

		protected virtual void SetUserEditableValues(CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		{
			var stashApplied = userEnteredStashManager.Apply(entryLineFee.UserEnteredStashSource);
			if (!stashApplied)
			{
				if (!intermediateResult.MethodOfPayment.IsEmpty)
				{
					entryLineFee.CF_MethodOfPayment = intermediateResult.MethodOfPayment;
				}
				else
				{
					entryLineFee.DefaultMethodOfPaymentIfEmpty();
				}
			}
		}

		protected virtual void CalculateEntryLineVatFee(CusEntryLine entryLine)
		{
			if (!ShouldCalculateSystemFeeForThisCode(entryLine, UniversalReferenceConstants.RefCusRateCodes.Vat) && !entryLine.HasAnyProcedureWithSuspendedVat)
			{
				var vatCalculator = entryLine.GetEntryLineVatCalculator();
				var calculatedVat = vatCalculator.CalculateVatFee();
				AddNewEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.Vat, ZString.Empty, calculatedVat);
			}
		}

		void CalculateEntryLineExtraFees(CusEntryLine entryLine)
			=> CalculateExtraFees(entryLine, GetExtraFeeCalculatorCollection(entryLine));

		void CalculateEntryLineNonVatableExtraFees(CusEntryLine entryLine)
			=> CalculateExtraFees(entryLine, GetNonVatableExtraFeeCalculatorCollection(entryLine));

		void CalculateExtraFees(CusEntryLine entryLine, IEnumerable<IExtraFeeCalculator> extraFeeCalculatorCollection)
		{
			foreach (var extraFeeCalculator in extraFeeCalculatorCollection)
			{
				CalculateExtraSystemFeesIfApplicable(entryLine, extraFeeCalculator);
			}
		}

		protected virtual void CalculateExtraSystemFeesIfApplicable(CusEntryLine entryLine, IExtraFeeCalculator extraFeeCalculator)
		{
			var rateCode = extraFeeCalculator.RateCode;

			if (!ShouldCalculateSystemFeeForThisCode(entryLine, rateCode))
			{
				var extraFeesIntermediateResults = extraFeeCalculator.CalculateExtraFees();

				foreach (var extraFeeIntermediateResult in extraFeesIntermediateResults)
				{
					AddNewEntryLineFee(entryLine, rateCode, ZString.Empty, extraFeeIntermediateResult);
				}
			}
		}

		IEnumerable<IExtraFeeCalculator> GetExtraFeeCalculatorCollection(CusEntryLine entryLine) => UseUniversalFeeCalculation
			? GetExtraFeeCalculatorCollectionCore(entryLine)
			: Enumerable.Empty<IExtraFeeCalculator>();

		protected virtual IEnumerable<IExtraFeeCalculator> GetExtraFeeCalculatorCollectionCore(CusEntryLine entryLine) => Enumerable.Empty<IExtraFeeCalculator>();

		IEnumerable<IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollection(CusEntryLine entryLine) => UseUniversalFeeCalculation
			? GetNonVatableExtraFeeCalculatorCollectionCore(entryLine)
			: Enumerable.Empty<IExtraFeeCalculator>();

		protected virtual IEnumerable<IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollectionCore(CusEntryLine entryLine) => Enumerable.Empty<IExtraFeeCalculator>();

		bool UseUniversalFeeCalculation => Declaration.Configuration.UseUniversalFeeCalculation(Declaration);

		public virtual decimal? CalculateAdValoremDutyRateForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var invoiceLineTariffAndCriteriaSets = GetTariffAndRateCriteriaSetsForInvoiceLine(invoiceLine);
			var rates = RateLoader.LoadRatesForMultipleCriteriaSets(invoiceLineTariffAndCriteriaSets);

			decimal? adjustedRate = default;
			var a00Rates = rates.FirstOrDefault(r => r.RateCode == UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
			if (a00Rates != null)
			{
				var dutyCalculator = new InvoiceLineDutyCalculator(invoiceLine, RateCalculationVisitorMode);
				var calculatedDuty = dutyCalculator.CleanFormulaAndCalculate(a00Rates);
				if (calculatedDuty.IntermediateResults.Count() == 1)
				{
					var dc = calculatedDuty.IntermediateResults.Single();
					if (dc.MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage)
					{
						adjustedRate = dc.AdjustedRate;
					}
				}
			}

			return adjustedRate;
		}

		protected RateCalculationVisitorMode RateCalculationVisitorMode =>
			ShouldDutyCalculationIncludeNonParticipatingMinMaxResults
				? RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults
				: RateCalculationVisitorMode.Default;
	}
}
