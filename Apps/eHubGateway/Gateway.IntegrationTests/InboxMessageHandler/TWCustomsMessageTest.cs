using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	class TWCustomsMessageTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestFailure_TWCustomsMessageTest_InvalidRecipientID_MessageNotInsertedIntoInbox()
		{
			var testNotExistClientID = "ENTNOTSVR";
			var invalidRecipientId = testNotExistClientID + "_TCA";
			var systemId = testNotExistClientID.Substring(0, 3) + testNotExistClientID.Substring(6, 3);
			var companyId = testNotExistClientID.Substring(3, 3);
			var config = string.Format(
				$@"<Configuration Name='TWCustomsNCATK' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
					<Group Type='System' Reference='{systemId}'>
					<Group Type='Company' Reference='{companyId}'>
						<Credential Name='Current'>
							<UserName>{testNotExistClientID}_TCA</UserName>
						</Credential>
					</Group>
					</Group>
				</Configuration>");
			using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
			{
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(config)))
				{
					AsserteHubClientExists(0, testNotExistClientID);
					AsserteHubClientRegistrationExists(0, Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE"), testNotExistClientID);
					var configMessagePK = Guid.NewGuid();
					var configMessage = new eHubMessage(configMessagePK, TestAuthenticatedClientID, eHubClientID, MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
					adapter.Outbox.AddMessage(configMessage);
					adapter.SendMessages();
					AsserteHubClientRegistrationExists(1, Guid.Parse("14B71BCE-A94C-4CC7-8D4E-B24140EB50FE"), testNotExistClientID);
					AsserteHubClientExists(1, testNotExistClientID);
				}
				var customsMessage = @"<TWCPluginServiceSendRequest xmlns='http://cargowise.com/ehub/products/TWCPluginRequest'>
											<systemId>ASXPRD</systemId>
											<companyId>TST</companyId>
											<staffCode>BRK</staffCode>
											<mailbox>CBK0339-0</mailbox>
											<messageType>ECD</messageType>
											<passwordType>TVA</passwordType>
											<entryNumber>CH100F500199</entryNumber>
											   <interchangeNum>68928408001004280001</interchangeNum>
											   <entryNumberType>EXP</entryNumberType>
											   <messageFormat>N5203</messageFormat>
											   <messageId/>
											   <messageBodyBase64></messageBodyBase64>
											<attachments/>
											<clientRegistrationId/>
											<registrationConfiguration/>
										</TWCPluginServiceSendRequest>";
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(customsMessage)))
				{
					var messagePK = Guid.NewGuid();
					var message = new eHubMessage(messagePK, testNotExistClientID, invalidRecipientId, MessageSchemaType.Xml, "TCA", "", stream);

					adapter.Outbox.AddMessage(message);
					var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
					Assert.That(exception.Message, Does.Contain(string.Format("Recipient ID {0} could not be found", invalidRecipientId)));

					AsserteHubClientExists(0, invalidRecipientId);
					using (var recipientAdapter = CreateAdapter(invalidRecipientId, TestAuthenticatedClientPassword))
                    {
						recipientAdapter.RetrieveMessages();
					}
					AsserteHubClientExists(1, invalidRecipientId);
					AssertCountOfeHubInboxMessage(1);
					AssertEmptyInboxMessage(messagePK);

					Assert.That(adapter.Outbox, Has.Exactly(1).Items, "Precondition");
					Assert.That(() => adapter.SendMessages(), Throws.Nothing);
					AssertCountOfeHubInboxMessage(2);
				}
			}
		}

		[Test]
		public void TestSuccess_ClientIDWithAuthWsExtensions()
		{
			using (var adapter = CreateAdapter(TCATestAuthenticatedClientID, TestAuthenticatedClientPassword))
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("Some content.")))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, TCATestAuthenticatedClientID, TestAuthenticatedClientID, MessageSchemaType.FlatFile, "TCA", "", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				AssertInboxMessage(messagePK, TCATestAuthenticatedClientIDPK, "TCA", TestAuthenticatedClientIDPK, "", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
			}
		}

		[Test]
		public void TestSuccess_ClientIDWithoutAuthWsExtensions()
		{
			using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("<Test>Some content</Test>")))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, TestAuthenticatedClientID, TCATestAuthenticatedClientID, MessageSchemaType.Xml, "TCA", "", stream);
				adapter.Outbox.AddMessage(message);

				adapter.SendMessages();
				AssertInboxMessage(messagePK, TestAuthenticatedClientIDPK, "TCA", TCATestAuthenticatedClientIDPK, "", "", "", stream.CompressAndEncode().ReadToEnd(), 0, 0);
			}
		}

		#region Constants

			const string TCATestAuthenticatedClientID = TestAuthenticatedClientID + "_TCA";
			static readonly Guid TCATestAuthenticatedClientIDPK = new Guid("E6B8C7AC-F7A0-4E93-83B7-7A74AEEAA1D7");
			static readonly Guid TestAuthenticatedClientIDPK = new Guid("2A33240B-1388-4CCB-B1D9-30DA10D2DB6A");

		#endregion
	}
}
