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
    public class ITCustomsMessageTest : GatewayIntegrationTestBase
    {
	    [Test]
        public void TestSuccess_SenderHasProductionLicence()
        {
            var adapter = CreateAdapter(ProdLicencedSenderID, TestAuthenticatedClientPassword);
            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(ITCustomsMessage)))
            {
                var messagePK = Guid.NewGuid();
                var message = new eHubMessage(messagePK, ProdLicencedSenderID, ITCustomsID, MessageSchemaType.Xml, "ITC", "", messageStream);
                adapter.Outbox.AddMessage(message);
                adapter.SendMessages();

                var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
				AssertInboxMessage(messagePK, ProdLicencedSenderPK, "ITC", ITCustomsPK, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalShipment", "", "", content, 0, 2);
            }
        }

        [Test]
        public void TestSuccess_SenderHasNoProductionLicence()
        {
            var adapter = CreateAdapter(TestLicencedSenderID, TestLicencedSenderPassword);
            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(ITCustomsMessage)))
            {
                var messagePK = Guid.NewGuid();
                var message = new eHubMessage(messagePK, TestLicencedSenderID, ITCustomsID, MessageSchemaType.Xml, "ITC", "", messageStream);
                adapter.Outbox.AddMessage(message);
                adapter.SendMessages();

                var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
				AssertInboxMessage(messagePK, TestLicencedSenderPK, "ITC", ITCustomsTestPK, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalShipment", "", "", content, 0, 2);
            }
        }

        [Test]
        public void TestFailure_SenderHasNoLicence()
        {
            var adapter = CreateAdapter(NotLicencedSenderID, NotLicencedSenderPassword);
            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(ITCustomsMessage)))
            {
                var messagePK = Guid.NewGuid();
                var message = new eHubMessage(messagePK, NotLicencedSenderID, ITCustomsID, MessageSchemaType.Xml, "ITC", "", messageStream);
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

        [Test]
	    public void TestSuccess_MonitoringSender()
	    {
		    var adapter = CreateAdapter(ITCustomsMonitoringClientID, TestLicencedSenderPassword);
		    using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(ITCustomsMessage)))
		    {
			    var messagePK = Guid.NewGuid();
			    var message = new eHubMessage(messagePK, ITCustomsMonitoringClientID, ITCustomsID, MessageSchemaType.Xml, "ITC", "", messageStream);
			    adapter.Outbox.AddMessage(message);
			    adapter.SendMessages();

			    var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
				AssertInboxMessage(messagePK, ITCustomsMonitoringClientPK, "ITC", ITCustomsPK, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalShipment", "", "", content, 0, 2);
		    }
	    }

		#region Messages

		string ITCustomsMessage = @"<ns0:UniversalShipment version=""1.1"" xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:Shipment>
  </ns0:Shipment>
</ns0:UniversalShipment>";
        
        #endregion

        const string ITCustomsID = "ITCustoms";
        static readonly Guid ITCustomsPK = Guid.Parse("94A41885-6D19-433B-877C-DBF5B89CB918");

        const string ITCustomsTestID = "ITCustomsTest";
        static readonly Guid ITCustomsTestPK = Guid.Parse("6C61C164-E104-48A9-9F44-6EA60AD48071");

        const string ProdLicencedSenderID = "ENTTSTSVR";
        const string ProdLicencedSenderPassword = "TSTPASSWORD";
        static readonly Guid ProdLicencedSenderPK = Guid.Parse("2A33240B-1388-4CCB-B1D9-30DA10D2DB6A");

        const string TestLicencedSenderID = "ENTTSTSVZ";
        const string TestLicencedSenderPassword = "TSTPASSWORD";
        static readonly Guid TestLicencedSenderPK = Guid.Parse("F1AD7276-54FB-4A78-8FFC-88C1E66CA57C");

        const string NotLicencedSenderID = "TestClientNoLicence";
        const string NotLicencedSenderPassword = "TSTPASSWORD";
        static readonly Guid NotLicencedSenderPK = Guid.Parse("AD73D667-F825-4C34-8A96-EA64866EDA63");

		private static readonly Guid ITCustomsMonitoringClientPK = new Guid("F4B040AA-CCFA-47B5-AAD0-AF7F3150BDD9");
		private const string ITCustomsMonitoringClientID = "T_____ITC";
	}
}
