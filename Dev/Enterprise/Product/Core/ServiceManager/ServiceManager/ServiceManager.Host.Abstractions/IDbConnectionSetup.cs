namespace ServiceManager.Host.Abstractions
{
	public interface IDbConnectionSetup
	{
		void TryConnectAndHandleErrors();
	}
}
