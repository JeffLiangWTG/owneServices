using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public abstract class ITOutgoingMessageProcessor : OutgoingMessageProcessor
{
	protected ITOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
	ZQuery messageFilter;

	ZQuery GetMessageFilterQuery()
	{
		var result = new ZQuery();
		result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.ITCustoms);
		result.AddToFilter(EDIMessageSchema.EM_MessageType, OutgoingMessageType);
		return result;
	}

	protected abstract string OutgoingMessageType { get; }
}
