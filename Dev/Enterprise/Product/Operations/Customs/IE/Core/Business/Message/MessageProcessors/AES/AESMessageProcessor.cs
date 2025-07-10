using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.Business.AES
{
	public abstract class AESMessageProcessor<TEDIMessage, TDataProvider> : MessageAttacheeMessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : AESInboundEDIMessage
	{
		public AESMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsExport;

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportAcknowledgements;

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override EDIMessage FindOriginalOutgoingMessageCore(BusinessObjectFactory factory, EDIMessage incomingMessage)
		{
			var outgoingMessage = base.FindOriginalOutgoingMessageCore(factory, incomingMessage);

			if (incomingMessage.EM_MessageType.EqualsIgnoringCase(AESIncomingMessageTypeList.Codes.IE509) || incomingMessage.EM_MessageType.EqualsIgnoringCase(AESIncomingMessageTypeList.Codes.IE529))
			{
				var linkedObject = outgoingMessage?.EM_LinkedObject;
				var systemCreateTimeUtc = incomingMessage.EM_SystemCreateTimeUtc;

				if (linkedObject is CusExitReport exitReport
					&& exitReport.Messages.Where(x => x.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.IECustomsExport)
						&& x.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Receive)
						&& x.EM_Status.EqualsIgnoringCase(EDIMessage.Status.ProcessedOK)
						&& x.EM_SystemCreateTimeUtc <= systemCreateTimeUtc)
							.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault() is InboundEDIMessage lastProcessedReceivedMessage
					&& lastProcessedReceivedMessage.EM_MessageType == AESIncomingMessageTypeList.Codes.IE556
					&& exitReport.Consignment is CusExitConsignment consignment
					&& exitReport.Header is CusExitHeader header
					&& (header.Declaration ?? header.Shipment?.JobDeclaration) is JobDeclaration declaration
					&& FindOriginalOutgoingMessage(factory, lastProcessedReceivedMessage) is EDIMessage lastOutgoingMessage)
				{
					systemCreateTimeUtc = lastOutgoingMessage.EM_SystemCreateTimeUtc;
					var movementReference = consignment.CXC_MovementReference;
					if (declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.MovementReferenceNumber.EqualsIgnoringCase(movementReference)) is CusEntryHeader entry
						&& entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.IECustomsExport)
						&& x.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Receive)
						&& x.EM_Status.EqualsIgnoringCase(EDIMessage.Status.ProcessedOK)
						&& x.EM_MessageType.EqualsIgnoringCase(AESIncomingMessageTypeList.Codes.IE528)
						&& x.EM_SystemCreateTimeUtc <= systemCreateTimeUtc) is EDIMessage incoming528Message)
					{
						outgoingMessage = FindOriginalOutgoingMessage(factory, incoming528Message) ?? outgoingMessage;
					}
				}
			}

			return outgoingMessage;
		}
	}
}
