using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Threading;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	[Explicit("Requires BizTalk server setup")]
	public class AUCustomsMessageHandlerTest : GatewayIntegrationTestBase
	{
		// BizTalk setup Required:
		// 1.  Create eHub2Gateway receive port. Refer to prod bindings and thub receive port GUI
		// 2.  Open C:\Program Files (x86)\Microsoft BizTalk Server 2010\BTSNTSvc64.exe.config and C:\Program Files (x86)\Microsoft BizTalk Server 2010\BTSNTSvc.exe.config​
		//     1.  In configuration/configSections insert <section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
		//     2.  In configuration insert:
		//         <entityFramework>
		//             <providers>
		//                 <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL" />
		//             </providers>
		//         </entityFramework>
		//     2.  In configuration insert:
		//         <connectionStrings>
		//             <clear/>
		//             <add name="CargoWise.eHub.DataAccess.Sql.eHubTransactions" connectionString="Data Source=localhost\ehubtest;Initial Catalog=eHubTransactions;User ID=eHubBizTalk;Password=Py_Xu0DWRpLi" />
		//             <add name="eHubTransactionsContext" connectionString="Data Source=localhost\ehubtest;Initial Catalog=eHubTransactions;User ID=eHubBizTalk;Password=Py_Xu0DWRpLi" providerName="System.Data.SqlClient" />
		//         </connectionStrings>
		//     3.  In configuration insert:
		//        <appSettings>
		//            <add key="CargoWise.eHub.DataAccess.InboxAccessor" value="CargoWise.eHub.DataAccess.Sql.InboxAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.SecirutyAccessor" value="CargoWise.eHub.DataAccess.Sql.SecurityAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.OutboxAccessor" value="CargoWise.eHub.DataAccess.Sql.OutboxAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.ExceptionsAccessor" value="CargoWise.eHub.DataAccess.Sql.ExceptionsAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.PartyAccessor" value="CargoWise.eHub.DataAccess.Cache.PartyAccessor, CargoWise.eHub.DataAccess.Cache, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.TransformAccessor" value="CargoWise.eHub.DataAccess.Cache.TransformAccessor, CargoWise.eHub.DataAccess.Cache, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.RegistryAccessor" value="CargoWise.eHub.DataAccess.Cache.RegistryAccessor, CargoWise.eHub.DataAccess.Cache, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.SubscriptionAccessor" value="CargoWise.eHub.DataAccess.Cache.SubscriptionAccessor, CargoWise.eHub.DataAccess.Cache, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//        
		//            <add key="CargoWise.eHub.DataAccess.Cache.PartyAccessor" value="CargoWise.eHub.DataAccess.Sql.PartyAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.Cache.TransformAccessor" value="CargoWise.eHub.DataAccess.Sql.TransformAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.Cache.RegistryAccessor" value="CargoWise.eHub.DataAccess.Sql.RegistryAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//            <add key="CargoWise.eHub.DataAccess.Cache.SubscriptionAccessor" value="CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor, CargoWise.eHub.DataAccess.Sql, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=4f570df270576350"/>
		//        </appSettings>
		[Test]
		public void TestSuccessResendMessage()
		{
			var trackingID = Guid.NewGuid();
			var authenticatedClientID = TestAuthenticatedClientID;

			var adapter = CreateAdapter(authenticatedClientID, TestAuthenticatedClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></AUCustoms>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(trackingID, TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "CMR", stream);
				
				// Send the same message twice
				adapter.Outbox.AddMessage(message);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
			}

			Thread.Sleep(30000); // Wait for biztalk to process the message

			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}'", trackingID);
				int messagesCount = (int)command.ExecuteScalar();
				Assert.AreEqual(2, messagesCount);
			}
		}
		[Test]
		public void TesteHub2GatewayServiceIsNotAvailable()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></AUCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "CMR", stream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
					Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
				}
				catch (RegistrationException e)
				{
					Assert.AreEqual("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code.", e.Message);
					Assert.IsInstanceOf(typeof(FaultException<ExceptionDetail>), e.InnerException);
					Assert.IsTrue(e.InnerException.ToString().Contains("System.ServiceModel.EndpointNotFoundException: Could not connect to net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"));
				}
			}
		}

		[Test]
		public void TestMessage_TransactionRollbacks_SqlTransactionFailsInInsertMessageReferenceProductComponent() // This test is for testing the atomicity between the PipelineComponents having seperate sql transactions. Expected behaviour: All the sql transaction roll backs if one of the pipelinecomponent breaks.
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></AUCustoms>";
			try
			{
				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "EXEC sp_rename 'InsertMessageReference', 'ChangeProcedureForTesting'";
					command.ExecuteScalar();

					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
					{
						var message = new eHubMessage(trackingID, TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "http://cargowise.com/ehub/products/", stream);
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

					command.CommandText = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}'", trackingID);
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
		public void Test_MessageReferenceAlreadyExistsForDifferentClient_SqlException50000() // This test the storedProcedure ('InsertMessageReference') in pipelinecomponent with sqlException number 50000
		{
			var trackingID = Guid.NewGuid();
			var authenticatedClientID = TestAuthenticatedClientID;

			var adapter = CreateAdapter(authenticatedClientID, TestAuthenticatedClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>AAL364P</Reference><Content>Something</Content></AUCustoms>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(trackingID, TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "http://cargowise.com/ehub/products/", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
				AssertInboxMessage(trackingID, TestAuthenticatedClientPK, ApplicationCode.AUCustoms, TestAUCustomsPK, "http://cargowise.com/ehub/products/", "", "", expectedInboxMessageContent, 0, 255);
				adapter.RetrieveMessages();
				AssertContainsFailedStatusMessage(adapter.Inbox, trackingID, TestAuthenticatedClientID);
			}
		}

		[Test]
		public void TestMessage_SqlExceptiononErrorNumber50000() //PipelineComponent (IncomingMessageLoggerProductComponent) throws sqlexception 50000
		{
			var trackingID = Guid.NewGuid();
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></AUCustoms>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(trackingID, TestClientID, "clientIDNotExistIneHubClient", MessageSchemaType.Xml, ApplicationCode.AUCustoms, "http://cargowise.com/ehub/products/", stream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
					Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.Contains("There was a failure executing the receive pipeline"));
					Assert.IsTrue(e.Message.Contains("Recipient ID clientIDNotExistIneHubClient could not be found. Sender ID is TSTCLIENT."));

					var messageExceptionDictionary = e.GetMessageExceptionDictionary();
					Assert.IsNotEmpty(messageExceptionDictionary, "MessageExceptionDictionary should not be empty.");
					Assert.AreEqual(1, messageExceptionDictionary.Count);

					var messageException1 = messageExceptionDictionary[trackingID];
					Assert.IsTrue(messageException1.Contains("There was a failure executing the receive pipeline"));
					Assert.IsTrue(messageException1.Contains("Recipient ID clientIDNotExistIneHubClient could not be found. Sender ID is TSTCLIENT."));
				}
			}

			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}'", trackingID);
				int messagesCount = (int)command.ExecuteScalar();
				Assert.AreEqual(0, messagesCount);
			}
		}

		[Test]
		public void TestMessage_SqlExceptiononErrorNumber8152() //String or binary data would be truncated - sqlexception 8152
		{
			var trackingID = Guid.NewGuid();
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			try
			{
				using (var connection = OpenEHubTransactionsConnection())
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "DELETE FROM eHubInboxMessage; ALTER TABLE eHubInboxMessage ALTER COLUMN EI_Content VARCHAR(5)";
					command.ExecuteReader();
				}

				var content = @"And must be enabled in order to use objects that are implemented using CLR integration.";
				var sampleMessage = string.Format(@"<AUCustoms xmlns=""http://cargowise.com/ehub/products/"">
<Reference>FHM773L</Reference>
<Content>{0}</Content>
</AUCustoms>", content);

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(trackingID, TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "http://cargowise.com/ehub/products/", stream);
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
					command.CommandText = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}'", trackingID);
					int messagesCount = (int)command.ExecuteScalar();
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
		public void TestMessage_ReadContextProperty_Success() //Pipeline throws long reference exception
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>This error is usually encountered when inserting a record in a table where one of the columns is a VARCHAR or CHAR data type and the length of the value being inserted is longer than the length of the column.</Reference><Content>Something</Content></AUCustoms>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(trackingID, TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "CMR", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				Thread.Sleep(30000); // Wait for biztalk to process the message
			}

			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}'", trackingID);
				int messagesCount = (int)command.ExecuteScalar();
				Assert.AreEqual(1, messagesCount);
			}
		}

		// WARNING! If interrupted this unittest will leave biztalk in a broken state. To fix the issue run: USE BizTalkMsgBoxDb; EXEC sp_rename 'SpoolForTesting', 'Spool'

		[Test]
		public void TestMessage_TransactionRollbacks_BiztalkServerThrowsException() // This test is for testing the atomicity between Biztalk and PipelineComponents. Expected beahviour: Transaction should rollback if BZTalk throws an exception after sql transacxtions.
		{
			var trackingID = Guid.NewGuid();

			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>FHM773L</Reference><Content>Something</Content></AUCustoms>";
			try
			{
				using (var btConnection = OpenBizTalkMsgBoxDbConnection())
				using (var command = btConnection.CreateCommand())
				{
					command.CommandText = "EXEC sp_rename 'Spool', 'SpoolForTesting'";
					command.ExecuteNonQuery();
				}

				try
				{
					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
					{
						var message = new eHubMessage(trackingID, TestClientID, TestAUCustomsID, MessageSchemaType.Xml, ApplicationCode.AUCustoms, "CMR", stream);
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
					command.CommandText = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}'", trackingID);
					int messagesCount = (int)command.ExecuteScalar();
					Assert.AreEqual(0, messagesCount);
				}
			}
			finally
			{
				using (var btConnection = OpenBizTalkMsgBoxDbConnection())
				using (var command = btConnection.CreateCommand())
				{
					command.CommandText = "EXEC sp_rename 'SpoolForTesting', 'Spool'";
					command.ExecuteNonQuery();
				}
			}
		}

		const string TestAUCustomsID = "AUCustoms";
		readonly Guid TestAUCustomsPK = Guid.Parse("EF0EBEAA-23FC-4309-94AC-A9D450B6305A");
	}
}
