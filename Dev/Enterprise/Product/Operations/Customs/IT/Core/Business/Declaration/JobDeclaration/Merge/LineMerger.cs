using System.Linq;

namespace Enterprise.Customs.IT.Business.Declaration;

public class LineMerger : EU.Business.Declaration.LineMerger
{
	public LineMerger(EU.Business.Declaration.JobDeclaration declaration)
		: base(declaration)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

	protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
	{
		base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();

		foreach (var entryHeader in Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Where(x => x.EntryInstruction != null))
		{
			var entryInstruction = entryHeader.EntryInstruction;
			entryHeader.CH_IncoTerm = entryInstruction.Incoterm;
			entryHeader.InvoiceAmountCurrency = entryInstruction.Currency;
		}
	}

	protected override Customs.Business.ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
	{
		return new LineNumberAssigner(entryHeader);
	}

	protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
	{
		base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

		foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
		{
			new CustomsValuation(entryHeader).Calculate();
		}
	}

	protected override void OnMerging()
	{
		base.OnMerging();

		Declaration.ResetApportionedPreviousDocuments();
	}
}
