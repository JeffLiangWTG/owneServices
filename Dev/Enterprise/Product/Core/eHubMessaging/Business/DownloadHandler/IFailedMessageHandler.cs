using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public interface IFailedMessageHandler
	{
		void UpdateFailedMessage(EDIInterchange interchange);
	}
}