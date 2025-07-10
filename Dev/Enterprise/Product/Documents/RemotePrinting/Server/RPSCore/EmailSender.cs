using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using MailManager;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public interface IEmailSender
	{
		void SendEmailToGroup(Guid recipientPk, string subject, string body);
		bool SendEmailToUser(string recipientStaffCode, string subject, string body);
		void SendLogsEmail(string recipientEmail, byte[] fileData, string fileName, string body);
	}

	public class EmailSender : IEmailSender
	{
		readonly DbConnection connection;
		readonly string fromAddress;

		public EmailSender(DbConnection connection, string fromAddress)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(fromAddress, nameof(fromAddress));

			this.connection = connection;
			this.fromAddress = fromAddress;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void SendEmailToGroup(Guid recipientGroupPk, string subject, string body)
		{
			const string sqlText = @"
				DECLARE @recipientEmails TABLE(GS_EmailAddress NVARCHAR(254))

				INSERT INTO @recipientEmails
				SELECT GS_EmailAddress FROM dbo.GlbStaff
				JOIN dbo.GlbGroupLink ON GK_GS = GS_PK
				WHERE GK_GG = @recipientGroupPk AND GS_EmailAddress != ''

				IF exists(SELECT 1 FROM @recipientEmails)
				BEGIN
					DECLARE @mailItemPk uniqueidentifier = NEWID();
					DECLARE @currentTime DATETIME = SYSUTCDATETIME();

					INSERT dbo.MailDBItems (MI_PK, MI_Direction, MI_ReceivedDateTime, MI_SendDateTime, MI_Subject, MI_Body, MI_From, MI_ContentType, MI_SystemCreateTimeUtc, MI_SystemCreateUser, MI_SystemLastEditTimeUtc, MI_SystemLastEditUser)
						VALUES (@mailItemPk, 'TRX', @currentTime, @currentTime, @subject, @body, @fromAddress, 'PLN', @currentTime, @currentUserCode, @currentTime, @currentUserCode)

					INSERT dbo.MailDBRecipients (MR_PK, MR_MI, MR_RecipientMailAddress, MR_RecipientType, MR_SystemCreateTimeUtc, MR_SystemLastEditTimeUtc, MR_SystemCreateUser, MR_SystemLastEditUser)
						SELECT NEWID(), @mailItemPk, GS_EmailAddress, 'TO', @currentTime, @currentTime, @currentUserCode, @currentUserCode
						FROM @recipientEmails
				END";

			using (var command = connection.Command(sqlText))
			{
				command.AddParameter("@subject", SqlDbType.NVarChar, subject);
				command.AddParameter("@body", SqlDbType.NVarChar, body);
				command.AddParameter("@fromAddress", SqlDbType.NVarChar, fromAddress);
				command.AddParameter("@recipientGroupPk", SqlDbType.UniqueIdentifier, recipientGroupPk);
				command.AddParameter("@currentUserCode", SqlDbType.VarChar, 3, GetUserInitials());

				command.ExecuteNonQuery();
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-84-0", Justification = "We want a failed cast to be an exception, that means the method didnt run as expected")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool SendEmailToUser(string recipientStaffCode, string subject, string body)
		{
			const string sql = @"
				DECLARE @userEmailAddress NVARCHAR(254) = (SELECT GS_EmailAddress FROM dbo.GlbStaff where GS_Code=@recipientStaffCode)
				IF (@userEmailAddress IS NOT NULL AND @userEmailAddress <> '')
				BEGIN
					DECLARE @mailItemPk UNIQUEIDENTIFIER = NEWID();
					DECLARE @currentUTC DATETIME = SYSUTCDATETIME();

					INSERT INTO dbo.MailDBItems (MI_PK, MI_Direction, MI_ReceivedDateTime, MI_SendDateTime, MI_Subject, MI_Body, MI_From, MI_ContentType, MI_SystemCreateTimeUtc, MI_SystemCreateUser, MI_SystemLastEditTimeUtc, MI_SystemLastEditUser)
					VALUES (@mailItemPk, 'TRX', @currentUTC, @currentUTC, @subject, @body, @fromAddress, 'PLN', @currentUTC, @currentUserCode, @currentUTC, @currentUserCode);

					INSERT dbo.MailDBRecipients (MR_PK, MR_MI, MR_RecipientMailAddress, MR_RecipientType, MR_SystemCreateTimeUtc, MR_SystemLastEditTimeUtc, MR_SystemCreateUser, MR_SystemLastEditUser)
					VALUES (NEWID(), @mailItemPk, @userEmailAddress, 'TO', @currentUTC, @currentUTC, @currentUserCode, @currentUserCode)

					SELECT 1
				END
				ELSE
					SELECT 0";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@recipientStaffCode", SqlDbType.VarChar, recipientStaffCode);
				command.AddParameter("@subject", SqlDbType.NVarChar, subject);
				command.AddParameter("@body", SqlDbType.NVarChar, body);
				command.AddParameter("@fromAddress", SqlDbType.NVarChar, fromAddress);
				command.AddParameter("@currentUserCode", SqlDbType.VarChar, 3, GetUserInitials());

				return (int)command.ExecuteScalar() != 0;
			}
		}

		static string GetUserInitials() => User.WebUserCode;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void SendLogsEmail(string recipientEmail, byte[] fileData, string fileName, string body)
		{
			try
			{
				var factory = new BusinessObjectFactory(connection);
				var mailItem = factory.New<MailItem>();

				mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
				mailItem.MI_SendDateTime = ZDateTime.UtcNow;
				mailItem.MI_Direction = DirectionList.Codes.Transmit;
				mailItem.MI_From = fromAddress;
				mailItem.MI_Subject = new EnterpriseInformationRetriever().LicenceCode + " - WebPrint Client Logs";

				mailItem.MI_Body = body;

				var mailAttachment = mailItem.MailAttachments.AddNew();
				mailAttachment.MA_Data = fileData;
				mailAttachment.MA_FileName = fileName;

				mailItem.AddRecipientForSystemCommunication(recipientEmail);

				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var msg = new StringBuilder();
				msg.Append("Error preparing WebPrint Logs email: ").AppendLine(ex.Message)
					.AppendLine()
					.Append("RecipientEmail: ").AppendLine(recipientEmail)
					.Append("FileData length: ").AppendLine((fileData?.Length ?? 0).ToString())
					.Append("FileName: ").AppendLine(fileName);

				ErrorReporter.ReportOnce(msg.ToString(), ex);
			}
		}
	}
}
