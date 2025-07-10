using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.FaxRouter.EventLogging
{
	public static class EventLog
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static void AddErrorEntry(String errorType, Exception e)
		{
			LogFile.AddLog("Error " + errorType, e.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static void AddErrorEntry(String errorType, String errorDescription)
		{
			LogFile.AddLog("Error " + errorType, errorDescription);
		}

		public static void ForwardCopyOfProblemEmailToFaxAdministrator(Guid mI_PK)
		{
			ForwardCopyOfProblemEmailToFaxAdministrator(mI_PK, string.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
		public static void ForwardCopyOfProblemEmailToFaxAdministrator(Guid mI_PK, string exception)
		{
			ZString recipients = Constants.FAX_ADMINISTRATOR_EMAIL;

			using (var connection = BaseDataModule.GetMailDBConnection())
			using (var command = new SqlCommand(@"UPDATE dbo.MailDBItems SET MI_Status = 'FAL' WHERE MI_PK = @MI_PK", connection))
			{
				command.Parameters.AddWithValue("@MI_PK", mI_PK);
				command.ExecuteNonQuery();
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			MailItem item = factory.Load<MailItem>(mI_PK);

			if (item != null)
			{
				if (!recipients.IsEmpty)
				{
					item.MI_Body += "------\r\n" + exception;

					MailItemCopySender copySender = new MailItemCopySender(new MailItem[] { item });
					copySender.MailAddressToSendCopyTo = recipients;
					copySender.SendCopyTo();
				}
			}
			else if (!recipients.IsEmpty)
			{
				IOutgoingMailManager mailCreator = OutgoingMailCreator.Instance;
				mailCreator.CreateAndSaveSimple("FaxRouter exception - mail PK " + mI_PK.ToString(), exception, recipients);
			}

			LogError(mI_PK, item, exception);
		}

		static void LogError(Guid mI_PK, MailItem item, string exception)
		{
			string attachments = string.Empty;
			string recipients = string.Empty;
			string body = string.Empty;
			if (item != null)
			{
				foreach (MailAttachment attachment in item.MailAttachments)
				{
					attachments += attachment.MA_FileName + ", ";
				}
				foreach (MailRecipient rec in item.MailRecipients)
				{
					recipients += rec.MR_RecipientMailAddress + ", ";
				}
				body = item.MI_Body;
			}

			LogError(mI_PK, attachments, recipients, body, exception);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		static void LogError(Guid mI_PK, string attachments, string recipients, string body, string exception)
		{
			LogFile.AddLog("Error", ZDateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss") + " (UTC): Some error happened with " + mI_PK.ToString() + " mail item." +
				"\n\rAttachments: " + attachments +
				"\n\rBody: " + body +
				"\n\rRecipients: " + recipients +
				"\n\r**********ERROR**********\n\r" + exception + "\n\r********************", "errorlog.txt");
		}
	}
}
