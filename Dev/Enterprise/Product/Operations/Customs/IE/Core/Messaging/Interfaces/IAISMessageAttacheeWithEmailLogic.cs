using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IAISMessageAttacheeWithEmailLogic : IAISMessageAttachee
	{
		bool ShouldSendEmailNotification(EDIMessage message);
	}
}
