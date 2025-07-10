using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business;

sealed class CUSRESMessageProcessor : BaseMessageProcessor<ICUSRESDataProvider>
{
	protected override EDIMessage GetOutgoingMessage(EDIMessage message, ICUSRESDataProvider dataProvider)
	{
		var messageAttacheeProvider = (IMessageAttacheeProvider<ICUSRESDataProvider>)ManifestController.New().MessageAttacheeProvider;
		var messageAttachee = messageAttacheeProvider.GetAttachee(message, dataProvider);
		return messageAttachee?.Messages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
	}

	protected override MultilingualString GetDiscardReasonWhenOutgoingMessageNotExist() => (NoResString)string.Empty;

	protected override SerializationKeysResult GetKeysResultWhenJobNumberIsEmpty() => SerializationKeysResult.UnconstrainedParallelProcessing;

	protected override void ProcessPreProcessOKMessageCore(EDIMessage message, ICUSRESDataProvider dataProvider, LoggingInformation logger)
	{
		if (message.EM_LinkedObject is IMessageAttachee attachee)
		{
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			attachee.EntryStatus = dataProvider.EntryStatus;
			attachee.MessageStatus = AEConstants.Messaging.MessageTypes.CUSRES;
		}
		else
		{
			message.EM_Status = EDIMessageStatusList.Codes.Discarded;
			var discardReason = !dataProvider.IsParsed
				? ResString.GetMultilingualString("506d4b39-c7a1-4e9b-9bd7-f939873cf209", "Cannot parse CUSRES message.")
				: ResString.GetMultilingualString("3942770a-2a29-494f-bfd9-b2da3ec0c013", "Cannot find corresponding linked object.");
			message.Notes.AddNew(true, Res.GetString("c7289f13-dd29-451e-965b-4a8564b04fbe", "Discard Reason"), discardReason);
			logger.LogWarning(Res.GetString("d2cd7704-ec96-4256-b86f-3056d28cb0e5", "Status set to Discarded due to the following reason: {0}", discardReason));
		}

		var cONTRLMessageProvider = new CONTRLMessageProvider(message, dataProvider.IsParsed);
		_ = new CONTRLMessageBuilder(cONTRLMessageProvider).PopulateMessages();
	}

	protected override IMessageInterpreter<ICUSRESDataProvider> GetMessageInterpreter(EDIMessage message) => new CUSRESMessageInterpreter(message);
}
