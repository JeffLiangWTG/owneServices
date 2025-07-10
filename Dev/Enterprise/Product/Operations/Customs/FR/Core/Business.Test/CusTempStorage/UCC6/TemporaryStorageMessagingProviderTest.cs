namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class TemporaryStorageMessagingProviderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageMessagingProviderTest<TemporaryStorageMessagingProvider>
	{
		protected override TemporaryStorageMessagingProvider GetProvider(EU.Business.CusTempStorage.TemporaryStorageHeader header)
		{
			return header.MessagingProvider as TemporaryStorageMessagingProvider;
		}
	}
}
