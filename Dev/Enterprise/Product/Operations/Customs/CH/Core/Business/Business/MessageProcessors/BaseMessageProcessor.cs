using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseMessageProcessor : ApplicationTypeMessageProcessor
{
	protected BaseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	public bool CanProcess(EDIMessage message)
	{
		return message.EM_ApplicationCode == ApplicationCode && MessageTypesToInclude.Contains(message.EM_MessageType) && (MessageSubTypesToIncludeCore.IsNullOrEmpty() || MessageSubTypesToIncludeCore.Contains(message.EM_MessageSubType.ToString()));
	}

	protected sealed override void ProcessMessageCore(EDIMessage message)
	{
		if (message is CHEDIMessage chMessage)
		{
			ProcessMessageCore(chMessage);

			if (message.EM_LinkedObject != null)
			{
				if (message.EM_Status.ToString() is EDIMessage.Status.Queued or EDIMessage.Status.PreProcessedOK)
				{
					message.EM_Status = EDIMessage.Status.ProcessedOK;
				}
			}
			else
			{
				if (message.EM_Status != EDIMessage.Status.Warning)
				{
					MarkMessageAsDiscardedAndLogWarning(message, $"Unable to link EDI Message '{message.EM_MessageNum}' to an existing business object.");
				}
			}
		}
	}
	protected abstract void ProcessMessageCore(CHEDIMessage message);

	protected void MarkMessageAsDiscardedAndLogWarning(EDIMessage message, string warningMessage)
	{
		message.EM_Status = EDIMessage.Status.Discarded;
		Logger?.LogWarning(warningMessage);
	}

	protected BusinessObject FindLinkedObjectByOutgoingSessionID(EDIMessage message)
	{
		BusinessObject result = null;
		if (message.Interchange is EDIInterchange interchange)
		{
			var sentEdiMessage = message.Factory.GetOutgoingMessageFromSessionId(interchange.EI_SessionGUID);
			result = sentEdiMessage?.EM_LinkedObject;
		}
		return result;
	}

	protected CusEntryHeader FindLinkedEntryHeaderByEntryNum(EDIMessage message, string entryNumber, bool anyVersion = false)
		=> new CusEntryHeader.Loader(message.Factory).FindByEntryNumber(entryNumber, anyVersion);

	protected void LogInformationIfValueUpdated(ZString parameterName, ZString oldValue, ZString newValue, ZString ediMessageNumber)
	{
		if (oldValue != newValue)
		{
			Logger?.Log(Res.GetString("7D4B624B-1737-4821-BA55-320D37229FEB", "{0} changed from '{1}' to '{2}' by the EDI Message '{3}'.", parameterName, oldValue, newValue, ediMessageNumber));
		}
	}
}
