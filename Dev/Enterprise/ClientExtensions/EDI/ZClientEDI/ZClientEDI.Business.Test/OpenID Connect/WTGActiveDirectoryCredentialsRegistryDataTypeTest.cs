using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static ZClientEDI.Business.WTGActiveDirectoryCredentialsRegistryItem;

namespace ZClientEDI.Business.Test
{
	[TestedType(typeof(WTGActiveDirectoryCredentialsRegistryDataType))]
	public class WTGActiveDirectoryCredentialsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<WTGActiveDirectoryCredentialsRegistryDataType>
	{
		protected override WTGActiveDirectoryCredentialsRegistryDataType GetNewDataType()
		{
			return new WTGActiveDirectoryCredentialsRegistryDataType(WTGActiveDirectoryCredentials.DefaultValue);
		}

		protected override string ExpectedEditorName
		{
			get { return "WTGActiveDirectoryCredentialsEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var wTGActiveDirectoryCredentials1 = new WTGActiveDirectoryCredentials()
			{
				DomainName = "fake.domain",
				DomainUserName = "Dexter",
				DomainUserPassword = "DasIstEinPasswort",
				OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
				IsEnabled = ZBool.True
			};

			var wTGActiveDirectoryCredentials2 = new WTGActiveDirectoryCredentials()
			{
				DomainName = "The.domain",
				DomainUserName = "Meter",
				DomainUserPassword = "DasIstEinPasswort",
				OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
				IsEnabled = ZBool.True
			};
			var serializer = new WTGActiveDirectoryCredentialsRegistryDataType(wTGActiveDirectoryCredentials1);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(wTGActiveDirectoryCredentials1, serializer.Serialise(wTGActiveDirectoryCredentials1)),
				new ValidSampleAndBinaryValueInDB(wTGActiveDirectoryCredentials2, serializer.Serialise(wTGActiveDirectoryCredentials2)),
			};
		}
	}
}
