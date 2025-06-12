using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Integration;
using CargoWise.eServices.TestHelpers.Database.Common;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[TestFixture]
	public class eHubStreamedServiceTest : GatewayIntegrationTestBase
	{
		private const string ConfigurationMessageType = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration";
		private const string ConfigMsgForward = @"<Configuration Name=""SGCustomsConfiguration"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""ENTSVR"">
	<Group Type=""Company"" Reference=""DES"">
	  <Group Type=""Staff"" Reference=""PES"">
		<Group Type=""ESB"" Reference=""name"" Status=""VAL"">
		  <Credential>
			<UserName>111</UserName>
			<Password />
		  </Credential>
		</Group>
	  </Group>
	</Group>
  </Group>
</Configuration>";
		private const string ConfigMsgNotForward = @"<Configuration Name=""JPAFR"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""ENTSVR"">
	<Group Type=""Company"" Reference=""DES"">
	  <Group Type=""Staff"" Reference=""PES"">
		<Group Type=""ESB"" Reference=""name"" Status=""VAL"">
		  <Credential>
			<UserName>222</UserName>
			<Password />
		  </Credential>
		</Group>
	  </Group>
	</Group>
  </Group>
</Configuration>";

		[WithProxyGatewayService]
		[Test]
		public void TestForwardToProxyGateway()
		{
			using (var adapterTest = CreateAdapter("ENTTSTZZH", TestAuthenticatedClientPassword))
			using (var adapterProd = CreateAdapter("ENTPRDJFL", TestAuthenticatedClientPassword))
			using (var messageStream1 = new MemoryStream(Encoding.UTF8.GetBytes("<Test>1</Test>")))
			using (var messageStream2 = new MemoryStream(Encoding.UTF8.GetBytes("<Test>2</Test>")))
			using (var configMsgForwardStream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigMsgForward)))
			using (var configMsgNotForwardStream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigMsgNotForward)))
			{
				// Messages not forwarded to proxy gateway because ShouldForwardToProxyGateway is false.
				Assert.That(bool.Parse(GetRemoteSettings("ShouldForwardToProxyGateway").ToString()), Is.False);

				var message1 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream1);
				var message2 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream2);
				var message3 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgForwardStream);
				var message4 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgNotForwardStream);
				adapterTest.Outbox.AddMessage(message1);
				adapterTest.Outbox.AddMessage(message2);
				adapterTest.Outbox.AddMessage(message3);
				adapterTest.Outbox.AddMessage(message4);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(4);

				// Messages not forwarded to proxy gateway because recipient is not in code mapping.
				SetRemoteSettings("ShouldForwardToProxyGateway", "True");
				SetRemoteSettings("ProxyGatewayAddress", WithProxyGatewayServiceAttribute.Current.HttpsEndpoint.AbsoluteUri);

				message1 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "T_____ITC", MessageSchemaType.Xml, "", "", messageStream1);
				message2 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "T_____ITC", MessageSchemaType.Xml, "", "", messageStream2);
				message3 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "T_____ITC", MessageSchemaType.Xml, "", ConfigurationMessageType, new MemoryStream(Encoding.UTF8.GetBytes(ConfigMsgForward)));
				adapterTest.Outbox.AddMessage(message1);
				adapterTest.Outbox.AddMessage(message2);
				adapterTest.Outbox.AddMessage(message3);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(7);

				// Messages not forwarded to proxy gateway because sender has prod licence.
				message1 = new eHubMessage(Guid.NewGuid(), "ENTPRDJFL", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream1);
				message2 = new eHubMessage(Guid.NewGuid(), "ENTPRDJFL", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream2);
				message3 = new eHubMessage(Guid.NewGuid(), "ENTPRDJFL", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgForwardStream);
				adapterProd.Outbox.AddMessage(message1);
				adapterProd.Outbox.AddMessage(message2);
				adapterProd.Outbox.AddMessage(message3);
				adapterProd.SendMessages();

				AssertCountOfeHubInboxMessage(10);

				// Messages forwarded to proxy gateway.
				var trackingId1 = Guid.NewGuid();
				var trackingId2 = Guid.NewGuid();

				message1 = new eHubMessage(trackingId1, "ENTTSTZZH", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream1);
				message2 = new eHubMessage(trackingId2, "ENTTSTZZH", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream2);
				adapterTest.Outbox.AddMessage(message1);
				adapterTest.Outbox.AddMessage(message2);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(10);

				// Configuration message not forwarded because configuration name is not in code mapping
				message1 = new eHubMessage(Guid.NewGuid(), "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgNotForwardStream);
				adapterTest.Outbox.AddMessage(message1);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(11);

				// Configuration message forwarded to proxy gateway
				var trackingId3 = Guid.NewGuid();
				message1 = new eHubMessage(trackingId3, "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgForwardStream);
				adapterTest.Outbox.AddMessage(message1);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(11);

				using (var connection = SqlServerHelper.OpenAdminSqlConnection("eHubTransactionsProxy"))
				using (var command = connection.CreateCommand())
				{
					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____SGC'
WHERE EI_MessageTrackingID = '{trackingId1}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>1</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(1));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____SGC'
WHERE EI_MessageTrackingID = '{trackingId2}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>2</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(1));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'eHub'
WHERE EI_MessageTrackingID = '{trackingId3}' AND EI_MessageType = '{ConfigurationMessageType}' AND EI_IsFlatFile = 0 AND EI_Status = 3";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(1));
				}
			}

			DeleteeHubInboxMessage();
		}

		[WithProxyGatewayService]
		[Test]
		public void TestForwardToProxyGateway_WithDifferentRecipients_ShouldForwardOnlyValidMessages()
		{
			using (var adapterTest = CreateAdapter("ENTTSTZZH", TestAuthenticatedClientPassword))
			using (var adapterProd = CreateAdapter("ENTPRDJFL", TestAuthenticatedClientPassword))
			using (var messageStream1 = new MemoryStream(Encoding.UTF8.GetBytes("<Test>1</Test>")))
			using (var messageStream2 = new MemoryStream(Encoding.UTF8.GetBytes("<Test>2</Test>")))
			using (var configMsgForwardStream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigMsgForward)))
			using (var configMsgNotForwardStream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigMsgNotForward)))
			{

				SetRemoteSettings("ShouldForwardToProxyGateway", "True");
				SetRemoteSettings("ProxyGatewayAddress", WithProxyGatewayServiceAttribute.Current.HttpsEndpoint.AbsoluteUri);

				var trackingId1 = Guid.NewGuid();
				var trackingId2 = Guid.NewGuid();
				var trackingId3 = Guid.NewGuid();
				var trackingId4 = Guid.NewGuid();
				var trackingId5 = Guid.NewGuid();
				var trackingId6 = Guid.NewGuid();
				var trackingId7 = Guid.NewGuid();
				var trackingId8 = Guid.NewGuid();

				var message1 = new eHubMessage(trackingId1, "ENTTSTZZH", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream1);
				var message2 = new eHubMessage(trackingId2, "ENTTSTZZH", "T_____ITC", MessageSchemaType.Xml, "", "", messageStream2);
				var message3 = new eHubMessage(trackingId3, "ENTTSTZZH", "T_____CAC", MessageSchemaType.Xml, "", "", messageStream1);
				var message4 = new eHubMessage(trackingId4, "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgForwardStream);
				var message5 = new eHubMessage(trackingId5, "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgNotForwardStream);
				adapterTest.Outbox.AddMessage(message1);
				adapterTest.Outbox.AddMessage(message2);
				adapterTest.Outbox.AddMessage(message3);
				adapterTest.Outbox.AddMessage(message4);
				adapterTest.Outbox.AddMessage(message5);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(3);

				var message6 = new eHubMessage(trackingId6, "ENTTSTZZH", "T_____SGC", MessageSchemaType.Xml, "", "", messageStream1);
				var message7 = new eHubMessage(trackingId7, "ENTTSTZZH", "T_____ITC", MessageSchemaType.Xml, "", "", messageStream2);
				var message8 = new eHubMessage(trackingId8, "ENTTSTZZH", "eHub", MessageSchemaType.Xml, "", ConfigurationMessageType, configMsgNotForwardStream);
				adapterTest.Outbox.AddMessage(message6);
				adapterTest.Outbox.AddMessage(message7);
				adapterTest.Outbox.AddMessage(message8);
				adapterTest.SendMessages();

				AssertCountOfeHubInboxMessage(5);

				using (var connection = SqlServerHelper.OpenAdminSqlConnection("eHubTransactionsProxy"))
				using (var command = connection.CreateCommand())
				{
					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____SGC'
WHERE EI_MessageTrackingID = '{trackingId1}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>1</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(1));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____SGC'
WHERE EI_MessageTrackingID = '{trackingId6}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>1</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(1));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'eHub'
WHERE EI_MessageTrackingID = '{trackingId4}' AND EI_MessageType = '{ConfigurationMessageType}' AND EI_IsFlatFile = 0 AND EI_Status = 3";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(1));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'eHub'
WHERE EI_MessageTrackingID = '{trackingId5}' AND EI_MessageType = '{ConfigurationMessageType}' AND EI_IsFlatFile = 0 AND EI_Status = 3";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(0));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____ITC'
WHERE EI_MessageTrackingID = '{trackingId2}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>2</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(0));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____ITC'
WHERE EI_MessageTrackingID = '{trackingId7}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>2</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(0));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'T_____CAC'
WHERE EI_MessageTrackingID = '{trackingId3}' AND EI_MessageType = '' AND EI_IsFlatFile = 0 AND EI_Status = 0 AND dbo.DecodeAndDecompress(EI_Content) = '<Test>1</Test>'";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(0));

					command.CommandText = $@"SELECT COUNT(*) FROM eHubInboxMessage
JOIN eHubClient sender ON EI_CC_Sender = sender.CC_PK AND sender.CC_ID = 'ENTTSTZZH'
JOIN eHubClient recipient ON EI_CC_Recipient = recipient.CC_PK AND recipient.CC_ID = 'eHub'
WHERE EI_MessageTrackingID = '{trackingId8}' AND EI_MessageType = '{ConfigurationMessageType}' AND EI_IsFlatFile = 0 AND EI_Status = 3";

					Assert.That((int)command.ExecuteScalar(), Is.EqualTo(0));
				}
			}
			DeleteeHubInboxMessage();
		}

		[Test]
		public void TestGatewayUnderMaintenance_NoConnectionToeHub2GatewayService()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var stream = new MemoryStream())
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, "NZCustomsTest", MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
					Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
				}
				catch (RegistrationException e)
				{
					Assert.That(e.Message.Contains("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code. ExceptionID: "));
					Assert.IsInstanceOf(typeof(FaultException<ExceptionDetail>), e.InnerException);
					Assert.IsTrue(e.InnerException.ToString().Contains("System.ServiceModel.EndpointNotFoundException: Could not connect to net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"));
				}
			}
		}

		[Test]
		public void TestUseAuthenticationWebServiceValidateClientSystem()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			var xml = @"";
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				AsserteHubClient("ENTTSTSVR");
				var message = new eHubMessage(Guid.NewGuid(), "ENTTSTSVR", "LittleFinger", MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/Stormborn", messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				AsserteHubClient("ENTTSTSVR", "Client", "Enterprise", true);
			}
		}

		[Test]
		public void TestSendMessages_ClientSystemRecordAddedForANewSystem()
		{
			// In this test new senderId exists in AuthenticationWebService, ediProd, and ediProdCache Test data but not in eHubTransaction.
			const string senderId = "SVZTTTSV5";
			const string senderPassword = "password";

			var adapter = CreateAdapter(senderId, senderPassword);
			string xml = @"<ns0:UniversalShipment version=""1.1"" xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:Shipment>
  </ns0:Shipment>
</ns0:UniversalShipment>";


			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				var systemID = GetSystemID(senderId);
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, senderId, TestClientID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/Schemas/Native", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentInsertDate = DateTime.UtcNow;
				adapter.SendMessages();

				AddRollback(() => DeleteSuccessStatusMessage(senderId, trackingID));
				AddRollback(() => DeleteeHubClientSystem(systemID));
				AssertInsertEHubClientSystem(systemID);
			}
		}

		void AssertInsertEHubClientSystem(string systemID)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				var commandText = @"SELECT TOP 1 * FROM eHubClientSystem where EH_ID = @SystemID";
				using (var command = new SqlCommand(commandText, connection))
				{
					command.CommandType = System.Data.CommandType.Text;
					command.Parameters.AddWithValue("@SystemID", systemID);
					var reader = command.ExecuteReader();
					Assert.IsTrue(reader.HasRows);
				}
			}
		}

		[Test]
		public void TestSendMessages_SenderIDNotExistIneHubClient_ThrowMessageSecurityException()
		{
			var senderIDNotExistIneHubClient = "clientIDNotExistIneHubClient";
			var adapter = CreateAdapter(senderIDNotExistIneHubClient, "Password");
			using (var stream = new MemoryStream())
			{
				var message = new eHubMessage(Guid.NewGuid(), senderIDNotExistIneHubClient, TestClientID, MessageSchemaType.Xml, "UNK", "Unknown", stream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
					Assert.Fail("MessageSecurityException is expected.");
				}
				catch (MessageSecurityException e)
				{
					Assert.IsTrue(e.InnerException.Message.Contains("ClientID or Password invalid"));
				}
			}
		}

		[Test]
		public void TestSendMessages_RecipientIDNotExistIneHubClient_SqlException50000_ThrowEHubAdapterExceptionContainingSqlExceptionMessage()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var stream = new MemoryStream())
			{
				var messageTrackingID = Guid.NewGuid();
				var expectedErrorMessage = $"Recipient ID clientIDNotExistIneHubClient could not be found. Sender ID is TSTCLIENT. MessageTrackingID is '{messageTrackingID}'.\r\n";
				var message = new eHubMessage(messageTrackingID, TestClientID, "clientIDNotExistIneHubClient", MessageSchemaType.Xml, "UNK", "Unknown", stream);
				adapter.Outbox.AddMessage(message);
				var e = Assert.Throws<eHubAdapterException>(() =>
				{
					adapter.SendMessages();
				});
				Assert.That(e.Message.Contains($"1 errors occured during processing send request:\r\n{expectedErrorMessage} ExceptionID: "));
				var exceptionDictionary = e.GetMessageExceptionDictionary();
				Assert.That(exceptionDictionary.Count, Is.EqualTo(1));
				Assert.AreEqual(exceptionDictionary[messageTrackingID], expectedErrorMessage);
			}
		}

		[Test]
		public void TestSendMessages_MandatoryFieldMissing_NullRecipientID_SqlException50000_ThrowEHubAdapterExceptionContainingSqlExceptionMessage()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var stream = new MemoryStream())
			{
				var messageTrackingID = Guid.NewGuid();
				var expectedErrorMessage = $"Recipient ID  could not be found. Sender ID is TSTCLIENT. MessageTrackingID is '{messageTrackingID}'.\r\n";
				var message = new eHubMessage(messageTrackingID, TestClientID, null, MessageSchemaType.Xml, "", "", stream);
				adapter.Outbox.AddMessage(message);
				var e = Assert.Throws<eHubAdapterException>(() =>
				{
					adapter.SendMessages();
				});
				Assert.That(e.Message.Contains($"1 errors occured during processing send request:\r\n{expectedErrorMessage} ExceptionID: "));
				var exceptionDictionary = e.GetMessageExceptionDictionary();
				Assert.That(exceptionDictionary.Count, Is.EqualTo(1));
				Assert.AreEqual(exceptionDictionary[messageTrackingID], expectedErrorMessage);
			}
		}

		[Test]
		public void TestSendMessages_ApplicationCodeExceedMaxLengthOfColumn_ThrowNoException()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var stream = new MemoryStream())
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestClientID, MessageSchemaType.Xml, "abcd", "", stream); //Max length of application code is 3.
				adapter.Outbox.AddMessage(message);

				try
				{
					adapter.SendMessages(); //Stored Procedure [dbo].[InsertInbox] truncates overlength parameter
				}
				catch (Exception ex)
				{
					Assert.Fail("No exception expected, but got: " + ex);
				}
			}
		}

		[Test]
		public void TestDatabaseOffline_SqlException4060_ShouldCauseSystemUnderMaintenanceExceptionContainingSqlExceptionMessage()
		{
			using (new DisposableAction(() => { SetEHubTransactionsOffline(); }, () => { SetEHubTransactionsOnline(); }))
			{
				var adapter = CreateAdapter(TestClientID, TestClientPassword);
				using (var stream = new MemoryStream())
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestClientID, MessageSchemaType.Xml, "UNK", "Unknown", stream);
					adapter.Outbox.AddMessage(message);
					var ex = Assert.Throws<MessageSecurityException>(() =>
					{
						adapter.SendMessages();
					});
					var innerMessage = ex.InnerException.Message;
					Assert.That(innerMessage, Does.StartWith("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
					Assert.That(innerMessage, Does.Contain("Cannot open database \"eHubTransactions\" requested by the login. The login failed."));
				}
			}
		}

		[Test]
		public void TestGatewayEndpoint_IsCompatibleWithNetcore_eHubAdapter()
		{
			var binFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Assert.That(binFolder != null, nameof(binFolder) + " != null");
			var netCoreTestClientPath = Path.Combine(binFolder, @"net6.0\Adapter.TestClient.exe");

			var inf = new ProcessStartInfo();
			inf.Arguments = $"adapter {this.GatewayHttpsUri} {TestAuthenticatedClientID} {TestAuthenticatedClientPassword}";
			inf.CreateNoWindow = true;
			inf.RedirectStandardOutput = true;
			inf.UseShellExecute = false;
			inf.RedirectStandardError = true;
			inf.FileName = netCoreTestClientPath;

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			adapter.SendMessages();

			var process = Process.Start(inf);
			process.WaitForExit(60000);

			var strOutput = process.StandardOutput.ReadToEnd();
			var strError = process.StandardError.ReadToEnd();

			var strMessageExtraInfo = $"\nOutput:\n{strOutput}\nError:\n{strError}";

			Assert.That(process.HasExited, "Request did not finish in the specified time." + strMessageExtraInfo);
			Assert.That(process.ExitCode == 0, "Failed to send message." + strMessageExtraInfo);
			Assert.That(strError.Length == 0, "Client application exited with errors." + strMessageExtraInfo);
			Assert.That(strOutput.Contains("Command executed successfully using adapter client."), "Client application did not work as expected." + strMessageExtraInfo);
		}

		[Test]
		public void TestGatewayEndpoint_IsCompatibleWithNetcore_AdapterAsync()
		{
			var binFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Assert.That(binFolder != null, nameof(binFolder) + " != null");
			var netCoreTestClientPath = Path.Combine(binFolder, @"net6.0\Adapter.TestClient.exe");

			var inf = new ProcessStartInfo();
			inf.Arguments = $"async {this.GatewayHttpsUri} {TestAuthenticatedClientID} {TestAuthenticatedClientPassword}";
			inf.CreateNoWindow = true;
			inf.RedirectStandardOutput = true;
			inf.UseShellExecute = false;
			inf.RedirectStandardError = true;
			inf.FileName = netCoreTestClientPath;

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			adapter.SendMessages();

			var process = Process.Start(inf);
			process.WaitForExit(60000);

			var strOutput = process.StandardOutput.ReadToEnd();
			var strError = process.StandardError.ReadToEnd();

			var strMessageExtraInfo = $"\nOutput:\n{strOutput}\nError:\n{strError}";

			Assert.That(process.HasExited, "Request did not finish in the specified time." + strMessageExtraInfo);
			Assert.That(process.ExitCode == 0, "Failed to send message." + strMessageExtraInfo);
			Assert.That(strError.Length == 0, "Client application exited with errors." + strMessageExtraInfo);
			Assert.That(strOutput.Contains("Command executed successfully using async client."), "Client application did not work as expected." + strMessageExtraInfo);
		}
	}
}
