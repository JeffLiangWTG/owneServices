using CargoWise.Common;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class EntryHeaderWrapper
	{
		public EntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			Instruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
			RandomInvoiceHeader = entryHeader.RandomHeader; // No need to check for null as RandomHeader is never null
		}

		public readonly CusEntryHeader EntryHeader;
		public readonly JobDeclaration Declaration;
		public readonly CusEntryInstruction Instruction;
		public readonly JobComInvoiceHeader RandomInvoiceHeader;
	}
}
