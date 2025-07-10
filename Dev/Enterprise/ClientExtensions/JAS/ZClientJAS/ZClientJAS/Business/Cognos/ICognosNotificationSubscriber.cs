using CargoWise.ComponentModel;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public interface ICognosNotificationSubscriber : INotifications
	{
		void AdvanceProgressBy(int percentage);
		void CompleteProgress();
		bool HasErrors { get; }
	}
}
