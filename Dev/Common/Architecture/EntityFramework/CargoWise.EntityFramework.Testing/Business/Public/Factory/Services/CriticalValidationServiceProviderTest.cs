namespace CargoWise.EntityFramework.Testing
{
	sealed class CriticalValidationServiceProviderTest : ServiceProviderTest
	{
		protected override ServiceProviderBase ServiceContainer
		{
			get { return serviceContainer; }
		}

		readonly CriticalValidationServiceProvider serviceContainer = new CriticalValidationServiceProvider();
	}
}
