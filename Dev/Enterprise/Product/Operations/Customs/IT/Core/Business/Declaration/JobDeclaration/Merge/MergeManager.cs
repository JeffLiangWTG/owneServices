using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business.Declaration;

public class MergeManager : EU.Business.Declaration.MergeManager
{
	public MergeManager(EU.Business.Declaration.JobDeclaration jobDec)
		: base(jobDec)
	{
	}

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

	protected override void OnMerged()
	{
		base.OnMerged();

		var declaration = Declaration;
		foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
		{
			var mergedLines = entryHeader.MergedLines;
			entryHeader.Validation.CheckLinesCount();
			mergedLines.ForEach(x => x.Fees.ApplyCustomsCompliantSort());

			var entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable = mergedLines.Where(x => x.IsDefaultLogicForSupportingDocumentC100Applicable);
			if (entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable.Any())
			{
				BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments?.Invoke(this, new EntryLineDefaultLogicForC100SupportingDocumentsEventArgs(entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable));
				declaration.ApplyEntryLineDefaultLogicForC100SupportingDocuments();
			}
			if (declaration.NeedsPortTax)
			{
				mergedLines.ForEach(x => x.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(invLine => invLine.AddInfoValidation.ValidateZG_PortTaxRate()));
			}
		}
	}

	public event EventHandler<EntryLineDefaultLogicForC100SupportingDocumentsEventArgs> BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments;
}

public class EntryLineDefaultLogicForC100SupportingDocumentsEventArgs : EventArgs
{
	public EntryLineDefaultLogicForC100SupportingDocumentsEventArgs(IEnumerable<CusEntryLine> entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable)
		: base()
	{
		EntryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable = Argument.NotNull(entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable, nameof(entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable));
	}

	public IEnumerable<CusEntryLine> EntryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable { get; }
}
