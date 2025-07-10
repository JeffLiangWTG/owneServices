using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public abstract class EntryHeaderMessageProvider : MessageProvider
	{
		protected EntryHeaderMessageProvider(CusEntryHeader entryHeader)
		{
			entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
			this.entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
			invoice = entryHeaderWrapper.RandomInvoiceHeader;
		}
		internal readonly EntryHeaderWrapper entryHeaderWrapper;
		internal readonly CusEntryHeader entryHeader;
		internal readonly JobDeclaration declaration;
		internal readonly CusEntryInstruction instruction;
		internal readonly JobComInvoiceHeader invoice;
	}
}
