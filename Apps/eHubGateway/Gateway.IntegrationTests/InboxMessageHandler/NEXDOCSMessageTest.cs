using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
    [TestFixture]
	public class NEXDOCSMessageTest : GatewayIntegrationTestBase
    {
        [Test]
        public void TestSuccess_SenderHasProductionLicence()
        {
            var adapter = CreateAdapter(ProdLicencedSenderID, TestAuthenticatedClientPassword);
            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(NEXDOCSMessage)))
            {
                var messagePK = Guid.NewGuid();
                var message = new eHubMessage(messagePK, ProdLicencedSenderID, NEXDOCSID, MessageSchemaType.Xml, "ANY", "", messageStream);
                adapter.Outbox.AddMessage(message);
                adapter.SendMessages();

                var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
				AssertInboxMessage(messagePK, ProdLicencedSenderPK, "ANY", NEXDOCSPK, "", "", "", content, 0, 0);
            }
        }

        [Test]
        public void TestSuccess_SenderHasNoProductionLicence()
        {
            var adapter = CreateAdapter(TestLicencedSenderID, TestLicencedSenderPassword);
            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(NEXDOCSMessage)))
            {
                var messagePK = Guid.NewGuid();
                var message = new eHubMessage(messagePK, TestLicencedSenderID, NEXDOCSID, MessageSchemaType.Xml, "ANY", "", messageStream);
                adapter.Outbox.AddMessage(message);
                adapter.SendMessages();

                var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
                AssertInboxMessage(messagePK, TestLicencedSenderPK, "ANY", NEXDOCSTestPK, "", "", "", content, 0, 0);
            }
        }

        [Test]
        public void TestFailure_SenderHasNoLicence()
        {
            var adapter = CreateAdapter(NotLicencedSenderID, NotLicencedSenderPassword);
            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(NEXDOCSMessage)))
            {
                var messagePK = Guid.NewGuid();
                var message = new eHubMessage(messagePK, NotLicencedSenderID, NEXDOCSID, MessageSchemaType.Xml, "ANY", "", messageStream);
                adapter.Outbox.AddMessage(message);
                try
                {
                    adapter.SendMessages();
                    Assert.Fail("Should have thrown 'Error retrieving licence details for sender.'");
                }
                catch (eHubAdapterException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("Error retrieving licence details for sender."));
                }
            }
        }

		#region Messages

		string NEXDOCSMessage = @"<ns0:UniversalShipment version=""1.1"" xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:Shipment>
  </ns0:Shipment>
</ns0:UniversalShipment>";
        
        #endregion

        const string NEXDOCSID = "NEXDOCS";
		static readonly Guid NEXDOCSPK = Guid.Parse("AFF4C52A-4628-4749-8BDA-338AEAF72917");

        const string NEXDOCSTestID = "NEXDOCSTest";
		static readonly Guid NEXDOCSTestPK = Guid.Parse("5B9F3F8E-A84B-4B98-AA2D-243F964D14D1");

        const string ProdLicencedSenderID = "ENTTSTSVR";
        const string ProdLicencedSenderPassword = "TSTPASSWORD";
        static readonly Guid ProdLicencedSenderPK = Guid.Parse("2A33240B-1388-4CCB-B1D9-30DA10D2DB6A");

        const string TestLicencedSenderID = "ENTTSTSVZ";
        const string TestLicencedSenderPassword = "TSTPASSWORD";
        static readonly Guid TestLicencedSenderPK = Guid.Parse("F1AD7276-54FB-4A78-8FFC-88C1E66CA57C");

        const string NotLicencedSenderID = "TestClientNoLicence";
        const string NotLicencedSenderPassword = "TSTPASSWORD";
        static readonly Guid NotLicencedSenderPK = Guid.Parse("AD73D667-F825-4C34-8A96-EA64866EDA63");
    }
}
