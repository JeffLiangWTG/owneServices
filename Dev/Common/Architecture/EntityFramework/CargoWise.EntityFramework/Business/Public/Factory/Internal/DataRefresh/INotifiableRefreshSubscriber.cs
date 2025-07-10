namespace CargoWise.EntityFramework
{
	public interface INotifiableRefreshSubscriber
	{
		void NotifyRefreshByTableStarting();
		void NotifyRefreshByTableCompleted();
	}
}
