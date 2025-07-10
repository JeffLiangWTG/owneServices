using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

class CHPOutboundMessageProcessor : OutgoingMessageProcessor
{
	public CHPOutboundMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.CHCustomsPassar));
	ZQuery messageFilter;

	protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new CHPInterchangeProvider(readyMessages);
}
