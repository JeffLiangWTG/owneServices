using System;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.BE.Business;

public class BECInboundInterchangeProcessor : InboundInterchangeProcessor
{
	public override bool IsInterchangeNotDeleted(EDIInterchange interchange)
	{
		return interchange.EI_Status != EDIInterchange.Status.Error && base.IsInterchangeNotDeleted(interchange);
	}

	protected override string[] ApplicationCodes => new[] { EDIInterchange.ApplicationCodes.BECustoms };

	protected override Type TypeOfInterchangeToCreate() => typeof(BECInterchange);

	protected override bool IsNoBranchFilter => true;

	protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
	{
		return messageCreator ?? (messageCreator = new InboundMessageCreator());
	}
	IInboundMessageCreator messageCreator;

	class InboundMessageCreator : IInboundMessageCreator
	{
		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			if (interchange is BECInterchange beInterchange)
			{
				var messageCreated = CreateMessagesFromInterchangeXml(beInterchange);
				if (!messageCreated)
				{
					interchange.Logs.AddNew(Events.ErrorReport, $"No message has been created for interchange {interchange.EI_InterchangeNum}");
					interchange.EI_Status = EDIInterchange.Status.Error;
				}
			}
		}

		static bool CreateMessagesFromInterchangeXml(BECInterchange interchange)
		{
			var result = true;
			var messageText = interchange.EI_BodyText;
			var (validMessageInfo, xmlDocument) = MessageHelper.ValidateMessageXML(messageText);

			if (!string.IsNullOrEmpty(validMessageInfo))
			{
				interchange.Logs.AddNew(Events.ErrorReport, $"{validMessageInfo}, the Message creation failed");
				result = false;
			}
			else
			{
				var interchangeNum = interchange.EI_InterchangeNum;
				var subMessageType = MessageHelper.GetMessageTypeByXml(xmlDocument);

				if (!subMessageType.IsEmpty)
				{
					var newEDIMessage = interchange.ContainedMessages.AddNew(typeof(BEMessage));

					newEDIMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
					newEDIMessage.EM_MessageText = messageText;
					newEDIMessage.EM_MessageType = interchange.EI_InterchangeType;
					newEDIMessage.EM_MessageSubType = subMessageType;
					newEDIMessage.EM_MessageNum = interchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);

					if (subMessageType == Constants.BECMessageTypes.CustomsServiceErrorUniversalEvent)
					{
						newEDIMessage.EM_MessageText = MessageHelper.GetXmlBody(newEDIMessage.GetEM_MessageTextReader());

						var linkedObject = MessageHelper.LocateHeaderByEdiInterchange(interchange);

						if (linkedObject != null)
						{
							newEDIMessage.EM_LinkedObject = linkedObject;
							if (linkedObject is CusEntryHeader)
							{
								newEDIMessage.EM_MessageType = SendMessageTypes.Codes.AES;
							}

							if (linkedObject is CusInBondHeader)
							{
								newEDIMessage.EM_MessageType = SendMessageTypes.Codes.NCT;
							}
						}
					}
				}
				else
				{
					interchange.Logs.AddNew(Events.ErrorReport, $"Interchange {interchangeNum}: Can not determine Message Sub Type for the Received Interchange");
					result = false;
				}
			}
			return result;
		}
	}
}
