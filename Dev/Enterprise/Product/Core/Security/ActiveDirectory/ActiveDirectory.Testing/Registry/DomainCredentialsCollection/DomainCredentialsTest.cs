using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(DomainCredentials))]
	class DomainCredentialsTest : RegistryBusinessObjectTemplateTestCase<DomainCredentials>
	{
		#region Validation Tests

		public void TestDomainNameIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			domainCredentials.DomainName = string.Empty;
			AssertHasError(domainCredentials.DomainNameInfo, "Please enter a Domain Name.");
		}

		public void TestDomainUserNameIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			domainCredentials.DomainUserName = string.Empty;
			AssertHasError(domainCredentials.DomainUserNameInfo, "Please enter a Domain User Name.");
		}

		public void TestDomainUserPasswordIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			domainCredentials.DomainUserPassword = string.Empty;
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "Please enter a Domain User Password.");
		}

		public void TestIsDefaultDomainIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			var domainCredentialsCollection = new DomainCredentialsCollection { domainCredentials }; // so that it has a parent collection to call validation
			domainCredentials.IsDefaultDomain = false;
			AssertHasError(domainCredentials.IsDefaultDomainInfo, "One domain must be set as Default Domain.");
		}

		public void TestUserOrganisationalUnitIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			domainCredentials.UserOrganisationalUnit = string.Empty;
			AssertHasWarning(domainCredentials.UserOrganisationalUnitInfo, "You have not entered an Users' Organizational Unit.");
		}

		public void TestGroupOrganisationalUnitIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			domainCredentials.GroupOrganisationalUnit = string.Empty;
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "You have not entered a Groups' Organizational Unit.");
		}

		public void TestDefaultPasswordIsCallingValidation()
		{
			var domainCredentials = GetTestBizo();
			domainCredentials.DefaultPassword = string.Empty;
			AssertHasError(domainCredentials.DefaultPasswordInfo, "Please enter a Default Password.");
		}

		public void TestDefaultPassword_DefaultValue()
		{
			var domainCredentials = new DomainCredentials();
			AssertEquals($"Default password's initial value should be {DomainCredentials.DefaultPasswordValue}", DomainCredentials.DefaultPasswordValue, domainCredentials.DefaultPassword);
		}

		#endregion

		#region Password Encryption Tests

		const string InitialisationVector = "3f09c9df-1bb8-494f-a6d1-0daee81a7be4";

		public void TestInitialisationVectorValue()
		{
			AssertEquals("Value of Enterprise.Security.ActiveDirectory.DomainCredentials.InitialisationVector should not have changed.",
				DomainCredentials.InitialisationVector,
				InitialisationVector);
		}

		public void TestSerialization()
		{
			string serializedString;
			var domainCredentials = new DomainCredentials();
			var xmlSerializer = new XmlSerializer(typeof(DomainCredentials));

			using (var writer = new StringWriter())
			{
				xmlSerializer.Serialize(writer, domainCredentials);
				serializedString = writer.ToString();
			}
			using (var reader = new StringReader(serializedString))
			{
				var newDomainCredentials = xmlSerializer.Deserialize(reader) as DomainCredentials;
				AssertEquals(domainCredentials.DomainName, newDomainCredentials.DomainName);
				AssertEquals(domainCredentials.DomainUserName, newDomainCredentials.DomainUserName);
				AssertEquals(domainCredentials.DomainUserPassword, newDomainCredentials.DomainUserPassword);
				AssertEquals(domainCredentials.IsDefaultDomain, newDomainCredentials.IsDefaultDomain);
				AssertEquals(domainCredentials.UserOrganisationalUnit, newDomainCredentials.UserOrganisationalUnit);
				AssertEquals(domainCredentials.GroupOrganisationalUnit, newDomainCredentials.GroupOrganisationalUnit);
				AssertEquals(domainCredentials.DefaultPassword, newDomainCredentials.DefaultPassword);
			}

			domainCredentials = new DomainCredentials()
			{
				DomainName = "miamimetropolice.com",
				DomainUserName = "dexter.morgan",
				DomainUserPassword = "ThisIsNotASafePassword!",
				IsDefaultDomain = true,
				UserOrganisationalUnit = "Bay/Harbour",
				GroupOrganisationalUnit = "Butcher",
				DefaultPassword = "DontChangeMePleeeeease"
			};

			using (var writer = new StringWriter())
			{
				xmlSerializer.Serialize(writer, domainCredentials);
				serializedString = writer.ToString();
				AssertEquals("The Domain User Password should not appear clearly in the serialized string.", -1, serializedString.IndexOf(domainCredentials.DomainUserPassword, StringComparison.OrdinalIgnoreCase));
			}
			using (var reader = new StringReader(serializedString))
			{
				var newDomainCredentials = xmlSerializer.Deserialize(reader) as DomainCredentials;
				AssertEquals(domainCredentials.DomainName, newDomainCredentials.DomainName);
				AssertEquals(domainCredentials.DomainUserName, newDomainCredentials.DomainUserName);
				AssertEquals(domainCredentials.DomainUserPassword, newDomainCredentials.DomainUserPassword);
				AssertEquals(domainCredentials.IsDefaultDomain, newDomainCredentials.IsDefaultDomain);
				AssertEquals(domainCredentials.UserOrganisationalUnit, newDomainCredentials.UserOrganisationalUnit);
				AssertEquals(domainCredentials.GroupOrganisationalUnit, newDomainCredentials.GroupOrganisationalUnit);
				AssertEquals(domainCredentials.DefaultPassword, newDomainCredentials.DefaultPassword);
			}
		}

		public void TestDeserialization()
		{
			DomainCredentials domainCredentials;
			var xmlSerializer = new XmlSerializer(typeof(DomainCredentials));

			var serializedString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><DomainCredentials><DomainName /><DomainUserName /><DomainUserPassword>At7ybPdVO17b7Pnrq+8Tvg==</DomainUserPassword><IsDefaultDomain>N</IsDefaultDomain><UserOrganisationalUnit /><GroupOrganisationalUnit /><DefaultPassword /></DomainCredentials>";
			using (var reader = new StringReader(serializedString))
			{
				domainCredentials = xmlSerializer.Deserialize(reader) as DomainCredentials;
			}

			AssertEquals(string.Empty, domainCredentials.DomainName);
			AssertEquals(string.Empty, domainCredentials.DomainUserName);
			AssertEquals(string.Empty, domainCredentials.DomainUserPassword);
			AssertEquals(false, domainCredentials.IsDeleted);
			AssertEquals(string.Empty, domainCredentials.UserOrganisationalUnit);
			AssertEquals(string.Empty, domainCredentials.GroupOrganisationalUnit);
			AssertEquals(string.Empty, domainCredentials.DefaultPassword);

			serializedString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><DomainCredentials><DomainName>miamimetropolice.com</DomainName><DomainUserName>dexter.morgan</DomainUserName><DomainUserPassword>P4yb6nqiMKmCT1p/IDpfOR5skYuJz2k+7Oe6qlNQe0633lLyu7veGERj8LJ1LEE/</DomainUserPassword><IsDefaultDomain>Y</IsDefaultDomain><UserOrganisationalUnit>Bay/Harbour</UserOrganisationalUnit><GroupOrganisationalUnit>Butcher</GroupOrganisationalUnit><DefaultPassword>DontChangeMePleeeeease</DefaultPassword></DomainCredentials>";

			using (var reader = new StringReader(serializedString))
			{
				domainCredentials = xmlSerializer.Deserialize(reader) as DomainCredentials;
			}

			AssertEquals("miamimetropolice.com", domainCredentials.DomainName);
			AssertEquals("dexter.morgan", domainCredentials.DomainUserName);
			AssertEquals("ThisIsNotASafePassword!", domainCredentials.DomainUserPassword);
			AssertEquals(true, domainCredentials.IsDefaultDomain);
			AssertEquals("Bay/Harbour", domainCredentials.UserOrganisationalUnit);
			AssertEquals("Butcher", domainCredentials.GroupOrganisationalUnit);
			AssertEquals("DontChangeMePleeeeease", domainCredentials.DefaultPassword);
		}

		DomainCredentials GetTestBizo()
		{
			return new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			};
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override DomainCredentials GetBusinessObjectToSerialise() => GetTestBizo();

		protected override DomainCredentials GetBusinessObjectToClone() => GetTestBizo();

		#endregion
	}
}
