using System;
using CargoWise.Customs.IT.MessageContracts.DocumentManagementService;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class EadTadRequestMessageCreationStrategy<TBusiness, TMessageParent> : DocumentManagementServiceRequestMessageCreationStrategy<TBusiness, TMessageParent>
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TMessageParent : BusinessObject
{
	public EadTadRequestMessageCreationStrategy(
		BusinessObjectFactory factory,
		IDocumentManagementServiceRequestContext<TBusiness, TMessageParent> messageCreationContext,
		string messageType
		) : base(factory, messageCreationContext)
	{
		if(messageType is not EDIMessageTypeList.Codes.TadRequest && messageType is not EDIMessageTypeList.Codes.EadRequest)
		{
			throw new ArgumentException("Invalid message type", nameof(messageType));
		}
		MessageType = messageType;
	}

	protected override ITEDIMessage AddNewEDIMessage() => CreateEDIMessage(MessageType, MessageType);

	protected override IXmlMessageBuilder CreateXmlMessageBuilder(IDocumentManagementServiceRequestContext<TBusiness, TMessageParent> context)
	{
		var messageWrapper = new MrnMessageWrapper(context.BusinessObject);
		return new DaeDatRequestMessageBuilder(messageWrapper);
	}

	ZString MessageType { get; }
}
