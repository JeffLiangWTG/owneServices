using System;
using System.Configuration;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.FaxRouter.EventLogging.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	sealed class EventLogTest : TransactionedTestCase
	{
		[DeveloperOnlyTest]
		public void TestForwardCopyOfProblemEmailToFaxAdministrator_DoesNotLockTheMailItem()
		{
			var mailItemPk = InsertTestEmailToFaxMailDBItem();

			try
			{
				EventLog.ForwardCopyOfProblemEmailToFaxAdministrator(mailItemPk, "Test");

				AssertNoExceptionThrown("Should still be able to update the mail item", () =>
				{
					using (var connection = MailDataModule.GetMailDBConnection())
					using (var command = new SqlCommand(@"UPDATE dbo.MailDBItems SET MI_Status = @MI_Status where MI_PK = @MI_PK", connection))
					{
						command.Parameters.AddWithValue("@MI_Status", "PRS");
						command.Parameters.AddWithValue("@MI_PK", mailItemPk);
						command.ExecuteNonQuery();
					}
				});
			}
			finally
			{
				using (var command = Db.Connection.Command(@"DELETE FROM dbo.MailDBItems where MI_SUBJECT like '%' + @MI_SUBJECT + '%'"))
				{
					command.AddParameter("@MI_SUBJECT", SqlDbType.VarChar, mailItemSubjectForTestInserts);
					command.ExecuteNonQuery();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ConfigurationSettings.AppSettings["MailDBConnectionString"] = String.Format(@"Data Source=localhost; Integrated Security=SSPI; Initial Catalog={0}; Application Name=FaxRouter; Connect Timeout=60; Pooling=false", Db.DatabaseName);
			ConfigurationSettings.AppSettings["ENTERPRISE_DATABASE_NAME"] = Db.DatabaseName;
			ConfigurationSettings.AppSettings["FAX_GATEWAY_ADMINISTRATOR_EMAIL"] = "test1@edi.com.au;test2@cargowise.com";
			ConfigurationSettings.AppSettings["AUTO_START"] = "1";

			ConfigurationSettings.AppSettings["FAX_GATEWAY_POLLING_INTERVAL"] = "30000";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_HIEGHT"] = "950";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_WIDTH"] = "750";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_TEMP_FILE_DIRECTORY"] = @"C:\Temp\";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_LOG"] = @"EventLog.txt";

			ConfigurationSettings.AppSettings["DAILY_REPORT_DISCREPANCY_THRESHOLD"] = "0";
		}

		#region Implementation

		Guid InsertTestEmailToFaxMailDBItem()
		{
			using (SqlConnection conn = BaseDataModule.GetMailDBConnection())
			{
				var mI_PK = Guid.NewGuid();

				using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBItems
(MI_PK, MI_Status, MI_DIRECTION, MI_ReceivedDateTime, MI_ContentType, MI_Encoding, MI_SUBJECT, MI_HEADER, MI_BODY, MI_FROM, MI_ReplyTo, MI_SystemCreateUser, MI_SystemCreateTimeUtc, MI_Application, MI_XMLInfo, MI_SystemLastEditUser, MI_SystemLastEditTimeUtc) VALUES
(@MI_PK, 'QUE', 'RCV', @MI_ReceivedDateTime, 'PLN', '', @MI_SUBJECT, @MI_Header, 'Test Body', @MI_From, 'Test ReplyTo', 'E', GETUTCDATE(), 'STD', '', 'E', GETUTCDATE())", conn))
				{
					sqlCmd.Parameters.AddWithValue("@MI_PK", mI_PK);
					sqlCmd.Parameters.AddWithValue("@MI_ReceivedDateTime", DateTime.Now.Date);
					sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", mailItemSubjectForTestInserts);
					sqlCmd.Parameters.AddWithValue("@MI_From", "<test.from@cargowise.com>");
					sqlCmd.Parameters.AddWithValue("@MI_Header", String.Format("From: <test.from@cargowise.com>\nSubject: {0}", mailItemSubjectForTestInserts));

					sqlCmd.ExecuteNonQuery();
				}

				return mI_PK;
			}
		}

		const string mailItemSubjectForTestInserts = "Test EDI Fax FD14A911-3AEA-4A1C-B098-F72530108307";

		#endregion
	}
}
