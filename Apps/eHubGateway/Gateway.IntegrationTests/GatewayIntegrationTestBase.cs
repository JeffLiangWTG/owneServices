using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Adapter;
using CargoWise.eServices.TestHelpers.Database.Common;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[WithGatewayService]
	public abstract class GatewayIntegrationTestBase : GenericTestBase
	{
		private static int maxRetries = 4;
		public override void SetUpCore()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			var endPoint = ConfigurationManager.AppSettings["CargoWise.eHub.Gateway.HealthCheck.Http.Endpoint"];

			for (int count = 1; count <= maxRetries; count++)
			{
				try
				{
					var request = HttpWebRequest.Create(endPoint);
					request.Credentials = CredentialCache.DefaultCredentials;

					using (var response = (HttpWebResponse)request.GetResponse())
					{
						Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Wrong status for health check");

						using (var responseStream = response.GetResponseStream())
						{
							using (var reader = new StreamReader(responseStream))
							{
								Assert.AreEqual("INFO(GatewayWebService): Service is alive.", reader.ReadToEnd());
							}
						}
					}
					break;
				}
				catch (Exception) when (count < maxRetries - 1)
				{
					Task.Delay(TimeSpan.FromSeconds(5)).Wait();
				}
			}
		}

		#region Implementation

		protected void SetEHubTransactionsOffline()
		{
			using (var connection = OpenMasterConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"ALTER DATABASE [eHubTransactions] SET OFFLINE WITH ROLLBACK IMMEDIATE";
				command.ExecuteNonQuery();
			}
		}

		protected void SetEHubTransactionsOnline()
		{
			using (var connection = OpenMasterConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"ALTER DATABASE [eHubTransactions] SET ONLINE";
				command.ExecuteNonQuery();
			}

			System.Threading.Thread.Sleep(5000); //Wait for database starting up.
		}

		protected void RenameTable(string databaseName, string oldName, string newName)
		{
			using (var connection = OpenMasterConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format(@"USE {0}; EXEC sp_rename '{1}', '{2}';", databaseName, oldName, newName);
				command.ExecuteNonQuery();
			}
		}

		protected void DropAirMessageProcessingService()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"
IF EXISTS (SELECT * FROM sys.services WHERE NAME = '//cargowise.com/eServices/AirMessageProcessingService')
BEGIN
	DROP SERVICE [//cargowise.com/eServices/AirMessageProcessingService]
END";
				command.ExecuteNonQuery();
			}
		}

		protected void CreateAirMessageProcessingService()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"
CREATE SERVICE [//cargowise.com/eServices/AirMessageProcessingService]
AUTHORIZATION [dbo]
ON QUEUE [dbo].[AirMessageQueue]
([//cargowise.com/eServices/HandleAirMessageContract]);";
				command.ExecuteNonQuery();
			}
		}

		protected void DropUSCustomseHubOutboxServiceTest()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"
IF EXISTS (SELECT * FROM sys.services WHERE NAME = '//cargowise.com/eServices/USCustoms/eHubOutboxServiceTest')
BEGIN
	DROP SERVICE [//cargowise.com/eServices/USCustoms/eHubOutboxServiceTest]
END";
				command.ExecuteNonQuery();
			}
		}

		protected void CreateUSCustomseHubOutboxServiceTest()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"
CREATE SERVICE [//cargowise.com/eServices/USCustoms/eHubOutboxServiceTest]
AUTHORIZATION [dbo]
ON QUEUE [dbo].[USCustomseHubOutboxQueueTest];";
				command.ExecuteNonQuery();
			}
		}

		protected void AsserteHubClientRegistrationCode(Guid registrationPK, string code, string client)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select CX_Code from eHubClientRegistration where CX_RT = '{registrationPK}' and CX_CC = (select CC_PK from eHubClient where CC_ID = '{client}')";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Assert.AreEqual(code, Convert.ToString(reader[0]));
						}
					}
				}
			}
		}

		protected void AsserteHubClientRegistrationPassword(Guid registrationPK, string password, string client)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select CX_Password1 from eHubClientRegistration where CX_RT = '{registrationPK}' and CX_CC = (select CC_PK from eHubClient where CC_ID = '{client}')";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Assert.AreEqual(password, Convert.ToString(reader[0]));
						}
					}
				}
			}
		}

		protected void AsserteHubClientRegistrationExists(int count, Guid registrationPK, string client)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select count(*) from eHubClientRegistration where CX_RT = '{registrationPK}' and CX_CC = (select CC_PK from eHubClient where CC_ID = '{client}')";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualCount = Convert.ToInt32(reader[0]);
							Assert.AreEqual(count, actualCount);
						}
					}
				}
			}
		}

		protected void AsserteHubClientSystemRegistrationExists(int count, Guid registrationPK, string client)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select count(*) from eHubClientSystemRegistration where CD_RT = '{registrationPK}' and CD_Code = '{client}' and CD_Flag1 = 0";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualCount = Convert.ToInt32(reader[0]);
							Assert.AreEqual(count, actualCount);
						}
					}
				}
			}
		}

		protected void AsserteHubClientSystemRegistrationCodeAndPassword(Guid registrationPK, string code, string password)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select CD_Code, CD_Attr1 from eHubClientSystemRegistration where CD_RT = '{registrationPK}' and CD_Code = '{code}' and CD_Attr1 = '{password}'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Assert.AreEqual(code, Convert.ToString(reader[0]));
							Assert.AreEqual(password, Convert.ToString(reader[1]));
						}
					}
				}
			}
		}

		protected void AsserteHubClientSystemRegistrationExistsBySystemId(int count, Guid registrationPK, string systemId)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select count(*) from eHubClientSystemRegistration where CD_RT = '{registrationPK}' and CD_Flag1 = 0 and CD_EH = (select EH_PK from eHubClientSystem where EH_ID = '{systemId}') ";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualCount = Convert.ToInt32(reader[0]);
							Assert.AreEqual(count, actualCount);
						}
					}
				}
			}
		}

		protected void AsserteHubCertificateExists(int count, string staffID)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select count(*) from ehubCertificate where CE_ID = '{staffID}'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualCount = Convert.ToInt32(reader[0]);
							Assert.AreEqual(count, actualCount);
						}
					}
				}
			}
		}

		protected void AsserteHubCertificateFile(int count, string file)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select count(*) from ehubCertificate where CE_Password = '{file}'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualCount = Convert.ToInt32(reader[0]);
							Assert.AreEqual(count, actualCount);
						}
					}
				}
			}
		}

		protected void AssertEHubClientRegistrationConfigXml(string expected, Guid registrationType, string clientId, string qualifier)
		{
			var sqlCommand = $"select CX_ConfigXml from ehubClientRegistration where CX_RT = '{registrationType}' and CX_Qualifier = '{qualifier}' and CX_CC = (select CC_PK from ehubClient where CC_ID = '{clientId}')";
			AssertConfigXml(sqlCommand, expected, "CX_ConfigXml");
		}

		protected void AssertEHubClientSystemRegistrationConfigXml(string expected, Guid registrationType, string systemId)
		{
			var sqlCommand = $"select CD_ConfigXml from ehubClientSystemRegistration where CD_RT = '{registrationType}' and CD_EH = (select EH_PK from eHubClientSystem where EH_ID = '{systemId}')";
			AssertConfigXml(sqlCommand, expected, "CD_ConfigXml");
		}

		private void AssertConfigXml(string sqlCommand, string expected, string sqlColumnName)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = sqlCommand;
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();
							AssertXmlAreEqual(expected, reader[sqlColumnName].ToString());
						}
						else
						{
							Assert.Fail("There is no matching record in db.");
						}
					}
				}
			}
		}

		protected void AsserteHubClientExists(int count, string client)
		{
			using (var connection = OpenEHubTransactionsConnection())
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $"select count(*) from eHubClient where CC_ID = '{client}'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualCount = Convert.ToInt32(reader[0]);
							Assert.AreEqual(count, actualCount);
						}
					}
				}
			}
		}

		protected void AssertCountOfeHubClientDialogue(int count)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"select count(*) from eHubClientDialogue";
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualCount = Convert.ToInt32(reader[0]);
						Assert.AreEqual(count, actualCount);
					}
				}
			}
		}

		protected void DeleteeHubClientDialogue()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"DELETE FROM eHubClientDialogue";
				command.ExecuteNonQuery();
			}
		}

		protected void AssertCountOfeHubInboxMessage(int count)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"select count(*) from eHubInboxMessage";
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualCount = Convert.ToInt32(reader[0]);
						Assert.AreEqual(count, actualCount);
					}
				}
			}
		}

		protected void DeleteeHubInboxMessage()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"DELETE FROM eHubInboxMessage";
				command.ExecuteNonQuery();
			}
		}

		protected void AssertInboxMessage(Guid sender, string applicationCode, Guid? recipient = null, string messageType = null, string emailSubjectOverride = "", string fileNameOverride = "", string content = null, int isFlatFile = 1, int status = 0)
		{
			Assert.IsTrue(FindInboxMessage(sender, applicationCode, recipient, messageType, emailSubjectOverride, fileNameOverride, content, isFlatFile, status), "Inbox message not found");
		}

		protected void AssertInboxMessage(Guid messageTrackingID, Guid sender, string applicationCode, Guid? recipient = null, string messageType = null, string emailSubjectOverride = "", string fileNameOverride = "", string content = null, int isFlatFile = 1, int status = 0)
		{
			Assert.IsTrue(FindInboxMessage(messageTrackingID, sender, applicationCode, recipient, messageType, emailSubjectOverride, fileNameOverride, content, isFlatFile, status), "Inbox message not found");
		}

		protected void AssertInboxMessageNoStatus(Guid messageTrackingID, Guid sender, string applicationCode, Guid? recipient = null, string messageType = null, string content = null, int isFlatFile = 1)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				var commandString = String.Format("SELECT COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '{0}' AND EI_CC_Sender = '{1}' AND EI_CC_Recipient = '{2}' AND EI_MessageType = '{3}' AND EI_ApplicationCode = '{4}' AND EI_IsFlatFile = {5} AND EI_Content = '{6}'", messageTrackingID, sender, recipient, messageType, applicationCode, isFlatFile, content);
				command.CommandText = commandString;
				Assert.IsTrue((int)command.ExecuteScalar() > 0, "Inbox message not found");
			}
		}

		protected void AssertEmptyInboxMessage(Guid trackingID)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT  COUNT(*) FROM eHubInboxMessage WHERE EI_MessageTrackingID = '" + trackingID + "'";
				var messageCount = (int)command.ExecuteScalar();
				Assert.IsTrue(messageCount == 0);
			}
		}

		protected bool FindInboxMessage(Guid sender, string applicationCode, Guid? recipient = null, string messageType = null, string emailSubjectOverride = "", string fileNameOverride = "", string content = null, int isFlatFile = 1, int status = 0)
		{
			var findMessage = false;
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT COUNT(*) FROM eHubInboxMessage WHERE " + InboxMessageSQLFilter(sender, recipient, messageType, applicationCode, isFlatFile, status, emailSubjectOverride, fileNameOverride, content);
				if ((int)command.ExecuteScalar() > 0)
				{
					findMessage = true;
				}
			}

			return findMessage;
		}

		protected bool FindInboxMessage(Guid messageTrackingID, Guid sender, string applicationCode, Guid? recipient = null, string messageType = null, string emailSubjectOverride = "", string fileNameOverride = "", string content = null, int isFlatFile = 1, int status = 0)
		{
			var findMessage = false;
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT COUNT(*) FROM eHubInboxMessage WHERE " + InboxMessageSQLFilter(messageTrackingID, sender, recipient, messageType, applicationCode, isFlatFile, status, emailSubjectOverride, fileNameOverride, content);
				if ((int)command.ExecuteScalar() > 0)
				{
					findMessage = true;
				}
			}

			return findMessage;
		}

		protected bool FindOutboxMessage(Guid sender, Guid? recipient = null, Guid? messageType = null, string emailSubjectOverride = "", string fileNameOverride = "", int status = 0, string xmlContent = null, string rawContent = null)
		{
			var findMessage = false;
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				var query = string.Format("SELECT COUNT(*) FROM eHubOutboxMessage WHERE {0}", OutboxMessageSQLFilter(sender, recipient, messageType, status, emailSubjectOverride, fileNameOverride, rawContent));
				command.CommandText = AddXmlVariableToQuery(xmlContent, "OI_XmlContent", query);
				if ((int)command.ExecuteScalar() > 0)
				{
					findMessage = true;
				}
			}
			return findMessage;
		}

		protected bool FindOutboxMessage(Guid messageTrackingID, Guid sender, Guid? recipient = null, Guid? messageType = null, string emailSubjectOverride = "", string fileNameOverride = "", int status = 0, string xmlContent = null, string rawContent = null)
		{
			var findMessage = false;
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				var query = string.Format("SELECT COUNT(*) FROM eHubOutboxMessage WHERE {0}", OutboxMessageSQLFilter(messageTrackingID, sender, recipient, messageType, status, emailSubjectOverride, fileNameOverride, rawContent));
				command.CommandText = AddXmlVariableToQuery(xmlContent, "OI_XmlContent", query);
				if ((int)command.ExecuteScalar() > 0)
				{
					findMessage = true;
				}
			}
			return findMessage;
		}

		protected void AssertEHubError(string source, string errorType, string description, string errorDetail)
		{
			var found = false;
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT COUNT(*) FROM eHubError WHERE " + EHubErrorSQLFilter(source, errorType, description, errorDetail);
				if ((int)command.ExecuteScalar() > 0)
				{
					found = true;
				}

				Assert.IsTrue(found, "Could not found '{0}' in {1}", EHubErrorSQLFilter(source, errorType, description, errorDetail),
					GetResultSets(command, "SELECT * FROM eHubError").GetXml());
			}
		}

		DataSet GetResultSets(SqlCommand command, string selectTableStatements)
		{
			command.CommandType = CommandType.Text;
			command.CommandText = selectTableStatements;
			return GetResultSets(command);
		}

		DataSet GetResultSets(SqlCommand command)
		{
			var ds = new DataSet();
			using (var adapter = new SqlDataAdapter(command))
			{
				adapter.Fill(ds);
			}
			return ds;
		}

		protected void AssertNoExceptionThrown(Action delegateCall)
		{
			try
			{
				delegateCall();
			}
			catch (Exception ex)
			{
				Assert.Fail("No exception is expected at this point but {0} was thrown.\r\n{1}", ex.Source, ex);
			}
		}

		private string InboxMessageSQLFilter(Guid sender, Guid? recipient, string schemaName, string applicationCode, int isFlatFile, int status, string emailSubjectOverride, string fileNameOverride, string content)
		{
			var filter = string.Format("EI_CC_Sender {0} AND EI_CC_Recipient {1} AND EI_MessageType {2} AND EI_ApplicationCode {3} AND EI_IsFlatFile {4} AND EI_Status {5} AND EI_EmailSubjectOverride {6} AND EI_FileNameOverride {7} AND EI_Content {8}",
											GetNullableSQLFilter(sender),
											GetNullableSQLFilter(recipient),
											GetNullableSQLFilter(schemaName),
											GetNullableSQLFilter(applicationCode),
											GetNullableSQLFilter(isFlatFile),
											GetNullableSQLFilter(status),
											GetNullableSQLFilter(emailSubjectOverride),
											GetNullableSQLFilter(fileNameOverride),
											GetNullableSQLFilter(content));

			return filter;
		}

		private string InboxMessageSQLFilter(Guid messageTrackingID, Guid sender, Guid? recipient, string schemaName, string applicationCode, int isFlatFile, int status, string emailSubjectOverride, string fileNameOverride, string content)
		{
			var filter = string.Format("EI_MessageTrackingID {0} AND EI_CC_Sender {1} AND EI_CC_Recipient {2} AND EI_MessageType {3} AND EI_ApplicationCode {4} AND EI_IsFlatFile {5} AND EI_Status {6} AND EI_EmailSubjectOverride {7} AND EI_FileNameOverride {8} AND EI_Content {9}",
											GetNullableSQLFilter(messageTrackingID),
											GetNullableSQLFilter(sender),
											GetNullableSQLFilter(recipient),
											GetNullableSQLFilter(schemaName),
											GetNullableSQLFilter(applicationCode),
											GetNullableSQLFilter(isFlatFile),
											GetNullableSQLFilter(status),
											GetNullableSQLFilter(emailSubjectOverride),
											GetNullableSQLFilter(fileNameOverride),
											GetNullableSQLFilter(content));

			return filter;
		}

		private string EHubErrorSQLFilter(string source, string errorType, string description, string errorDetail)
		{
			var filter = string.Format("EE_Source {0} AND EE_ErrorType {1} AND EE_Description {2} AND CAST(EE_ErrorDetail as varchar(max)) {3}",
											GetNullableSQLFilter(source),
											GetNullableSQLFilter(errorType),
											GetNullableSQLFilter(description),
											GetNullableSQLFilter(errorDetail));

			return filter;
		}

		protected void AssertInboxXmlContent(Guid messageTrackingID, Guid? messageType = null, string xmlContent = null)
		{
			var findMessage = false;
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				var query = string.Format("SELECT COUNT(*) FROM eHubInboxXmlContent INNER JOIN eHubInboxMessage ON EX_EI_Inbox = EI_PK WHERE EI_MessageTrackingID {0} AND EX_DT_Source {1}", GetNullableSQLFilter(messageTrackingID), GetNullableSQLFilter(messageType));
				command.CommandText = AddXmlVariableToQuery(xmlContent, "EX_XmlContent", query);
				if ((int)command.ExecuteScalar() > 0)
				{
					findMessage = true;
				}

				Assert.IsTrue(findMessage);
			}
		}

		private string OutboxMessageSQLFilter(Guid sender, Guid? recipient, Guid? schemaName, int status, string emailSubjectOverride, string fileNameOverride, string content)
		{
			var filter = string.Format("OI_CC_Sender {0} AND OI_CC_Recipient {1} AND OI_DT_Target {2} AND OI_Status {3} AND OI_OverrideEmailSubject {4} AND OI_OverrideFileName {5} AND OI_Content {6}",
																			GetNullableSQLFilter(sender),
																			GetNullableSQLFilter(recipient),
																			GetNullableSQLFilter(schemaName),
																			GetNullableSQLFilter(status),
																			GetNullableSQLFilter(emailSubjectOverride),
																			GetNullableSQLFilter(fileNameOverride),
																			GetNullableSQLFilter(content));

			return filter;
		}

		private string OutboxMessageSQLFilter(Guid messageTrackingID, Guid sender, Guid? recipient, Guid? schemaName, int status, string emailSubjectOverride, string fileNameOverride, string content)
		{
			var filter = string.Format("OI_MessageTrackingID {0} AND OI_CC_Sender {1} AND OI_CC_Recipient {2} AND OI_DT_Target {3} AND OI_Status {4} AND OI_OverrideEmailSubject {5} AND OI_OverrideFileName {6} AND OI_Content {7}",
																			GetNullableSQLFilter(messageTrackingID),
																			GetNullableSQLFilter(sender),
																			GetNullableSQLFilter(recipient),
																			GetNullableSQLFilter(schemaName),
																			GetNullableSQLFilter(status),
																			GetNullableSQLFilter(emailSubjectOverride),
																			GetNullableSQLFilter(fileNameOverride),
																			GetNullableSQLFilter(content));

			return filter;
		}

		private string AddXmlVariableToQuery(string xmlContent, string column, string query)
		{
			if (!string.IsNullOrWhiteSpace(xmlContent))
			{
				query = string.Format("DECLARE @xml XML = '{0}'; {1} AND CAST({2} as varchar(max)) = CAST(@xml as varchar(max))", xmlContent, query, column);
			}
			return query;
		}

		protected string GetNullableSQLFilter(object column)
		{
			return column == null ? "IS NULL" : "= '" + column.ToString() + "'";
		}

		protected void DeleteeHubClientSystem(string systemID)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"
DELETE FROM eHubClientSystem
WHERE EH_ID = @systemID";
				command.Parameters.AddWithValue("@systemID", systemID);
				command.ExecuteNonQuery();
			}
		}

		protected void AssertSuccessStatusMessage(string recipientID, Guid trackingID)
		{
			AssertStatusMessage(recipientID, trackingID, "MessageStatusSuccess");
		}

		protected void AssertFailedStatusMessage(string recipientID, Guid trackingID)
		{
			AssertStatusMessage(recipientID, trackingID, "MessageStatusFailed");
		}

		private void AssertStatusMessage(string recipientID, Guid trackingID, string statusMessage)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"select count(*) from eHubOutboxMessage
where
OI_MessageTrackingID = @trackingID
and OI_CC_Recipient = (select CC_PK from eHubClient where CC_ID = @clientID)
and OI_DT_Target = (select DT_PK from eHubMessageType where DT_Code = @statusMessage)";
				command.Parameters.AddWithValue("@clientID", recipientID);
				command.Parameters.AddWithValue("@trackingID", trackingID);
				command.Parameters.AddWithValue("@statusMessage", statusMessage);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualCount = Convert.ToInt32(reader[0]);
						Assert.AreEqual(1, actualCount);
					}
				}
			}
		}

		protected void AsserteHubClient(string clientID, string ownerCategory = null, string systemCategory = null, bool? requireStatusResponse = null)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"Select CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse from eHubClient where CC_ID = @clientID";
				command.Parameters.AddWithValue("@clientID", clientID);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (ownerCategory == null && systemCategory == null)
						{
							Assert.IsTrue(reader.HasRows);
						}
						else
						{
							Assert.IsTrue(Convert.ToString(reader["CC_OwnerCategory"]) == ownerCategory && Convert.ToString(reader["CC_SystemCategory"]) == systemCategory && Convert.ToBoolean(reader["CC_RequireStatusResponse"]) == requireStatusResponse.Value);
						}
					}
				}
			}
		}

		protected void DeleteSuccessStatusMessage(string recipientID, Guid trackingID)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"
DELETE FROM eHubOutboxMessage
WHERE OI_MessageTrackingID = @trackingID and OI_CC_Recipient = (select CC_PK from eHubClient where CC_ID = @clientID)";
				command.Parameters.AddWithValue("@clientID", recipientID);
				command.Parameters.AddWithValue("@trackingID", trackingID);
				command.ExecuteNonQuery();
			}
		}

		protected void SeteHubClientSystemRegistrationsValid()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"UPDATE eHubClientSystemRegistration SET CD_Flag1 = 1";
				command.ExecuteNonQuery();
			}
		}

		protected void DeleteeHubClientSystemRegistrations()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"DELETE FROM eHubClientSystemRegistration";
				command.ExecuteNonQuery();
			}
		}

		protected void DeleteeHubClientRegistrations()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"DELETE FROM eHubClientRegistration";
				command.ExecuteNonQuery();
			}
		}

		protected void DeleteeHubCertificate()
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = @"DELETE FROM eHubCertificate";
				command.ExecuteNonQuery();
			}
		}

		protected void UpdateEHubClientRegistrationConfigXml(Guid registrationType, string clientId, string qualifier, string configXml)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $@"
UPDATE eHubClientRegistration
SET
	CX_ConfigXml = {configXml ?? "NULL"}
WHERE
	CX_RT = '{registrationType}'
AND CX_Qualifier = '{qualifier}'
AND CX_CC = (SELECT CC_PK FROM ehubClient WHERE CC_ID = '{clientId}')";
				command.ExecuteNonQuery();
			}
		}

		protected void DeleteMessageType(Guid messageTypePk)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $@"DELETE eHubOutboxMessage WHERE OI_DT_Target = '{messageTypePk}'
DELETE eHubMessageType WHERE DT_PK = '{messageTypePk}'";
				command.ExecuteNonQuery();
			}
		}

		protected void InsertMessageType(Guid messageTypePk, string messageTypeCode)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $@"INSERT INTO eHubMessageType (DT_PK, DT_Code) VALUES('{messageTypePk}', '{messageTypeCode}')";
				command.ExecuteNonQuery();
			}
		}

		protected void AssertContainsTheSucessStatusMessage(IMessageInbox inbox, Guid trackingID, string recipientID)
		{
			Assert.IsTrue(inbox.Count > 0);
			Assert.IsTrue(inbox.Any(m => IsThatSuccessStatusMessage(m, trackingID, recipientID)));
		}

		protected bool IsThatSuccessStatusMessage(IeHubMessage message, Guid trackingID, string recipientID)
		{
			return message.SchemaName == MessageStatusSuccessCode && message.TrackingID == trackingID && message.RecipientID == recipientID;
		}

		protected void AssertContainsFailedStatusMessage(IMessageInbox inbox, Guid trackingID, string recipientID)
		{
			Assert.IsTrue(inbox.Count > 0);
			Assert.IsTrue(inbox.Any(m => IsThatFailedStatusMessage(m, trackingID, recipientID)));
		}

		protected bool IsThatFailedStatusMessage(IeHubMessage message, Guid trackingID, string recipientID)
		{
			return message.SchemaName == MessageStatusFailCode && message.TrackingID == trackingID && message.RecipientID == recipientID;
		}

		protected int GetCountBillingTransaction(string priceItemCode, int billableCount, string reportingSource, string clientID, string clientNumber, string staffCode, string reference1, string reference2, string reference3, string reference4, string reference5, int version, string category, string branch, Guid? messageTrackingId)
		{
			var dbEntry = new StringBuilder();
			using (var connection = OpenBillingConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT COUNT(*) FROM Staging WHERE " + BillingTransactionSQLFilter(priceItemCode, billableCount, reportingSource, clientID, clientNumber, staffCode, reference1, reference2, reference3, reference4, reference5, version, category, branch, messageTrackingId);

				return (int)command.ExecuteScalar();
			}
		}

		protected string GetBillingStagingTransactionsInfo()
		{
			var dbEntry = new StringBuilder();
			using (var connection = OpenBillingConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT * FROM Staging ORDER BY TX_ServiceOccuredUTC DESC";
				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows)
					{
						dbEntry.AppendLine("\n(No records found in Staging)");
					}
					else
					{
						while (reader.Read())
						{
							dbEntry.AppendLine($"\nDB Entry: " +
							                   $"PriceItemCode={reader["TX_PriceItemCode"]}, " +
							                   $"BillableCount={reader["TX_BillableCount"]}, " +
							                   $"ReportingSource={reader["TX_ReportingSource"]}, " +
							                   $"ClientID={reader["TX_ClientID"]}, " +
							                   $"ClientNumber={reader["TX_ClientNumber"]}, " +
							                   $"StaffCode={(reader.IsDBNull(reader.GetOrdinal("TX_ClientStaffCode")) ? "NULL" : reader["TX_ClientStaffCode"])}, " +
							                   $"Reference1={reader["TX_Reference1"]}, " +
							                   $"Reference2={(reader.IsDBNull(reader.GetOrdinal("TX_Reference2")) ? "NULL" : reader["TX_Reference2"])}, " +
							                   $"Reference3={(reader.IsDBNull(reader.GetOrdinal("TX_Reference3")) ? "NULL" : reader["TX_Reference3"])}, " +
							                   $"Reference4={(reader.IsDBNull(reader.GetOrdinal("TX_Reference4")) ? "NULL" : reader["TX_Reference4"])}, " +
							                   $"Reference5={(reader.IsDBNull(reader.GetOrdinal("TX_Reference5")) ? "NULL" : reader["TX_Reference5"])}, " +
							                   $"Version={(reader.IsDBNull(reader.GetOrdinal("TX_Version")) ? "NULL" : reader["TX_Version"])}, " +
							                   $"Category={reader["TX_Category"]}, " +
							                   $"Branch={(reader.IsDBNull(reader.GetOrdinal("TX_Branch")) ? "NULL" : reader["TX_Branch"])}, " +
							                   $"ServiceOccuredUTC={reader["TX_ServiceOccuredUTC"]}" +
							                   $"TrackingID={(reader.IsDBNull(reader.GetOrdinal("TX_MessageTrackingID")) ? "NULL" : reader["TX_MessageTrackingID"])}");
						}
					}
				}

				return  dbEntry.ToString();
			}
		}
		protected async Task<int> GetCountBillingTransactionWithRetry(string priceItemCode, int billableCount, string reportingSource, string clientID, string clientNumber, string staffCode, string reference1, string reference2, string reference3, string reference4, string reference5, int version, string category, string branch, Guid? messageTrackingId , int maxRetry = 10, int delayMs = 6000)
		{
			for (int attempt = 1; attempt <= maxRetry; attempt++)
			{
				var count = GetCountBillingTransaction(priceItemCode, billableCount, reportingSource, clientID, clientNumber, staffCode, reference1, reference2, reference3, reference4, reference5, version, category, branch, messageTrackingId);
				if (count == 0 && attempt < maxRetry)
				{
					Console.WriteLine($"Not found. Attempt {attempt}. Retrying...");
					await Task.Delay(delayMs);
					continue;
				}

				return count;
			}

			return 0;
		}

		protected void DeleteBillingStaging()
		{
			using (var connection = OpenBillingConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "DELETE FROM dbo.Staging";
				command.ExecuteNonQuery();
			}
		}

		private string BillingTransactionSQLFilter(string priceItemCode, int billableCount, string reportingSource, string clientID, string clientNumber, string staffCode, string reference1, string reference2, string reference3, string reference4, string reference5, int version, string category, string branch, Guid? messageTrackingId)
		{
			var filter = string.Format("TX_PriceItemCode {0} AND TX_BillableCount {1} AND TX_ReportingSource {2} AND TX_ClientID {3} AND TX_ClientNumber {4} AND TX_ClientStaffCode {5} AND TX_Reference1 {6} AND TX_Reference2 {7} AND TX_Reference3 {8} AND TX_Reference4 {9} AND TX_Version {10} AND TX_Category {11} AND TX_Branch {12} AND TX_Reference5 {13} AND TX_MessageTrackingID {14}",
											GetNullableSQLFilter(priceItemCode),
											GetNullableSQLFilter(billableCount),
											GetNullableSQLFilter(reportingSource),
											GetNullableSQLFilter(clientID),
											GetNullableSQLFilter(clientNumber),
											GetNullableSQLFilter(staffCode),
											GetNullableSQLFilter(reference1),
											GetNullableSQLFilter(reference2),
											GetNullableSQLFilter(reference3),
											GetNullableSQLFilter(reference4),
											GetNullableSQLFilter(version),
											GetNullableSQLFilter(category),
											GetNullableSQLFilter(branch),
											GetNullableSQLFilter(reference5),
											GetNullableSQLFilter(messageTrackingId));

			return filter;
		}

		protected static void AssertLastQueueMessageContains(string expectedMessage)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT TOP 1 CONVERT(VARCHAR(MAX), CONVERT(VARBINARY(MAX), message_body, 1)) FROM sys.transmission_queue ORDER BY enqueue_time desc";
				var messageBody = (string)command.ExecuteScalar();
				Assert.IsTrue(messageBody.Contains(expectedMessage), "Expected contain: {0} \r\n But actual is: {1}", expectedMessage, messageBody);
			}
		}

		protected static void AssertFromServiceName(string expectedFromServiceName)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT TOP 1 from_service_name FROM sys.transmission_queue ORDER BY enqueue_time desc";
				var messageBody = (string)command.ExecuteScalar();
				Assert.IsTrue(messageBody.Contains(expectedFromServiceName));
			}
		}

		protected static void AssertToServiceName(string expectedToServiceName)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT TOP 1 to_service_name FROM sys.transmission_queue ORDER BY enqueue_time desc";
				var messageBody = (string)command.ExecuteScalar();
				Assert.IsTrue(messageBody.Contains(expectedToServiceName));
			}
		}

		public static void AssertXmlAreEqual(string expectedXmlStream, string actualXmlString)
		{
			var xmlDiff = new XmlDiff();
			var xmlDiffgram = new XDocument();
			var expectedXDoc = XDocument.Parse(expectedXmlStream);
			var actualXDoc = XDocument.Parse(actualXmlString);

			using (var expectedRdr = expectedXDoc.CreateReader())
			using (var actualRdr = actualXDoc.CreateReader())
			using (var diffWrtr = xmlDiffgram.CreateWriter())
			{
				if (xmlDiff.Compare(expectedRdr, actualRdr, diffWrtr)) return;
			}

			var tempFile = Path.GetTempFileName();
			using (var writer = XmlWriter.Create(tempFile, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
				actualXDoc.Save(writer);
			Console.WriteLine("Actual XML: " + tempFile);
			Console.WriteLine("XML Diff:");
			Console.WriteLine(xmlDiffgram.ToString());
			Assert.Fail("AssertXmlAreEqual failed. Output: " + tempFile);
		}

		protected static void AssertMessageExceptionDictionary(eHubAdapterException ex, Guid trackingID, string errorMessage)
		{
			var messageExceptionDictionary = ex.GetMessageExceptionDictionary();
			Assert.NotNull(messageExceptionDictionary);
			StringAssert.Contains(errorMessage, messageExceptionDictionary[trackingID]);
			StringAssert.Contains(errorMessage, ex.Message);
		}

		protected static string GetSystemID(string clientID)
		{
			return clientID.Substring(0, 3) + clientID.Substring(clientID.Length - 3, 3);
		}

		protected static object GetRemoteSettings(string serverName, string siteName, string application, string key)
		{
			Microsoft.Web.Administration.ConfigurationElement element = null;
			using (var manager = WithGatewayServiceAttribute.Current.GetServerManager())
			{
				var config = manager.GetWebConfiguration(siteName, application);
				var section = config.GetSection("appSettings");
				var settings = section.GetCollection();
				element = settings.FirstOrDefault(x => x.GetAttributeValue("key").ToString() == key);
			}
			return element == null ? null : element.GetAttributeValue("value");
		}

		protected static object GetRemoteSettings(string key)
		{
			return WithGatewayServiceAttribute.Current.GetRemoteSettings(key);
		}

		protected static void SetRemoteSettings(string key, string value)
		{
			WithGatewayServiceAttribute.Current.SetRemoteSettings(key, value);
		}

		protected eHubAdapter CreateAdapter(string clientId, string password)
		{
			return new eHubAdapter(GatewayHttpsUri, clientId, password);
		}

		protected string GetGatewayServerIP4Address()
		{
			return GetIP4Address(GetGatewayServerHostName());
		}

		string GetGatewayServerHostName()
		{
			return GatewayServerName.ToLower() == "localhost" ? Dns.GetHostName() : GatewayServerName;
		}

		protected static string GetIP4Address(string hostName)
		{
			var hostAddresses = Dns.GetHostAddresses(hostName);
			var iPAddress = hostAddresses.Where(_ => _.AddressFamily.ToString() == "InterNetwork").FirstOrDefault();
			return iPAddress == null ? string.Empty : iPAddress.ToString();
		}

		protected static  WebRequest CreateRequestWithTimeout(string endPoint)
		{
			var request = (HttpWebRequest)WebRequest.Create(endPoint);
			request.Credentials = CredentialCache.DefaultCredentials;
			request.ReadWriteTimeout = 30000;
			return request;
		}

		protected static SqlConnection OpenBizTalkMsgBoxDbConnection() =>
			SqlServerHelper.OpenAdminSqlConnection("biztalk");

		protected static SqlConnection OpenMasterConnection() =>
			SqlServerHelper.OpenAdminSqlConnection("master");

		protected static SqlConnection OpenEHubTransactionsConnection() =>
			SqlServerHelper.OpenAdminSqlConnection("eHubTransactions");

		protected static SqlConnection OpenBillingConnection() =>
			SqlServerHelper.OpenAdminSqlConnection("CargoWise_eServices_Billing");

		protected static SqlConnection OpenEdiProdCacheConnectionString() =>
			SqlServerHelper.OpenAdminSqlConnection("ediProdCache");

		protected static readonly string InboxAccessorUnprocessedMessageLimit = ConfigurationManager.AppSettings["InboxAccessorUnprocessedMessageLimit"];

		protected static readonly Guid TestClientPK = new Guid("4DC629AA-6B69-4453-805A-77E4D3BF144F");
		protected static readonly Guid CACustomsMonitoringClientPK = new Guid("F0CDFC73-66F8-4C7F-B288-D283293321AC");
		protected const string TestClientID = "TSTCLIENT";
		protected const string NonCW1_Client = "NONCW1TSTCLIENT";
		protected const string CACustomsMonitoringClientID = "T_____CAC";
		protected const string TestClientPassword = "TSTPASSWORD";
		protected static readonly Guid TestAuthenticatedClientPK = Guid.Parse("2A33240B-1388-4CCB-B1D9-30DA10D2DB6A");
		protected const string TestAuthenticatedClientID = "ENTTSTSVR";
		protected const string TestAuthenticatedClientPassword = "TESTPASSWORD";
		protected const string TestClientSystemID = "TSTENT";
		protected static readonly Guid TestClientSystemPK = new Guid("BF4A5714-505C-46B5-B3AC-111BAC8F8A74");
		protected const string eHubClientID = "eHub";
		protected static readonly Guid eHubClientPK = new Guid("9819EFF9-9CD8-4622-B58E-32A251115791");
		protected static readonly Guid MessageStatusSuccessPK = new Guid("14D86704-5B69-499E-B71E-05A708D004AA");
		protected const string MessageStatusSuccessCode = "MessageStatusSuccess";
		protected static readonly Guid MessageStatusFailPK = new Guid("6E0425D6-5D3E-42A2-8B74-2FF03C4520B9");
		protected const string MessageStatusFailCode = "MessageStatusFailed";
		protected override string CommonTestDataLocation { get { return ".TestDataBase.TestData."; } }
		protected override string TestDataSchemaLocation { get { return ".TestDataBase.Schemas."; } }

		protected string GatewayServerName => WithGatewayServiceAttribute.Current.ServerName;
		protected string GatewaySiteName => WithGatewayServiceAttribute.Current.SiteName;

		protected Uri GatewayHttpsUri => WithGatewayServiceAttribute.Current.HttpsEndpoint;
		protected Uri GatewayHttpUri => WithGatewayServiceAttribute.Current.HttpEndpoint;

		#endregion
	}

	sealed class DisposableAction : IDisposable
	{
		internal DisposableAction(Action create, Action dispose)
		{
			if (create == null) throw new ArgumentNullException(nameof(create));
			if (dispose == null) throw new ArgumentNullException(nameof(dispose));
			this.dispose = dispose;
			create();
		}

		readonly Action dispose;

		public void Dispose()
		{
			dispose();
		}
	}
}
