using System.IO;
using System.Xml;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SecondarySMTPServer))]
	sealed class SecondarySMTPServerTest : RegistryBusinessObjectTemplateTestCase<SecondarySMTPServer>
	{
		public void TestSMTPServerValidation()
		{
			var smtpServer = new SecondarySMTPServer();

			smtpServer.SMTPServer = string.Empty;
			AssertHasError(smtpServer.SMTPServerInfo, "Please enter a value.");

			smtpServer.SMTPServer = "mail.server.com";
			AssertNoError(smtpServer.SMTPServerInfo, "Please enter a value.");
		}

		public void TestSMTPServerSettingsMustBeUnique()
		{
			var collection = new SecondarySMTPServerCollection();
			var smtpServer1 = collection.AddNew();
			smtpServer1.SMTPServer = "mail.server1.com";
			smtpServer1.SMTPUsername = "username1@domain.com";

			var smtpServer2 = collection.AddNew();
			smtpServer2.SMTPServer = "mail.server1.com";
			smtpServer2.SMTPUsername = "username1@domain.com";

			AssertHasRowError(smtpServer2, "SMTP Server + SMTP User Name has already been set up. SMTP Server settings must be unique.");

			smtpServer2.SMTPUsername = "username2@domain.com";
			AssertNoRowError(smtpServer2, "SMTP Server + SMTP User Name has already been set up. SMTP Server settings must be unique.");

			smtpServer1.SMTPUsername = "username2@domain.com";
			AssertHasRowError(smtpServer1, "SMTP Server + SMTP User Name has already been set up. SMTP Server settings must be unique.");

			smtpServer1.SMTPServer = "mail.server2.com";
			AssertNoRowError(smtpServer1, "SMTP Server + SMTP User Name has already been set up. SMTP Server settings must be unique.");
		}

		public void TestSMTPSenderAddressValidation()
		{
			var secondarySMTPServer = new SecondarySMTPServer();

			secondarySMTPServer.SMTPSenderAddress = string.Empty;
			AssertHasError(secondarySMTPServer.SMTPSenderAddressInfo, "Please enter a value.");

			secondarySMTPServer.SMTPSenderAddress = "test";
			AssertHasError(secondarySMTPServer.SMTPSenderAddressInfo, "Email Address is not valid .");

			secondarySMTPServer.SMTPSenderAddress = "test@test.com";
			AssertNoError(secondarySMTPServer.SMTPSenderAddressInfo, "Email Address is not valid .");
		}

		public void TestSMTPSecureConnectionValidation()
		{
			var secondarySMTPServer = new SecondarySMTPServer();

			secondarySMTPServer.SMTPSecureConnection = string.Empty;
			AssertHasError(secondarySMTPServer.SMTPSecureConnectionInfo, "Please enter a value.");

			secondarySMTPServer.SMTPSecureConnection = "123";
			AssertHasError(secondarySMTPServer.SMTPSecureConnectionInfo, "Enter a valid selection.");

			secondarySMTPServer.SMTPSecureConnection = SecureConnectionTypes.None;
			AssertNoError(secondarySMTPServer.SMTPSecureConnectionInfo, "Enter a valid selection.");
		}

		public void TestSetSupportedDomain()
		{
			var secondarySMTPServer = new SecondarySMTPServer();
			AssertNoExceptionThrown(() =>
			{
				secondarySMTPServer.SupportedDomains = null;
				secondarySMTPServer.SupportedDomains = string.Empty;
			});

			secondarySMTPServer.SupportedDomains = "domain1.com,  domain2.com  ";
			AssertEquals("domain1.com, domain2.com", secondarySMTPServer.SupportedDomains);

			secondarySMTPServer.SupportedDomains = "domain2.com,\tdomain3.com";
			AssertEquals("domain2.com, domain3.com", secondarySMTPServer.SupportedDomains);

			secondarySMTPServer.SupportedDomains = "domain1.com,domain2.com\r\ndomain3.com,,domain4.com";
			AssertEquals("domain1.com, domain2.com, domain3.com, domain4.com", secondarySMTPServer.SupportedDomains);
		}

		public void TestSupportedDomainsValidation()
		{
			var collection = new SecondarySMTPServerCollection();
			var smtpServer1 = collection.AddNew();

			smtpServer1.SupportedDomains = string.Empty;
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Please enter a value.");

			smtpServer1.SupportedDomains = "  ";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Please enter a value.");

			smtpServer1.SupportedDomains = ".";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: ..");

			smtpServer1.SupportedDomains = "test";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: test.");

			smtpServer1.SupportedDomains = "test.@#$";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: test.@#$.");

			smtpServer1.SupportedDomains = "test.@#$.com";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: test.@#$.com.");

			smtpServer1.SupportedDomains = "test@#$test.com";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: test@#$test.com.");

			smtpServer1.SupportedDomains = "测试域名.com";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Supported Domains only accepts Western European languages characters.");

			smtpServer1.SupportedDomains = "test.com";
			AssertNoError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: test.");

			smtpServer1.SupportedDomains = "test.domain.com.cn";
			AssertNoError(smtpServer1.SupportedDomainsInfo, "Invalid Domains: test.");

			smtpServer1.SupportedDomains = "domain1.com, domain1.com";
			AssertHasError(smtpServer1.SupportedDomainsInfo, "Duplicated Domains: domain1.com. Supported Domain must be unique.");

			smtpServer1.SupportedDomains = "domain1.com, domain2.com";
			AssertNoError(smtpServer1.SupportedDomainsInfo, "Duplicated Domains: domain1.com. Supported Domain must be unique.");

			var smtpServer2 = collection.AddNew();
			smtpServer2.SupportedDomains = "domain1.com";
			AssertHasError(smtpServer2.SupportedDomainsInfo, "Duplicated Domains: domain1.com. Supported Domain must be unique.");

			smtpServer2.SupportedDomains = "domain3.com";
			AssertNoError(smtpServer2.SupportedDomainsInfo, "Duplicated Domains: domain1.com. Supported Domain must be unique.");
		}

		public void TestDefaultValues()
		{
			var secondarySMTPServer = new SecondarySMTPServer();
			CombineAssertions(() =>
			{
				AssertEquals(25, secondarySMTPServer.SMTPPort);
				AssertEquals(SecureConnectionTypes.None, secondarySMTPServer.SMTPSecureConnection);
				AssertEquals(true, secondarySMTPServer.AllowEmailsToBeSentFromUsersAddress);
			});
		}

		public void TestXmlSerialization()
		{
			string result;
			var secondarySMTPServer = new SecondarySMTPServer()
			{
				SMTPServer = "mail.server.com",
				SMTPPort = 587,
				SMTPSecureConnection = SecureConnectionTypes.SSL,
				SMTPUsername = "user@server.com",
				SMTPPassword = "password",
				AllowEmailsToBeSentFromUsersAddress = true,
				SMTPSenderAddress = "sender@server.com",
				SupportedDomains = "domain1.com, domain2.com"
			};

			var expectedSerializedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><SecondarySMTPServer><SMTPServer>mail.server.com</SMTPServer><SMTPPort>587</SMTPPort><SMTPSecureConnection>SSL</SMTPSecureConnection><SMTPUsername>user@server.com</SMTPUsername><SMTPPassword>password</SMTPPassword><AllowEmailsToBeSentFromUsersAddress>Y</AllowEmailsToBeSentFromUsersAddress><SMTPSenderAddress>sender@server.com</SMTPSenderAddress><SupportedDomains>domain1.com, domain2.com</SupportedDomains></SecondarySMTPServer>";
			using (StringWriter stringWriter = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
			{
				Serializer.Serialize(writer, secondarySMTPServer);
				result = stringWriter.ToString();
			}

			AssertEquals(expectedSerializedXml, result);
		}

		public void TestXmlDeserialization()
		{
			SecondarySMTPServer deserializedBizo;

			var xmlToDeserialize = "<?xml version=\"1.0\" encoding=\"utf-16\"?><SecondarySMTPServer><SMTPServer>mail.server.com</SMTPServer><SMTPPort>587</SMTPPort><SMTPSecureConnection>SSL</SMTPSecureConnection><SMTPUsername>user@server.com</SMTPUsername><SMTPPassword>password</SMTPPassword><AllowEmailsToBeSentFromUsersAddress>N</AllowEmailsToBeSentFromUsersAddress><SMTPSenderAddress>sender@server.com</SMTPSenderAddress><SupportedDomains>domain1.com, domain2.com</SupportedDomains></SecondarySMTPServer>";
			using (StringReader stringReader = new StringReader(xmlToDeserialize))
			using (XmlTextReader reader = new XmlTextReader(stringReader))
			{
				deserializedBizo = (SecondarySMTPServer)Serializer.Deserialize(reader);
			}

			CombineAssertions(() =>
			{
				AssertEquals("mail.server.com", deserializedBizo.SMTPServer);
				AssertEquals(587, deserializedBizo.SMTPPort);
				AssertEquals(SecureConnectionTypes.SSL, deserializedBizo.SMTPSecureConnection);
				AssertEquals("user@server.com", deserializedBizo.SMTPUsername);
				AssertEquals("password", deserializedBizo.SMTPPassword);
				AssertEquals(false, deserializedBizo.AllowEmailsToBeSentFromUsersAddress);
				AssertEquals("sender@server.com", deserializedBizo.SMTPSenderAddress);
				AssertEquals("domain1.com, domain2.com", deserializedBizo.SupportedDomains);
			});
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override SecondarySMTPServer GetBusinessObjectToClone() => new SecondarySMTPServer();

		protected override SecondarySMTPServer GetBusinessObjectToSerialise() => new SecondarySMTPServer();

		ZXmlSerializer Serializer => serializer ?? (serializer = ZXmlSerializer.New(typeof(SecondarySMTPServer)));
		ZXmlSerializer serializer;

		#endregion
	}
}
