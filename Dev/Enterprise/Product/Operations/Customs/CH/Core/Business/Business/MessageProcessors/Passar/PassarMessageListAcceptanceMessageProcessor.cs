using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class PassarMessageListAcceptanceMessageProcessor : BaseMessageListAcceptanceMessageProcessor
{
	public PassarMessageListAcceptanceMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Passar Message List Acceptance Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;
}
