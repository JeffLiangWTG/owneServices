using Enterprise.Integration;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.AirlineMessagingCargoIMPVersion
{
	[TestedType(typeof(CargoImpVersionRegistryItem))]
	sealed class CargoImpVersionRegistryItemTest : StronglyTypedRegistryItemTestCase<CargoImpVersionConfiguration>
	{
		protected override StronglyTypedRegistryItem<CargoImpVersionConfiguration, CargoImpVersionConfiguration> GetNewRegistryItem()
		{
			return new CargoImpVersionRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
