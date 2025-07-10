using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(DomainCredentialsCollectionRegistryItem))]
	class DomainCredentialsCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<DomainCredentialsCollection>
	{
		protected override StronglyTypedRegistryItem<DomainCredentialsCollection, DomainCredentialsCollection> GetNewRegistryItem()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			};
			domainCredentialsCollection.Add(domainCredentials);

			return new DomainCredentialsCollectionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, domainCredentialsCollection);
		}
	}
}
