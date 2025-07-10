namespace Enterprise.ServiceManager.Host
{
	class ApplicationExitProxy : IApplicationExitProxy
	{
		public void Exit(int exitCode)
		{
			System.Environment.Exit(exitCode);
		}
	}
}