using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class PassarMessageListRejectionMessageProcessor : BaseMessageListRejectionMessageProcessor
{
	public PassarMessageListRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Passar Message List Rejection Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;
}
