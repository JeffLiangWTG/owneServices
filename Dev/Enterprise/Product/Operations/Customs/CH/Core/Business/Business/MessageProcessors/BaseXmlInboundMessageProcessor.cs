using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseXmlInboundMessageProcessor<T> : BaseInboundMessageProcessor<T>
{
	protected BaseXmlInboundMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override T DeserializeResponse(CHEDIMessage message) => CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<T>(message.EM_MessageText);
}
