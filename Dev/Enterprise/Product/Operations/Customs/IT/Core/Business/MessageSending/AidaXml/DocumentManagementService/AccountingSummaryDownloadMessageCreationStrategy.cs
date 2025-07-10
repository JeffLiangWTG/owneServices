using CargoWise.Customs.IT.MessageContracts.DocumentManagementService;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class AccountingSummaryDownloadMessageCreationStrategy : DocumentManagementServiceRequestMessageCreationStrategy<CusEntryHeader>
{
	public AccountingSummaryDownloadMessageCreationStrategy(BusinessObjectFactory factory, IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> messageCreationContext)
		: base(factory, messageCreationContext)
	{
	}

	protected override ITEDIMessage AddNewEDIMessage() => CreateEDIMessage(EDIMessageTypeList.Codes.AccountingSummaryDownload, EDIMessageTypeList.Codes.AccountingSummaryDownload);

	protected override IXmlMessageBuilder CreateXmlMessageBuilder(IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> context)
	{
		var messageWrapper = new MrnAndIutMessageWrapper(context.BusinessObject, EDIMessageTypeList.Codes.AccountingSummaryRequest);
		return new DownloadAccountingSummaryMessageBuilder(messageWrapper);
	}
}
