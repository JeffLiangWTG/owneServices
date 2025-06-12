using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class ITCustomsRequestResponseMessageHandlerTest : GatewayIntegrationTestBase
	{
		const string TestRegistryUpdateID = "TSTRegistryUpdate";

		[Test]
		public void TestTriggerJobStatus_Success()
		{
			using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
			{
				var trackingID = Guid.NewGuid();
				var content = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files>
		<File>
			<Name>4UDG0927.R0A</Name>
		</File>
		<File>
			<Name>4UDG0927.R0B</Name>
		</File>
		<File/>
	</Files>
</ITCustoms>";
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
				{
					var systemID = GetSystemID(TestClientID);
					
					var message = new eHubMessage(trackingID, TestClientID, eHubClientID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse", messageStream);
					adapter.Outbox.AddMessage(message);

					var currentInsertDate = DateTime.UtcNow;
					var pollingStartUTCList = GetIT_PollingStartUTC(systemID, "4UDG0927.%0A");
					Assert.True(pollingStartUTCList.All(x => x < currentInsertDate), string.Join(", ", pollingStartUTCList));
					pollingStartUTCList = GetIT_PollingStartUTC(systemID, "4UDG0927.%0B");
					Assert.True(pollingStartUTCList.All(x => x < currentInsertDate), string.Join(", ", pollingStartUTCList));

					adapter.SendMessages();

					pollingStartUTCList = GetIT_PollingStartUTC(systemID, "4UDG0927.%0A");
					Assert.True(pollingStartUTCList.All(x => x >= currentInsertDate), "Expected >= " + currentInsertDate + ". But they was: " + string.Join(", ", pollingStartUTCList));
					pollingStartUTCList = GetIT_PollingStartUTC(systemID, "4UDG0927.%0B");
					Assert.True(pollingStartUTCList.All(x => x >= currentInsertDate), "Expected >= " + currentInsertDate + ". But they was: " + string.Join(", ", pollingStartUTCList));
				}
				adapter.RetrieveMessages();
				AssertContainsTheSucessStatusMessage(adapter.Inbox, trackingID, TestClientID);
			}
		}

		[Test]
		public void TestTriggerJobStatus_EmptyFileName_Fail()
		{
			using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
			{
				var trackingID = Guid.NewGuid();
				var content = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files></Files>
</ITCustoms>";
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
				{
					var systemID = GetSystemID(TestClientID);
					
					var message = new eHubMessage(trackingID, TestClientID, eHubClientID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse", messageStream);
					adapter.Outbox.AddMessage(message);

					var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
					StringAssert.Contains("Message does not contains FileName.\r\n ExceptionID: ", exception.Message);
				}
			}
		}

		[Test]
		public void TestTriggerJobStatus_MissingFileNameInXML_Fail()
		{
			using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
			{
				var content = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files1/>
</ITCustoms>";
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
				{
					var systemID = GetSystemID(TestClientID);
					var trackingID = Guid.NewGuid();
					var message = new eHubMessage(trackingID, TestClientID, eHubClientID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse", messageStream);
					adapter.Outbox.AddMessage(message);

					var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
					StringAssert.Contains("Message does not contains FileName.\r\n ExceptionID: ", exception.Message);
				}
			}
		}

		[Test]
		public void TestTriggerJobStatus_InvalidXML_Fail()
		{
			using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
			{
				var content = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files>
		<File>
			<Name>4UDG0927.R0A</Name>
		</File>
</ITCustomsInvalid>";
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
				{
					var systemID = GetSystemID(TestClientID);
					var trackingID = Guid.NewGuid();
					var message = new eHubMessage(trackingID, TestClientID, eHubClientID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse", messageStream);// TODO: George - update application code
					adapter.Outbox.AddMessage(message);

					var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
					StringAssert.Contains("The 'Files' start tag on line 2 position 3 does not match the end tag of 'ITCustomsInvalid'. Line 6, position 3.\r\n\r\n ExceptionID: ", exception.Message);
				}
			}
		}

		[Test]
		public void TestSqlException_Fail()
		{
			using (var adapter = CreateAdapter(TestClientID, TestClientPassword))
			{
				var content = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files>
		<File>
			<Name>4UDG0927.R0A</Name>
		</File>
	</Files>
</ITCustoms>";

				using (var action = new DisposableAction(() => RenameTable("eHubTransactions", "eHubITCustomsJobStatus", "eHubITCustomsJobStatus_"), () => RenameTable("eHubTransactions", "eHubITCustomsJobStatus_", "eHubITCustomsJobStatus")))
				{
					using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
					{
						var systemID = GetSystemID(TestClientID);
						var trackingID = Guid.NewGuid();
						var message = new eHubMessage(trackingID, TestClientID, eHubClientID, MessageSchemaType.Xml, "UnKnownApplicationCode", "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse", messageStream);// TODO: George - update application code
						adapter.Outbox.AddMessage(message);
						var currentInsertDate = DateTime.UtcNow;

						try
						{
							adapter.SendMessages();
							Assert.Fail("eHubAdapterException is expected.");
						}
						catch (eHubAdapterException ex)
						{
							AssertMessageExceptionDictionary(ex, trackingID, "Invalid object name 'dbo.eHubITCustomsJobStatus'");
						}
					}
				}
			}
		}

		List<DateTime> GetIT_PollingStartUTC(string systemID, string filePattern)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				var commandText = @"SELECT IT_PollingStartUTC FROM eHubITCustomsJobStatus WHERE IT_EH_ClientSystem IN (SELECT TOP 1 EH_PK FROM eHubClientSystem WHERE EH_ID = @SystemID) AND IT_FileName like @FileName";
				using (var command = new SqlCommand(commandText, connection))
				{
					command.CommandType = System.Data.CommandType.Text;
					command.Parameters.AddWithValue("@SystemID", systemID);
					command.Parameters.AddWithValue("@FileName", filePattern);
					using (var reader = command.ExecuteReader())
					{
						var listPollingStartUTC = new List<DateTime>();
						while (reader.Read())
						{
							listPollingStartUTC.Add(Convert.ToDateTime(reader[0]));
						}
						return listPollingStartUTC;
					}
				}
			}
		}
	}
}
