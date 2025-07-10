using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputMessageListAcceptanceMessageProcessor : BaseMessageListAcceptanceMessageProcessor
{
	public CharteraOutputMessageListAcceptanceMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Message List Acceptance Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsCharteraOutput;
}
