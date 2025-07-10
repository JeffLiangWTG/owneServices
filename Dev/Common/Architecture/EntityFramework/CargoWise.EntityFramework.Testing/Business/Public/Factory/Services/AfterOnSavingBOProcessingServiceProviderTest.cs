namespace CargoWise.EntityFramework.Testing
{
	sealed class AfterOnSavingBOProcessingServiceProviderTest : ServiceProviderTest
	{
		protected override ServiceProviderBase ServiceContainer
		{
			get { return serviceContainer; }
		}

		readonly AfterOnSavingBOProcessingServiceProvider serviceContainer = new AfterOnSavingBOProcessingServiceProvider();
	}
}
