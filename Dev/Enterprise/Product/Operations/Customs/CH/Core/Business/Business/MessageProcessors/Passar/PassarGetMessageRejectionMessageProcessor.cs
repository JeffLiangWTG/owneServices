using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class PassarGetMessageRejectionMessageProcessor : BaseGetMessageRejectionMessageProcessor
{
	public PassarGetMessageRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Passar Get Message Rejection Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;
}
