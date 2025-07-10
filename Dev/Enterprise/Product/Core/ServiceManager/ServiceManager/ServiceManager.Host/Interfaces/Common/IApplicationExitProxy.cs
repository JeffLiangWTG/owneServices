namespace Enterprise.ServiceManager.Host
{
	public interface IApplicationExitProxy
	{
		void Exit(int exitCode);
	}
}