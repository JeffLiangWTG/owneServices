using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface INudgingFailedUserNotification
	{
		void Attach(INudgingController nudgingController);
	}
}
