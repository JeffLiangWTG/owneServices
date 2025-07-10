using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public class XTradeOutgoingMessageProcessor : OutgoingMessageProcessor
{
	public XTradeOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override ZQuery MessageFilter => messageFilter ?? (messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.ITCustomsXTrade));
	ZQuery messageFilter;

	protected sealed override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new XTradeInterchangeProvider(readyMessages);
}
