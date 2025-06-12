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
	class CNCustomsMessageTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestFaulure_CNCustomsMessageTest_InvalidRecipientID_MessageNotInsertedIntoInbox()
		{
			var testNotExistClientID = "ENTNOTSVR";
			var invalidRecipientId = testNotExistClientID + "_CSW";
			var systemId = testNotExistClientID.Substring(0, 3) + testNotExistClientID.Substring(6, 3);
			var companyId = testNotExistClientID.Substring(3, 3);
			var config = string.Format(
				$@"<Configuration Name='CNCustomsSW' Version='1.0' xmlns='http://www.wisetechglobal.com/Schemas/Configuration'> 
					<Group Type='System' Reference='{systemId}'>
					<Group Type='Company' Reference='{companyId}'>
						<Credential Name='Current'>
							<UserName>{testNotExistClientID}</UserName>
						</Credential>
					</Group>
					</Group>
				</Configuration>");
			using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
			{
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(config)))
				{
					AsserteHubClientExists(0, testNotExistClientID);
					AsserteHubClientRegistrationExists(0, Guid.Parse("D21A3460-9BC4-45A2-9731-A05F2CD0EBA1"), testNotExistClientID);
					var configMessagePK = Guid.NewGuid();
					var configMessage = new eHubMessage(configMessagePK, TestAuthenticatedClientID, eHubClientID, MessageSchemaType.Xml, "HUB", "http://www.wisetechglobal.com/Schemas/Configuration#Configuration", stream);
					adapter.Outbox.AddMessage(configMessage);
					adapter.SendMessages();
					AsserteHubClientRegistrationExists(1, Guid.Parse("D21A3460-9BC4-45A2-9731-A05F2CD0EBA1"), testNotExistClientID);
					AsserteHubClientExists(1, testNotExistClientID);
				}
				var customsMessage = @"<UniversalInterchange xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:ns0='http://cargowise.com/ehub/core/2011/02' version='1.1'>
						<Header>
							<SenderID>ASXCHNPRD</SenderID>
							<RecipientID>CNCustoms</RecipientID>
						</Header>
						<Body>
							<UniversalShipment version='1.1'>
								<Shipment>
								</Shipment>
							</UniversalShipment>
						</Body>
					</UniversalInterchange>";
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(customsMessage)))
				{
					var messagePK = Guid.NewGuid();
					var message = new eHubMessage(messagePK, testNotExistClientID, invalidRecipientId, MessageSchemaType.Xml, "CSW", "", stream);

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
			using (var adapter = CreateAdapter(CWSTestAuthenticatedClientID, TestAuthenticatedClientPassword))
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("Some content.")))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, CWSTestAuthenticatedClientID, TestAuthenticatedClientID, MessageSchemaType.FlatFile, "CSW", "", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				AssertInboxMessage(messagePK, CSWTestAuthenticatedClientIDPK, "CSW", TestAuthenticatedClientIDPK, "", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
			}
		}

		[Test]
		public void TestSuccess_ClientIDWithoutAuthWsExtensions()
		{
			using (var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword))
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("<Test>Some content</Test>")))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, TestAuthenticatedClientID, CWSTestAuthenticatedClientID, MessageSchemaType.Xml, "CSW", "", stream);
				adapter.Outbox.AddMessage(message);

				adapter.SendMessages();
				AssertInboxMessage(messagePK, TestAuthenticatedClientIDPK, "CSW", CSWTestAuthenticatedClientIDPK, "", "", "", stream.CompressAndEncode().ReadToEnd(), 0, 0);
			}
		}

		#region Constants

		const string CWSTestAuthenticatedClientID = TestAuthenticatedClientID + "_CSW";
		static readonly Guid CSWTestAuthenticatedClientIDPK = new Guid("E6B8C7AC-F7A0-4E93-83B7-7A74AEEAA1D7");
		static readonly Guid TestAuthenticatedClientIDPK = new Guid("2A33240B-1388-4CCB-B1D9-30DA10D2DB6A");

		#endregion
	}
}
