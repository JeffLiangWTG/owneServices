using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using MimeKit;

namespace Enterprise.EConversation.ServiceTasks
{
	public static class BusinessObjectEmailAttacher
	{
		public class ConversationMessages
		{
			public ConversationMessages(string emailBodyMessage, string attachmentMessage)
			{
				EmailBodyMessage = emailBodyMessage;
				AttachmentMessage = attachmentMessage;
			}

			public string EmailBodyMessage { get; private set; }
			public string AttachmentMessage { get; private set; }
		}

		public static void AttachEmail(IAllowAttachEmailsToEDocs workTask, string docType, params MailItem[] mailItems)
		{
			DocManagerInfo docSupportInfo = workTask.DocManagerInfo;
			foreach (MailItem mailItem in mailItems)
			{
				string fileName = GenerateLegalFileName(mailItem.MI_Subject);

				docSupportInfo.AddFileOrDocument(GetContentsForOutlookExpressMessage(mailItem), fileName, docType);
				mailItem.MI_Status = MailStatus.Processed;
			}

			SavedEventHandlerContainer container = new SavedEventHandlerContainer();
			container.Handler = delegate(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					factory.Saved -= container.Handler;
					var factoriesToSave =
						(from mailItem in mailItems
						 where mailItem.HasChanges && !docSupportInfo.MasterFactory.ChildParticipants.Contains(mailItem.Factory)
						 select (ITransactionParticipant)mailItem.Factory).Distinct().Union(new ITransactionParticipant[] { docSupportInfo.MasterFactory });
					BusinessObjectFactory.SaveTogether(factoriesToSave.ToArray());
				}
			};
			workTask.Factory.Saved += container.Handler;
		}

		class SavedEventHandlerContainer
		{
			public BusinessObjectFactory.SavedEventHandler Handler { get; set; }
		}

		public static string GenerateLegalFileName(ZString subject)
		{
			ZString result = subject.IsEmpty ? "Email.eml" : subject.SubstringSafe(0, 200) + ".eml";

			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				result = result.Replace(new string(invalidChar, 1), string.Empty);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		public static ConversationMessages ProcessAttachments(IAllowAttachEmailsToEDocs workTask, Email mailDoc, string docType, BusinessObjectEConversationAttacher convoAttacher)
		{
			var emailBodyForConversation = mailDoc.LatestMessageInBody;
			var emailAttachmentMessage = string.Empty;

			var emailAttachmentWithUrlMessageBuilder = new ZStringBuilder();
			var nonInlineAttachmentMessageBuilder = new ZStringBuilder();
			var iterNum = 0;
			foreach (var emailAttachment in mailDoc.Message.GetFullAttachments().Where(x => x.GetData().Length > 0))
			{
				var emailAttachmentFileName = emailAttachment.GetName();
				if (string.IsNullOrEmpty(emailAttachmentFileName))
				{
					emailAttachmentFileName = emailAttachment.GetSafeFileName();
				}

				if (string.IsNullOrEmpty(emailAttachmentFileName))
				{
					continue;
				}

				var (eDocFileName, eDocFileUrl) = AddToEDoc(workTask, docType, convoAttacher, emailAttachment, emailAttachmentFileName);
				var isInlineAttachment = HandleInlineAttachment(mailDoc.HtmlBody, emailAttachment, emailAttachmentFileName, eDocFileName, ref emailBodyForConversation);

				if (!isInlineAttachment)
				{
					iterNum++;
					nonInlineAttachmentMessageBuilder.Append($"Attachment {iterNum}: [{eDocFileName}]");
				}

				if (!string.IsNullOrEmpty(eDocFileUrl))
				{
					emailAttachmentWithUrlMessageBuilder.Append($"[{eDocFileName}]({eDocFileUrl})");
				}
			}

			if (!nonInlineAttachmentMessageBuilder.IsEmpty)
			{
				emailBodyForConversation += System.Environment.NewLine + nonInlineAttachmentMessageBuilder.ToStringWithNewLineBetweenAppends();
			}

			if (!emailAttachmentWithUrlMessageBuilder.IsEmpty)
			{
				emailAttachmentMessage = "Email Attachments";
				emailAttachmentMessage += System.Environment.NewLine + emailAttachmentWithUrlMessageBuilder.ToStringWithNewLineBetweenAppends();
			}

			return new ConversationMessages(emailBodyForConversation, emailAttachmentMessage);
		}

		static (string, string) AddToEDoc(IAllowAttachEmailsToEDocs workTask, string docType, BusinessObjectEConversationAttacher convoAttacher, MimeEntity attach, string fileName)
		{
			var eDocFileName = string.Empty;
			var eDocFileUrl = string.Empty;
			bool alreadyExists = false;

			var bizO = workTask as BusinessObject;
			var docSupportInfo = workTask.DocManagerInfo;

			int minFileSize = 20 * 1024; // 20 KB

			var isImage = FileImporter.IsSupportedImageFile(Path.GetExtension(fileName));
			var attachData = attach.GetData();
			var shouldAttachEDoc = attachData.Length > minFileSize || !isImage;

			if (shouldAttachEDoc)
			{
				var attachEDoc = docSupportInfo.AddFileOrDocument(attachData, GetFileNameWithExtension(fileName, attach), docType);
				eDocFileName = attachEDoc.FileName.Replace("[", "(").Replace("]", ")");

				foreach (IeDoc edoc in docSupportInfo.AllEDocs)
				{
					if (edoc.UniqueKey != attachEDoc.UniqueKey && edoc.ImageData == attachEDoc.ImageData)
					{
						(attachEDoc as StorageDocsBase).Delete();
						attachEDoc = edoc;
						eDocFileName = edoc.FileName;
						alreadyExists = true;
						break;
					}
				}

				if (attachEDoc != null && !alreadyExists)
				{
					docSupportInfo.AddLogsForNewDocument(bizO, attachEDoc);
					eDocFileUrl = ObjectFactory.Get<IShowEDocUrlHandler>().Create(attachEDoc);
				}
			}

			return (eDocFileName, eDocFileUrl);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		static bool HandleInlineAttachment(string mailDocHtmlBody, MimeEntity emailAttachment, string emailAttachmentFileName, string eDocFileName, ref string emailBodyForConversation)
		{
			bool emailBodyHasInlineAttachment = false;

			var contentIdIsNull = string.IsNullOrEmpty(emailAttachment.ContentId);
			if (!contentIdIsNull)
			{
				var inlineFileName = !string.IsNullOrEmpty(eDocFileName) ? FormattableString.Invariant($"[{eDocFileName}]") : string.Empty;

				var inlineFormats = new[]
				{
					string.Format(CultureInfo.InvariantCulture, "[image: {0}]", emailAttachmentFileName),
					string.Format(CultureInfo.InvariantCulture, "[cid:{0}]", emailAttachment.ContentId)
				};

				foreach (var inline in inlineFormats)
				{
					if (emailBodyForConversation.Contains(inline))
					{
						emailBodyHasInlineAttachment = true;
						emailBodyForConversation = emailBodyForConversation.Replace(inline, inlineFileName);
					}
				}

				if (!emailBodyHasInlineAttachment)
				{
					var htmlInlineFormat = string.Format(CultureInfo.InvariantCulture, "cid:{0}", emailAttachment.ContentId);
					var altNameInline = GetAltNameInline(mailDocHtmlBody, htmlInlineFormat);
					var bodyForConversationContainsAltName = altNameInline != null && emailBodyForConversation.Contains(altNameInline);
					if (bodyForConversationContainsAltName)
					{
						emailBodyHasInlineAttachment = true;
						emailBodyForConversation = emailBodyForConversation.Replace(altNameInline, inlineFileName);
					}
				}
			}

			return emailBodyHasInlineAttachment;
		}

		static string GetFileNameWithExtension(string fileName, MimeEntity attach)
		{
			if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
			{
				return fileName;
			}

			if (!string.IsNullOrEmpty(attach.GetSafeFileName()))
			{
				var suggestedExtension = Path.GetExtension(attach.GetSafeFileName());
				if (!string.IsNullOrEmpty(suggestedExtension))
				{
					return fileName + "." + suggestedExtension;
				}
			}

			return attach is MessagePart ? fileName + ".EML" : fileName;
		}

		static string GetAltNameInline(string htmlBody, string htmlInlineFormat)
		{
			var match = GetRegexAltNameMatch(htmlBody, htmlInlineFormat);
			return match != null ? "[" + match.Groups[2].Value.Replace(System.Environment.NewLine, " ").Replace("\n", " ") + "]" : null;
		}

		static Match GetRegexAltNameMatch(string htmlBody, string htmlInlineFormat)
		{
			var regexAltName = new Regex($@"(src=\""{htmlInlineFormat}\"") alt=\""([^\""]*)\""");
			if (regexAltName.IsMatch(htmlBody))
			{
				var match = regexAltName.Match(htmlBody);
				if (match.Groups.Count == 3)
				{
					return match;
				}
			}

			return null;
		}

		public static byte[] GetContentsForOutlookExpressMessage(MailItem mail)
		{
			return Encoding.ASCII.GetBytes(GetEmailTextForOutlookExpressMessage(mail));
		}

		public static string GetEmailTextForOutlookExpressMessage(MailItem mailItem)
		{
			return mailItem.MI_Header + System.Environment.NewLine + System.Environment.NewLine + mailItem.MI_Body;
		}
	}
}

