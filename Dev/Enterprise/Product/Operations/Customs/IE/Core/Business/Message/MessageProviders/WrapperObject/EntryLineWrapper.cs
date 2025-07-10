using CargoWise.Common;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class EntryLineWrapper
	{
		public EntryLineWrapper(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			EntryLine = entryLine;
			RandomInvoiceLine = Argument.NotNull(entryLine.RandomLine, nameof(entryLine.RandomLine));
			RandomInvoiceHeader = Argument.NotNull(RandomInvoiceLine.InvoiceHeader, nameof(RandomInvoiceLine.InvoiceHeader));
			this.entryHeaderWrapper = entryHeaderWrapper;
		}

		internal readonly EntryHeaderWrapper entryHeaderWrapper;
		public CusEntryHeader EntryHeader => entryHeaderWrapper.EntryHeader;
		public JobDeclaration Declaration => entryHeaderWrapper.Declaration;
		public CusEntryInstruction Instruction => entryHeaderWrapper.Instruction;
		public readonly CusEntryLine EntryLine;
		public readonly JobComInvoiceLine RandomInvoiceLine;
		public readonly JobComInvoiceHeader RandomInvoiceHeader;
	}
}
