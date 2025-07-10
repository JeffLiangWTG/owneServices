using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Organisations
{
	[TestedType(typeof(BoleroEBLForOrganisationConfigurationRegistryItem))]
	sealed class BoleroEBLForOrganisationConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<BoleroEBLForOrganisationConfiguration>
	{
		protected override StronglyTypedRegistryItem<BoleroEBLForOrganisationConfiguration, BoleroEBLForOrganisationConfiguration> GetNewRegistryItem()
		{
			return new BoleroEBLForOrganisationConfigurationRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new BoleroEBLForOrganisationConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = "962E67D4-2A75-4404-BE21-43FF50ED5159", GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = "CE031983-FD17-4CA5-867F-D8E07FBE5E17", Timeout = 30 });
		}
	}
}
