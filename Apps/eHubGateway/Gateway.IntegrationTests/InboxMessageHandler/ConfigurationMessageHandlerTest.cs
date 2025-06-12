using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.Configuration.Schemas;
using CargoWise.eServices.Encryption.Client.Encryptor;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
    [TestFixture]
    class ConfigurationMessageHandlerTest : GatewayIntegrationTestBase
    {
	    [Test]
        public void TestConfigurationMessageHandler_Unknown()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var xmlString = "<Configuration Name='Test' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'></Configuration>";
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
                    StringAssert.Contains("Handler for configuration 'Test' is not implemented.", exception.Message);
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_NEXDOCSSystemLevel()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var systemId = TestClientID.Substring(0, 3) + TestClientID.Substring(6, 3);
                var companyId = TestClientID.Substring(3, 3);
                var config = string.Format(
@"<Configuration Name='NEXDOCSSystemLevel' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
	<Group Type='System' Reference='{0}'>
        <Group Type='Company' Reference='{1}'>
		    <Group Type='NGT' Status='VAL'>
			    <Credential Name='Current'>
				    <Password>bJ0ibpkU+PAEavFiLQWIDJAmSt+MEbjv+5/ex8FCVZepUApHe4opxogd3yT6f+b51nRJcQD5JJJ2P5UZyX2i4doEXVnADb/GGCKoH/4f+WLCfvP5kSGnyI3dfibXXIruiy+KErqULY7onTnhVWAx8KpgMXKy1p21vCiUnUfK+zA=</Password>
			    </Credential>
		    </Group>
		</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    AssertInboxMessage(message.TrackingID, TestClientPK, "UDM", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        "", "", messageStream.CompressAndEncode().ReadToEnd(), 0, 3);
                    Assert.IsTrue(FindOutboxMessage(message.TrackingID, eHubClientPK, TestClientPK, MessageStatusSuccessPK, null, null,
                        rawContent: "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_NEXDOCSUserLevel()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var systemId = TestClientID.Substring(0, 3) + TestClientID.Substring(6, 3);
                var companyId = TestClientID.Substring(3, 3);
                var config = String.Format(
@"<Configuration Name='NEXDOCSUserLevel' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
	<Group Type='System' Reference='{0}'>
        <Group Type='Company' Reference='{1}'>
		    <Group Type='Staff' Reference='TST'>
			    <Group Type='NUT' Status='VAL'>
				    <Credential Name='Current'>
					    <Password>nHJ1DwOU0ig9lX1o8UVHWX7kb3JtmKqhlFv3kEl1d7hdrc4Jb7J5FNUZfEm1DDeWuHMfOn7O7lqriJwz5QFKrKf31dqjFVyXwcSNYYfqIQDdE3DlOl4NRt9MRDZJFMSJpBPyBHEfgtJreA7L/gUzNhcgm8eGWnP3EOY6Rh4qUic=</Password>
				    </Credential>
			    </Group>
		    </Group>
		</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    AssertInboxMessage(message.TrackingID, TestClientPK, "UDM", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        "", "", messageStream.CompressAndEncode().ReadToEnd(), 0, 3);
                    Assert.IsTrue(FindOutboxMessage(message.TrackingID, eHubClientPK, TestClientPK, MessageStatusSuccessPK, null, null,
                        rawContent: "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_GLSHK()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var systemId = TestClientID.Substring(0, 3) + TestClientID.Substring(6, 3);
                var companyId = TestClientID.Substring(3, 3);
                var config = String.Format(
                    @"<Configuration Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""{0}"">
		<Group Type=""Company"" Reference=""{1}"">
			<Group Type=""PIMA"" Reference=""EZCAGT88USAFS/ATL85"">
				<FTP>
					<Server>111.glshk.com</Server>
					<UserName>abc</UserName>
					<Password>s5WRMitdNAhsVFw3nc2EY+09BaEEZq2jGBirfsUMCXMCAcokXFm9R1qNUTGwMEeLZHBoeAuGOcKskuBPfDaFwaeHxpKC4tuCVJiOOlI2puVOYsQUfRVh/JuzaQ9dSx6pG05n4d++gdd7j5ay6wM/u6gKmYTbTgdqU5sAuvbBlqA=</Password>
                    <ReceiveFolder>/home/agents/ca/xxxhost/out</ReceiveFolder>
				</FTP>
			</Group>			
		</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    AssertInboxMessage(message.TrackingID, TestClientPK, "UDM", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        "", "", messageStream.CompressAndEncode().ReadToEnd(), 0, 3);
                    Assert.IsTrue(FindOutboxMessage(message.TrackingID, eHubClientPK, TestClientPK, MessageStatusSuccessPK, null, null,
                        rawContent: "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
        }

        #region ConfigurationMessageHandler_SGCustoms

        const string BBBPassword = "JZJCOGRRnaf7dK/JP4pgabcp2EcA3n6kAs4cZfsZPxGjcj6muXBXSNz01MGGosYuHcB4z1ZI6gE8e72Hux9o3t96Zd9/rWrXASseD5QSr1fZ45gPTfi7aK6RD6FHqrF3+uSMC5sBFRUcdIYxOqF+/Fc7k29OzZwu9CAROuaKIwY=";
        const string DDDPassword = "Jq94iSMUP9cYTVgAI79So0M/4rQu/uLkR0A2MoNoRGKL9QMN/bWvA3BlstIfa9SPXylAaHFHXzEKeLIQDctWaP895LwFIUAgAXT40PZF+p6mvUsjXulHw7rPjswxfGdXfYfFw+VGCjZmQCM0FBhQ2iqAqDC+xoxBd11j9TdeFcs=";

        [Test]
        public void TestConfigurationMessageHandler_SGCustoms_ValidationFailed()
        {
            var clientLevelRegistrationType = Guid.Parse("51518101-1C8B-4266-9401-293AA8841913");
            AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);

            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var config = $@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTTST"">
		<Group Type=""Company"" Reference=""AAA"">
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Credential Name=""User1"">
						<UserName>Hadil</UserName>
						<Password />
					</Credential>
				</Group>
			</Group>
			<Group Type=""Staff"" Reference=""ZZH"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Credential Name=""User2"">
						<UserName>Zheyang</UserName>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    var expectedXml = new XmlDocument();
                    expectedXml.LoadXml($@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTTST"" Status=""INV"">
		<Annotations>
			<Item Name=""StatusReason"">Client system 'TSTTST' doesn't exist.</Item>
		</Annotations>
		<Group Type=""Company"" Reference=""AAA"" Status=""INV"">
			<Annotations>
				<Item Name=""StatusReason"">Client 'TSTAAATST' doesn't exist.</Item>
			</Annotations>
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Annotations>
						<Item Name=""StatusReason"">Password in User1's credential must not be empty.</Item>
					</Annotations>
					<Credential Name=""User1"">
						<UserName>Hadil</UserName>
						<Password />
					</Credential>
				</Group>
			</Group>
			<Group Type=""Staff"" Reference=""ZZH"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Annotations>
						<Item Name=""StatusReason"">Password in User2's credential must not be empty.</Item>
					</Annotations>
					<Credential Name=""User2"">
						<UserName>Zheyang</UserName>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>");
                    var expectedConfiguration = ConfigurationMessage.DeserializeFromXmlDocument(expectedXml);
                    var expectedStream = ConfigurationMessage.SerializeToStream(expectedConfiguration);
                    var outboxContent = expectedStream.CompressAndEncode().ReadToEnd();

                    AssertInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        isFlatFile: 0, status: 255, content: messageStream.CompressAndEncode().ReadToEnd());
                    AssertInboxMessage(eHubClientPK, "HUB", TestClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        isFlatFile: 0, status: 2, content: "H4sIAAAAAAAEAL2TzWqDQBSFX+Xixp3TZFmMILYkbkKJaRbZTfQaB5wZ8Y4Yn62LPlJfoaMhP7Y0i7QUhOuMnu+cOeLH27sfaZWLfVNzI7SCJZc4c5J51JDRkkYPHdhgTfZm5ky8BwcOslQ0cwpjqkfG2rb1WkFoMC32pd7x0ku1ZElaoOTExqTAn9e6qWDdVb1dRwalAyvMsUaV2q11sraXA4nhprEm8XJjRaFS2gwECvzYak55h7dWyKlnR6VAZYAGKrhHlAuZRlKuATwIMp7Pen3gsxHzOlWkZcVVN4oVhuGvMvVhLOOuPBaY56M0z0/x1yZPHy5MU90oc1/YF07U6joDoeCVsJ64BGmNmT2B4CVIawGWAzsElJXpfggfXSRHl4FlU/Sz3wkWPBOlz85r/+zMLOyit4vhlJd5u5jtdvEvxUz/sJjpdTHbAjuu9tfV3Kzj2xz9bsEn6wlks+gDAAA=");
                    Assert.That(FindOutboxMessage(eHubClientPK, TestClientPK, new Guid("313B842C-2ED9-4092-87F6-20BCC957F94A"), null, null, 0, null, outboxContent),
                        Is.True);

                    var expectedOutboxContent = new StringBuilder("Error during validating Configuration Message:");
                    expectedOutboxContent.AppendLine("Client system 'TSTTST' doesn't exist.");
                    expectedOutboxContent.AppendLine("Client 'TSTAAATST' doesn't exist.");
                    expectedOutboxContent.AppendLine("Password in User1's credential must not be empty.");
                    expectedOutboxContent.AppendLine("Password in User2's credential must not be empty.");

                    using (var expectedMessageStream =
                        new MemoryStream(Encoding.UTF8.GetBytes(expectedOutboxContent.ToString().TrimEnd())))
                    {
                        var failedOutboxContent = expectedMessageStream.CompressAndEncode().ReadToEnd();

                        Assert.That(
                            FindOutboxMessage(eHubClientPK, TestClientPK,
                                new Guid("6E0425D6-5D3E-42A2-8B74-2FF03C4520B9"), null, null, 0, null, failedOutboxContent),
                            Is.True);
                    }
                }

                AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
            }
            DeleteeHubClientRegistrations();
        }

		[Test]
        public void TestConfigurationMessageHandler_SGCustoms_ValidationFailedCannotDecryptPassword()
        {
            var clientLevelRegistrationType = Guid.Parse("51518101-1C8B-4266-9401-293AA8841913");
            AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);

            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var config = $@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTTST"">
		<Group Type=""Company"" Reference=""AAA"">
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Credential Name=""User1"">
						<UserName>Hadil</UserName>
						<Password>abcdefg</Password>
					</Credential>
				</Group>
			</Group>
			<Group Type=""Staff"" Reference=""ZZH"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Credential Name=""User2"">
						<UserName>Zheyang</UserName>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    var expectedXml = new XmlDocument();
                    expectedXml.LoadXml($@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTTST"" Status=""INV"">
		<Annotations>
			<Item Name=""StatusReason"">Client system 'TSTTST' doesn't exist.</Item>
		</Annotations>
		<Group Type=""Company"" Reference=""AAA"" Status=""INV"">
			<Annotations>
				<Item Name=""StatusReason"">Client 'TSTAAATST' doesn't exist.</Item>
			</Annotations>
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Annotations>
						<Item Name=""StatusReason"">Failed to decrypt User1's password. Error: Error occurred while decoding OAEP padding.</Item>
					</Annotations>
					<Credential Name=""User1"">
						<UserName>Hadil</UserName>
						<Password>abcdefg=</Password>
					</Credential>
				</Group>
			</Group>
			<Group Type=""Staff"" Reference=""ZZH"">
				<Group Type=""SGCustomsAccount"" Status=""INV"">
					<Annotations>
						<Item Name=""StatusReason"">Password in User2's credential must not be empty.</Item>
					</Annotations>
					<Credential Name=""User2"">
						<UserName>Zheyang</UserName>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>");
                    var expectedConfiguration = ConfigurationMessage.DeserializeFromXmlDocument(expectedXml);
                    var expectedStream = ConfigurationMessage.SerializeToStream(expectedConfiguration);
                    var outboxContent = expectedStream.CompressAndEncode().ReadToEnd();

                    AssertInboxMessage(TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        isFlatFile: 0, status: 255, content: messageStream.CompressAndEncode().ReadToEnd());
                    AssertInboxMessage(eHubClientPK,  "HUB", TestClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        isFlatFile: 0, status: 2, content: "H4sIAAAAAAAEALWTwU6EMBCGX2XCZW9UPZouCcFV96LGXT3srVsGaEJb0pYgz+bBR/IVHNB1F40makxIhhn4//n6E54fn3hmTaHK1omgrIEroXEerS6y1ger/eRhBPfoPN3Mo+P4KIIHXRs/j6oQmlPGuq6LO+UxoKzK2m5FHUur2UpWqIVnU6eEXzjbNrDum2Fd7wPqCG6xQIdG0mi9WtMVwSqI0NKS5dU9iVJjbBgdfMKXpNnxjm/dovCDd1YrNAH86AqzV6sZ5Ba9mQXAB+VDzNmgTzibeB5SZVY3wvQTrDRN/8Q0wJDHr3jIsCgmNIuz5cckdx8uldK2JvwO9lyoGnMIFnKUrm8C3Hl0xzMPjfC+sy6PYeGcdaevBayUrXMk6SpSDiqbK1PCdbq4IU0+NF+cMCMZJaNE/YYyriLUoQ6T5FLkqubsvec3bxCJ2Moci3LO2fuIs70hNWM2+/p9nJvN5b/EuYMDZcYgTyhIuT+2phVAPrBFQN2E/idJnRwmtamwF6Y8zOrbOD7VyU+avADv/qnSHgQAAA==");
                    Assert.That(FindOutboxMessage(eHubClientPK, TestClientPK, new Guid("313B842C-2ED9-4092-87F6-20BCC957F94A"), null, null, 0, null, outboxContent), Is.True);

                    var expectedOutboxContent = new StringBuilder("Error during validating Configuration Message:");
                    expectedOutboxContent.AppendLine("Client system 'TSTTST' doesn't exist.");
                    expectedOutboxContent.AppendLine("Client 'TSTAAATST' doesn't exist.");
                    expectedOutboxContent.AppendLine("Failed to decrypt User1's password. Error: Error occurred while decoding OAEP padding.");
                    expectedOutboxContent.AppendLine("Password in User2's credential must not be empty.");

                    using (var expectedMessageStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutboxContent.ToString().TrimEnd())))
                    {
                        var failedOutboxContent = expectedMessageStream.CompressAndEncode().ReadToEnd();
                        Assert.That(
                            FindOutboxMessage(eHubClientPK, TestClientPK,
                                new Guid("6E0425D6-5D3E-42A2-8B74-2FF03C4520B9"), null, null, 0, null, failedOutboxContent),
                            Is.True);
                    }
                }
                AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
            }
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_SGCustoms_AddUpdateDelete()
        {
            var clientRegistrationType = Guid.Parse("51518101-1C8B-4266-9401-293AA8841913");
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");

            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var addConfig = $@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"" Status=""INV"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"" Status=""VAL"">
					<Credential Name=""User1"">
						<UserName>Hadil</UserName>
						<Password>{BBBPassword}</Password>
					</Credential>
					<Credential Name=""User2"">
						<UserName>AAACE06TXP3LMHG</UserName>
						<Password>{DDDPassword}</Password>
					</Credential>
				</Group>
			</Group>
			<Group Type=""Staff"" Reference=""ZZH"">
				<Group Type=""SGCustomsAccount"" Status=""VAL"">
					<Credential Name=""User3"">
						<UserName>Zheyang</UserName>
						<Password>{BBBPassword}</Password>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(addConfig)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    AsserteHubClientRegistrationExists(2, clientRegistrationType, TestClientID);
                    AsserteHubClientRegistrationPassword(clientRegistrationType, "BBBPassword", TestClientID);
                }

                var mixedConfig = $@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"" Status=""INV"">
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"" Status=""VAL"">
					<Credential Name=""User1"">
						<UserName>Hadil2</UserName>
						<Password>{DDDPassword}</Password>
					</Credential>
				</Group>
			</Group>
			<Group Type=""Staff"" Reference=""ZZH"">
				<Group Type=""SGCustomsAccount"" Status=""VAL"" />
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(mixedConfig)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    AsserteHubClientRegistrationExists(1, clientRegistrationType, TestClientID);
                    AsserteHubClientRegistrationPassword(clientRegistrationType, "DDDPassword", TestClientID);

                    Assert.That(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()),
                        Is.True);
                    Assert.That(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="),
                        Is.True);
                }
            }

            DeleteeHubClientRegistrations();
        }

		[Test]

        public void TestConfigurationMessageHandler_SGCustoms_DoesNotInsertToInboxAndOutboxOnException()
        {
            var clientRegistrationType = Guid.Parse("51518101-1C8B-4266-9401-293AA8841913");
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");

            using (new DisposableAction(() => DeleteMessageType(statusSuccessMessageType), () => InsertMessageType(statusSuccessMessageType, "MessageStatusSuccess")))
            {
                using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
                {
                    var config = $@"
<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Staff"" Reference=""EDI"">
				<Group Type=""SGCustomsAccount"">
					<Credential Name=""User1"">
						<UserName>Hadil</UserName>
						<Password>{BBBPassword}</Password>
					</Credential>
					<Credential Name=""User2"">
						<UserName>AAACE06TXP3LMHG</UserName>
						<Password>{DDDPassword}</Password>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";

                    using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                    {
                        var trackingID = Guid.NewGuid();
                        var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                            "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                        adapter.Outbox.AddMessage(message);
                        Assert.That(() => adapter.SendMessages(), Throws.TypeOf<eHubAdapterException>().With.Message.Contains("Sequence contains no elements\r\n ExceptionID: "));

                        Assert.That(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()),
                            Is.False);
                        Assert.That(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="),
                            Is.False);
                        AsserteHubClientRegistrationExists(0, clientRegistrationType, "TSTCLIENT");
                    }
                }
            }

            DeleteeHubClientRegistrations();
        }

        #endregion

        #region ConfigurationMessageHandler_TWCustoms

        [Test]
        public void TestConfigurationMessageHandler_TWCustoms()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var config = @"<Configuration Name='TWCustomsSubscribers' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
	<Group Type='System' Reference='TSTENT'>
		<Group Type='Company' Reference='CLI'>
			<Group Type='Staff' Reference='staffId'>
				<Group Type='MailBoxID' Reference='mailboxId2' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    AssertInboxMessage(message.TrackingID, TestClientPK, "UDM", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                        "", "", messageStream.CompressAndEncode().ReadToEnd(), 0, 3);
                    Assert.IsTrue(FindOutboxMessage(message.TrackingID, eHubClientPK, TestClientPK, MessageStatusSuccessPK, null, null,
                        rawContent: "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_TWCustomsDelete()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {

                var registrationType = Guid.Parse("0E58E060-DF3D-4B38-8677-D34A6D8F9A33");

                var config = @"<Configuration Name='TWCustomsSubscribers' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
	<Group Type='System' Reference='TSTENT'>
		<Group Type='Company' Reference='CLI'>
			<Group Type='Staff' Reference='staffId'>
				<Group Type='MailBoxID' Reference='TSBKN00281-A' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
				<Group Type='MailBoxID' Reference='TSBKN00282-C' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
				<Group Type='MailBoxID' Reference='TSBKN00283-C' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
				<Group Type='MailBoxID' Reference='TSBKN00284-C' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }
                AsserteHubClientRegistrationExists(4, registrationType, "TSTCLIENT");

                var anotherConfig = @"<Configuration Name='TWCustomsSubscribers' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
	<Group Type='System' Reference='TSTENT'>
		<Group Type='Company' Reference='CLI'>
			<Group Type='Staff' Reference='staffId'>
				<Group Type='MailBoxID' Reference='TSBKN00281-A' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
				<Group Type='MailBoxID' Reference='TSBKN00282-C' Status='VAL'>
					<Item Name='Platform'>TVA</Item>
					<Item Name='ReceiveAutomatically'>1</Item> 	
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(anotherConfig)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }
                AsserteHubClientRegistrationExists(2, registrationType, "TSTCLIENT");

                var thirdConfig = @"<Configuration Name='TWCustomsSubscribers' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
	<Group Type='System' Reference='TSTENT'>
		<Group Type='Company' Reference='CLI'>
			<Group Type='Staff' Reference='staffId'>
			</Group>
		</Group>
	</Group>
</Configuration>";
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(thirdConfig)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID, MessageSchemaType.Xml, "UDM",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }
                AsserteHubClientRegistrationExists(0, registrationType, "TSTCLIENT");
            }
        }
        #endregion

        #region ConfigurationMessageHandler_CNCustoms

        [Test]
        public void TestConfigurationMessageHandler_CNCustoms_Add()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("D21A3460-9BC4-45A2-9731-A05F2CD0EBA1");
                var clientID = "ENTRTRSVR";
                var systemId = clientID.Substring(0, 3) + clientID.Substring(6, 3);
                var companyId = clientID.Substring(3, 3);
                var config = string.Format(
                    @"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTRTRSVR_CSW</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), clientID, eHubClientID, MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClient("ENTRTRSVR", "Client", "Enterprise", true);
                    AsserteHubClient("ENTTSTSVR_CSW", "Client", "Third Party", false);

                    AsserteHubClientRegistrationExists(1, registrationType, "ENTRTRSVR");
                    AsserteHubClientRegistrationCode(registrationType, "ENTRTRSVR_CSW", "ENTRTRSVR");
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_CNCustoms_UserNameEmptyWithNewClientID()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("D21A3460-9BC4-45A2-9731-A05F2CD0EBA1");
                var clientID = "AAABBBCCC";
                var systemId = clientID.Substring(0, 3) + clientID.Substring(6, 3);
                var companyId = clientID.Substring(3, 3);

                var config = string.Format(
                    @"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName></UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                        MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(0, registrationType, clientID);
                    AsserteHubClientExists(0, clientID);
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_CNCustoms_Delete()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("D21A3460-9BC4-45A2-9731-A05F2CD0EBA1");
                var systemId = TestAuthenticatedClientID.Substring(0, 3) + TestAuthenticatedClientID.Substring(6, 3);
                var companyId = TestAuthenticatedClientID.Substring(3, 3);
                var config = string.Format(
                    @"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTTSTSVR_CSW</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                        MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
                    AsserteHubClient("ENTTSTSVR_CSW", "Client", "Third Party", false);

                    AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                    AsserteHubClientRegistrationCode(registrationType, "ENTTSTSVR_CSW", "ENTTSTSVR");

                    config = string.Format(
                        @"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName></UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                    using (var messageStream1 = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                    {
                        var message1 = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                            MessageSchemaType.Xml,
                            "HUB",
                            "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream1);
                        adapter.Outbox.AddMessage(message1);
                        adapter.SendMessages();

                        AsserteHubClientRegistrationExists(0, registrationType, "ENTTSTSVR");
                        AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
                    }
                }
            }
        }

[Test]
        public void TestConfigurationMessageHandler_CNCustoms_Update()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("D21A3460-9BC4-45A2-9731-A05F2CD0EBA1");
                var systemId = TestAuthenticatedClientID.Substring(0, 3) + TestAuthenticatedClientID.Substring(6, 3);
                var companyId = TestAuthenticatedClientID.Substring(3, 3);
                var config = string.Format(
                    @"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTTSTSVR_CSW</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                        MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
                    AsserteHubClient("ENTTSTSVR_CSW", "Client", "Third Party", false);

                    AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                    AsserteHubClientRegistrationCode(registrationType, "ENTTSTSVR_CSW", "ENTTSTSVR");


                    config = string.Format(
                        @"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTVFGSVR_CSW</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                    using (var messageStream1 = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                    {
                        var message1 = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID,
                            MessageSchemaType.Xml,
                            "HUB",
                            "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream1);
                        adapter.Outbox.AddMessage(message1);
                        adapter.SendMessages();

                        AsserteHubClient("ENTVFGSVR_CSW", "Client", "Third Party", false);
                        AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                        AsserteHubClientRegistrationCode(registrationType, "ENTVFGSVR_CSW", "ENTTSTSVR");
                    }
                }
            }
        }

        #endregion

        #region ConfigurationMessageHandler_BR_ACAS

        [Test]
        public void TestConfigurationMessageHandler_BRCustoms_Add()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("E30CD2EF-8241-4E45-A911-2DE294494773");
                var registrationTypeToken = Guid.Parse("1F73528E-2EF6-404B-9615-6D24A9D51200");
                var clientID = "ENTTSTSVR";
                var systemId = clientID.Substring(0, 3) + clientID.Substring(6, 3);
                var companyId = clientID.Substring(3, 3);
                var staffID = "BOB";

                var config = $@"<Configuration Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""> 
	<Group Type='System' Reference='{systemId}' >
	<Group Type='Company' Reference='{companyId}' >
	<Group Type='Staff' Reference='{staffID}' >
		<Certificate>
			<File>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</File>
			<Passphrase>S/Z0Dl6yTi4DSjV9uftYUPncLuAI4cHnbf337puuxGLFIQ3ku8hacqKvLYp4cT+Zh5pAgxzM5Gs2xzpY5UC0StXDRIOpcmERfuFxD5dgSSsqa3vo+hpCSrysKCAKriLUxZrhG9zbsQ85oYZxFZQlML55zurSowRpZNg2fLShpJs=</Passphrase>
		</Certificate>
	</Group>
	</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {

                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, clientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    AsserteHubClientRegistrationExists(0, registrationType, "ENTTSTSVR");
                    AsserteHubCertificateExists(0, "BOB");
                    AsserteHubClientSystemRegistrationExists(0, registrationTypeToken, staffID);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }

                AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                AsserteHubCertificateExists(1, "BOB");
                AsserteHubClientSystemRegistrationExists(1, registrationTypeToken, staffID);
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_BRCustoms_Update()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("E30CD2EF-8241-4E45-A911-2DE294494773");
                var registrationTypeToken = Guid.Parse("1F73528E-2EF6-404B-9615-6D24A9D51200");
                var clientID = "ENTTSTSVR";
                var systemId = clientID.Substring(0, 3) + clientID.Substring(6, 3);
                var companyId = clientID.Substring(3, 3);
                var staffID = "BOB";

                var config =
                    $@"<Configuration Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""> 
	<Group Type='System' Reference='{systemId}' >
	<Group Type='Company' Reference='{companyId}' >
	<Group Type='Staff' Reference='{staffID}' >
		<Certificate>
			<File>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</File>
			<Passphrase>S/Z0Dl6yTi4DSjV9uftYUPncLuAI4cHnbf337puuxGLFIQ3ku8hacqKvLYp4cT+Zh5pAgxzM5Gs2xzpY5UC0StXDRIOpcmERfuFxD5dgSSsqa3vo+hpCSrysKCAKriLUxZrhG9zbsQ85oYZxFZQlML55zurSowRpZNg2fLShpJs=</Passphrase>
		</Certificate>
	</Group>
	</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, clientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }

                AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                AsserteHubCertificateExists(1, "BOB");
                AsserteHubCertificateFile(1, "SY4F17N1");
                Assert.AreEqual("1BC17E85C50C89089FE9921A97133055F13D444A", GetCertificateThumbPrint("BOB"));
                AsserteHubClientSystemRegistrationExists(1, registrationTypeToken, staffID);

                config =
                    $@"<Configuration Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""> 
	<Group Type='System' Reference='{systemId}' >
	<Group Type='Company' Reference='{companyId}' >
	<Group Type='Staff' Reference='{staffID}' >
		<Certificate>
			<File Filename='17381504_LENIO_ANDRE_ZANATTA_72061480004.p12'>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</File>
			<Passphrase>Y0ZV4/LBdOn11KFKCgbsHlpW68q52I8P0/yLDrE56J4XMEWyPztOMGCWFxKId5EjJqH3B+0C8Zh+mXhAVYZxu0YA7EWZBncJxIR4x2lzR5cujtzKCfMprFI9k4E2+IwY8uBnCa4ptOn+dB+YdzLYjN2durpdq74JTT/xspxMyPo=</Passphrase>
		</Certificate>
	</Group>
	</Group>
	</Group>
</Configuration>";

                SeteHubClientSystemRegistrationsValid();
                AsserteHubClientSystemRegistrationExists(0, registrationTypeToken, staffID);

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, clientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }

                AsserteHubCertificateExists(1, "BOB");
                AsserteHubCertificateFile(1, "pibe19");
                Assert.AreEqual("6F8539C7368A216C06CB7FF8895B815DF3B76A86", GetCertificateThumbPrint("BOB"));
                AsserteHubClientSystemRegistrationExists(1, registrationTypeToken, staffID);
            }
        }

        protected string GetCertificateThumbPrint(string staffID)
        {
            using (var connection = OpenEHubTransactionsConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"select TOP 1 CE_Thumbprint from ehubCertificate where CE_ID = '{staffID}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader[0].ToString();
                        }
                    }
                }
            }
            return String.Empty;
        }

        [Test]
        public void TestConfigurationMessageHandler_BRCustoms_Delete()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("E30CD2EF-8241-4E45-A911-2DE294494773");
                var registrationTypeToken = Guid.Parse("1F73528E-2EF6-404B-9615-6D24A9D51200");

                var clientID = "ENTTSTSVR";
                var systemId = clientID.Substring(0, 3) + clientID.Substring(6, 3);
                var companyId = clientID.Substring(3, 3);
                var staffID = "BOB";
                const string SY4F17N1 =
                    "S/Z0Dl6yTi4DSjV9uftYUPncLuAI4cHnbf337puuxGLFIQ3ku8hacqKvLYp4cT+Zh5pAgxzM5Gs2xzpY5UC0StXDRIOpcmERfuFxD5dgSSsqa3vo+hpCSrysKCAKriLUxZrhG9zbsQ85oYZxFZQlML55zurSowRpZNg2fLShpJs=";

                var config =
                    $@"<Configuration Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""> 
	<Group Type='System' Reference='{systemId}' >
	<Group Type='Company' Reference='{companyId}' >
	<Group Type='Staff' Reference='{staffID}' >
		<Certificate>
			<File>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</File>
			<Passphrase>{SY4F17N1}</Passphrase>
		</Certificate>
	</Group>
	</Group>
	</Group>
</Configuration>";
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, clientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }

                AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                AsserteHubCertificateExists(1, "BOB");
                AsserteHubClientSystemRegistrationExists(1, registrationTypeToken, staffID);

                config =
                    $@"<Configuration Name=""Advanced Air Cargo Report - Brazil"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""> 
	<Group Type='System' Reference='{systemId}' >
	<Group Type='Company' Reference='{companyId}' >
	<Group Type='Staff' Reference='{staffID}' >
	</Group>
	</Group>
	</Group>
</Configuration>";
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, clientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                }

                AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                AsserteHubCertificateExists(0, "BOB");
                AsserteHubClientSystemRegistrationExists(0, registrationTypeToken, staffID);
            }
        }

        #endregion

        #region ConfigurationMessageHandler_GlobalInvoice_India

        [Test]
        public void TestConfigurationMessageHandler_IndiaGlobalInvoice_ValidationFailed()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var clientLevelRegistrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");
                var serverLevelRegistrationType = Guid.Parse("284FB230-F154-4A39-9538-6D6AE41AD4FE");
                var clientSystem = "TSTENT";

                var config = $@"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTTST"">
		<Group Type=""Company"" Reference=""AAA"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>{EhubClientEncryptor.Encrypt("Hadil@#123")}</Password>
				</Credential>
			</Group>
		</Group>
		<Credential Name=""Service-Provider"">
			<UserName>AAACE06TXP3LMHG</UserName>
			<Password>{EhubClientEncryptor.Encrypt("wk1zEeSs97MAao5UIiWC")}</Password>
		</Credential>
	</Group>
</Configuration>";

                var expectedXml = new XmlDocument();
                expectedXml.LoadXml($@"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" 
	xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTTST"" Status=""INV"">
		<Annotations>
			<Item Name=""StatusReason"">{Convert.ToBase64String(Encoding.UTF8.GetBytes("Client system 'TSTTST' doesn't exist."))}</Item>
		</Annotations>
		<Group Type=""Company"" Reference=""AAA"" Status=""INV"">
			<Annotations>
				<Item Name=""StatusReason"">{Convert.ToBase64String(Encoding.UTF8.GetBytes("Client 'TSTAAATST' doesn't exist."))}</Item>
			</Annotations>
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"" />
			</Group>
		</Group>
		<Credential Name=""Service-Provider"" />
	</Group>
</Configuration>");

                var expectedOutboxContent = new StringBuilder("Error during validating Configuration Message:");
                expectedOutboxContent.AppendLine("Client system 'TSTTST' doesn't exist.");
                expectedOutboxContent.AppendLine("Client 'TSTAAATST' doesn't exist.");

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var expectedConfiguration = ConfigurationMessage.DeserializeFromXmlDocument(expectedXml);
                    var expectedStream = ConfigurationMessage.SerializeToStream(expectedConfiguration);

                    using (var expectedMessageStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutboxContent.ToString().TrimEnd())))
                    {
                        var failedOutboxContent = expectedMessageStream.CompressAndEncode().ReadToEnd();

                        var trackingID = Guid.NewGuid();
                        var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                        AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
                        AsserteHubClientSystemRegistrationExistsBySystemId(0, serverLevelRegistrationType, clientSystem);

                        adapter.Outbox.AddMessage(message);

                        adapter.SendMessages();

                        var outboxContent = expectedStream.CompressAndEncode().ReadToEnd();
                        AssertInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 255, content: messageStream.CompressAndEncode().ReadToEnd());
                        AssertInboxMessage(eHubClientPK, "HUB", TestClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 2, content: "H4sIAAAAAAAEAKWR3U6DMBiGb6Xh3ME2d2JgCUM3WQyRX5edlfINGqElpcDw1jzwkrwFu7lE0QMPTNp8SfO9z/skfX99Mx3ODjRvBZaUM+ThCizNZRnFCEogUnBGCaKs45RQlqNmaCRUGkpANCpgadOJoaFjVbLG0gop6xtd7/t+0tMGJJAiL3mKywnhlR6SAirc6KNGbWluBG9rFA21ag4v+AAOIIAR9RSFkToaCiWWrSpxvUSFbMa4PBOapemqzEX9cysA3JzY/uxY758WhnvvLcg8KNPQZsnai093O6ye01nyktLMcDfJNd55xkNuWaZ+wi1NfVTxXdLhVY3ZMLK0bfufik4Wx9Ng5cdrNX3mboJuv/Pa7dzP97uiJnO//VttJTAjxcjs7tZVMo6ADJikuLxYRPhY4wGEhnQFPDO+5q/tEERHCVw9Ct7R7Gdq9KPLD5FiAFxWAgAA");
                        Assert.IsTrue(FindOutboxMessage(eHubClientPK, TestClientPK, new Guid("313B842C-2ED9-4092-87F6-20BCC957F94A"), null, null, 0, null, outboxContent));
                    }
                }

                AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
                AsserteHubClientSystemRegistrationExistsBySystemId(0, serverLevelRegistrationType, clientSystem);
            }
            DeleteeHubClientRegistrations();
            DeleteeHubClientSystemRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_IndiaGlobalInvoice_NotExistDBRecord_EmptyCredential()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var clientLevelRegistrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");
                var serverLevelRegistrationType = Guid.Parse("284FB230-F154-4A39-9538-6D6AE41AD4FE");
                var clientSystem = "TSTENT";

                var config = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
				</Credential>
				<Credential Name=""Service-Provider"" />
			</Group>
		</Group>
		<Credential Name=""Service-Provider"" />
	</Group>
</Configuration>";

                var expectedConfigClientRegistration = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil</UserName>
		<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
	</Credential>
</Config>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {

                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
                    AsserteHubClientSystemRegistrationExistsBySystemId(0, serverLevelRegistrationType, clientSystem);

                    adapter.Outbox.AddMessage(message);

                    adapter.SendMessages();

                    Assert.IsTrue(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()));
                }

                AsserteHubClientRegistrationExists(1, clientLevelRegistrationType, TestClientID);
                AssertEHubClientRegistrationConfigXml(expectedConfigClientRegistration, clientLevelRegistrationType, TestClientID, "EDI");
                AsserteHubClientSystemRegistrationExistsBySystemId(0, serverLevelRegistrationType, clientSystem);
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_IndiaGlobalInvoice_Add()
        {
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var clientLevelRegistrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");
                var serverLevelRegistrationType = Guid.Parse("284FB230-F154-4A39-9538-6D6AE41AD4FE");
                var clientSystem = "TSTENT";

                var config = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
				</Credential>
			</Group>
		</Group>
		<Credential Name=""Service-Provider"">
			<UserName>AAACE06TXP3LMHG</UserName>
			<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
		</Credential>
	</Group>
</Configuration>";

                var expectedConfigClientRegistration = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil</UserName>
		<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
	</Credential>
</Config>";

                var expectedConfigSystemRegistration = @"<Config>
	<Credential Name=""Service-Provider"">
		<UserName>AAACE06TXP3LMHG</UserName>
		<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
	</Credential>
</Config>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {

                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
                    AsserteHubClientSystemRegistrationExistsBySystemId(0, serverLevelRegistrationType, clientSystem);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();
                    Assert.IsTrue(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()));
                    Assert.IsTrue(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }

                AsserteHubClientRegistrationExists(1, clientLevelRegistrationType, TestClientID);
                AsserteHubClientSystemRegistrationExistsBySystemId(1, serverLevelRegistrationType, clientSystem);
                AssertEHubClientRegistrationConfigXml(expectedConfigClientRegistration, clientLevelRegistrationType, TestClientID, "EDI");
                AssertEHubClientSystemRegistrationConfigXml(expectedConfigSystemRegistration, serverLevelRegistrationType, clientSystem);
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_IndiaGlobalInvoice_Delete()
        {
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var registrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");
                var serverLevelRegistrationType = Guid.Parse("284FB230-F154-4A39-9538-6D6AE41AD4FE");
                var clientSystem = "TSTENT";

                var config = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
				</Credential>
			</Group>
		</Group>
		<Credential Name=""Service-Provider"">
			<UserName>AAACE06TXP3LMHG</UserName>
			<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
		</Credential>
	</Group>
</Configuration>";

                var deleteConfig = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
			</Group>
		</Group>
	</Group>
</Configuration>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(1, registrationType, TestClientID);
                    AsserteHubClientSystemRegistrationExistsBySystemId(1, serverLevelRegistrationType, clientSystem);
                }

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(deleteConfig)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(0, registrationType, TestClientID);
                    AsserteHubClientSystemRegistrationExistsBySystemId(0, serverLevelRegistrationType, clientSystem);
                    Assert.IsTrue(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()));
                    Assert.IsTrue(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_IndiaGlobalInvoice_Update()
        {
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var registrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");

                var config = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
				</Credential>
				<Credential Name=""Service-Provider"">
					<UserName>AAACE06TXP3LMHG</UserName>
					<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";

                var updateConfig = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil2</UserName>
					<Password>IU6vr0S+yzx70EOrDMsuNppYHavRwZ/hY+xQaV7tQOlVZ1EQEsIlprPr9E2EH3iMihj5siK95zoZyyN8m/vpX1dRVJJpBFfxjhglAU4dnYiAMksRot8DNEiBDet+xM/0hpy8hlkVCwk6CGYK3StNSTtKwxDMJc62LkAvvXx+K2I=</Password>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";

                var expectedInsert = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil</UserName>
		<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
	</Credential>
	<Credential Name=""Service-Provider"">
		<UserName>AAACE06TXP3LMHG</UserName>
		<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
	</Credential>
</Config>";

                var expectedUpdate = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil2</UserName>
		<Password>IU6vr0S+yzx70EOrDMsuNppYHavRwZ/hY+xQaV7tQOlVZ1EQEsIlprPr9E2EH3iMihj5siK95zoZyyN8m/vpX1dRVJJpBFfxjhglAU4dnYiAMksRot8DNEiBDet+xM/0hpy8hlkVCwk6CGYK3StNSTtKwxDMJc62LkAvvXx+K2I=</Password>
	</Credential>
</Config>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(1, registrationType, TestClientID);
                    AssertEHubClientRegistrationConfigXml(expectedInsert, registrationType, TestClientID, "EDI");
                }

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(updateConfig)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(1, registrationType, TestClientID);
                    AssertEHubClientRegistrationConfigXml(expectedUpdate, registrationType, TestClientID, "EDI");
                    Assert.IsTrue(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()));
                    Assert.IsTrue(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_IndiaGlobalInvoice_SkipUpdate()
        {
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var registrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");

                var config = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
				</Credential>
				<Credential Name=""Service-Provider"">
					<UserName>AAACE06TXP3LMHG</UserName>
					<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";

                var updateConfig = @"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil2</UserName>
					<Password>IU6vr0S+yzx70EOrDMsuNppYHavRwZ/hY+xQaV7tQOlVZ1EQEsIlprPr9E2EH3iMihj5siK95zoZyyN8m/vpX1dRVJJpBFfxjhglAU4dnYiAMksRot8DNEiBDet+xM/0hpy8hlkVCwk6CGYK3StNSTtKwxDMJc62LkAvvXx+K2I=</Password>
				</Credential>
				<Credential Name=""Service-Provider"" />
			</Group>
		</Group>
	</Group>
</Configuration>";

                var expectedInsert = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil</UserName>
		<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
	</Credential>
	<Credential Name=""Service-Provider"">
		<UserName>AAACE06TXP3LMHG</UserName>
		<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
	</Credential>
</Config>";

                var expectedUpdate = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil2</UserName>
		<Password>IU6vr0S+yzx70EOrDMsuNppYHavRwZ/hY+xQaV7tQOlVZ1EQEsIlprPr9E2EH3iMihj5siK95zoZyyN8m/vpX1dRVJJpBFfxjhglAU4dnYiAMksRot8DNEiBDet+xM/0hpy8hlkVCwk6CGYK3StNSTtKwxDMJc62LkAvvXx+K2I=</Password>
	</Credential>
	<Credential Name=""Service-Provider"">
		<UserName>AAACE06TXP3LMHG</UserName>
		<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
	</Credential>
</Config>";

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(1, registrationType, TestClientID);
                    AssertEHubClientRegistrationConfigXml(expectedInsert, registrationType, TestClientID, "EDI");
                }

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(updateConfig)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(1, registrationType, TestClientID);
                    AssertEHubClientRegistrationConfigXml(expectedUpdate, registrationType, TestClientID, "EDI");
                    Assert.IsTrue(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()));
                    Assert.IsTrue(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                }
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_Exception_GenerateFailResponse()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var registrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");

                var config = $@"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
				</Credential>
				<Credential Name=""Service-Provider"">
					<UserName>AAACE06TXP3LMHG</UserName>
					<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";

                var updateConfig = $@"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil2</UserName>
					<Password>IU6vr0S+yzx70EOrDMsuNppYHavRwZ/hY+xQaV7tQOlVZ1EQEsIlprPr9E2EH3iMihj5siK95zoZyyN8m/vpX1dRVJJpBFfxjhglAU4dnYiAMksRot8DNEiBDet+xM/0hpy8hlkVCwk6CGYK3StNSTtKwxDMJc62LkAvvXx+K2I=</Password>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";

                var expectedInsert = @"<Config>
	<Credential Name=""Taxpayer"">
		<UserName>Hadil</UserName>
		<Password>vhxaZIzHKm74NWSKu9IERvyMWRrKhs3IISk4wBZj9nz0R5452h8YC3AoCgup0Z7Eqpj+yYSvrn1Hsj1pXuqlh5J5FghbUx0UMP6fawhUUh4negZBIvseOzQzxb53l9mYsfWbciU9GrSOJurmW4JhKzkr7KtpeviXfrVcxrXik5E=</Password>
	</Credential>
	<Credential Name=""Service-Provider"">
		<UserName>AAACE06TXP3LMHG</UserName>
		<Password>WhblZ+/Cq9ZnhmwoDu+fYxYhT1eLrrNmu9R6TIM6LTNCtrEK6kKsttkA6fWSuh0TfnXmjA8VJ6CiWY0nWa/r+L0CupKLYzJjuvaJq3EEwEQmEAdouGMFxSz6fAT5ArN9OxrT+sLoNsfVfE2iJJuVVZXSBbrvvtxLHV/5daGRbSo=</Password>
	</Credential>
</Config>";

                var expectedOutboxXml = new XmlDocument();
                expectedOutboxXml.LoadXml($@"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"" Status=""INV"">
		<Annotations>
			<Item Name=""StatusReason"">{Convert.ToBase64String(Encoding.UTF8.GetBytes(@"Value cannot be null.
Parameter name: s"))}</Item>
		</Annotations>
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"" />
			</Group>
		</Group>
	</Group>
</Configuration>");

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationExists(1, registrationType, "TSTCLIENT");
                    AssertEHubClientRegistrationConfigXml(expectedInsert, registrationType, "TSTCLIENT", "EDI");
                }

                UpdateEHubClientRegistrationConfigXml(registrationType, "TSTCLIENT", "EDI", null);

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(updateConfig)))
                {
                    var expectedConfiguration = ConfigurationMessage.DeserializeFromXmlDocument(expectedOutboxXml);
                    var expectedOutboxStream = ConfigurationMessage.SerializeToStream(expectedConfiguration);
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    adapter.Outbox.AddMessage(message);
                    var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
                    StringAssert.Contains($"1 errors occured during processing send request:\r\nValue cannot be null.\r\nParameter name: s\r\n ExceptionID: ", exception.Message);

                    var failOutboxContent = new MemoryStream(Encoding.UTF8.GetBytes("AAA")).CompressAndEncode().ReadToEnd();
                    AssertInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 255, content: messageStream.CompressAndEncode().ReadToEnd());
                    AssertInboxMessage(eHubClientPK, "HUB", TestClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 2, content: "H4sIAAAAAAAEAG2QS2rDMBRFtyK0gCgpdNBiG1onMQolkNh1PjNZfrEFlmQkuY7X1kGX1C1U+Qzq0pFA3HvOlb4/v4JYq5OoOsOc0AqtmYQQU1UKhqAB7oxWgiOhPrTgQlXIDtaBxCgHY30hxLPJFKOzbJQNce1c+0xI3/eTXlhwwOuq0QVrJlxLkvIaJLNkZMRRkBjdtSgbWm9O7/gtnMCA4v4qS7PFOsModcx1XkLXuS+9KKXdlWCjgPrOffottQVmL+xcLm25e68OD8uukE9TmqwamjzOiqTv5pt2c9iv6mKXT497WhVy6Y6ZrngfhgG5IKOAjDS/h8ZatkwNo6XxG/3znFfDFK9HocX8EooNlKCcYM19d8bOLRvAYES89sr45xx9XfQDCyxoTr8BAAA=");
                    Assert.IsTrue(FindOutboxMessage(eHubClientPK, TestClientPK, new Guid("313B842C-2ED9-4092-87F6-20BCC957F94A"), null, null, 0, null, expectedOutboxStream.CompressAndEncode().ReadToEnd()));
                    AssertFailedStatusMessage("TSTCLIENT", trackingID);

                    AsserteHubClientRegistrationExists(1, registrationType, "TSTCLIENT");
                }
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_Exception_NotInsertInboxOutbox()
        {
            var statusSuccessMessageType = Guid.Parse("14D86704-5B69-499E-B71E-05A708D004AA");
            using (new DisposableAction(() => DeleteMessageType(statusSuccessMessageType), () => InsertMessageType(statusSuccessMessageType, "MessageStatusSuccess")))
            {
                using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
                {
                    var registrationType = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE");

                    var config =
                        $@"<Configuration Name=""India electronic invoicing system"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""CLI"">
			<Group Type=""Branch"" Reference=""EDI"">
				<Credential Name=""Taxpayer"">
					<UserName>Hadil</UserName>
					<Password>{EhubClientEncryptor.Encrypt("Hadil@#123")}</Password>
				</Credential>
				<Credential Name=""Service-Provider"">
					<UserName>AAACE06TXP3LMHG</UserName>
					<Password>{EhubClientEncryptor.Encrypt("wk1zEeSs97MAao5UIiWC")}</Password>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";


                    using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                    {
                        var trackingID = Guid.NewGuid();
                        var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                            "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                        adapter.Outbox.AddMessage(message);

                        var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
                        StringAssert.Contains("Sequence contains no elements\r\n ExceptionID: ", exception.Message);

                        Assert.IsFalse(FindInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 3, content: messageStream.CompressAndEncode().ReadToEnd()));
                        Assert.IsFalse(FindOutboxMessage(trackingID, eHubClientPK, TestClientPK, statusSuccessMessageType, null, null, 0, null, "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="));
                        AsserteHubClientRegistrationExists(0, registrationType, "TSTCLIENT");
                    }
                }
            }

            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        #endregion

        #region ConfigurationHandler_TWCustomsTCA

        [Test]
        public void TestConfigurationMessageHandler_TWCustomsNCATK_ValidationFailed()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var clientLevelRegistrationType = Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE");
                var config = $@"
<Configuration Name=""TWCustomsNCATK"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""AAA"">
				<Credential Name=""Current"">
					<UserName>TSTT_TCA</UserName>
					<Password/>
				</Credential>
		</Group>
	</Group>
</Configuration>";

                var expectedXml = new XmlDocument();
                expectedXml.LoadXml($@"
<Configuration Name=""TWCustomsNCATK"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""AAA"" Status=""INV"">
		<Annotations>
			<Item Name=""StatusReason"">Invalid TCA Client ID formatting: 'TSTT_TCA'.</Item>
		</Annotations>
		</Group>
	</Group>
</Configuration>");

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var expectedOutboxContent = new StringBuilder("Error during validating Configuration Message:");
                    expectedOutboxContent.AppendLine("Invalid TCA Client ID formatting: 'TSTT_TCA'.");

                    var expectedConfiguration = ConfigurationMessage.DeserializeFromXmlDocument(expectedXml);
                    var expectedStream = ConfigurationMessage.SerializeToStream(expectedConfiguration);
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);

                    adapter.Outbox.AddMessage(message);

                    adapter.SendMessages();

                    var outboxContent = expectedStream.CompressAndEncode().ReadToEnd();
                    AssertInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 255, content: messageStream.CompressAndEncode().ReadToEnd());
                    AssertInboxMessage(eHubClientPK, "HUB", TestClientPK, "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 2, content: "H4sIAAAAAAAEAF2PvU7EMBCEX2Xl5roY2lMSyTIIRUgpLtZRIhM2iSX/RPHmcnk2Ch6JV8A5KAjVFjM7883Xx2cug+9MP0+aTPBQa4cFUy9yjhRcrKVQzwzOOMWkFuw+u2NwddbHgg1E45HzZVmyxUQkbIfehjdtszY43rQDOh35Lp6V+dMU5hHUOqaaZo2EjsEJO5zQt1tzox5r9c8ngxu1X3dGIQSDhjTNCaWqz+lFeB/o1hPLvErJv2t+XCfUcSOo/EVb8w5KCpDWoCeoHqALk9NExvdHOCQI9Zr0Q5bzLafM+S6b3+D+3N3I8htALH13VgEAAA==");
                    Assert.IsTrue(FindOutboxMessage(eHubClientPK, TestClientPK, new Guid("313B842C-2ED9-4092-87F6-20BCC957F94A"), null, null, 0, null, outboxContent));

                    using (var expectedMessageStream =
                        new MemoryStream(Encoding.UTF8.GetBytes(expectedOutboxContent.ToString().TrimEnd())))
                    {
                        var failedOutboxContent = expectedMessageStream.CompressAndEncode().ReadToEnd();
                        Assert.IsTrue(FindOutboxMessage(eHubClientPK, TestClientPK,
                            new Guid("6E0425D6-5D3E-42A2-8B74-2FF03C4520B9"), null, null, 0, null, failedOutboxContent));
                    }
                }

                AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
            }
            DeleteeHubClientRegistrations();
            DeleteeHubClientSystemRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_TWCustomsNCATK_ValidationFailed_NewClient()
        {
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                var clientLevelRegistrationType = Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE");
                var config = $@"
<Configuration Name=""TWCustomsNCATK"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"">
		<Group Type=""Company"" Reference=""ABA"">
				<Credential Name=""Current"">
					<UserName>TSTABAENT_TCA</UserName>
					<Password/>
				</Credential>
		</Group>
	</Group>
</Configuration>";

                var expectedXml = new XmlDocument();
                expectedXml.LoadXml($@"
<Configuration Name=""TWCustomsNCATK"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""TSTENT"" Status=""INV"">
		<Annotations>
			<Item Name=""StatusReason"">Invalid Client System ID.</Item>
		</Annotations>
		<Group Type=""Company"" Reference=""ABA"">
		</Group >
	</Group>
</Configuration>");

                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var expectedConfiguration = ConfigurationMessage.DeserializeFromXmlDocument(expectedXml);
                    var expectedStream = ConfigurationMessage.SerializeToStream(expectedConfiguration);

                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);

                    AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);

                    adapter.Outbox.AddMessage(message);

                    var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
                    var expectedExceptionMessage = "Invalid Client System ID.";

                    StringAssert.Contains(expectedExceptionMessage, exception.Message);

                    var outboxContent = expectedStream.CompressAndEncode().ReadToEnd();
                    var failOutboxContent = new MemoryStream(Encoding.UTF8.GetBytes(expectedExceptionMessage))
                        .CompressAndEncode().ReadToEnd();
                    AssertInboxMessage(trackingID, TestClientPK, "HUB", eHubClientPK,
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 255,
                        content: messageStream.CompressAndEncode().ReadToEnd());
                    AssertInboxMessage(eHubClientPK, "HUB", TestClientPK,
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", isFlatFile: 0, status: 2,
                        content: "H4sIAAAAAAAEAFWPQU7EMAxFr2LlAA1sUVqpBDSqkLqYRsM6FM80UuJUjUvp2VhwJK5AOsOCrryw3/vfP1/fSkc6u8s8WXaRoLUBS2Fe9Zw4htTq2rwIOOGU8rYU98WdgM/gKZViYB4fpFyWpVhcQsZ+uPj4Zn3RxyC7fsBgk9zpRaUOU5xHMOuYY7o1MQYBRzzjhNRvyZ15bo2Aji3POaRpTxmqiSJfDalSTWb+et6ujmjT5m7ow3r3Dto7JIabHZqnQsmNqZTcef430TGMltZdlfqxFiAzdT3Mc/dK9QsnjK9PPAEAAA==");
                    Assert.IsTrue(FindOutboxMessage(eHubClientPK, TestClientPK,
                        new Guid("313B842C-2ED9-4092-87F6-20BCC957F94A"), null, null, 0, null, outboxContent));
                }

                AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, TestClientID);
            }
            DeleteeHubClientRegistrations();
            DeleteeHubClientSystemRegistrations();
        }

        [Test]
        public void TestConfigurationMessageHandler_TWCustomsNCATK_Add()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE");
                var clientID = "ENTRTRSVR";
                var systemId = clientID.Substring(0, 3) + clientID.Substring(6, 3);
                var companyId = clientID.Substring(3, 3);
                var config = string.Format(
                    @"<Configuration Name='TWCustomsNCATK' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTRTRSVR_TCA</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), clientID, eHubClientID, MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClient("ENTRTRSVR", "Client", "Enterprise", true);
                    AsserteHubClient("ENTTSTSVR_TCA", "Client", "Third Party", false);

                    AsserteHubClientRegistrationExists(1, registrationType, "ENTRTRSVR");
                    AsserteHubClientRegistrationCode(registrationType, "ENTRTRSVR_TCA", "ENTRTRSVR");
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_TWCustomsNCATK_Delete()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE");
                var systemId = TestAuthenticatedClientID.Substring(0, 3) + TestAuthenticatedClientID.Substring(6, 3);
                var companyId = TestAuthenticatedClientID.Substring(3, 3);
                var config = string.Format(
                    @"<Configuration Name='TWCustomsNCATK' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTTSTSVR_TCA</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                        MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
                    AsserteHubClient("ENTTSTSVR_TCA", "Client", "Third Party", false);

                    AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                    AsserteHubClientRegistrationCode(registrationType, "ENTTSTSVR_TCA", "ENTTSTSVR");

                    config = string.Format(
                        @"<Configuration Name='TWCustomsNCATK' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName></UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                    using (var messageStream1 = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                    {
                        var message1 = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                            MessageSchemaType.Xml,
                            "HUB",
                            "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream1);
                        adapter.Outbox.AddMessage(message1);
                        adapter.SendMessages();

                        AsserteHubClientRegistrationExists(0, registrationType, "ENTTSTSVR");
                        AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
                    }
                }
            }
        }

        [Test]
        public void TestConfigurationMessageHandler_TWCustomsNCATK_Update()
        {
            using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
            {
                var registrationType = Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE");
                var systemId = TestAuthenticatedClientID.Substring(0, 3) + TestAuthenticatedClientID.Substring(6, 3);
                var companyId = TestAuthenticatedClientID.Substring(3, 3);
                var config = string.Format(
                    @"<Configuration Name='TWCustomsNCATK' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTTSTSVR_TCA</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                {
                    var message = new eHubMessage(Guid.NewGuid(), TestAuthenticatedClientID, eHubClientID,
                        MessageSchemaType.Xml,
                        "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
                    AsserteHubClient("ENTTSTSVR_TCA", "Client", "Third Party", false);

                    AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                    AsserteHubClientRegistrationCode(registrationType, "ENTTSTSVR_TCA", "ENTTSTSVR");


                    config = string.Format(
                        @"<Configuration Name='TWCustomsNCATK' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
	<Group Type='System' Reference='{0}' >
	<Group Type='Company' Reference='{1}' >
		<Credential Name='Current' >
			<UserName>ENTVFGSVR_TCA</UserName >
		</Credential>
	</Group>
	</Group>
</Configuration>", systemId, companyId);
                    using (var messageStream1 = new MemoryStream(Encoding.UTF8.GetBytes(config)))
                    {
                        var message1 = new eHubMessage(Guid.NewGuid(), TestClientID, eHubClientID,
                            MessageSchemaType.Xml,
                            "HUB",
                            "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", messageStream1);
                        adapter.Outbox.AddMessage(message1);
                        adapter.SendMessages();

                        AsserteHubClient("ENTVFGSVR_TCA", "Client", "Third Party", false);
                        AsserteHubClientRegistrationExists(1, registrationType, "ENTTSTSVR");
                        AsserteHubClientRegistrationCode(registrationType, "ENTVFGSVR_TCA", "ENTTSTSVR");
                    }
                }
            }
        }

        #endregion

        #region ConfigurationMessageHandler_JPCustoms
        [Test]
        public void TestAddNewCredential_Success()
        {
            var clientLevelRegistrationType = Guid.Parse("F637D5F5-C64C-4376-9608-24DCFA80F45D");
            var systemLevelRegistrationType = Guid.Parse("5A9EF07E-72BD-4B34-9B53-9F2D31E35F3C");

            var adapter = CreateAdapter(TestClientID, TestClientPassword);
            string xmlString = @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
    <Credential Name='Current'>
      <UserName>FDSGFDS</UserName>
      <Password>ltTUCO/XtIyHyvd5itAi02I6zR/EJE2B+ygJO/9rTyZqTjXjLccT4Ewem5ttYZXOZ/O4MkrX3xu4LuqBiu0gacig2LqLaCVdrL/jOYg0IL2jK10jobL1bCj3yNArFhId4BDqDxquk2WkmdsOe6fUcK5R6BEjlyjmAc6lkEUl7vQ=</Password>
    </Credential>
    <Group Type='Company' Reference='DAU'>
      <Credential Name='Current'>
        <UserName>PWSAU</UserName>
        <Password>uTxe+JplgNWl5HyDaTTWW0uV9+EdO5oxRyV9a4IDwETJI8a8z0WNfWmfFgjkpC8FOszZBhGrMAgqkkJgvqQ9/ErSSGYwbh385K6ijjs1lWg6687C8Zr2MecNn211p4hcJeruNzYaIDfwhwqfIgiG1EiLGxM8cYanINdvToZDPG4=</Password>
      </Credential>
    </Group>
    <Group Type='Company' Reference='CLI'>
      <Credential Name='Current'>
        <UserName>AXXXXXXX</UserName>
        <Password>ysSDWBzgfmZIx2GJNQqJnUsfTQqQPw/gz4bhRJhxd6Y56PpnrPYFddnDBtM7QxqXEeTB0VL2CjScqGVf5WXQalIu7Ak7hfMVlYGO34TXvZ2yWGysAweiHYi0m5dmpsEWGj8HXmpV1kv2Fqkpf0V2CdRfJMbwrzaOFLoXfEx8N1o=</Password>
      </Credential>
    </Group>
  </Group>
</Configuration>";
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlString)))
            {
                var systemID = GetSystemID(TestClientID);
                var trackingID = Guid.NewGuid();
                var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                adapter.Outbox.AddMessage(message);
                adapter.SendMessages();

                AssertSuccessStatusMessage(TestClientID, trackingID);
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "AXXXXXXX", "TSTCLIENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "FFFFFFF", "TSTCLIENT");
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "PWSAU", "TSTDAUENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "121212", "TSTDAUENT");
                AsserteHubClientSystemRegistrationCodeAndPassword(systemLevelRegistrationType, "FDSGFDS", "TEST1");
                adapter.RetrieveMessages();
                AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestClientID);
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestlJPCredentials_DeleteSystem()

        {
            var clientLevelRegistrationType = Guid.Parse("F637D5F5-C64C-4376-9608-24DCFA80F45D");
            var systemLevelRegistrationType = Guid.Parse("5A9EF07E-72BD-4B34-9B53-9F2D31E35F3C");
            string xmlRemoveEverything =
                @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
  </Group>
</Configuration>";
            string xmlAddString =
                @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
    <Credential Name='Current'>
      <UserName>FDSGFDS</UserName>
      <Password>ltTUCO/XtIyHyvd5itAi02I6zR/EJE2B+ygJO/9rTyZqTjXjLccT4Ewem5ttYZXOZ/O4MkrX3xu4LuqBiu0gacig2LqLaCVdrL/jOYg0IL2jK10jobL1bCj3yNArFhId4BDqDxquk2WkmdsOe6fUcK5R6BEjlyjmAc6lkEUl7vQ=</Password>
    </Credential>
    <Group Type='Company' Reference='DAU'>
      <Credential Name='Current'>
        <UserName>PWSAU</UserName>
        <Password>uTxe+JplgNWl5HyDaTTWW0uV9+EdO5oxRyV9a4IDwETJI8a8z0WNfWmfFgjkpC8FOszZBhGrMAgqkkJgvqQ9/ErSSGYwbh385K6ijjs1lWg6687C8Zr2MecNn211p4hcJeruNzYaIDfwhwqfIgiG1EiLGxM8cYanINdvToZDPG4=</Password>
      </Credential>
    </Group>
    <Group Type='Company' Reference='CLI'>
      <Credential Name='Current'>
        <UserName>AXXXXXXX</UserName>
        <Password>ysSDWBzgfmZIx2GJNQqJnUsfTQqQPw/gz4bhRJhxd6Y56PpnrPYFddnDBtM7QxqXEeTB0VL2CjScqGVf5WXQalIu7Ak7hfMVlYGO34TXvZ2yWGysAweiHYi0m5dmpsEWGj8HXmpV1kv2Fqkpf0V2CdRfJMbwrzaOFLoXfEx8N1o=</Password>
      </Credential>
    </Group>
  </Group>
</Configuration>";
            using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlAddString)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AsserteHubClientRegistrationCode(clientLevelRegistrationType, "AXXXXXXX", "TSTCLIENT");
                    AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "FFFFFFF", "TSTCLIENT");
                    AsserteHubClientRegistrationCode(clientLevelRegistrationType, "PWSAU", "TSTDAUENT");
                    AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "121212", "TSTDAUENT");
                    AsserteHubClientSystemRegistrationCodeAndPassword(systemLevelRegistrationType, "FDSGFDS", "TEST1");
                }

                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlRemoveEverything)))
                {
                    var trackingID = Guid.NewGuid();
                    var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB",
                        "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                    adapter.Outbox.AddMessage(message);
                    adapter.SendMessages();

                    AssertSuccessStatusMessage(TestClientID, trackingID);
                    AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, "TSTDAUENT");
                    AsserteHubClientRegistrationExists(0, clientLevelRegistrationType, "TSCLIENT");
                    AsserteHubClientSystemRegistrationExists(0, systemLevelRegistrationType, "FDSGFDS");
                }
            }

            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestDeleteSome_UpdateSomeCredentials_Success()
        {
            var clientLevelRegistrationType = Guid.Parse("F637D5F5-C64C-4376-9608-24DCFA80F45D");
            var systemLevelRegistrationType = Guid.Parse("5A9EF07E-72BD-4B34-9B53-9F2D31E35F3C");
            var adapter = CreateAdapter(TestClientID, TestClientPassword);

            string xmlString = @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
    <Group Type='Company' Reference='DAU'>
      <Credential Name='Current'>
        <UserName>PWSAU</UserName>
      </Credential>
    </Group>
    <Group Type='Company' Reference='CLI'>
      <Credential Name='Current'>
        <UserName>AXXXXXXX</UserName>
        <Password>ysSDWBzgfmZIx2GJNQqJnUsfTQqQPw/gz4bhRJhxd6Y56PpnrPYFddnDBtM7QxqXEeTB0VL2CjScqGVf5WXQalIu7Ak7hfMVlYGO34TXvZ2yWGysAweiHYi0m5dmpsEWGj8HXmpV1kv2Fqkpf0V2CdRfJMbwrzaOFLoXfEx8N1o=</Password>
      </Credential>
    </Group>
  </Group>
</Configuration>";
            string xmlAddString = @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
    <Credential Name='Current'>
      <UserName>FDSGFDS</UserName>
      <Password>ltTUCO/XtIyHyvd5itAi02I6zR/EJE2B+ygJO/9rTyZqTjXjLccT4Ewem5ttYZXOZ/O4MkrX3xu4LuqBiu0gacig2LqLaCVdrL/jOYg0IL2jK10jobL1bCj3yNArFhId4BDqDxquk2WkmdsOe6fUcK5R6BEjlyjmAc6lkEUl7vQ=</Password>
    </Credential>
    <Group Type='Company' Reference='DAU'>
      <Credential Name='Current'>
        <UserName>PWSAU</UserName>
        <Password>uTxe+JplgNWl5HyDaTTWW0uV9+EdO5oxRyV9a4IDwETJI8a8z0WNfWmfFgjkpC8FOszZBhGrMAgqkkJgvqQ9/ErSSGYwbh385K6ijjs1lWg6687C8Zr2MecNn211p4hcJeruNzYaIDfwhwqfIgiG1EiLGxM8cYanINdvToZDPG4=</Password>
      </Credential>
    </Group>
    <Group Type='Company' Reference='CLI'>
      <Credential Name='Current'>
        <UserName>AXXXXXXX</UserName>
        <Password>ysSDWBzgfmZIx2GJNQqJnUsfTQqQPw/gz4bhRJhxd6Y56PpnrPYFddnDBtM7QxqXEeTB0VL2CjScqGVf5WXQalIu7Ak7hfMVlYGO34TXvZ2yWGysAweiHYi0m5dmpsEWGj8HXmpV1kv2Fqkpf0V2CdRfJMbwrzaOFLoXfEx8N1o=</Password>
      </Credential>
    </Group>
  </Group>
</Configuration>";

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlAddString)))
            {
                var systemID = GetSystemID(TestClientID);
                var trackingID = Guid.NewGuid();
                var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                adapter.Outbox.AddMessage(message);
                adapter.SendMessages();

                AssertSuccessStatusMessage(TestClientID, trackingID);
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "AXXXXXXX", "TSTCLIENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "FFFFFFF", "TSTCLIENT");
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "PWSAU", "TSTDAUENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "121212", "TSTDAUENT");
                AsserteHubClientSystemRegistrationCodeAndPassword(systemLevelRegistrationType, "FDSGFDS", "TEST1");
            }

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlString)))
            {
                var trackingID = Guid.NewGuid();
                var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                adapter.Outbox.AddMessage(message);
                var currentInsertDate = DateTime.UtcNow;
                adapter.SendMessages();

                AssertSuccessStatusMessage(TestClientID, trackingID);
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "AXXXXXXX", "TSTCLIENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "FFFFFFF", "TSTCLIENT");
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "PWSAU", "TSTDAUENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "121212", "TSTDAUENT");
                AsserteHubClientSystemRegistrationExists(0, systemLevelRegistrationType, "FDSGFDS");
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestAddNewCredential_SameUserSuccess()
        {
            var clientLevelRegistrationType = Guid.Parse("F637D5F5-C64C-4376-9608-24DCFA80F45D");
            var systemLevelRegistrationType = Guid.Parse("5A9EF07E-72BD-4B34-9B53-9F2D31E35F3C");
            var adapter = CreateAdapter(TestClientID, TestClientPassword);
            string xmlString = @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
    <Credential Name='Current'>
      <UserName>FDSGFDS</UserName>
      <Password>ltTUCO/XtIyHyvd5itAi02I6zR/EJE2B+ygJO/9rTyZqTjXjLccT4Ewem5ttYZXOZ/O4MkrX3xu4LuqBiu0gacig2LqLaCVdrL/jOYg0IL2jK10jobL1bCj3yNArFhId4BDqDxquk2WkmdsOe6fUcK5R6BEjlyjmAc6lkEUl7vQ=</Password>
    </Credential>
    <Group Type='Company' Reference='DAU'>
      <Credential Name='Current'>
        <UserName>FDSGFDS</UserName>
        <Password>ltTUCO/XtIyHyvd5itAi02I6zR/EJE2B+ygJO/9rTyZqTjXjLccT4Ewem5ttYZXOZ/O4MkrX3xu4LuqBiu0gacig2LqLaCVdrL/jOYg0IL2jK10jobL1bCj3yNArFhId4BDqDxquk2WkmdsOe6fUcK5R6BEjlyjmAc6lkEUl7vQ=</Password>
      </Credential>
    </Group>
  </Group>
</Configuration>";
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlString)))
            {
                var systemID = GetSystemID(TestClientID);
                var trackingID = Guid.NewGuid();
                var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                adapter.Outbox.AddMessage(message);
                var currentInsertDate = DateTime.UtcNow;
                adapter.SendMessages();

                AssertSuccessStatusMessage(TestClientID, trackingID);
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "FDSGFDS", "TSTDAUENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "TEST1", "TSTDAUENT");
                AsserteHubClientSystemRegistrationCodeAndPassword(systemLevelRegistrationType, "FDSGFDS", "TEST1");
                adapter.RetrieveMessages();
                AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestClientID);
            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        [Test]
        public void TestAddNewCredentialFromSenderIfSystemValid()
        {
            var clientLevelRegistrationType = Guid.Parse("F637D5F5-C64C-4376-9608-24DCFA80F45D");
            var systemLevelRegistrationType = Guid.Parse("5A9EF07E-72BD-4B34-9B53-9F2D31E35F3C");
            var adapter = CreateAdapter(TestClientID, TestClientPassword);
            string xmlString = @"<Configuration xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Name='JPAFR' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'>
  <Group Type='System' Reference='TSTENT'>
    <Credential Name='Current'>
      <UserName>FDSGFDS</UserName>
      <Password>ltTUCO/XtIyHyvd5itAi02I6zR/EJE2B+ygJO/9rTyZqTjXjLccT4Ewem5ttYZXOZ/O4MkrX3xu4LuqBiu0gacig2LqLaCVdrL/jOYg0IL2jK10jobL1bCj3yNArFhId4BDqDxquk2WkmdsOe6fUcK5R6BEjlyjmAc6lkEUl7vQ=</Password>
    </Credential>
  </Group>
</Configuration>";
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlString)))
            {
                var trackingID = Guid.NewGuid();
                var message = new eHubMessage(trackingID, TestClientID, "eHub", MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
                adapter.Outbox.AddMessage(message);
                var currentInsertDate = DateTime.UtcNow;
                adapter.SendMessages();

                AssertSuccessStatusMessage(TestClientID, trackingID);
                AsserteHubClientRegistrationCode(clientLevelRegistrationType, "TSTCLIENT", "TSTCLIENT");
                AsserteHubClientRegistrationPassword(clientLevelRegistrationType, "TEST1", "TSTCLIENT");
                AsserteHubClientSystemRegistrationCodeAndPassword(systemLevelRegistrationType, "FDSGFDS", "TEST1");

            }
            DeleteeHubClientSystemRegistrations();
            DeleteeHubClientRegistrations();
        }

        #endregion

    }
}
