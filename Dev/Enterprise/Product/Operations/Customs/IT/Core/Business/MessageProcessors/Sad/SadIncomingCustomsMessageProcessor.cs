using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public abstract class SadIncomingCustomsMessageProcessor<TCustomsInterchange> : IncomingCustomsMessageProcessor<ISadCustomsLinkedObjectAdapter>
	where TCustomsInterchange : CustomsInterchange, new()
{
	protected SadIncomingCustomsMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ITCustoms;

	protected sealed override string MessageFriendlyNameCore => (NoResString)"Italian Customs Message Response";
	protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeToInclude };
	protected sealed override bool ShouldCloneMessage => true;

	protected abstract void PerformTypeSpecificActionsCore(TCustomsInterchange messageInterchange, IncomingCustomsMessageProcessData processData);
	protected abstract ZString MessageTypeToInclude { get; }

	protected override ISadCustomsLinkedObjectAdapter GetAdapter(ICustomsLinkedObjectAdapterProvider customsLinkedObjectAdapterProvider) => customsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter();

	protected override void ProcessIncomingCustomsMessage(IncomingCustomsMessageProcessData processData)
	{
		var messageCustomsInterchange = LoadCustomsInterchange(processData.ReceivedMessage.EM_MessageText);

		CheckFileNamesMatch((ITEDIInterchange)processData.SentInterchange, (ITEDIInterchange)processData.ReceivedInterchange);
		PerformTypeSpecificActions(messageCustomsInterchange, processData);
	}

	void CheckFileNamesMatch(ITEDIInterchange sentInterchange, ITEDIInterchange receivedInterchange)
	{
		var sentFileName = sentInterchange.GetFileNameFromHeaderText();
		var receivedFileName = receivedInterchange.GetFileNameFromHeaderText();

		if (!CustomsInterchangeHeader.MatchCustomsMessageFileNames(sentFileName, receivedFileName))
		{
			throw new IncomingMessageDoesNotMatchWithIdocException(Res.GetString("FF0C32F0-B4A3-435D-9B54-5812B2C7A75D", "Received File Name: {0} does not match with Sent IDOC File Name: {1}", receivedFileName, sentFileName));
		}
	}

	void PerformTypeSpecificActions(TCustomsInterchange messageInterchange, IncomingCustomsMessageProcessData processData)
	{
		try
		{
			PerformTypeSpecificActionsCore(messageInterchange, processData);
		}
		catch (IncomingMessageDoesNotMatchWithStatusException incomingMessageDoesNotMatchWithStatusException)
		{
			AddInformationNoteAndLog(processData.ReceivedInterchange, incomingMessageDoesNotMatchWithStatusException.Message);
			processData.DoNotAttachAllMessagesToEntries();
		}
	}

	TCustomsInterchange LoadCustomsInterchange(ZString messageText)
	{
		var messageInterchangeResult = CustomsInterchange.LoadSafe<TCustomsInterchange>(messageText);
		if (!messageInterchangeResult.IsValid)
		{
			throw new UnableToInterpretInterchangeException(messageInterchangeResult.Exception);
		}
		return messageInterchangeResult.Interchange;
	}

	void AddInformationNoteAndLog(EDIInterchange receivedInterchange, string infoMessage)
	{
		AddNoteToInterchange(receivedInterchange, (NoResString)"CargoWiseOne info", infoMessage);
		Logger.Log(infoMessage);
	}

	void AddNoteToInterchange(EDIInterchange interchange, ZString description, ZString noteAsText)
	{
		var note = interchange.Notes.AddNew();
		note.ST_Description = description;
		note.ST_NoteDataAsText = noteAsText;
	}
}
