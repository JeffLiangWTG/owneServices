namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceHostMessageDispatcher
	{
		void SendErrorReport(string taskCode);
		void SendGrpcPortLockAcquired(int port);
	}
}
