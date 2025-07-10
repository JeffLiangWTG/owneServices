using CargoWise.Customs.IT.MessageContracts.DocumentManagementService;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class Eur1RequestMessageCreationStrategy : DocumentManagementServiceRequestMessageCreationStrategy<CusEntryHeader>
{
	public Eur1RequestMessageCreationStrategy(BusinessObjectFactory factory, IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> messageCreationContext) : base(factory, messageCreationContext)
	{
	}

	protected override ITEDIMessage AddNewEDIMessage() => CreateEDIMessage(EDIMessageTypeList.Codes.Eur1Request, EDIMessageTypeList.Codes.Eur1Request);

	protected override IXmlMessageBuilder CreateXmlMessageBuilder(IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> context)
	{
		var messageWrapper = new MrnMessageWrapper(context.BusinessObject);
		return new Eur1RequestMessageBuilder(messageWrapper);
	}
}
