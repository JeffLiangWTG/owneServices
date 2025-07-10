using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public static class MailKitMailBuilder
	{
		public static MimeMessage BuildMimeMessage(this MailItem mailItem, bool shouldFillAttachments = true)
		{
			var rawEmailBytes = mailItem.IsMIMEEmail ? mailItem.RawEmailBytes.ToArray() : null;
			if (rawEmailBytes != null && rawEmailBytes.Length > 0)
			{
				using (var stream = new MemoryStream(rawEmailBytes))
				{
					return MimeMessage.Load(stream);
				}
			}
			else
			{
				return BuildMimeMessageCore(mailItem, shouldFillAttachments);
			}
		}

		public static MimeMessage BuildMimeMessageCore(MailItem mailItem, bool shouldFillAttachments = true)
		{
			var result = new MimeMessage();
			FillFromAndRecipients(result, mailItem);

			var bodyBuilder = new BodyBuilder();
			FillBody(bodyBuilder, mailItem);

			if (shouldFillAttachments)
			{
				FillAttachments(bodyBuilder, mailItem);
			}
			result.Body = bodyBuilder.ToMessageBody();

			result.Subject = mailItem.MI_Subject;

			if (!mailItem.MI_ReplyTo.IsEmpty)
			{
				result.ReplyTo.Add(MailboxAddress.Parse(mailItem.MI_ReplyTo));
			}

			if (mailItem.OverrideMailSettingForBounceBackCapturing)
			{
				result.Body.Headers.Add(MailItem.MI_SenderStaffIDHeaderIndex, mailItem.MI_SenderStaffID);
				result.Body.Headers.Add(MailItem.MI_BusinessEntityIDHeaderIndex, mailItem.MI_BusinessEntityID);
				result.Body.Headers.Add(MailItem.MI_BusinessEntityTableCodeHeaderIndex, mailItem.MI_BusinessEntityTableCode);
				result.Body.Headers.Add(MailItem.MI_BusinessEntityJobNumberHeaderIndex, mailItem.MI_BusinessEntityjobNumber);
				result.Body.Headers.Add(MailItem.MI_DocumentNameHeaderIndex, mailItem.EncodedDocumentName);

				if (Env.Registry.AllowEmailsToBeSentFromUsersAddress)
				{
					var returnPath = string.Empty;
					MailboxAddress.TryParse(mailItem.MI_From, out var mimeStaffEmailAddress);
					MailboxAddress.TryParse(Env.Registry.MailboxEmailAddress, out var mimeMailboxEmailAddress);
					var selectNDRPath = Env.Registry.SelectNDRPath;
					if (selectNDRPath == Constants.SelectNDRPath.Codes.MB)
					{
						returnPath = mimeMailboxEmailAddress?.Address;
					}
					else if (selectNDRPath == Constants.SelectNDRPath.Codes.CP)
					{
						if (mimeStaffEmailAddress?.Domain == mimeMailboxEmailAddress?.Domain)
						{
							returnPath = mimeMailboxEmailAddress?.Address;
						}
						else
						{
							returnPath = mimeStaffEmailAddress?.Address;
						}
					}
					else if (selectNDRPath == Constants.SelectNDRPath.Codes.SU)
					{
						returnPath = mimeStaffEmailAddress?.Address;
					}

					if (!string.IsNullOrEmpty(returnPath))
					{
						result.Headers.Add(MailItem.MI_ReturnPathHeaderIndex, $"<{returnPath}>");
					}
				}
				else
				{
					result.Headers.Add("Return-Path", "<" + Env.Registry.MailboxEmailAddress + ">");
				}

				var listUnsubscribe = mailItem.MI_ListUnsubscribe;
				if (!listUnsubscribe.IsEmpty)
				{
					result.Body.Headers.Add(MailItem.MI_ListUnsubscribeHeaderIndex, listUnsubscribe);
				}
			}

			if (!mailItem.MI_SystemCreateTimeUtc.IsEmpty)
			{
				result.Date = mailItem.MI_SystemCreateTimeUtc.ToDateTime().ToLocalTime();
			}

			return result;
		}

		static void FillFromAndRecipients(MimeMessage message, MailItem mailItem)
		{
			InternetAddressList.TryParse(mailItem.MI_From, out var fromAddresses);
			if (fromAddresses?.Count > 0)
			{
				message.From.Add(fromAddresses[0]);
			}

			foreach (IMailRecipient recipient in mailItem.MailRecipients)
			{
				InternetAddressList mailBoxList = null;

				if (recipient.MR_RecipientType == nameof(MailRecipient.RecipientTypes.TO))
				{
					mailBoxList = message.To;
				}
				else if (recipient.MR_RecipientType == nameof(MailRecipient.RecipientTypes.CC))
				{
					mailBoxList = message.Cc;
				}
				else if (recipient.MR_RecipientType == nameof(MailRecipient.RecipientTypes.BCC))
				{
					mailBoxList = message.Bcc;
				}

				if (InternetAddressList.TryParse(ParserOptions.Default, recipient.EmailAddress, out var addresses))
				{
					mailBoxList?.AddRange(addresses);
				}
			}
		}

		static void FillBody(BodyBuilder builder, MailItem mailItem)
		{
			if (mailItem.MI_ContentType == EmailContentTypes.Calendar.ContentTypeCode)
			{
				var calendarPart = new TextPart((NoResString)"calendar")
				{
					ContentTransferEncoding = ContentEncoding.Base64,
					Text = mailItem.MI_Body,
				};
				calendarPart.ContentType.Parameters.Add("method", mailItem.MI_Body.Contains("METHOD:CANCEL") ? "CANCEL" : "REQUEST");
				builder.Attachments.Add(calendarPart);
			}
			else if (mailItem.MI_ContentType == EmailContentTypes.HTML.ContentTypeCode)
			{
				builder.HtmlBody = mailItem.MI_Body;
			}
			else
			{
				builder.TextBody = mailItem.MI_Body;
			}
		}

		static void FillAttachments(BodyBuilder builder, MailItem mailItem)
		{
			var pattern = new Regex(@"[\\/:*?\<>|]");

			foreach (var attachment in mailItem.MailAttachments.OfType<MailAttachment>())
			{
				MimeEntity attachmentMime;

				if (mailItem.AttachmentIsReferredToInHTMLBody(attachment, out var attachmentFileName))
				{
					attachmentMime = builder.LinkedResources.Add(attachmentFileName, attachment.MA_Data);
					attachmentMime.ContentId = attachmentFileName;
				}
				else
				{
					attachmentFileName = attachment.MA_FileName;
					var cleanFileName = pattern.Replace(attachmentFileName, string.Empty);
					attachmentMime = builder.Attachments.Add(cleanFileName, attachment.MA_Data);
				}

				attachmentMime.ContentDisposition.FileName = attachmentFileName;

				if (attachmentMime is MimePart mimePart)
				{
					mimePart.ContentTransferEncoding = ContentEncoding.Base64;
				}

				if (!Enum.TryParse<ParameterEncodingMethod>(Env.Registry.AttachmentEncodingFormat, ignoreCase: true, out var attachmentEncoding))
				{
					ErrorReporter.ReportOnce("AttachmentEncodingParseError", $"The entered attachment encoding format '{Env.Registry.AttachmentEncodingFormat}' cannot be parsed into a valid ParameterEncodingMethod enum value.");
					attachmentEncoding = ParameterEncodingMethod.Rfc2047;
				}

				foreach (var param in attachmentMime.ContentDisposition.Parameters)
				{
					param.EncodingMethod = attachmentEncoding;
				}
			}
		}
	}
}
