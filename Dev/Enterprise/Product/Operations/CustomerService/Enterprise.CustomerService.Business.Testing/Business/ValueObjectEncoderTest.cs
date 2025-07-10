using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	sealed class ValueObjectEncoderTest : TestCase
	{
		public void TestEncryptDecrypt()
		{
			Xsd.OrgContact contact = new Xsd.OrgContact();
			contact.Name = "Mickey O'Rilley";
			contact.Mobile = "09383277";
			contact.EmailAddress = "morilley@cargowise.com";

			string encryptedText = ValueObjectEncoder.Encrypt(contact);
			Xsd.OrgContact decryptedContact = ValueObjectEncoder.Decrypt<Xsd.OrgContact>(encryptedText);
			AssertEquals("morilley@cargowise.com", decryptedContact.EmailAddress);
			AssertEquals("09383277", decryptedContact.Mobile);
			AssertEquals("Mickey O'Rilley", decryptedContact.Name);
		}

		public void TestSerializeDeserialize()
		{
			Xsd.OrgContact contact = new Xsd.OrgContact();
			contact.Name = "Mickey O'Rilley";
			contact.Mobile = "09383277";
			contact.EmailAddress = "morilley@cargowise.com";

			string xmlData = ValueObjectEncoder.Serialize(contact);
			Xsd.OrgContact convertedObject = ValueObjectEncoder.Deserialize<Xsd.OrgContact>(xmlData);
			AssertEquals("morilley@cargowise.com", convertedObject.EmailAddress);
			AssertEquals("09383277", convertedObject.Mobile);
			AssertEquals("Mickey O'Rilley", convertedObject.Name);
		}

		public void TestDeserialize()
		{
			string xmlData = @"
<OrgContact xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='http://www.edi.com.au/EnterpriseService/'>
  <Name>Ted Burhan</Name>
  <JobTitle>Developer</JobTitle>
  <EmailAddress>ted.burhan@cargowise.com</EmailAddress>
</OrgContact>
";
			Xsd.OrgContact convertedObject = ValueObjectEncoder.Deserialize<Xsd.OrgContact>(xmlData);
			AssertEquals("ted.burhan@cargowise.com", convertedObject.EmailAddress);
			AssertEquals("Developer", convertedObject.JobTitle);
			AssertEquals("Ted Burhan", convertedObject.Name);
		}

		public void TestEncryptDecryptText()
		{
			string encryptedText = ValueObjectEncoder.EncryptText("MEH MEH");
			AssertEquals("MEH MEH", ValueObjectEncoder.DecryptText(encryptedText));
		}
	}
}
