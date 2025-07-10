using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business.Declaration;

public class SupportingDocumentToEntriesLinkProvider
{
	public SupportingDocumentToEntriesLinkProvider(SupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}
	readonly SupportingDocument supportingDocument;

	public IEnumerable<CusEntryHeader> EntryHeaders => EntryLines.Where(entryLine => entryLine.Header != null).Select(entryLine => entryLine.Header).Distinct();

	public IEnumerable<CusEntryLine> EntryLines
	{
		get
		{
			var entryLineHashSet = new HashSet<CusEntryLine>();
			switch (supportingDocument.Parent)
			{
				case JobDeclaration jobDeclaration:
					AppendEntryLines(jobDeclaration, entryLineHashSet);
					break;
				case JobComInvoiceHeader invoiceHeader:
					AppendEntryLines(invoiceHeader, entryLineHashSet);
					break;
				case JobComInvoiceLine invoiceLine:
					AppendEntryLine(invoiceLine, entryLineHashSet);
					break;
			}
			return entryLineHashSet;
		}
	}

	#region Implementation

	void AppendEntryLines(JobDeclaration jobDeclaration, HashSet<CusEntryLine> entryLineHashSet)
	{
		foreach (JobComInvoiceHeader invoiceHeader in jobDeclaration.Invoices)
		{
			AppendEntryLines(invoiceHeader, entryLineHashSet);
		}
	}

	void AppendEntryLines(JobComInvoiceHeader invoiceHeader, HashSet<CusEntryLine> entryLineHashSet)
	{
		foreach (JobComInvoiceLine invoiceLine in invoiceHeader.InvoiceLines)
		{
			AppendEntryLine(invoiceLine, entryLineHashSet);
		}
	}

	void AppendEntryLine(JobComInvoiceLine invoiceLine, HashSet<CusEntryLine> entryLineHashSet)
	{
		AppendIfNotNull(entryLineHashSet, invoiceLine.CusEntryLine);
	}

	void AppendIfNotNull(HashSet<CusEntryLine> entryLineHashSet, CusEntryLine entryLine)
	{
		if (entryLine != null)
		{
			entryLineHashSet.Add(entryLine);
		}
	}

	#endregion
}
