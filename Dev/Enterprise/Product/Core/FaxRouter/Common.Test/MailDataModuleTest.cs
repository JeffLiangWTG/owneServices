using System;
using System.Configuration;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.FaxRouter.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	sealed class MailDataModuleTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestGetTotalNewReceivedMailItemsBySubjectSubstring()
		{
			var mailDataModule = new MailDataModule();
			AssertEquals("Precondition: GetTotalNewReceivedMailItemsBySubjectSubstring(Test EDI Fax)", 0, mailDataModule.GetTotalNewReceivedMailItemsBySubjectSubstring("Test EDI Fax"));

			try
			{
				InsertTestEmailToFaxMailDBItems(10);

				AssertEquals("GetTotalNewReceivedMailItemsBySubjectSubstring(Test EDI Fax)", 10, mailDataModule.GetTotalNewReceivedMailItemsBySubjectSubstring("Test EDI Fax"));
			}
			finally
			{
				RemoveTestMailDBItems();
			}
		}

		[DeveloperOnlyTest]
		public void TestGetTotalNewReceivedMailItemsBySender()
		{
			var mailDataModule = new MailDataModule();
			AssertEquals("Precondition: GetTotalNewReceivedMailItemsBySender(FAX_ACK_SENDER)", 0, mailDataModule.GetTotalNewReceivedMailItemsBySender(MailDataModule.FAX_ACK_SENDER));

			try
			{
				InsertTestFaxAcknowledgementsMailDBItems(10);
				AssertEquals("GetTotalNewReceivedMailItemsBySender(FAX_ACK_SENDER)", 10, mailDataModule.GetTotalNewReceivedMailItemsBySender(MailDataModule.FAX_ACK_SENDER));
			}
			finally
			{
				RemoveTestMailDBItems();
			}
		}

		void RemoveTestMailDBItems()
		{
			using (SqlConnection conn = MailDataModule.GetMailDBConnection())
			{
				using (var sqlCmd = new SqlCommand(@"DELETE FROM dbo.MailDBItems where MI_SUBJECT = @MI_SUBJECT", conn))
				{
					sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", String.Format("Test EDI Fax {0}", uniqueIdentifierForTestInserts));
					sqlCmd.ExecuteNonQuery();
				}
				conn.Close();
			}
		}

		void InsertTestEmailToFaxMailDBItems(int amount)
		{
			using (SqlConnection conn = BaseDataModule.GetMailDBConnection())
			{
				for (var i = 0; i < amount; i++)
				{
					using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBItems
	(MI_PK, MI_Status, MI_DIRECTION, MI_ReceivedDateTime, MI_ContentType, MI_Encoding, MI_SUBJECT, MI_HEADER, MI_BODY, MI_FROM, MI_ReplyTo, MI_SystemCreateUser, MI_SystemCreateTimeUtc, MI_Application, MI_XMLInfo, MI_SystemLastEditUser, MI_SystemLastEditTimeUtc) VALUES
	(@MI_PK, 'QUE', 'RCV', @MI_ReceivedDateTime, 'PLN', '', @MI_SUBJECT, 'Test Header', 'Test Body', 'Test From', 'Test ReplyTo', 'E', GETUTCDATE(), 'STD', '', 'E', GETUTCDATE())", conn))
					{
						sqlCmd.Parameters.AddWithValue("@MI_PK", Guid.NewGuid());
						sqlCmd.Parameters.AddWithValue("@MI_ReceivedDateTime", DateTime.Now.Date);
						sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", String.Format("Test EDI Fax {0}", uniqueIdentifierForTestInserts));
						sqlCmd.ExecuteNonQuery();
					}
				}
				conn.Close();
			}
		}

		void InsertTestFaxAcknowledgementsMailDBItems(int amount)
		{
			using (SqlConnection conn = BaseDataModule.GetMailDBConnection())
			{
				for (var i = 0; i < amount; i++)
				{
					using (var sqlCmd = new SqlCommand(@"INSERT INTO dbo.MailDBItems
	(MI_PK, MI_Status, MI_DIRECTION, MI_ReceivedDateTime, MI_ContentType, MI_Encoding, MI_SUBJECT, MI_HEADER, MI_BODY, MI_FROM, MI_ReplyTo, MI_SystemCreateUser, MI_SystemCreateTimeUtc, MI_Application, MI_XMLInfo, MI_SystemLastEditUser, MI_SystemLastEditTimeUtc) VALUES
	(@MI_PK, 'QUE', 'RCV', @MI_ReceivedDateTime, 'PLN', '', @MI_SUBJECT, 'Test Header', 'Test Body', @MI_FROM, 'Test ReplyTo', 'E', GETUTCDATE(), 'STD', '', 'E', GETUTCDATE())", conn))
					{
						sqlCmd.Parameters.AddWithValue("@MI_PK", Guid.NewGuid());
						sqlCmd.Parameters.AddWithValue("@MI_ReceivedDateTime", DateTime.Now.Date);
						sqlCmd.Parameters.AddWithValue("@MI_SUBJECT", String.Format("Test EDI Fax {0}", uniqueIdentifierForTestInserts));
						sqlCmd.Parameters.AddWithValue("@MI_FROM", MailDataModule.FAX_ACK_SENDER);
						sqlCmd.ExecuteNonQuery();
					}
				}
				conn.Close();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ConfigurationSettings.AppSettings["MailDBConnectionString"] = String.Format(@"Data Source=localhost; Integrated Security=SSPI; Initial Catalog={0}; Application Name=FaxRouter; Connect Timeout=60; Pooling=false", Db.DatabaseName);
			ConfigurationSettings.AppSettings["ENTERPRISE_DATABASE_NAME"] = Db.DatabaseName;
			ConfigurationSettings.AppSettings["AUTO_START"] = "1";

			ConfigurationSettings.AppSettings["EDI_ACK_SMTP_SERVER"] = "xch.syd.edi";
			ConfigurationSettings.AppSettings["FAX_ACK_SENDER"] = "test_sender@edi.com.au";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_POLLING_INTERVAL"] = "30000";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_HIEGHT"] = "950";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_WIDTH"] = "750";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_TEMP_FILE_DIRECTORY"] = @"C:\Temp\";

			ConfigurationSettings.AppSettings["ACK_FORMAT"] = "TNZ";
			ConfigurationSettings.AppSettings["DAILY_REPORT_DISCREPANCY_THRESHOLD"] = "0";
			ConfigurationSettings.AppSettings["NOREPLY_EMAIL"] = "noreply@cargowise.com";

			uniqueIdentifierForTestInserts = Guid.NewGuid();
		}

		Guid uniqueIdentifierForTestInserts;
	}
}
