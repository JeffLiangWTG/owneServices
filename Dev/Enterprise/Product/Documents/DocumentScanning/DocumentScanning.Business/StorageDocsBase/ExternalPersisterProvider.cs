namespace Enterprise.DocumentScanning.Business
{
	public class ExternalPersisterProvider : IExternalPersisterProvider
	{
		public IExternalPersister GetExternalPersister(string externalPersisterType)
		{
			IExternalPersister externalPersister;
			switch (externalPersisterType)
			{
				case Core.Constants.EDocsStorageProviders.Code.S3:
					externalPersister = new AWSPersister();
					break;
				default:
					externalPersister = null;
					break;
			}

			return externalPersister;
		}
	}
}
