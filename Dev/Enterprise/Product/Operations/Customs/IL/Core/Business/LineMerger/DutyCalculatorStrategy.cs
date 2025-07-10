using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IL.Business
{
	public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
					: base(declaration)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

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

		protected bool ShouldCalculateDutiesForEntryLine(CusEntryLine entryLine) => true;

		protected ZBool ShouldStash(CusEntryLine entryLine, CusEntryLineFee entryLineFee) => true;

		protected void CalculateEntryLineFees(CusEntryLine entryLine)
		{
			var entryLineTariffAndCriteriaSets = GetTariffAndRateCriteriaSetsForInvoiceLine(entryLine.RandomLine);
			var rates = RateLoader.LoadRatesForMultipleCriteriaSets(entryLineTariffAndCriteriaSets);

			PopulateEntryLineFees(entryLine, rates.Where(rate => rate != null && !ShouldCalculateSystemFeeForThisCode(entryLine, rate.RateCode)));
		}

		protected void PopulateEntryLineFees(CusEntryLine entryLine, IEnumerable<RateView> rates) => rates.ForEach(rate => CalculateUniversalRateFees(entryLine, rate));

		protected bool ShouldCalculateSystemFeeForThisCode(CusEntryLine entryLine, string rateCode) => entryLine.Fees.HasOverrideFeeOfGivenCode(rateCode);

		protected IUniversalDutyCalculator GetNewEntryLineDutyCalculator(CusEntryLine entryLine) => new EntryLineDutyCalculator(entryLine, RateCalculationVisitorMode);

		protected bool ShouldDutyCalculationIncludeNonParticipatingMinMaxResults => false;

		protected void AddNewEntryLineFee(CusEntryLine entryLine, ZString rateCode, ZString overrideReasonCode, IDutyCalculationIntermediateResult calculatedFee, RateView rateForCalculation = null)
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
			}
		}

		protected ZString GetOverrideReasonCode(RateView rateView) => ZString.Empty;

		protected void SetBaseValue(CusEntryLine entryLine, CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		{
			entryLineFee.CF_BaseValue = intermediateResult.BaseValue;
		}

		protected void SetChargeAmount(CusEntryLine entryLine, CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		{
			entryLineFee.CF_ChargeAmount = intermediateResult.ParticipatingAmount;
		}

		protected void SetUserEditableValues(CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
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

		protected void CalculateEntryLineVatFee(CusEntryLine entryLine)
		{
			if (!ShouldCalculateSystemFeeForThisCode(entryLine, Constants.EntryLineFee.VATFeeTypeCode) && !entryLine.HasAnyProcedureWithSuspendedVat)
			{
				var vatCalculator = entryLine.GetEntryLineVatCalculator();
				var calculatedVat = vatCalculator.CalculateVatFee();
				if (!calculatedVat?.Amount.IsEmpty ?? false)
				{
					AddNewEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, ZString.Empty, calculatedVat);
				}
			}
		}

		protected RateCalculationVisitorMode RateCalculationVisitorMode =>
			ShouldDutyCalculationIncludeNonParticipatingMinMaxResults
				? RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults
				: RateCalculationVisitorMode.Default;

		protected override bool ShouldCalculateDuties => true;

		void CalculateUniversalRateFees(CusEntryLine entryLine, RateView rateForCalculation)
		{
			var dutyCalculator = GetNewEntryLineDutyCalculator(entryLine);
			var calculatedDuty = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);
			var rateCode = rateForCalculation.RateCode;
			var overrideReasonCode = GetOverrideReasonCode(rateForCalculation);

			calculatedDuty.IntermediateResults.ForEach(calculatedFee
				=> AddNewEntryLineFee(entryLine, rateCode, overrideReasonCode, calculatedFee, rateForCalculation));
		}

		void CalculateAllEntryLineFees(CusEntryLine entryLine)
		{
			CalculateEntryLineFees(entryLine);
			CalculateEntryLineVatFee(entryLine);
		}

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

		readonly UserEnteredStashManager userEnteredStashManager = new UserEnteredStashManager();
	}
}
