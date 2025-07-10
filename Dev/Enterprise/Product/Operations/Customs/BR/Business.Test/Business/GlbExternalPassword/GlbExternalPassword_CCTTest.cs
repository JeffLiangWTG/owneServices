using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_CCT))]
	public class GlbExternalPassword_CCTTest : GlbExternalPasswordWithCertificateTest<GlbExternalPassword_CCT>
	{
		public void TestHumanReadableNameCore()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var externalPasswordBr = wrapper.CCTPassword;
			AssertEquals("HumanReadableNameCore for CCT", "CCT Certificate", externalPasswordBr.HumanReadableName);
		}

		public override void TestOnlySendCredentialIfNeeded()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			Factory.Save();
			var interchange = GetLatestEHubConfigurationInterchanges();
			AssertXMLEquals("send message when saving", ExpectedXML, Regex.Replace(interchange.EI_BodyText, @"(?<=<Passphrase>).*(?=</Passphrase>)", "CaRgOw1sE"));
			password.GP_PasswordType = ZString.Empty;
			password.GP_Certificate = ZBlob.Empty;
			password.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			interchange.Delete();
			Factory.Save();
			interchange = GetLatestEHubConfigurationInterchanges();
			AssertXMLEquals("send message when password is changed as empty", ExpectedDeletedOrEmptyXML, interchange.EI_BodyText.ToString());
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			Factory.Save();
			interchange.Delete();
			password.Delete();
			Factory.Save();
			interchange = GetLatestEHubConfigurationInterchanges();
			AssertXMLEquals("send message when password is deleted", ExpectedDeletedOrEmptyXML, interchange.EI_BodyText.ToString());
		}

		public void TestCreateSubscriptionWhenCertificateIsLoaded()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(newStaff);
			var password = wrapper.CCTPassword;
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			var subscriptions = wrapper.EventSubscriptions;
			AssertEquals("Subscription created", 1, subscriptions.Count);
			var subscription = (GlbExternalPassword_BRS)subscriptions.First();
			AssertEquals("Subscription event id", EventIdList.Codes.DuexHistoric, subscription.GP_UserID);
			password.GP_Certificate = null;
			AssertEquals("Subscription - not created a new one", 1, subscriptions.Count);
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			AssertEquals("Subscription - not created a new one yet", 1, subscriptions.Count);
		}

		EDIInterchange GetLatestEHubConfigurationInterchanges()
		{
			var query = new ZDBOnlyQuery(typeof(EDIInterchange));
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.Configuration);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_From, ((IGlbCompany)GlbCompany.CurrentCompany).LicenceKeyIdentifier);
			query.AddToFilter(EDIInterchangeSchema.EI_To, MasterFiles.Business.Customs.XmlCredential.Constants.Configuration.EHubRecipient);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.eHub);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			return Factory.LoadTop1<EDIInterchange>(query);
		}

		public override void TestReadOnly()
		{
			AssertEquals("GP_IssueDateInfo.ReadOnly", true, GlbExternalPassword.GP_IssueDateInfo.ReadOnly);
			AssertEquals("GP_ExpiryDateInfo.ReadOnly", true, GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly);
			AssertEquals("GP_PasswordStatusInfo.ReadOnly", true, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);
		}

		public override void TestSetDefaultValues()
		{
			var password = Factory.New<GlbExternalPassword_CCT>();
			AssertEquals("CCT", password.GP_PasswordType);
			AssertEquals("", password.GP_PasswordStatus);
		}

		public void TestGetMessageAttrDictionary()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "ABC";
			var wrapper = BRGlbStaffWrapper.Get(newStaff);
			var password = wrapper.CCTPassword;
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			var value = string.Empty;
			var dictionary = password.GetMessageAttrDictionary();

			dictionary.TryGetValue("cw1.key", out value);
			AssertEquals("key", X509Certificate2TestHelper.ValidCertificate_KeyPEM, value);

			dictionary.TryGetValue("cw1.certificate", out value);
			AssertEquals("certificate", X509Certificate2TestHelper.ValidCertificate_CertPEM, value);
		}

		public void TestCheckIsValidCertificate()
		{
			var staff = Factory.New<GlbStaff>();
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_ExpiryDate = ZDateTime.Now.AddDays(-3);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			AssertEquals("False when the expiry date is in the pass", false, password.IsValidCertificate);

			password.GP_ExpiryDate = ZDateTime.Now.AddDays(36);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Invalid;
			AssertEquals("False when the Status is Invalid", false, password.IsValidCertificate);

			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			AssertEquals("True when the certificate is on time and the password is valid", true, password.IsValidCertificate);
		}

		const string ExpectedXML = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ABC"">
        <Certificate Name=""Certificate"">
          <File>MIIIMgIBAzCCB+wGCSqGSIb3DQEHAaCCB90EggfZMIIH1TCCAxoGCSqGSIb3DQEHAaCCAwsEggMHMIIDAzCCAv8GCyqGSIb3DQEMCgECoIICsjCCAq4wKAYKKoZIhvcNAQwBAzAaBBQDyTWE/n1o0nXWXjtcd5LxHMDf2gICBAAEggKAwQjl8/9yZAMxxAzhIkvQSpPg03hu1jl1v/IX2sdQzwpr0bGcsPgrgn9TlTKhBoVG7ORFOwIJsN9/6ULZG9lKy1YA6fAoORBLKC95PO8g6omgHsVWy9i1jAkyFY8FO9puiS2K63koYOoEpUynW81SpzFf+xiGIgf71Nju1UdBHYHJ4JIIh7nZOuhX0ErglJaqZLxjKqenUqoXXJDLWtrxhu5cek8zYbrACRLPB0LnV3Ue1LpirnDwVJrgQdD3uUYFl5c3tdBgMpI0/jKD9mZnz/qqo0cSUePoVUHlI1oQFyzt3krG1Z7PMgjFzf6FkyQB8MgxurtRsYCa3HL423v/eJGtdcEtPvwE/S0/gpe2lICQxWpyi+1VboHvtB1F7vkE4hUwW3rMVsi9/uM89DJ9yl4lbhjy9QFKECYZ9zX7KC12AA2exT6fw6PrR4QvyLfD5swhiMxnC1NEq10zk/ABFV63K8jQhx34uCs++fG8Si6hUB0PH18rjGyjgTeQheE5GHtfqXj67CIsl6IfcwxnSmjX9nRqA1fIyZiQQ5eaJGiLWwgrnA27VzJiuCnrJ6cSQPprjZ7AsO3dv0MdaQ1btoUV3dUzGB4CDG1SOOmJrKW/VlAmJbbmZHxWN0E33G5C9K62ED+Kblq4Wx8K7kc9fcb053qzhqw+2AQ+7uoxNP8E8V0+spqqY0wpSMSQoAO6xd1cSJqkdsAUaQoAn1NQ7kZ48AkV7o9KUynMVHDnSzHxTjg6bMRfU5ROlcInY6It2SDQbAwARyYb4rc3ms1mVuEtCW0qpH1rqJglzr2NspoaHvA/36FiRV43PZnbPGuN5f3pPZDdgQXZ5o5ymO7WwDE6MBUGCSqGSIb3DQEJFDEIHgYAcwBzAGwwIQYJKoZIhvcNAQkVMRQEElRpbWUgMTUwMDYyNTU2OTU4MTCCBLMGCSqGSIb3DQEHBqCCBKQwggSgAgEAMIIEmQYJKoZIhvcNAQcBMCgGCiqGSIb3DQEMAQYwGgQU6s9h8daGgGkXc2Ngd0NFlEyfvhwCAgQAgIIEYJlQ/jlQqSNCz44fOCiEbrHWdIPbSXpzKMkVYsD5hy0U4Aj6Ulit0AK242QIywQx6U22C62hAHOsTZ93i2tn0BzTGOqLVVX3gEsZDbR4JqY1YMLmG1YFrxaZZnsCWj4dccs/5RWH8vLpQbZ+NECekq8rRkKGba+QdXkitQQxyX9Gml8IoIeX0y4MioEoqVe9InS32OSMERtJysiS0m1DogE2OmMscyIpByX/v7RNqlZI7D9fcUlfYcvvZKqCtglP8EhRLuJIfr/Cx7xuoSBqJ9nGTxbDuX5L8F2xfH+IYnwCUmJ9ObmD1dBqNr9nFSGZlv5QyeDzQzFN72orbtFo9QbzM1K179TkkS0OUzv2vypBWXuz+7MKZ+53YKS7rJbKn/VOITM8aWB3HKjnqVttBdTMCZptVMvGM2Z1dw9p/UqPG5wEVb57brdkwYgIF8hIMDBBHx4OpL8P+gHKeCzSgX+DxZTlZ7KTuCcANUCs6egfdzVo8WUE2AjDGR0xM6hVby+12+id3foR9LfJjGPlW433K4/xkqNvH52f2hEO3yoSS6zfriUBJ+QZ+vCIJ0/9p9H+J3bP9rVEIP+Xk8XDLUk4tTh6g04NaYiJYdQHDcZaNEJAfhh44T+wG2giQ7CFMDukT0OLmI8PLWFpbTL1W89uzjhgycMmdrksNObXoWVYKXHa9akvbwPMucSmq7In8FroWhcnPBD/Cl2u+KszWJgPNPVu82cHUHCAMyiafeCfNt4V3nnjnwaqAxOfrlYmlVT6FVmtPcCYasXpsp6vg+fuUlWyzGQrKMwSSIFRfTPuqmT7J6RLU2TlGzWC1LMGiiDEbGaacjsSSMBNHfbJqd8tPKBp5Mkb+fygCuf5c8tq+ZNsUVVxeaWdXFQPeqZe1LZW7wbZ7pQxJHSx0ZsQusA76EASqzL1qg5oIVcrc+xCfuLLg9+geb46D7O5oAi/1hBsDFo6IXSJkapb8wzlsjtHX8evmOlRHjY7uH3Kaz+GSWD/6lXxDf+uJhS7tCvVhrR3uFm3zWKN7tsNyWNA+srq51x5SMMQjvyvGd8Tid07nXwA2pDdril6kqmTXuavj/XLzcxgev5byjyI2qusYAihjozGlEs+sa7pUUEjFFw+Cr/Priwz9fc7r7JZTd86qZHKfvFd03ih2DP5EHae5OChVcHsx71zC1O6pw+zrOmti9bcqOiVW9r0Hn8ZZHqK+FF0BIzrRo8Sv1OGiSdC3HTzfjzNwZZHJrHbgmK7NnMsGyUE9jUs0CFbQzKug6wGx3iRmvZC1e0k/s11oOJkaspUO77BXJIphlTp7YsYEKsmwmfeGYa3HBFkvS5HidzPuyechVv6QhEpkinIrWUszC/tLjpL1d/i/NXidVnN6rOziLvNhP5+6Kb3N0estk5TTp070S4we8AUWn8H8rN7LoNB05/yvWG9cpRNOk2/f9oREBsDR3j/tEHEqe0HNEzz99VT7XUm32ni9jUz1DLrlrkwPTAhMAkGBSsOAwIaBQAEFKXsLjF4ZWd6ZYf+qHwjobDF5JRwBBRemLJAncQxDYfcHjyMdiopkPQPxAICBAA=</File>
          <Passphrase>CaRgOw1sE</Passphrase>
        </Certificate>
      </Group>
    </Group>
  </Group>
</Configuration>";

		const string ExpectedDeletedOrEmptyXML = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ABC"" />
    </Group>
  </Group>
</Configuration>";
	}
}
