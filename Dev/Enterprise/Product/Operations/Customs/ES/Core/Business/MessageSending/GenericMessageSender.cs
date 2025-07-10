using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public abstract class GenericMessageSender<TMessageBuilderData, TMessageSendingObject>
		where TMessageBuilderData : class
		where TMessageSendingObject : BaseMessageSendingObject
	{
		public abstract List<TMessageBuilderData> GetMessageBuildersData();
		protected abstract List<TMessageBuilderData> GetIndividualMessageBuilder(TMessageSendingObject objectToSend, ICertificateProvider certificateData);
	}
}
