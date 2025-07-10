using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsFallbackConfigurationRegistryItem))]
sealed class NctsFallbackConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<NctsFallbackConfiguration>
{
	protected override StronglyTypedRegistryItem<NctsFallbackConfiguration, NctsFallbackConfiguration> GetNewRegistryItem() => new NctsFallbackConfigurationRegistryItem("", null,
		null, null, RegistryStorageFlags.All, RegistryOptions.IsOnlyForDevelopers, new NctsFallbackConfiguration { Start = ZDateTime.Now, CustomsIncidentNumber = "1234567890" });
}
