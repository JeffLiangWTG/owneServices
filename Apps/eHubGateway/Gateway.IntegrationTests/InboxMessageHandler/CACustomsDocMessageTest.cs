using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class CACustomsDocMessageTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestCACustomsMessageHandlerTest_SenderHasTestLicense_MessageInsertedIntoInboxAndOutbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), SenderIDWithTestLicence, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestClientPassword);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsTrue(FindInboxMessage(message.TrackingID, SenderPKWithTestLicence, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0, 2));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithTestLicence, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0, 2));

				Assert.IsTrue(FindOutboxMessage(message.TrackingID, SenderPKWithTestLicence, RecipientPKForTest, null, "", "", 0, null, content));
				Assert.IsFalse(FindOutboxMessage(message.TrackingID, SenderPKWithTestLicence, RecipientPKForProduction, null, "", "", 0, null, content));

			}
		}

		[Test]
		public void TestCACustomsMessageHandlerTest_SenderHasProductionLicense_MessageInsertedIntoInboxAndOutbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), SenderIDWithProductionLicence, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestAuthenticatedClientPassword);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsTrue(FindInboxMessage(message.TrackingID, SenderPKWithProductionLicense, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0, 2));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithProductionLicense, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0, 2));

				Assert.IsTrue(FindOutboxMessage(message.TrackingID, SenderPKWithProductionLicense, RecipientPKForProduction, null, "", "", 0, null, content));
				Assert.IsFalse(FindOutboxMessage(message.TrackingID, SenderPKWithTestLicence, RecipientPKForTest, null, "", "", 0, null, content));
			}
		}

		[Test]
		public void TestCACustomsMessageHandlerTest_SenderHasInvalidLicense_MessageNotInsertedIntoInboxOrOutbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), SenderIDWithInvalidLicence, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestClientPassword);
				adapter.Outbox.AddMessage(message);
				var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
				Assert.That(exception.Message.Contains("Error retrieving licence details for sender"));

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithInvalidLicence, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0, 2));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithInvalidLicence, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0, 2));

				Assert.IsFalse(FindOutboxMessage(message.TrackingID, SenderPKWithTestLicence, RecipientPKForTest, null, "", "", 0, null, content));
				Assert.IsFalse(FindOutboxMessage(message.TrackingID, SenderPKWithTestLicence, RecipientPKForTest, null, "", "", 0, null, content));
			}
		}

		[Test]
		public void TestCACustomsMessageHandlerTest_MonitoringSender_MessageInsertedIntoInboxAndOutbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), CACustomsMonitoringClientID, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestClientPassword);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsTrue(FindInboxMessage(message.TrackingID, CACustomsMonitoringClientPK, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0, 2));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, CACustomsMonitoringClientPK, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0, 2));

				Assert.IsTrue(FindOutboxMessage(message.TrackingID, CACustomsMonitoringClientPK, RecipientPKForProduction, null, "", "", 0, null, content));
				Assert.IsFalse(FindOutboxMessage(message.TrackingID, CACustomsMonitoringClientPK, RecipientPKForTest, null, "", "", 0, null, content));
			}
		}

		private const string ApplicationCode = "UDM";
		private readonly Guid RecipientPKForTest = Guid.Parse("ABB9A931-370C-4B2E-A159-BD62117F45CF"); // ID = CACustomsDocTest

		private const string RecipientIDForProduction = "CACustomsDoc";
		private readonly Guid RecipientPKForProduction = Guid.Parse("2337B2F2-CE59-40A6-BC56-D18779E21DD0");

		private const string SenderIDWithTestLicence = "ENTTSTSVZ";
		private readonly Guid SenderPKWithTestLicence = Guid.Parse("E0F40A8A-FA6B-43FE-A96E-1D731D34D1DA");

		private const string SenderIDWithProductionLicence = "ENTTSTSVR";
		private readonly Guid SenderPKWithProductionLicense = Guid.Parse("C3C7C44E-2BF3-43EB-97D7-04F2C12C70E1");

		private const string SenderIDWithInvalidLicence = "ENTTSTSVI";
		private readonly Guid SenderPKWithInvalidLicence = Guid.Parse("2C627C46-AE1F-4523-A4EA-DE75CB2EE326");

		private const string SchemaName = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalShipment";
		private const string SampleMessage = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""></UniversalShipment>";
	}
}
