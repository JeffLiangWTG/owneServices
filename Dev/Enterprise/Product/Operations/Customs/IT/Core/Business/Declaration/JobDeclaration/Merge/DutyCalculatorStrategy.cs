using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class DutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
{
	public DutyCalculatorStrategy(JobDeclaration declaration)
		: base(declaration)
	{
	}

	JobDeclaration Declaration => (JobDeclaration)declaration;

	protected override IEnumerable<IExtraFeeCalculator> GetExtraFeeCalculatorCollectionCore(EU.Business.Declaration.CusEntryLine entryLine)
	{
		var itEntryLine = (CusEntryLine)entryLine;
		if (Declaration.NeedsPortTax)
		{
			yield return new HarbourFeeCalculator(new HarbourFeeCalculationDataProvider(itEntryLine));
		}
	}

	protected override IEnumerable<IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollectionCore(EU.Business.Declaration.CusEntryLine entryLine)
	{
		var itEntryLine = (CusEntryLine)entryLine;
		if (itEntryLine.RequiresVATExemption)
		{
			yield return new VatExemptionCalculator(itEntryLine);
		}
	}

	protected override bool ShouldCalculateDutiesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine)
	{
		var itEntryLine = entryLine as CusEntryLine;
		return itEntryLine?.Header?.EntryInstruction?.IsDutiesAndFeeCalculationAllowed() ?? true;
	}

	protected override bool ShouldCalculateSystemFeeForThisCode(EU.Business.Declaration.CusEntryLine entryLine, string rateCode)
	{
		return base.ShouldCalculateSystemFeeForThisCode(entryLine, rateCode) || (entryLine.Fees is CusEntryLineFeeCollection itEntryLineFeeCollection && itEntryLineFeeCollection.HasExcludeActionForGivenFeeType(rateCode));
	}

	protected override ZBool ShouldStash(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee) => false;
}
