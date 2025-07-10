using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class SingleWindowIncomingMessageProcessor<TCustomsResponse> : IncomingCustomsMessageProcessor<ISingleWindowCustomsLinkedObjectAdapter>
{
	protected SingleWindowIncomingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ITCustoms;

	protected sealed override bool ShouldCloneMessage => false;

	protected sealed override void ProcessIncomingCustomsMessage(IncomingCustomsMessageProcessData processData)
	{
		var entryAdapter = GetOneAndOnlyEntryAdapterFromCollection(processData.EntryAdapters);
		var customsResponse = LoadCustomsResponse(processData.ReceivedMessage.EM_MessageText);
		ProcessSingleWindowMessage(entryAdapter, customsResponse);
	}

	protected abstract void ProcessSingleWindowMessage(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, TCustomsResponse customsResponse);

	protected void AddStatusUpdatedLog(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, ZString type)
	{
		const string customsControlChannel = "CCC";

		var eventParameters = new KeyValuePair<string, string>[]
		{
			new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, customsControlChannel),
			new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, type),
		};
		entryAdapter.AddLog(Events.StatusUpdated, ZDateTime.Now, eventParameters);
	}

	protected abstract TCustomsResponse LoadCustomsResponseCore(ZString messageText);

	protected override ISingleWindowCustomsLinkedObjectAdapter GetAdapter(ICustomsLinkedObjectAdapterProvider customsLinkedObjectAdapterProvider) => customsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter();

	#region Implementation

	TCustomsResponse LoadCustomsResponse(ZString messageText)
	{
		try
		{
			return LoadCustomsResponseCore(messageText);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			throw new UnableToInterpretInterchangeException(ex);
		}
	}

	#endregion
}
