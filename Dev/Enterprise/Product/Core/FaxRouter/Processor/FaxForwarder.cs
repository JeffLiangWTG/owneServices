using System;
using System.IO;
#if NETFRAMEWORK
#pragma warning disable CW1086 // Do not use System.Web.Mail
using System.Web.Mail;
#pragma warning restore CW1086
#else
using System.Net.Mail;
using System.Net.Mime;
#endif

using Enterprise.Environment;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.FaxRouter.Processor
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1086:DoNotUseSystemWebMail", Justification = "Baseline")]
	public class FaxForwarder : FaxDataModule
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public void ProcessFaxJobToFaxHandler(String aTiffFilename, MailDBItemDataLine aMailDBItem)
		{
			string message = string.Empty;
			string error;
			string uniqueTiffFilename = GetUniqueTiffFilename();
			File.Copy(aTiffFilename, uniqueTiffFilename, false);
			String aControlFilename = FAX_GATEWAY_TEMP_FILE_DIRECTORY + aMailDBItem.FaxRecipientId + ".ctl";

			if (CreateControlFile(aControlFilename,
										uniqueTiffFilename,
										aMailDBItem.FaxRecipientId,
										aMailDBItem.FaxRecipientNumber,
										aMailDBItem.FaxRecipientAttentionName,
										aMailDBItem.FaxRecipientCompany,
										aMailDBItem.ChargeCode
									))
			{
				if (SendFax(uniqueTiffFilename, aControlFilename))
				{
					MarkRecipientJobAsSent(aMailDBItem.FaxRecipientId);
				}

				TempFile.TryDelete(aControlFilename, out error);
				message += error;
			}
			TempFile.TryDelete(aTiffFilename, out error);
			message += error;
			TempFile.TryDelete(uniqueTiffFilename, out error);
			message += error;
			if (!string.IsNullOrEmpty(message))
			{
				LogFile.EmailAdmin("There was a problem deleting file(s)", "Error message is:" + System.Environment.NewLine + message);
			}
		}

		protected virtual string GetUniqueTiffFilename()
		{
			return Path.Combine(Env.TempPath, Guid.NewGuid().ToString().ToLower() + ".tif");
		}

		protected virtual void DeleteFile(string path)
		{
			File.Delete(path);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		bool CreateControlFile(String filename, String attachmentFilename, Guid faxRecipientId, String faxNumber, String attentionName, String companyName, String chargeCode)
		{
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}

			StreamWriter sW = File.CreateText(filename);

			sW.WriteLine("!FXW");
			sW.WriteLine("!EXTATTACH,faxsend," + Path.GetFileName(attachmentFilename));
			sW.WriteLine("!PASSTHROUGH");
			sW.WriteLine("UID=" + FAX_ACK_RETURN_EMAIL);
			sW.WriteLine("!Target");
			sW.WriteLine("Dest=" + faxNumber);
			sW.WriteLine("Attn=" + attentionName);
			sW.WriteLine("Comp=" + companyName);
			sW.WriteLine(Constants.CHARGE_CODE_CONTROL_FILE_FIELD_NAME + "=" + faxRecipientId.ToString());
			sW.WriteLine("!PASSTHROUGH-END");
			sW.WriteLine("!FXW-END");
			sW.Close();

			if (File.Exists(filename))
			{
				return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		bool SendFax(String tiffFilename, String controlFilename)
		{
			new Faxing.Engine.TiffPageRotator().RotateLandscapeToPortrait(tiffFilename);

#if NETFRAMEWORK
			MailMessage message = new MailMessage();
			MailAttachment cTLFile = new MailAttachment(controlFilename, MailEncoding.Base64);
			MailAttachment tIFFFile = new MailAttachment(tiffFilename, MailEncoding.Base64);

			message.Attachments.Add(cTLFile);
			message.Attachments.Add(tIFFFile);

			message.From = FAX_GATEWAY_EMAIL;
			message.To = FAX_HANDLER_EMAIL;
			message.Subject = "WWFax Email";
			message.BodyFormat = MailFormat.Text;
			message.Body = "NO BODY";

			SendMail(EDI_FAXJOB_SMTP_SERVER, message);
#else
			using (var message = new MailMessage())
			{
				message.From = new MailAddress(FAX_GATEWAY_EMAIL);
				message.To.Add(FAX_HANDLER_EMAIL);
				message.Subject = "WWFax Email";
				message.Body = "NO BODY";
				message.IsBodyHtml = false;

				using (var ctlStream = File.OpenRead(controlFilename))
				using (var tiffStream = File.OpenRead(tiffFilename))
				{
					var ctlAttachment = new Attachment(ctlStream, Path.GetFileName(controlFilename), MediaTypeNames.Application.Octet);
					var tiffAttachment = new Attachment(tiffStream, Path.GetFileName(tiffFilename), MediaTypeNames.Application.Octet);
					message.Attachments.Add(ctlAttachment);
					message.Attachments.Add(tiffAttachment);

					using (var client = new SmtpClient(EDI_FAXJOB_SMTP_SERVER))
					{
						client.Send(message); // Synchronous send for consistency with .NET Framework
					}
				}
			}

#endif
			return true;
		}

		protected virtual void SendMail(string smtpServer, MailMessage mailMessage)
		{
#if NETFRAMEWORK
			SmtpMail.SmtpServer = smtpServer;
			SmtpMail.Send(mailMessage);
#else
			using (var client = new SmtpClient(smtpServer))
			{
				client.Send(mailMessage);
			}
#endif
		}
	}
}
