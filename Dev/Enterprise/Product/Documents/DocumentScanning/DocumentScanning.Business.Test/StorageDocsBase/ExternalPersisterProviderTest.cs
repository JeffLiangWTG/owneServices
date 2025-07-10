using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class ExternalPersisterProviderTest : TransactionedTestCase
	{
		public void TestGetExternalPersister()
		{
			var provider = new ExternalPersisterProvider();
			var externalPersister = provider.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3);
			AssertType<AWSPersister>("Should be AWSPersister", externalPersister);

			externalPersister = provider.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.DB);
			AssertNull("Should be null for DB", externalPersister);

			externalPersister = provider.GetExternalPersister("BLA");
			AssertNull("Should be null for unknown type", externalPersister);
		}
	}
}
