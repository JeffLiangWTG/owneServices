using System;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class AirMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestSuccess()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var xml = @"
				<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
					<MessageType>FHL</MessageType>
					<Priority>QK</Priority>
					<Carrier>QF</Carrier>
					<CreationDateTime>2020-03-18T04:22:06.8300000Z</CreationDateTime>
					<HAWB>S00227270</HAWB>
					<MAWB>08157865695</MAWB>
					<IssuingCarrierAgentIATACode>02-3 4330/2124</IssuingCarrierAgentIATACode>
					<Body>
		<![CDATA[FHL/4
MBI/081-57865695SYDNRT/T8K855
HBS/S00227270/SYDNRT/8/K855//RESPIRATORY DEV
/NSC
TXT/RESPIRATORY DEVICES VOL 14.790 M3
SHP/RES MED PTY LTD
/1 ELIZABETH MACARTHUR DRIVE
/BELLA VISTA/NSW
/AU/2153/TE/61288841345
CNE/FUKUDA DENSHI CO LTD  TECH SERVICE 
/TOKYO 2-23-5 KITAUENO
/TAITO-KU/13
/JP/110-0014
CVD/AUD/CC/NVD/205337.16/XXX
]]>
					</Body>
				</CargoIMP>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, TestClientID, TestAirServiceID, MessageSchemaType.Xml, ApplicationCode.CIM, "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL", stream);
				var attemptCount = 0;
				var retry = false;
				do
				{
					attemptCount++;
					try
					{
						adapter.Outbox.AddMessage(message);
						adapter.SendMessages();
					}
					catch (SqlException)
					{
						if (attemptCount > 5)
						{
							throw;
						}

						retry = true;
					}
				} while (retry);

				AssertInboxMessageNoStatus(messagePK, TestClientPK, ApplicationCode.CIM, TestAirServicePK, "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL", stream.CompressAndEncode().ReadToEnd(), 0);
			}
		}

		[Test]
		public void TestGatewayUnderMaintenance_NotFoundAirMessageProcessingService()
		{
			using (new DisposableAction(() => { DropAirMessageProcessingService(); }, () => { CreateAirMessageProcessingService(); }))
			{
				var adapter = CreateAdapter(TestClientID, TestClientPassword);
				using (var stream = new MemoryStream())
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestAirServiceID, MessageSchemaType.FlatFile, ApplicationCode.CIM, "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL", stream);
					adapter.Outbox.AddMessage(message);

					try
					{
						adapter.SendMessages();
						Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
					}
					catch (RegistrationException e)
					{
						StringAssert.Contains("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code. ExceptionID: ", e.Message);
						Assert.IsInstanceOf(typeof(FaultException<ExceptionDetail>), e.InnerException);
						Assert.IsTrue(e.InnerException.ToString().Contains("Could not find any service with the name '//cargowise.com/eServices/AirMessageProcessingService' in this database."));
					}
				}
			}
		}

		const string TestAirServiceID = "eHubAirService";
		static readonly Guid TestAirServicePK = new Guid("56c24dba-ecbf-4c4d-82ab-1352d929b06d");
		const string TestAirServicePassword = "TestPassword";
	}
}
