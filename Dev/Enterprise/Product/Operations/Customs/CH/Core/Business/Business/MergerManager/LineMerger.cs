using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class LineMerger : Customs.Business.LineMerger
{
	public LineMerger(JobDeclaration declaration) : base(declaration)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

	protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
	{
		base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();
		CalculateAdditionalTaxValue();
		SynchronizeCusEntryLineFees();
	}

	void CalculateAdditionalTaxValue()
	{
		var additionalTaxes = Declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.AdditionalTaxes.Cast<CusLineTariffDetail>());

		new ApplicableRateLoader(Declaration.Factory).LoadRatesForMultipleCriteriaSets(GetRateLoadTariffCriteriaSets(additionalTaxes));

		foreach (var additionalTax in additionalTaxes)
		{
			additionalTax.SetAdditionalTaxBaseValue();
			if (additionalTax.ShouldCalculateAdditionalTaxValueAfterMerge)
			{
				additionalTax.CalculateAdditionalTaxValue();
			}
		}
	}

	void SynchronizeCusEntryLineFees()
	{
		foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
		{
			PurgeCusEntryLineFees(invoiceLine);

			var groupedTaxesAndFees = GetCollectedAdditionalTaxesAndFees(invoiceLine).GroupBy(g => g.TariffType).Select(c => new { GroupedTariffType = c.Key, SumAmount = c.Sum(s => s.Amount) }).OrderBy(o => o.GroupedTariffType).ToList();
			foreach (var taxAndFee in groupedTaxesAndFees)
			{
				invoiceLine.CusEntryLine.Fees.AddOrUpdate(taxAndFee.GroupedTariffType, taxAndFee.SumAmount);
			}

			var entryVATAmount = GetEntryVATAmount(invoiceLine);
			if (entryVATAmount != null)
			{
				invoiceLine.CusEntryLine.Fees.AddOrUpdate(entryVATAmount.TariffType, entryVATAmount.Amount);
			}
		}
	}

	void PurgeCusEntryLineFees(JobComInvoiceLine invoiceLine)
	{
		foreach (var fee in invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Where(f => !f.IsDTYorVAT).ToArray())
		{
			invoiceLine.CusEntryLine.Fees.RemoveAndDelete(fee);
		}
	}

	TaxAndFee GetEntryVATAmount(JobComInvoiceLine invoiceLine)
	{
		var taxOrFeeList = RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(invoiceLine.Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.CusEntryFeeTypes.VAT, invoiceLine.EffectiveAssessmentDate);
		var vatValue = taxOrFeeList.FirstOrDefault(i => i.ZZF_Code == invoiceLine.JI_ZZF_NKTaxType);
		var preVATAmount = invoiceLine.JI_CustomsValue + invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate;

		return (vatValue != null && preVATAmount > 0) ? new TaxAndFee() { TariffType = Core.Constants.Customs.CusEntryFeeTypes.VAT, Amount = ZArchitecture.Core.Utilities.Round(preVATAmount * vatValue.ZZF_Value * 20, 0) / 20 } : null;
	}

	IEnumerable<RateLoadTariffCriteriaSet> GetRateLoadTariffCriteriaSets(IEnumerable<CusLineTariffDetail> additionalTaxes)
	{
		foreach (var additionalTax in additionalTaxes)
		{
			var universalTariff = additionalTax.UniversalTariff;
			if (universalTariff != null)
			{
				yield return new RateLoadTariffCriteriaSet(universalTariff, additionalTax.RateSelectionCriteria);
			}
		}
	}

	List<TaxAndFee> GetCollectedAdditionalTaxesAndFees(JobComInvoiceLine invoiceLine)
	{
		var collectedTaxesAndFees = new List<TaxAndFee>();

		foreach (var additionalTax in invoiceLine.AdditionalTaxes)
		{
			collectedTaxesAndFees.Add(new TaxAndFee() { TariffType = additionalTax.BZ_TaxType, Amount = additionalTax.BZ_Value });
		}

		foreach (var additionalFee in invoiceLine.AdditionalFees)
		{
			collectedTaxesAndFees.Add(new TaxAndFee() { TariffType = additionalFee.BZ_Tariff, Amount = additionalFee.BZ_Value });
		}

		return collectedTaxesAndFees;
	}

	class TaxAndFee
	{
		public TaxAndFee() { }

		public ZString TariffType { get; set; }
		public ZDecimal Amount { get; set; }
	}
}
