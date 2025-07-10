using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.Business;

public class BEOutgoingMessageProcessor : OutgoingMessageProcessor
{
	public BEOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new BEInterchangeProvider(readyMessages);

	protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
	ZQuery messageFilter;

	static ZQuery GetMessageFilterQuery()
	{
		var result = new ZQuery();
		result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BECustoms);
		result.AddToFilter(EDIMessageSchema.EM_MessageType, BEMessageProcessorHelper.GetOutgoingMessageTypes());
		return result;
	}
}
