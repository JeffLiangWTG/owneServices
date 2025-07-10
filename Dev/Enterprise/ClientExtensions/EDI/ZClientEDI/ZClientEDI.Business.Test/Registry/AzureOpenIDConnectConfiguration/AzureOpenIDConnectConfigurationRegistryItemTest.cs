using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AzureOpenIDConnectConfigurationRegistryItem))]
	public class AzureOpenIDConnectConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AzureOpenIDConnectConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<AzureOpenIDConnectConfigurationCollection, AzureOpenIDConnectConfigurationCollection> GetNewRegistryItem()
		{
			return new AzureOpenIDConnectConfigurationRegistryItem(
				"AzureApplicationManagement",
				null,
				null,
				null,
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport
				);
		}

		protected override AzureOpenIDConnectConfigurationCollection ValidValue
		{
			get
			{
				var collection = new AzureOpenIDConnectConfigurationCollection
				{
					new AzureOpenIDConnectConfiguration
					{
						Code = "PRD",
						AuthorityUrl = "https://www.example.com",
						ClientID = "Test"
					}
				};

				return collection;
			}
		}
	}
}
