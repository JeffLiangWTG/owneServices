using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputMessageListRejectionMessageProcessor : BaseMessageListRejectionMessageProcessor
{
	public CharteraOutputMessageListRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Message List Rejection Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsCharteraOutput;
}
