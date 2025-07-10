using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExchangeRateToleranceRegistryItem))]
	class ExchangeRateToleranceRegistryItemTest : StronglyTypedRegistryItemTestCase<ExchangeRateToleranceConfiguration>
	{
		protected override StronglyTypedRegistryItem<ExchangeRateToleranceConfiguration, ExchangeRateToleranceConfiguration> GetNewRegistryItem()
		{
			var defaultConfiguration = new ExchangeRateToleranceConfiguration();
			defaultConfiguration.ExchangeRateToleranceCollection.Add(Business.ExchangeRateTolerance.GetDefaultExchangeRateTolerance());
			return new ExchangeRateToleranceRegistryItem("", null, null, null, RegistryStorageFlags.System, defaultConfiguration);
		}
	}
}
