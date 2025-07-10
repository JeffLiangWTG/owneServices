using CargoWise.Common;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Business;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public abstract class InboundMessageInterpreter<TDataProvider> : BaseInboundMessageInterpreter<TDataProvider>
	{
		protected InboundMessageInterpreter(BaseEDIMessage message, TDataProvider provider) : base(message, provider)
		{
			messageAttachee = Argument.NotNull((IMessageAttachee)message.EM_LinkedObject, nameof(message.EM_LinkedObject));
			relatedJob = Argument.NotNull(messageAttachee.RelatedJob, nameof(messageAttachee.RelatedJob));
		}
		protected readonly IMessageAttachee messageAttachee;
		protected readonly IRelatedJob relatedJob;
	}
}
