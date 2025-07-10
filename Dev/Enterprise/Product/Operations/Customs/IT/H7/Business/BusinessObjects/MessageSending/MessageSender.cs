using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class MessageSender : EU.H7.Business.MessageSender
{
	public MessageSender(IH7MessageSendingObject sendingObject)
		: base(sendingObject)
	{
		this.bill = sendingObject.Bill as AsycudaBill;
	}

	readonly AsycudaBill bill;

	protected override IXmlMessageBuilder CreateMessageBuilder(EDIMessage message)
	{
		var result = default(IXmlMessageBuilder);
		var messageType = SendingObject.Action;

		switch (messageType)
		{
			case ITH7MessageTypes.Codes.H7D:
				result = new H7MessageBuilder(new H7MessageWrapper(bill));
				break;
			case ITH7MessageTypes.Codes.H7Q:
			case ITH7MessageTypes.Codes.H7C:
			case ITH7MessageTypes.Codes.H7M:
			default:
				throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
		}

		return result;
	}

	protected override void PopulateMessageDetails(BusinessObjectFactory factory, IXmlMessageBuilder messageBuilder, EDIMessage message)
	{
		try
		{
			var xmlMessage = messageBuilder.GenerateXmlMessage();
			var lrn = bill.GetAndSetLRNIfNeeded();
			var messageText = ReplaceLrnPlaceholderWithValue(xmlMessage.GetSerializedString(), lrn);

			message.EM_MessageText = SendingObject.MessageCreated(messageText);
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ITCustomsXTrade;
			message.EM_MessageType = SendingObject.Action;
			message.EM_MessageSubType = MessageSubTypeH7;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkedObject = SendingObject.Bill;
			message.EM_ApplicationReference = SharedJobMessageTypeList.Codes.Import;
			message.MessageNumberStrategy = new FixedMessageNumberStrategy(lrn);
		}
		catch (Exception e)
		{
			message.Delete();

			throw new InvalidOperationException("Failed to generate XML message", e);
		}
	}

	string ReplaceLrnPlaceholderWithValue(string serializedXml, string localRefNumber)
	{
		return serializedXml.Replace(ITEDIMessage.ITMessageNumberPlaceholder, localRefNumber);
	}

	protected override EDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<ITEDIMessage>();

	const string MessageSubTypeH7 = "H7";
}
