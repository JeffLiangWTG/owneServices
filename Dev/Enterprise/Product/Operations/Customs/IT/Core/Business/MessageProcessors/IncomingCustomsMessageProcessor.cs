using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.IT.Business;

public abstract class IncomingCustomsMessageProcessor<TCustomsLinkedObjectAdapter> : ApplicationTypeMessageProcessor
	where TCustomsLinkedObjectAdapter : ICustomsLinkedObjectAdapter
{
	protected IncomingCustomsMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override void ProcessMessageCore(EDIMessage message)
	{
		var receivedInterchange = message.Interchange;
		if (receivedInterchange != null)
		{
			try
			{
				var sentInterchange = GetRelatedSentInterchange(receivedInterchange);
				var linkedObjects = GetInterchangeLinkedObjects(sentInterchange);

				var processData = new IncomingCustomsMessageProcessData()
				{
					ReceivedMessage = message,
					ReceivedInterchange = receivedInterchange,
					SentInterchange = sentInterchange,
					EntryAdapters = GetBusinessObjectAdapters(linkedObjects),
				};
				ProcessIncomingCustomsMessage(processData);

				AttachMessageToRelatedEntriesIfNeeded(processData);
				SetMessageAndIntechangeStatus(message, EDIMessage.Status.Received, receivedInterchange, EDIInterchange.Status.Received);
			}
			catch (CustomsMessageProcessorException customsMessageProcessorEx)
			{
				MarkMessageAndInterchangeAsFailed(receivedInterchange, message, customsMessageProcessorEx.Message);
				LogError(customsMessageProcessorEx.Message);
			}
		}
		else
		{
			SetMessageStatus(message, EDIMessage.Status.Failed);
			LogError(Res.GetString("9C46BCD9-0597-4EE8-AA6F-77275C167F36", "Incoming Message is not linked to any Interchange"));
		}
	}

	protected abstract bool ShouldCloneMessage { get; }

	protected abstract void ProcessIncomingCustomsMessage(IncomingCustomsMessageProcessData processData);

	/// <summary>
	/// Ensures entry adapter collection has one and only one element.
	/// </summary>
	/// <returns>Single entry adapter from collection</returns>
	protected TCustomsLinkedObjectAdapter GetOneAndOnlyEntryAdapterFromCollection(IEnumerable<TCustomsLinkedObjectAdapter> entryAdapters)
	{
		if (entryAdapters.Count() == 1)
		{
			return entryAdapters.Single();
		}
		throw new MoreThanOneEntryHeaderFoundException();
	}

	#region Implementation

	ITEDIInterchange GetRelatedSentInterchange(EDIInterchange receivedInterchange)
	{
		var trackingId = GetReceivedTrackingId(receivedInterchange);
		var sentInterchange = trackingId.IsEmpty ? null : MessageProcessorHelper.GetSentInterchangeWithTrackingId(trackingId, receivedInterchange.Factory);
		return sentInterchange
			?? throw new CouldNotFindRelatedBusinessObjectException(MessageProcessorHelper.UnableToLocateTheRelatedSentInterchangeWithSessionGuid(trackingId));
	}

	protected virtual ZGuid GetReceivedTrackingId(EDIInterchange receivedInterchange)
	{
		var trackingId = MessageProcessorHelper.RetrieveValueOfXmlNode(receivedInterchange.EI_HeaderText, "eHubTrackingIDFromSentInterchange");
		return trackingId.IsEmpty ? ZGuid.Empty : new ZGuid(trackingId);
	}

	void MarkMessageAndInterchangeAsFailed(EDIInterchange interchange, EDIMessage message, ZString errorMessage)
	{
		SetMessageAndIntechangeStatus(message, EDIMessage.Status.Failed, interchange, EDIInterchange.Status.Failed);
		AddNoteToInterchange(interchange, (NoResString)"CargoWiseOne error", errorMessage);
	}

	void AddNoteToInterchange(EDIInterchange interchange, ZString description, ZString noteAsText)
	{
		var note = interchange.Notes.AddNew();
		note.ST_Description = description;
		note.ST_NoteDataAsText = noteAsText;
	}

	void SetMessageAndIntechangeStatus(EDIMessage message, ZString messageStatus, EDIInterchange interchange, ZString interchangeStatus)
	{
		SetMessageStatus(message, messageStatus);
		interchange.EI_Status = interchangeStatus;
	}

	void SetMessageStatus(EDIMessage message, ZString messageStatus) => message.EM_Status = messageStatus;
	void LogError(string errorMessage) => Logger.Log(Integration.LogType.Error, errorMessage);

	IEnumerable<ICustomsLinkedObjectAdapterProvider> GetInterchangeLinkedObjects(ITEDIInterchange sentInterchange)
	{
		foreach (var linkedObject in sentInterchange.ContainedMessages.Cast<EDIMessage>().Where(x => x.EM_LinkedObject != null).Select(x => x.EM_LinkedObject))
		{
			if (!(linkedObject is ICustomsLinkedObjectAdapterProvider customsLinkedObjectAdapterProvider))
			{
				throw new CouldNotFindRelatedBusinessObjectException($"'{linkedObject.GetType()}' type is not a {nameof(ICustomsLinkedObjectAdapterProvider)}");
			}
			yield return customsLinkedObjectAdapterProvider;
		}
	}

	IEnumerable<TCustomsLinkedObjectAdapter> GetBusinessObjectAdapters(IEnumerable<ICustomsLinkedObjectAdapterProvider> customsLinkedObjectAdapterProviders)
	{
		try
		{
			var adapters = new List<TCustomsLinkedObjectAdapter>();
			foreach (var linkedObject in customsLinkedObjectAdapterProviders)
			{
				adapters.Add(GetAdapter(linkedObject));
			}
			return adapters;
		}
		catch (NotSupportedException ex)
		{
			throw new CustomsMessageProcessorException(Res.GetString("D41AF01C-E0F2-4B53-8E5E-17DF816C300C", "Unsupported linked business object found."), ex);
		}
	}

	protected abstract TCustomsLinkedObjectAdapter GetAdapter(ICustomsLinkedObjectAdapterProvider customsLinkedObjectAdapterProvider);

	void AttachMessageToRelatedEntriesIfNeeded(IncomingCustomsMessageProcessData processData)
	{
		foreach (TCustomsLinkedObjectAdapter entryAdapter in processData.EntryAdapters)
		{
			if (processData.ShouldAttachMessageToEntry(entryAdapter))
			{
				var messageToAttach = ShouldCloneMessage ? CloneMessage(entryAdapter, processData) : processData.ReceivedMessage;
				entryAdapter.AddMessage(messageToAttach);
			}
		}
	}

	EDIMessage CloneMessage(TCustomsLinkedObjectAdapter entryAdapter, IncomingCustomsMessageProcessData processData)
	{
		Argument.NotNull(processData, nameof(processData));

		var messageTextToClone = processData.GetClonedMessageTextStrategy(entryAdapter)?.GetClonedMessageText();

		var messageToClone = processData.ReceivedMessage;
		var lastSuccessfullySentMessage = entryAdapter.GetLastSuccessfullySentMessage();

		var clonedMessage = messageToClone.Factory.New<ITEDIMessage>();
		clonedMessage.EM_ApplicationCode = ApplicationCodes.ITCustoms;
		clonedMessage.EM_Status = EDIMessage.Status.Received;
		clonedMessage.EM_MessageType = messageToClone.Interchange.EI_InterchangeType;
		clonedMessage.EM_MessageSubType = lastSuccessfullySentMessage.EM_MessageSubType;
		clonedMessage.EM_MessageNum = lastSuccessfullySentMessage.EM_MessageNum;
		clonedMessage.EM_MessageText = messageTextToClone ?? messageToClone.EM_MessageText;
		clonedMessage.EM_ReceiveTransmit = messageToClone.EM_ReceiveTransmit;
		clonedMessage.EM_EI = messageToClone.EM_EI;
		return clonedMessage;
	}

	protected IncomingMessageDoesNotMatchWithStatusException GetNewStatusAlreadySetException(ICustomsLinkedObjectAdapter entryAdapter)
	{
		var exceptionMessage = Res.GetString("FB216480-50C2-45D1-9083-F05D3CA8F6E4", "Entry {0} for job {1} not processed: Status already set.", entryAdapter.EntryReferenceNumber, entryAdapter.JobReferenceNumber);
		return new IncomingMessageDoesNotMatchWithStatusException(exceptionMessage);
	}

	#endregion

	protected class IncomingCustomsMessageProcessData
	{
		public EDIMessage ReceivedMessage { get; set; }
		public EDIInterchange ReceivedInterchange { get; set; }
		public EDIInterchange SentInterchange { get; set; }
		public IEnumerable<TCustomsLinkedObjectAdapter> EntryAdapters { get; set; }

		public IncomingCustomsMessageProcessData()
		{
			entriesPkDoNotAttachMessage = new List<ZGuid>();
		}
		readonly List<ZGuid> entriesPkDoNotAttachMessage;

		public void DoNotAttachAllMessagesToEntries()
		{
			entriesPkDoNotAttachMessage.Clear();
			foreach (var entry in EntryAdapters)
			{
				entriesPkDoNotAttachMessage.Add(entry.PK);
			}
		}

		public void DoNotAttachMessageToEntry(TCustomsLinkedObjectAdapter entryAdapter) => entriesPkDoNotAttachMessage.Add(entryAdapter.PK);

		public bool ShouldAttachMessageToEntry(TCustomsLinkedObjectAdapter entryAdapter) => !entriesPkDoNotAttachMessage.Contains(entryAdapter.PK);

		public void AddClonedMessageTextStrategy(TCustomsLinkedObjectAdapter entryAdapter, IGetClonedMessageTextStrategy strategy)
		{
			Argument.NotNull(entryAdapter, nameof(entryAdapter));
			Argument.NotNull(strategy, nameof(strategy));
			if (messageClonedTextStrategies.ContainsKey(entryAdapter))
			{
				throw new CustomsMessageProcessorException(Res.GetString("BDC01A5F-3A5E-418B-836D-4B4AB54A43D7", "More than one cloning message strategy has been defined for the same entry"));
			}
			messageClonedTextStrategies.Add(entryAdapter, strategy);
		}

		public IGetClonedMessageTextStrategy GetClonedMessageTextStrategy(TCustomsLinkedObjectAdapter entryAdapter)
		{
			Argument.NotNull(entryAdapter, nameof(entryAdapter));
			messageClonedTextStrategies.TryGetValue(entryAdapter, out var strategy);
			return strategy;
		}

		readonly Dictionary<TCustomsLinkedObjectAdapter, IGetClonedMessageTextStrategy> messageClonedTextStrategies = new Dictionary<TCustomsLinkedObjectAdapter, IGetClonedMessageTextStrategy>();
	}

	protected interface IGetClonedMessageTextStrategy
	{
		string GetClonedMessageText();
	}
}
