using CargoWise.Customs.IT.MessageContracts.DocumentManagementService;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsElectronicFolderStatusRequestMessageCreationStrategy : DocumentManagementServiceRequestMessageCreationStrategy<NctsHeader, NctsDepartureMovementHeader>
{
	public NctsElectronicFolderStatusRequestMessageCreationStrategy(
		BusinessObjectFactory factory,
		IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> messageCreationContext
	) : base(factory, messageCreationContext)
	{
	}

	protected override ITEDIMessage AddNewEDIMessage() => CreateEDIMessage(EDIMessageTypeList.Codes.ElectronicFolderQuery, EDIMessageTypeList.Codes.ElectronicFolderQuery);

	protected override IXmlMessageBuilder CreateXmlMessageBuilder(IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> context)
	{
		var messageWrapper = new MrnMessageWrapper(context.BusinessObject);
		return new StatusRequestMessageBuilder(messageWrapper);
	}
}
