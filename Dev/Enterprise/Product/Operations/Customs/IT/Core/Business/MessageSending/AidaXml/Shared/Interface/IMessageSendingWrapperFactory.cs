using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public static class CustomsMessageSending
{
	public static class IT
	{
		public interface IMessageSendingWrapperFactory
		{
			IJobDeclarationCustomsMessageWrapper GetNewJobDeclarationCustomsMessageWrapper(JobDeclaration declaration);

			ICusEntryHeaderCustomsMessageWrapper GetNewCusEntryHeaderCustomsMessageWrapper(CusEntryHeader entryHeader);

			ICusEntryInstructionCustomsMessageWrapper GetNewCusEntryInstructionCustomsMessageWrapper(CusEntryInstruction entryInstruction);

			ICusEntryLineCustomsMessageWrapper GetNewCusEntryLineCustomsMessageWrapper(CusEntryLine entryLine);
		}

		public interface IExportMessageSendingWrapperFactory
		{
			IJobDeclarationCustomsMessageWrapper GetNewJobDeclarationCustomsMessageWrapper(JobDeclaration declaration);

			ICusEntryHeaderCustomsMessageWrapper GetNewCusEntryHeaderCustomsMessageWrapper(CusEntryHeader entryHeader);

			IExportCusEntryInstructionCustomsMessageWrapper GetNewCusEntryInstructionCustomsMessageWrapper(CusEntryInstruction entryInstruction);

			ICusEntryLineCustomsMessageWrapper GetNewCusEntryLineCustomsMessageWrapper(CusEntryLine entryLine);
		}
	}
}
