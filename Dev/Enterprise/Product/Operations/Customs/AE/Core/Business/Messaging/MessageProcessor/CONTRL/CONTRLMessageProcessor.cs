using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business;

sealed class CONTRLMessageProcessor : BaseMessageProcessor<ICONTRLDataProvider>
{
	protected override EDIMessage GetOutgoingMessage(EDIMessage message, ICONTRLDataProvider dataProvider) => message.Factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

	protected override void ProcessPreProcessOKMessageCore(EDIMessage message, ICONTRLDataProvider dataProvider, LoggingInformation logger)
	{
		if (message.EM_LinkedObject is IMessageAttachee attachee)
		{
			(attachee.EntryStatus, attachee.MessageStatus) = GetStatuses(dataProvider.InterchangeResponse.ActionCode);
		}

		message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
	}

	protected override IMessageInterpreter<ICONTRLDataProvider> GetMessageInterpreter(EDIMessage message) => new CONTRLMessageInterpreter(message);

	static (ZString EntryStatus, ZString MessageStatus) GetStatuses(string actionCoded) => actionCoded switch
	{
		AEConstants.Messaging.ActionCodedList.ActionCoded4 => (AEConstants.Messaging.StatusCodes.ERR, AEConstants.Messaging.MessageTypes.CONTRL),
		AEConstants.Messaging.ActionCodedList.ActionCoded7 => (AEConstants.Messaging.StatusCodes.ERR, AEConstants.Messaging.MessageTypes.CONTRL),
		AEConstants.Messaging.ActionCodedList.ActionCoded8 => (AEConstants.Messaging.StatusCodes.Acknowledged, AEConstants.Messaging.MessageTypes.CONTRL),
		_ => (AEConstants.Messaging.StatusCodes.Unknown, AEConstants.Messaging.MessageTypes.CONTRL)
	};
}
