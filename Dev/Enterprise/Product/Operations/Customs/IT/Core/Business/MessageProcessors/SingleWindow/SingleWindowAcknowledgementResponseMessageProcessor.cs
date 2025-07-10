using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;

using static Enterprise.Customs.IT.Business.MessageProcessorConstants.InterchangeTypes;
namespace Enterprise.Customs.IT.Business;

public class SingleWindowAcknowledgementResponseMessageProcessor : SingleWindowIncomingMessageProcessor<ZString>
{
	public SingleWindowAcknowledgementResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Single Window Acknowledgement Message";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SingleWindowPositiveAckResponseMessageType, SingleWindowNegativeAckResponseMessageType };

	protected override ZString LoadCustomsResponseCore(ZString messageText) => messageText;

	protected override void ProcessSingleWindowMessage(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, ZString customsResponse)
	{
		//Acknowledgement Messages do not require any processing. They only get linked to the relevant entry.
	}
}
