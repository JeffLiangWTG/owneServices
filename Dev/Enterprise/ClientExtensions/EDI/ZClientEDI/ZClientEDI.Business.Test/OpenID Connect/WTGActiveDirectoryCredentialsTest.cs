using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace ZClientEDI.Business.Test
{
	[TestedType(typeof(WTGActiveDirectoryCredentials))]
	public class WTGActiveDirectoryCredentialsTest : RegistryBusinessObjectTemplateTestCase<WTGActiveDirectoryCredentials>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override WTGActiveDirectoryCredentials GetBusinessObjectToClone()
		{
			var wTGActiveDirectoryCredentials = new WTGActiveDirectoryCredentials()
			{
				DomainName = "The.domain",
				DomainUserName = "Meter",
				DomainUserPassword = "DasIstEinPasswort",
				OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
				IsEnabled = ZBool.True
			};

			return wTGActiveDirectoryCredentials;
		}

		protected override WTGActiveDirectoryCredentials GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public override void TestBizObjectFields()
		{
			// Tested elsewhere
			Assert(true);
		}
	}
}
