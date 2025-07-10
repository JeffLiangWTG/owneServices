using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace ZClientEDI.Business.Test
{
	[TestedType(typeof(WTGActiveDirectoryCredentialsRegistryItem))]
	public class WTGActiveDirectoryCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<WTGActiveDirectoryCredentials>
	{
		protected override StronglyTypedRegistryItem<WTGActiveDirectoryCredentials, WTGActiveDirectoryCredentials> GetNewRegistryItem()
		{
			return new WTGActiveDirectoryCredentialsRegistryItem(
						"WTGActiveDirectoryCredentials",
						null,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						WTGActiveDirectoryCredentials.DefaultValue);
		}

		protected override WTGActiveDirectoryCredentials ValidValue
		{
			get
			{
				var wTGActiveDirectoryCredentials = new WTGActiveDirectoryCredentials()
				{
					DomainName = "fake.domain",
					DomainUserName = "Dexter",
					DomainUserPassword = "DasIstEinPasswort",
					OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
					IsEnabled = ZBool.True
				};

				return wTGActiveDirectoryCredentials;
			}
		}
	}
}
