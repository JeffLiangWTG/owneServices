using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Security.ActiveDirectory.Registry;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test.Registry
{
	class ADRegistryProviderTest : TransactionedTestCase
	{
		public void TestLoadReadOnlyDomainCredentials_NotSet_ReturnsEmpty()
		{
			var credentialsProvider = ObjectFactory.Get<IDomainCredentialsProvider>();

			AssertNotNull("The IDomainCredentialsLoader has been instantiated from ObjectFactory", credentialsProvider);
			Assert("IDomainCredentialsLoader is wired to ActiveDirectoryRegistry", credentialsProvider is ADRegistryProvider);
			Assert("ActiveDirectory registry has not configured the domain credentials yet", !credentialsProvider.DomainCredentialsCollection.Any());
		}

		public void TestLoadDomainCredentialsFromRegistry()
		{
			var domainCredentialsCollection = ADTestHelper.CreateDomainCredentialsCollection();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domainCredentialsCollection);

			var credentialsProvider = ObjectFactory.Get<IDomainCredentialsProvider>();
			var loadedDomainCredentials = credentialsProvider.DomainCredentialsCollection;

			AssertNotNull("We have loaded the configured domain credentials from registry", loadedDomainCredentials);
			AssertEquals("The number of loaded domain credentials is the same as configured", domainCredentialsCollection.Count, loadedDomainCredentials.Count());

			foreach (var domainCredential in domainCredentialsCollection.Cast<DomainCredentials>())
			{
				Assert(loadedDomainCredentials.Any(x =>
					x.DomainName == domainCredential.DomainName &&
					x.DomainUserName == domainCredential.DomainUserName &&
					x.DomainUserPassword == domainCredential.DomainUserPassword &&
					x.UserOrganisationalUnit == domainCredential.UserOrganisationalUnit &&
					x.GroupOrganisationalUnit == domainCredential.GroupOrganisationalUnit &&
					x.DefaultPassword == domainCredential.DefaultPassword &&
					x.IsDefaultDomain == domainCredential.IsDefaultDomain));
			}
		}
	}
}
