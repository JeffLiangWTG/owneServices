using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class MessageSendingWrapperFactory : IMessageSendingWrapperFactory
{
	ICusEntryHeaderCustomsMessageWrapper IMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(CusEntryHeader entryHeader)
	{
		return new CusEntryHeaderCustomsMessageWrapper(entryHeader);
	}

	ICusEntryInstructionCustomsMessageWrapper IMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(CusEntryInstruction entryInstruction)
	{
		return new CusEntryInstructionCustomsMessageWrapper(entryInstruction);
	}

	ICusEntryLineCustomsMessageWrapper IMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(CusEntryLine entryLine)
	{
		return new CusEntryLineCustomsMessageWrapper(entryLine);
	}

	IJobDeclarationCustomsMessageWrapper IMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(JobDeclaration declaration)
	{
		return new JobDeclarationCustomsMessageWrapper(declaration);
	}
}
