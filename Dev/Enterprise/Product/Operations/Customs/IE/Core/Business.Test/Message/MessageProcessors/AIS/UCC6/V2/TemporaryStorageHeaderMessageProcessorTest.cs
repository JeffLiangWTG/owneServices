using CargoWise.Types;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	abstract class TemporaryStorageHeaderMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> : MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, TemporaryStorageHeader, TemporaryStorageHeader>
	where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
	where TInboundEDIMessage : InboundEDIMessage
	where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (TemporaryStorageHeader declaration, TemporaryStorageHeader messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_JobReference = "MAN0001000";
			temporaryStorageHeader.AMA_GB = Branch.PK;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			temporaryStorageHeader.Messages.Add(outgoingMessage);
			return (temporaryStorageHeader, temporaryStorageHeader, outgoingMessage, incomingMessage);
		}
	}
}
