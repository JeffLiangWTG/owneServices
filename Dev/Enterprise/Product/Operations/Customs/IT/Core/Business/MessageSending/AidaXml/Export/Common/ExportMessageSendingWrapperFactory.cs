using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class ExportMessageSendingWrapperFactory : CustomsMessageSending.IT.IExportMessageSendingWrapperFactory
{
	IJobDeclarationCustomsMessageWrapper CustomsMessageSending.IT.IExportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(JobDeclaration declaration)
	{
		return new JobDeclarationCustomsMessageWrapper(declaration);
	}

	ICusEntryHeaderCustomsMessageWrapper CustomsMessageSending.IT.IExportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(CusEntryHeader entryHeader)
	{
		return new ExportCusEntryHeaderCustomsMessageWrapper(entryHeader);
	}

	IExportCusEntryInstructionCustomsMessageWrapper CustomsMessageSending.IT.IExportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(CusEntryInstruction entryInstruction)
	{
		return new ExportCusEntryInstructionCustomsMessageWrapper(entryInstruction);
	}

	ICusEntryLineCustomsMessageWrapper CustomsMessageSending.IT.IExportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(CusEntryLine entryLine)
	{
		return new CusEntryLineCustomsMessageWrapper(entryLine);
	}
}
