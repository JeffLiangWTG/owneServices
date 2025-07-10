namespace Enterprise.ServiceManager.Host
{
	internal interface IDSATaskRunner
	{
		void RunDsaTaskIfDbServerRestarts();
	}
}