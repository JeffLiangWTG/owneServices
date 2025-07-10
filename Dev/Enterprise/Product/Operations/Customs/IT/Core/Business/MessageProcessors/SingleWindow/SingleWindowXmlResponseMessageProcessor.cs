using System;
using System.Collections.Generic;
using CargoWise.Customs.IT.MessageDefinitions.SingleWindow;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IT.Business.MessageProcessorConstants.InterchangeTypes;

namespace Enterprise.Customs.IT.Business;

public class SingleWindowXmlResponseMessageProcessor : SingleWindowIncomingMessageProcessor<ISingleWindowStatusCustomsResponse>
{
	public SingleWindowXmlResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Single Window XML Response Message";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SingleWindowStatusResponseMessageType };

	protected override void ProcessSingleWindowMessage(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, ISingleWindowStatusCustomsResponse customsResponse)
	{
		var controlChannel = customsResponse.ControlChannel;
		if (!controlChannel.IsEmpty)
		{
			entryAdapter.SetEntryCustomsChannel(controlChannel);
			AddStatusUpdatedLog(entryAdapter, controlChannel);
		}

		var releaseCode = customsResponse.ReleaseCode;

		if (!releaseCode.IsEmpty && !entryAdapter.IsEntryCleared)
		{
			entryAdapter.SetEntryAsCleared(customsResponse.ReleaseDate);
			entryAdapter.InsertOrUpdateReleaseCode(releaseCode, customsResponse.ReleaseDate);
		}
		GenerateDocuments(entryAdapter);
	}

	protected override ISingleWindowStatusCustomsResponse LoadCustomsResponseCore(ZString messageText)
	{
		var customsResponse = messageText.DeserializeToObject<esito_bolletta>();
		return new SingleWindowStatusCustomsResponseWrapper(customsResponse);
	}

	void GenerateDocuments(ISingleWindowCustomsLinkedObjectAdapter entryAdapter)
	{
		try
		{
			entryAdapter.GenerateDocuments();
		}
		catch (Exception ex)
		{
			throw new CustomsMessageProcessorException(Res.GetString("8DDBDDF0-F6D2-4A82-A8FB-0A279610D1BF", "Generation documents process failed"), ex);
		}
	}
}
