namespace Enterprise.DocumentScanning.Business
{
	public interface IExternalPersisterProvider
	{
		IExternalPersister GetExternalPersister(string externalPersisterType);
	}
}
