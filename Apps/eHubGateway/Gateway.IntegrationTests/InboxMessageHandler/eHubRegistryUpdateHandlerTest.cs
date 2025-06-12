using System;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class eHubRegistryUpdateHandlerTests : GatewayIntegrationTestBase
	{
		const string TestRegistryUpdateID = "TSTRegistryUpdate";

		[Test]
		public void TestInsertEHubClientSystem_Success()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var url = "http://test.url/nudge";
			var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", url);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var systemID = GetSystemID(TestClientID);
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentInsertDate = DateTime.UtcNow;
				DeleteClientSystem(systemID);
				adapter.SendMessages();

				AddRollback(() => DeleteSuccessStatusMessage(TestClientID, trackingID));
				AssertUpdatedEHubClientSystem(systemID, url, currentInsertDate, currentInsertDate);
				AssertSuccessStatusMessage(TestClientID, trackingID);

				adapter.RetrieveMessages();
				AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestClientID);
			}
		}

		[Test]
		public void TestUpdateEHubClientSystem_Success()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			var url = "http://test.url/nudge";
			var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", url);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var systemID = GetSystemID(TestAuthenticatedClientID);
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestAuthenticatedClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentUpdateDate = DateTime.UtcNow;
				adapter.SendMessages();

				AddRollback(() => DeleteSuccessStatusMessage(TestAuthenticatedClientID, trackingID));
				AssertUpdatedEHubClientSystem(systemID, url, DateTime.ParseExact("2016-11-03T00:31:57", "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture), currentUpdateDate);
				AssertSuccessStatusMessage(TestAuthenticatedClientID, trackingID);

				adapter.RetrieveMessages();
				AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestAuthenticatedClientID);
			}
		}

		[Test]
		public void TestUpdateEHubClientSystem_EmptyURL_Success()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL> </EHINudgeURL></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate");

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var systemID = GetSystemID(TestAuthenticatedClientID);
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestAuthenticatedClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentUpdateDate = DateTime.UtcNow;
				adapter.SendMessages();

				AddRollback(() => DeleteSuccessStatusMessage(TestAuthenticatedClientID, trackingID));
				AssertUpdatedEHubClientSystem(systemID, "", DateTime.ParseExact("2016-11-03T00:31:57", "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture), currentUpdateDate);
				AssertSuccessStatusMessage(TestAuthenticatedClientID, trackingID);

				adapter.RetrieveMessages();
				AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestAuthenticatedClientID);
			}
		}

		[Test]
		public void TestUpdateEHubClientSystem_TrimmingURL_Success()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			var url = "  http://test.url/nudge  \t\r\n";
			var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", url);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var systemID = GetSystemID(TestAuthenticatedClientID);
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestAuthenticatedClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentUpdateDate = DateTime.UtcNow;
				adapter.SendMessages();

				AddRollback(() => DeleteSuccessStatusMessage(TestAuthenticatedClientID, trackingID));
				AssertUpdatedEHubClientSystem(systemID, url.Trim(), DateTime.ParseExact("2016-11-03T00:31:57", "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture), currentUpdateDate);
				AssertSuccessStatusMessage(TestAuthenticatedClientID, trackingID);

				adapter.RetrieveMessages();
				AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestAuthenticatedClientID);
			}
		}

		[Test]
		public void TestTableNameChanged_SqlException208_ShouldCauseSystemUnderMaintenanceException_Fail()
		{
			using (var action = new DisposableAction(() => RenameTable("eHubTransactions", "eHubClientSystem", "_eHubClientSystem"), () => RenameTable("eHubTransactions", "_eHubClientSystem", "eHubClientSystem")))
			{
				var adapter = CreateAdapter(TestClientID, TestClientPassword);
				var url = "http://test.url/nudge";
				var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", url);

				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
				{
					var trackingID = Guid.NewGuid();
					var message = new eHubMessage(trackingID, TestClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
					adapter.Outbox.AddMessage(message);
					var currentInsertDate = DateTime.UtcNow;
					try
					{
						adapter.SendMessages();
						Assert.Fail("RegistrationException is expected");
					}
					catch (RegistrationException e)
					{
						Assert.That(e.Message.Contains("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code. ExceptionID: "));
						Assert.IsInstanceOf(typeof(FaultException<ExceptionDetail>), e.InnerException); 
						StringAssert.Contains("System.Data.SqlClient.SqlException: Invalid object name 'dbo.eHubClientSystem'.", e.InnerException.ToString());
					}
				}
			}
		}

		[Test]
		public void TestUrlExceedsMaxLength_SqlException8152_Fail()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var url = new string('a', 600);
			var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", url);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentInsertDate = DateTime.UtcNow;
				try
				{
					adapter.SendMessages();
					Assert.Fail("eHubAdapterException is expected");
				}
				catch (eHubAdapterException ex)
				{
					AssertMessageExceptionDictionary(ex, trackingID, @"String or binary data would be truncated.
The statement has been terminated.");
					Assert.That(ex.Message.Contains($"The statement has been terminated.\r\n ExceptionID:"));
				}
			}
		}

		[Test]
		public void TestExceptionDictionary_InvalidXML_Fail()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var url = new String('a', 600);
			var content = "ABCD";

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentInsertDate = DateTime.UtcNow;
				try
				{
					adapter.SendMessages();
					Assert.Fail("eHubAdapterException is expected");
				}
				catch (eHubAdapterException ex)
				{
					AssertMessageExceptionDictionary(ex, trackingID, "Data at the root level is invalid. Line 1, position 1.");
					Assert.That(ex.Message.Contains($"Data at the root level is invalid. Line 1, position 1.\r\n ExceptionID:"));
				}
			}
		}

		[Test]
		public void TestExceptionDictionary_MissingEHINudgeURLNode_Fail()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var url = new String('a', 600);
			var content = string.Format(@"<eHubRegistryUpdate xmlns=""{0}""></eHubRegistryUpdate>", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate");

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestClientID, TestRegistryUpdateID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate", messageStream);
				adapter.Outbox.AddMessage(message);
				var currentInsertDate = DateTime.UtcNow;
				try
				{
					adapter.SendMessages();
					Assert.Fail("eHubAdapterException is expected");
				}
				catch (eHubAdapterException ex)
				{
					AssertMessageExceptionDictionary(ex, trackingID, "Message does not contains EHI nudge url.");
					Assert.That(ex.Message.Contains($"Message does not contains EHI nudge url.\r\n ExceptionID:"));
				}
			}
		}

		void AssertUpdatedEHubClientSystem(string systemID, string url, DateTime insertUTC, DateTime updateUTC)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				var commandText = @"SELECT TOP 1 * FROM eHubClientSystem WHERE EH_ID = @SystemID AND EH_URL = @URL AND EH_InsertUTC >= @InsertUTC AND EH_LastUpdateUTC >= @UpdateUTC";
				using (var command = new SqlCommand(commandText, connection))
				{
					command.CommandType = System.Data.CommandType.Text;
					command.Parameters.AddWithValue("@SystemID", systemID);
					command.Parameters.AddWithValue("@URL", url);
					command.Parameters.AddWithValue("@InsertUTC", insertUTC);
					command.Parameters.AddWithValue("@UpdateUTC", updateUTC);
					var reader = command.ExecuteReader();
					Assert.IsTrue(reader.HasRows);
				}
			}
		}

		void DeleteClientSystem(string systemID)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				var commandText = @"DELETE eHubClientSystem WHERE EH_ID = @SystemID";
				using (var command = new SqlCommand(commandText, connection))
				{
					command.CommandType = System.Data.CommandType.Text;
					command.Parameters.AddWithValue("@SystemID", systemID);
					var rowEffected = command.ExecuteNonQuery();
					Assert.IsTrue(rowEffected > 0);
				}
			}
		}
	}
}
