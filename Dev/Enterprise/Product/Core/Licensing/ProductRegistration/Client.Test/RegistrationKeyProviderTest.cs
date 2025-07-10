using System;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.ProductRegistration.Client.ProductRegister;

namespace Enterprise.ProductRegistration.Client.Test
{
	public class RegistrationKeyProviderTest : TransactionedTestCase
	{
		public void TestSetKey()
		{
			TestHelper.KeyForTest = null;

			AssertNotEquals("ENT", RawDataRegistry.Instance.SystemEnterpriseCode.Value);
			AssertNotEquals("SYD", DataRegistry.Instance.PhysicalServerID);

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var signedKey = CargoWise.ProductRegistration.Service.MessageSigner.SignXml(TestUnsignedKeyXml);
			var encryptedKey = encoder.Encrypt(signedKey);
			var provider = new RegistrationKeyProvider();
			provider.SetKey(signedKey);
			AssertEquals(encryptedKey, RawDataRegistry.Instance.EncryptedRegistrationKey.Value);
			AssertEquals("ENT", RawDataRegistry.Instance.SystemEnterpriseCode.Value);
			AssertEquals("SYD", DataRegistry.Instance.PhysicalServerID);
			AssertEquals($"ENT{EnvProxy.Instance.CurrentCompany.Code}SYD", EnvProxy.Instance.CurrentCompany.LicenceKeyIdentifier);

			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDI");
			DataRegistry.Instance.PhysicalServerID = "DAT";

			provider.SetKey("invalid key");
			AssertEquals("invalid key is ignored", encryptedKey, RawDataRegistry.Instance.EncryptedRegistrationKey.Value);

			provider.SetKey(TestUnsignedKeyXml);
			AssertEquals("unsigned key is ignored", encryptedKey, RawDataRegistry.Instance.EncryptedRegistrationKey.Value);

			provider.SetKey("");
			AssertEquals("blank key is for unregistering", "", RawDataRegistry.Instance.EncryptedRegistrationKey.Value);

			AssertEquals("invalid, blankor unsigned keys do not update SystemEnterpriseCode registry", "EDI", RawDataRegistry.Instance.SystemEnterpriseCode.Value);
			AssertEquals("invalid, blank or unsigned keys do not update PhysicalServerID registry", "DAT", DataRegistry.Instance.PhysicalServerID);
		}

		public void TestKeyXmlPair()
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();

			var signedKey = CargoWise.ProductRegistration.Service.MessageSigner.SignXml(TestUnsignedKeyXml);
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, encoder.Encrypt(signedKey));

			var provider = new RegistrationKeyProvider();

			var keyXmlPair = provider.KeyXmlPair;

			AssertEquals("MyServer", keyXmlPair.Key.DbUniqueKey.ServerName);
			AssertEquals("MyDb", keyXmlPair.Key.DbUniqueKey.DatabaseName);
			AssertEquals(new DateTime(2014, 8, 22), keyXmlPair.Key.IssueDate);
		}

		public void TestKeyXmlPair_NotInstalled()
		{
			var provider = new RegistrationKeyProvider();
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			var keyXmlPair = provider.KeyXmlPair;
			AssertEquals("", keyXmlPair.Xml);
			AssertEquals(0, keyXmlPair.Key.DatabaseNumber);
		}

		public void TestKeyXmlPair_NotXml()
		{
			var provider = new RegistrationKeyProvider();
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var registryValue = encoder.Encrypt("this is not valid XML");
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var keyXmlPair = provider.KeyXmlPair;
			AssertEquals(0, keyXmlPair.Key.DatabaseNumber);
		}

		public void TestKeyXmlPair_NotEncrypted()
		{
			var provider = new RegistrationKeyProvider();
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var signedKey = CargoWise.ProductRegistration.Service.MessageSigner.SignXml(TestUnsignedKeyXml);
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, signedKey);
			var keyXmlPair = provider.KeyXmlPair;
			AssertEquals(0, keyXmlPair.Key.DatabaseNumber);
		}

		public void TestKeyXmlPair_NotSigned()
		{
			var provider = new RegistrationKeyProvider();
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var registryValue = encoder.Encrypt(TestUnsignedKeyXml);
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var keyXmlPair = provider.KeyXmlPair;
			AssertEquals(0, keyXmlPair.Key.DatabaseNumber);
		}

		public void TestKeyXmlPair_NotValidSignature()
		{
			var provider = new RegistrationKeyProvider();
			var signedKey = CargoWise.ProductRegistration.Service.MessageSigner.SignXml(TestUnsignedKeyXml);
			var tamperedKey = signedKey.Replace(
				"<ExpiryDate>2014-09-22T00:00:00</ExpiryDate>",
				"<ExpiryDate>2015-01-01T00:00:00</ExpiryDate>");
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var registryValue = encoder.Encrypt(TestUnsignedKeyXml);
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var keyXmlPair = provider.KeyXmlPair;
			AssertEquals(0, keyXmlPair.Key.DatabaseNumber);
		}

		public const string TestUnsignedKeyXml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
	<ServerName>MyServer</ServerName>
	<DatabaseName>MyDb</DatabaseName>
	<DatabaseCreated>2014-03-03T00:00:00</DatabaseCreated>
	<GroupId xsi:nil=""true"" />
  </DbUniqueKey>
  <IssueDate>2014-08-22T00:00:00</IssueDate>
  <ExpiryDate>2014-09-22T00:00:00</ExpiryDate>
  <EnterpriseCode>ENT</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
</RegistrationKey>";
	}
}
