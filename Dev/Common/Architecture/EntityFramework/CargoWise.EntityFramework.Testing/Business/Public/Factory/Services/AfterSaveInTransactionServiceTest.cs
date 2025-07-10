namespace CargoWise.EntityFramework.Testing
{
	sealed class AfterSaveInTransactionServiceTest : ServiceProviderTest
	{
		protected override ServiceProviderBase ServiceContainer
		{
			get { return serviceContainer; }
		}

		readonly AfterSaveInTransactionService serviceContainer = new AfterSaveInTransactionService();
	}
}
