using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class NZCustomsTestMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestFail_IsFailNZCustomsMessageDeliveryConfigured()
		{
			var oldSettingValue = GetRemoteSettings(FailNZCustomsMessageDeliveryKey);

			try
			{
				SetRemoteSettings(FailNZCustomsMessageDeliveryKey, "true");

				var adapter = CreateAdapter(TestClientID, TestClientPassword);

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, NZCustomsTestID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
					AssertInboxMessage(message.TrackingID, TestClientPK, ApplicationCode.NZCustoms, NZCustomsTestPK, "http://cargowise.com/ehub/products/#NZCustoms", "", "", expectedInboxMessageContent, 0, 255);
					AssertEHubError("GTW", "UKN", "Cannot be delivered as this system does not have a NZCustoms connection.", NZCustomsMessageHandlerTest.GetErrorDetails(TestClientID, NZCustomsTestID));
				}
			}
			finally
			{
				SetRemoteSettings(FailNZCustomsMessageDeliveryKey, oldSettingValue.ToString());
			}
		}

		[Test]
		public void TestFail_eHub2GatewayServiceIsNotAvailable()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, NZCustomsTestID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
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
					var innerException = e.InnerException.ToString();
					Assert.IsTrue(Regex.IsMatch(innerException, @"System\.ServiceModel\.EndpointNotFoundException: Could not connect to net\.tcp://(.*?):11809/eHub2Gateway/eHub2Gateway\.svc"));
				}
			}
		}

		const string TestClientWithProductionLicenceID = "ENTTSTSVR";

		[Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestMessage_SqlExceptiononErrorNumber50000() //PipelineComponent (IncomingMessageLoggerProductComponent) throws sqlexception 50000
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></NZCustoms>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(trackingID, TestAuthenticatedClientID, "clientIDNotExistIneHubClient", MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
					Thread.Sleep(30000); // Wait for biztalk to process the message
					Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");

				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.Contains("There was a failure executing the receive pipeline"));
					Assert.IsTrue(e.Message.Contains("Recipient ID clientIDNotExistIneHubClient could not be found. Sender ID is ENTTSTSVR."));

					var messageExceptionDictionary = e.GetMessageExceptionDictionary();
					Assert.IsNotEmpty(messageExceptionDictionary, "MessageExceptionDictionary should not be empty.");
					Assert.AreEqual(1, messageExceptionDictionary.Count);

					var messageException1 = messageExceptionDictionary[trackingID];
					Assert.IsTrue(messageException1.Contains("There was a failure executing the receive pipeline"));
					Assert.IsTrue(messageException1.Contains("Recipient ID clientIDNotExistIneHubClient could not be found. Sender ID is ENTTSTSVR."));
				}
			}
		}

		[Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void Test_MessageReferenceAlreadyExistsForDifferentClient_SqlException50000() // This test the storedProcedure ('InsertMessageReference') in pipelinecomponent with sqlException number 500000
		{
			var trackingID = Guid.NewGuid();

            var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>000100001</Reference><Content>Something</Content></NZCustoms>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
			{
                var message = new eHubMessage(trackingID, TestClientID, NZCustomsTestID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);

				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
			    var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
                AssertInboxMessage(trackingID, TestClientPK, ApplicationCode.NZCustoms, NZCustomsTestPK, "http://cargowise.com/ehub/products/", "", "", expectedInboxMessageContent, 0, 255);
			    adapter.RetrieveMessages();
			    AssertContainsFailedStatusMessage(adapter.Inbox, trackingID, TestClientID);
			}
		}

		[Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestMessage_SqlExceptiononErrorNumber8152() //String or binary data would be truncated - sqlexception 8152
		{
			var trackingID = Guid.NewGuid();
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			try
			{
				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "ALTER TABLE eHubInboxMessage ALTER COLUMN EI_Content VARCHAR(5)";
					command.ExecuteReader();
				}

				var content = @"And must be enabled in order to use objects that are implemented using CLR integration.";
				var sampleMessage = string.Format(@"<NZCustoms xmlns=""http://cargowise.com/ehub/products/"">
<Reference>FHM773L</Reference>
<Content>{0}</Content>
</NZCustoms>", content);

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(trackingID, TestClientID, NZCustomsTestID, MessageSchemaType.Xml,
						ApplicationCode.NZCustoms, "NZM", stream);
					adapter.Outbox.AddMessage(message);
					try
					{
						adapter.SendMessages();
						Thread.Sleep(30000); // Wait for biztalk to process the message
					    Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
					}
					catch (eHubAdapterException e)
					{
						Assert.IsTrue(e.Message.Contains("There was a failure executing the receive pipeline"));
						Assert.IsTrue(e.Message.Contains("String or binary data would be truncated"));

						var messageExceptionDictionary = e.GetMessageExceptionDictionary();
						Assert.IsNotEmpty(messageExceptionDictionary, "MessageExceptionDictionary should not be empty.");
						Assert.AreEqual(1, messageExceptionDictionary.Count);

						var messageException1 = messageExceptionDictionary[trackingID];
						Assert.IsTrue(messageException1.Contains("There was a failure executing the receive pipeline"));
						Assert.IsTrue(messageException1.Contains("String or binary data would be truncated"));
					}
				}

				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT COUNT(*) FROM eHubInboxMessage";
					int messagesCount = (int) command.ExecuteScalar();
					Assert.AreEqual(0, messagesCount);
				}
			}
			finally
			{
				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "ALTER TABLE eHubInboxMessage ALTER COLUMN EI_Content nvarchar(max)";
					command.ExecuteReader();
				}
			}
		}

		[Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestMessage_TransactionRollbacks_SqlTransactionFailsInInsertMessageReferenceProductComponent() // This test is for testing the atomicity between the PipelineComponents having seperate sql transactions. Expected behaviour: All the sql transaction roll backs if one of the pipelinecomponent breaks.
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></NZCustoms>";
			try
			{
				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "EXEC sp_rename 'InsertMessageReference', 'ChangeProcedureForTesting'";
					command.ExecuteScalar();

					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
					{
						var message = new eHubMessage(trackingID, TestClientID, NZCustomsTestID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);
						adapter.Outbox.AddMessage(message);
						try
						{
							adapter.SendMessages();
							Thread.Sleep(30000); // Wait for biztalk to process the message
						    Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
						}
						catch (RegistrationException e)
						{
							Assert.IsTrue(e.Message.StartsWith("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
						}
					}

					Thread.Sleep(30000); // Wait for biztalk to process the message

					command.CommandText = "SELECT COUNT(*) FROM eHubInboxMessage";
					int messagesCount = (int)command.ExecuteScalar();
					Assert.AreEqual(0, messagesCount);
				}
			}
			finally
			{
				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "EXEC sp_rename 'ChangeProcedureForTesting', 'InsertMessageReference'";
					command.ExecuteScalar();
				}
			}
		}

		[Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestMessage_ReadContextProperty_Success() //Pipeline throws long reference exception
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>This error is usually encountered when inserting a record in a table where one of the columns is a VARCHAR or CHAR data type and the length of the value being inserted is longer than the length of the column.</Reference><Content>Something</Content></NZCustoms>";

			try
			{
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(trackingID, TestClientID, NZCustomsTestID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "NZC", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					Thread.Sleep(30000); // Wait for biztalk to process the message
				    Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
                }
			}
			catch (RegistrationException e)
			{
				Assert.IsTrue(e.Message.StartsWith("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
			}
			
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT COUNT(*) FROM eHubInboxMessage";
				int messagesCount = (int)command.ExecuteScalar();
				Assert.AreEqual(1, messagesCount);
			}
		}

		[Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestMessage_TransactionRollbacks_BiztalkServerThrowsException() // This test is for testing the atomicity between Biztalk and PipelineComponents. Expected beahviour: Transaction should rollback if BZTalk throws an exception after sql transacxtions.
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></NZCustoms>";
			try
			{
				using (var btConnection = OpenBizTalkMsgBoxDbConnection())
				{
					using (var command = btConnection.CreateCommand())
					{
						command.CommandText = "EXEC sp_rename 'Spool', 'SpoolForTesting'";
						command.ExecuteNonQuery();
					}
				}

				try
				{
					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
					{
						var message = new eHubMessage(trackingID, TestClientID, NZCustomsTestID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "NZC", stream);
						adapter.Outbox.AddMessage(message);
						adapter.SendMessages();

						Thread.Sleep(30000); // Wait for biztalk to process the message
					    Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
                    }
				}
				catch (RegistrationException e)
				{
					Assert.IsTrue(e.Message.StartsWith("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
				}

				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT COUNT(*) FROM eHubInboxMessage";
					int messagesCount = (int)command.ExecuteScalar();
					Assert.AreEqual(0, messagesCount);
				}
			}
			finally
			{
				using (var btConnection = OpenBizTalkMsgBoxDbConnection())
				{
					using (var command = btConnection.CreateCommand())
					{
						command.CommandText = "EXEC sp_rename 'SpoolForTesting', 'Spool'";
						command.ExecuteNonQuery();
					}
				}
			}
		}

		const string NZCustomsTestID = "NZCustomsTest";
		readonly Guid NZCustomsTestPK = Guid.Parse("482CACC8-0096-4D01-95D7-B573BB72D023");
		const string FailNZCustomsMessageDeliveryKey = "FailNZCustomsMessageDelivery";
		const string SampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>51358596K</Reference><Type>Text</Type><Authentication /><Content>VU5BOisuPyAnVU5CK1VOT0E6Mis1MTM1ODU5Nks6WlpaK0NVU1NXVDpaWlorMTcwMTA2OjA5MzkrNDUnVU5IKzQ1K0NVU0RFQzpEOjk2QjpVTidCR00rOTI5K0IwMDAwMjgxMCs5J0NTVCsrMTA6MTA1OjE0MydMT0MrOStVU0xHQidMT0MrMTErTlpBS0wnTE9DKzQxK05aQUtMJ0RUTSsxNTE6MjAxNzAxMTI6MTAyJ0dJUytBVEY6MTEwOjE0MzoxMjM0NTYnR0lTK01DRDoxMTA6MTQzOllOTk5OJ01FQStXVCtBQUQrS0dNOjEyNTg2J0VRRCtDTitQT05VMzI1NjI1MCsrKys1J1JGRitCTTpMR0JBS0wyODEwJ1BBQyswKytQSydSRkYrQk06MDAyODEwJ1JGRitBQVE6UE9OVTMyNTYyNTAnUEFDKzgrKzA3J1REVCsyMCs1MDkrMSsrKysrOjo6Q0FQIENPUlJJRU5URVMnTkFEK0FMKzUxMzUyMzY4SjpaWlo6MTQzJ05BRCtDQis1MTM1ODU5Nks6WlpaOjE0MydVTlMrRCdETVMrMUJOWjAwOC0xKzkzNSdUT0QrKytGT0I6MTA2OjE0MydDU1QrMSs4NDMxNDEwMDAwSzoxNjk6MTQzJ0ZUWCtBQUErKytCVUNLRVRTIFNIT1ZFTFMgR1JBQlMgQU5EIEdSSVBTJ0xPQysyNytVUydMT0MrMzUrVVMnTkFEK1NVKzAwNzEwODQxWTpaWlo6MTQzJ01PQSsxNDoxMzAwMC4wMDpOWkQnQ1VYKzIrKzEuMDAnTU9BKzQwOjEzMDAwJ01PQSs2NDoxMjE0J01PQSs3MDozJ0dJUytOOjEwOToxNDMnVEFYKzErQ1VEJ01PQSsxNjE6NjUwLjAwJ1RBWCsxK0dTVCdNT0ErMTYxOjIyMzAuMDUnVU5TK1MnQ05UKzQ6MSdDTlQrNToxJ0NOVCsxMTo4J1RBWCszK0NVRCsrMTMwMDAnTU9BKzE2MTo2NTAuMDAnVEFYKzMrR1NUJ01PQSsxNjE6MjIzMC4wNSdUQVgrNCtUT1QnTU9BKzE2MToyODgwLjA1J0dJUytCOjEzNDoxNDMnQVVUK0xLTERAQkdJSkRLSEdGQEErNDAwMDYyMDZFJ1VOVCs1MCs0NSdVTlorMSs0NSc=</Content></NZCustoms>";
	}
}
