namespace Enterprise.Customs.CN.Business.Testing;

public class CNEntryHeaderTestData
{
	public CNEntryHeaderTestData(JobDeclaration declaration, CusEntryHeader cusEntryHeader, CusEntryLine entryLine, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusEntryInstruction instruction)
	{
		JobDeclaration = declaration;
		EntryInstruction = instruction;
		InvoiceHeader = invoiceHeader;
		InvoiceLine = invoiceLine;
		EntryHeader = cusEntryHeader;
		EntryLine = entryLine;
	}

	public JobDeclaration JobDeclaration { get; }
	public CusEntryInstruction EntryInstruction { get; }
	public JobComInvoiceHeader InvoiceHeader { get; }
	public JobComInvoiceLine InvoiceLine { get; }
	public CusEntryHeader EntryHeader { get; }
	public CusEntryLine EntryLine { get; }
}
