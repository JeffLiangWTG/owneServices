using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class SadCustomsExitAndReleaseMessageProcessor<TCustomsInterchange, TApplicationResponse> : SadIncomingCustomsMessageProcessor<TCustomsInterchange>
	where TCustomsInterchange : CustomsInterchange, ICustomsApplicationResponseProvider<TApplicationResponse>, new()
	where TApplicationResponse : IMrnProvider
{
	protected SadCustomsExitAndReleaseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override void PerformTypeSpecificActionsCore(TCustomsInterchange messageInterchange, IncomingCustomsMessageProcessData processData)
	{
		foreach (var applicationResponse in messageInterchange.ApplicationResponses)
		{
			var entryAdapter = processData.EntryAdapters.FirstOrDefault(header => header.Mrn == applicationResponse.Mrn);
			if (!IsEntryAllowedToBeProcessed(entryAdapter, applicationResponse))
			{
				continue;
			}

			var entryNumber = entryAdapter.GetNewCusEntryNumber();
			PopulateCusEntryNum(entryNumber, applicationResponse);
			UpdateEntryStatus(entryAdapter, applicationResponse);
			PerformMessageTypeSpecificActions(entryAdapter, applicationResponse, processData.ReceivedMessage);
		}
	}

	protected abstract ZString MessageInfo { get; }
	protected abstract bool CusEntryNumAlreadyExists(ISadCustomsLinkedObjectAdapter entryAdapter);
	protected abstract void UpdateEntryStatus(ISadCustomsLinkedObjectAdapter entryAdapter, TApplicationResponse applicationResponse);
	protected abstract void PopulateCusEntryNum(CusEntryNumber entryNumber, TApplicationResponse applicationResponse);
	protected virtual void PerformMessageTypeSpecificActions(ISadCustomsLinkedObjectAdapter entryAdapter, TApplicationResponse applicationResponse, EDIMessage receivedMessage) { }

	bool IsEntryAllowedToBeProcessed(ISadCustomsLinkedObjectAdapter entryAdapter, TApplicationResponse applicationResponse)
	{
		if (entryAdapter == null)
		{
			Logger.LogWarning($"{MessageInfo} message processing: EntryHeader with MRN {applicationResponse.Mrn} couldn't be found.");
			return false;
		}
		if (entryAdapter.IsIncomingMessageAlreadyLinked(MessageTypeToInclude))
		{
			Logger.LogWarning($"{MessageInfo} message processing: EDI Message type already linked to Entry {entryAdapter.Mrn}");
			return false;
		}
		if (CusEntryNumAlreadyExists(entryAdapter))
		{
			Logger.LogWarning($"{MessageInfo} message processing: {MessageInfo} CusEntryNum already exists for Entry {entryAdapter.Mrn}");
			return false;
		}
		return true;
	}
}
