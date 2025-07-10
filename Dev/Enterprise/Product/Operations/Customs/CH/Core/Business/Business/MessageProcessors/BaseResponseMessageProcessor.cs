using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseResponseMessageProcessor<T> : BaseInboundMessageProcessor<T> where T : IMessageDetail
{
	public BaseResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override T DeserializeResponse(CHEDIMessage message) => (T)message?.MessageDetail;

	protected override bool RequiresPreProcessingCore => true;

	protected override BusinessObject FindLinkedObject(EDIMessage message, T xmlObject) => FindLinkedObjectByReference(message, xmlObject);
}
