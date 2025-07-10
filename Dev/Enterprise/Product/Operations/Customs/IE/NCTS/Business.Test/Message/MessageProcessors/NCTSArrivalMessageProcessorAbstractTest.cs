using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	abstract class NCTSArrivalMessageProcessorAbstractTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> :
		MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, NctsHeader, NctsHeader>
	where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
	where TInboundEDIMessage : InboundEDIMessage
	where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (NctsHeader declaration, NctsHeader messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			header.BH_JobReference = "B00001000";
			header.BH_GB = Branch.PK;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			header.Messages.Add(outgoingMessage);
			return (header, header, outgoingMessage, incomingMessage);
		}
	}
}
