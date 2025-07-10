using CargoWise.Customs.IT.MessageContracts.DocumentManagementService;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class SummaryProspectusRequestMessageCreationStrategy : DocumentManagementServiceRequestMessageCreationStrategy<CusEntryHeader>
{
	public SummaryProspectusRequestMessageCreationStrategy(BusinessObjectFactory factory, IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> messageCreationContext)
		: base(factory, messageCreationContext)
	{
	}

	protected override ITEDIMessage AddNewEDIMessage() => CreateEDIMessage(EDIMessageTypeList.Codes.SummaryProspectusRequest, EDIMessageTypeList.Codes.SummaryProspectusRequest);

	protected override IXmlMessageBuilder CreateXmlMessageBuilder(IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> context)
	{
		var messageWrapper = new MrnMessageWrapper(context.BusinessObject);
		return new ProspectusRequestMessageBuilder(messageWrapper);
	}
}
