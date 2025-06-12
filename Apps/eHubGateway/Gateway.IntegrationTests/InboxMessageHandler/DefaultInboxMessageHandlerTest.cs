using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class DefaultInboxMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestSuccess()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(uknownMessage)))
			{
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestClientID, TestDefaultClientID, MessageSchemaType.Xml, "unknown", "http://cargowise.com/ehub/products/", messageStream.CompressAndEncode());
				adapter.Outbox.AddMessage(message);
				Assert.DoesNotThrow(() => adapter.SendMessages());

				AssertInboxMessage(trackingID, TestClientPK, "unk", TestDefaultClientPK, "http://cargowise.com/ehub/products/", "", "", messageCompressedAndEncoded, 0, 0);
			}
		}

		[Test]
		public void TestSuccess_MessageLargerThan8KB()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			var testMessage = new string('A', 10240);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(testMessage)))
			{
				var trackingID = Guid.NewGuid();
				var message = new eHubMessage(trackingID, TestClientID, TestDefaultClientID, MessageSchemaType.Xml, "unknown", "http://cargowise.com/ehub/products/", messageStream);
				adapter.Outbox.AddMessage(message);
				Assert.DoesNotThrow(() => adapter.SendMessages());

				using (var compressedStream = messageStream.CompressAndEncode())
				using (var reader = new StreamReader(compressedStream))
				{
					AssertInboxMessage(trackingID, TestClientPK, "unk", TestDefaultClientPK, "http://cargowise.com/ehub/products/", "", "", reader.ReadToEnd(), 0, 0);
				}
			}
		}

		[Test]
		public void TestFailure_TooManyMessages()
		{
			var value = GetRemoteSettings(InboxAccessorUnprocessedMessageLimit);
			Assert.NotNull(value, $"{InboxAccessorUnprocessedMessageLimit} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			int limit = 0;
			var success = Int32.TryParse(value.ToString(), out limit);
			Assert.IsTrue(success, string.Format("Could not parse {0} into int", limit));

			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(uknownMessage)))
			{
				var trackingID = Guid.NewGuid();
				var trackingIDList = new System.Collections.Generic.List<Guid>();
				for (int i = 0; i <= limit; i++)
				{
					trackingID = Guid.NewGuid();
					trackingIDList.Add(trackingID);
					var message = new eHubMessage(trackingID, TestClientID, TestDefaultClientID, MessageSchemaType.Xml, "unknown", "http://cargowise.com/ehub/products/", messageStream.CompressAndEncode());
					adapter.Outbox.AddMessage(message);
				}
				Assert.DoesNotThrow(() => adapter.SendMessages());

				trackingID = Guid.NewGuid();
				var failingMessage = new eHubMessage(trackingID, TestClientID, TestDefaultClientID, MessageSchemaType.Xml, "unknown", "http://cargowise.com/ehub/products/", messageStream.CompressAndEncode());
				adapter.Outbox.AddMessage(failingMessage);

				try
				{
					adapter.SendMessages();
					Assert.Fail("RegistrationException is expected.");
				}
				catch (RegistrationException ex)
				{
					Assert.That(ex.Message.Contains("The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. Please ignore the following message: Too Many Requests not a valid ediEnterprise licence code. ExceptionID: "));
				}
				finally
				{
					AddRollback(() =>
					{
						using (var connection = OpenEHubTransactionsConnection())
						{
							var i = 0;
							var parameters = trackingIDList.ToDictionary(x => string.Format("@trackID{0}", i++), x => x);
							var enumerator = parameters.GetEnumerator();

							var commandText = string.Format("DELETE FROM eHubInboxMessage WHERE EI_MessageTrackingID IN ({0})", string.Join(",", parameters.Keys));
							using (var command = new SqlCommand(commandText, connection))
							{
								command.CommandType = System.Data.CommandType.Text;
								command.Parameters.AddRange(trackingIDList.Select(x =>
								{
									if (enumerator.MoveNext())
									{
										return new SqlParameter(enumerator.Current.Key, enumerator.Current.Value);
									}
									return null;
								}).ToArray());
								command.ExecuteNonQuery();
							}
						}
					});
				}
			}
		}

		[Test]
		public void TestNonExistentClientID_SqlException50000()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(uknownMessage)))
			{
				var trackingID = Guid.NewGuid();
				var expectedErrorMessage = $"Recipient ID Fake could not be found. Sender ID is TSTCLIENT. MessageTrackingID is '{trackingID}'.\r\n";
				var failingMessage = new eHubMessage(trackingID, TestClientID, NonexistingClientID, MessageSchemaType.Xml, "unknown", "http://cargowise.com/ehub/products/", messageStream.CompressAndEncode());
				adapter.Outbox.AddMessage(failingMessage);

				try
				{
					adapter.SendMessages();
					Assert.Fail("eHubAdapterException is expected.");
				}
				catch (eHubAdapterException ex)
				{
					var dict = ex.GetMessageExceptionDictionary();
					Assert.AreEqual(dict.Count, 1);
					var enumerator = dict.GetEnumerator();
					if (enumerator.MoveNext())
					{
						Assert.AreEqual(trackingID, enumerator.Current.Key);
						Assert.AreEqual(expectedErrorMessage, enumerator.Current.Value);
					}
					Assert.That(ex.Message.Contains($"1 errors occured during processing send request:\r\n{expectedErrorMessage} ExceptionID: "));

				}
			}
		}

		[Test]
		public void TestInvalidTableName_SqlException208_ShouldCauseSystemUnderMaintenanceException()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			using (var action = new DisposableAction(() => RenameTable("eHubTransactions", "eHubInboxMessage", "eHubInboxMessage_"), () => RenameTable("eHubTransactions", "eHubInboxMessage_", "eHubInboxMessage")))
			{
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(uknownMessage)))
				{
					var trackingID = Guid.NewGuid();
					var failingMessage = new eHubMessage(trackingID, TestClientID, TestDefaultClientID, MessageSchemaType.Xml, "unknown", "http://cargowise.com/ehub/products/", messageStream.CompressAndEncode());
					adapter.Outbox.AddMessage(failingMessage);

					try
					{
						adapter.SendMessages();
						Assert.Fail("SystemUnderMaintananceException is expected.");
					}
					catch (eHubAdapterException ex)
					{
						Assert.That(ex.Message.Contains("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
					}
				}
			}
		}

		#region Messages

		const string uknownMessage =
@"<Unknown xmlns=""http://cargowise.com/ehub/products/"">
    <Reference>FHM773L</Reference>
    <Content>Something</Content>
</Unknown>";

		const string messageCompressedAndEncoded = "H4sIAAAAAAAEACXOwQ6CIACA4QfyECxyeKTWJmKgog67qWmZOp2UE56+Wv/5O/w+0pT8O5PwEth6Fcbsosx9Ulv3y2tQjJ36WfNQpQBpqfFqzSZgtgrr5ZGVDw46PJkez6lEMxwag5xcEsKqiUsAWfiVSSZsHak9t8WGq7aI45xC2w6HoBzcQKmjU41zkIzX5j0BJ6WdMqBoy0XfiH/fPMj83+AHjfbKyawAAAA=";

		#endregion Messages

		#region Implementation

		const string TestDefaultClientID = "Default";
		const string NonexistingClientID = "Fake";
		readonly Guid TestDefaultClientPK = Guid.Parse("5F7BFF78-DB87-4F3C-B630-9366E2EFB105");

		#endregion Implementation
	}
}
