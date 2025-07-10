using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public static class MimeMessageExtensions
	{
		public static IEnumerable<MimeEntity> GetFullAttachments(this MimeMessage message)
			=> message.BodyParts.Where(x => x is MessagePart || x?.ContentDisposition?.FileName != null || x?.ContentType?.Name != null);

		public static IEnumerable<MimeEntity> GetNonVisualAttachments(this MimeMessage message)
			=> message.BodyParts.Where(x =>
			x is MessagePart
			|| (x?.ContentDisposition?.FileName != null || x?.ContentType?.Name != null) && x?.ContentDisposition?.Disposition == ContentDisposition.Attachment);

		public static IEnumerable<MimeEntity> GetVisualAttachments(this MimeMessage message)
			=> message.BodyParts.Where(x =>
			(x?.ContentDisposition?.FileName != null || x?.ContentType?.Name != null)
			&& (x?.ContentDisposition == null || x?.ContentDisposition?.Disposition == ContentDisposition.Inline));

		public static byte[] GetData(this MimeEntity mimeEntity)
		{
			using (var memoryStream = new MemoryStream())
			{
				if (mimeEntity is MimePart mimePartAttachment)
				{
					mimePartAttachment.Content?.DecodeTo(memoryStream);
				}
				else if (mimeEntity is MessagePart messagePartAttachment)
				{
					messagePartAttachment.Message?.WriteTo(memoryStream);
				}
				return memoryStream.ToArray();
			}
		}

		public static string GetName(this MimeEntity mimeEntity)
		{
			return mimeEntity?.ContentDisposition?.FileName ?? mimeEntity?.ContentType?.Name ?? string.Empty;
		}

		public static byte[] GetData(this MimeMessage message)
		{
			using (var memoryStream = new MemoryStream())
			{
				message.WriteTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		public static MimeMessage CreateMessageFromEml(string eml)
		{
			var byteData = System.Text.Encoding.UTF8.GetBytes(eml);
			return CreateMessageFromEml(byteData);
		}

		public static MimeMessage CreateMessageFromEml(byte[] emlData)
		{
			try
			{
				using (var memoryStream = new MemoryStream(emlData, false))
				{
					return MimeMessage.Load(memoryStream);
				}
			}
			catch (FormatException)
			{
				//Try again, but add a newline to the start first.
				emlData = System.Text.Encoding.UTF8.GetBytes(System.Environment.NewLine + System.Text.Encoding.UTF8.GetString(emlData));
				using (var memoryStream = new MemoryStream(emlData, false))
				{
					return MimeMessage.Load(memoryStream);
				}
			}
		}

		public static string RenderAddressWithBrackets(this MailboxAddress mailboxAddress)
		{
			var mailBoxAddress = mailboxAddress?.Address ?? string.Empty;
			var mailBoxToString = mailboxAddress?.ToString() ?? string.Empty;
			if (!string.IsNullOrEmpty(mailBoxToString) && mailBoxAddress == mailBoxToString && !mailBoxToString.StartsWith("<") && !mailBoxToString.EndsWith(">"))
			{
				mailBoxToString = $"<{mailBoxToString}>";
			}
			return mailBoxToString;
		}

		public static string GetSafeFileName(this MimeEntity mimePart)
		{
			return GetSafeFileName(mimePart.GetName(), mimePart.ContentType.MimeType);
		}

		internal static string GetSafeFileName(string unsafePath, string mimeType)
		{
			MimeTypes.TryGetExtension(mimeType, out var fileExtension);
			if (string.IsNullOrEmpty(fileExtension))
			{
				fileExtension = ".dat";
			}

			if (string.IsNullOrEmpty(unsafePath))
			{
				return $"{GenerateRandomFileName()}{fileExtension}";
			}

			var safePath = MakeFilenameSafe.MakeSafe(unsafePath, '_');
			var fileName = Path.GetFileName(safePath);
			if (string.IsNullOrEmpty(fileName) || fileName == ".")
			{
				return $"{GenerateRandomFileName()}{fileExtension}";
			}

			return PathValidation.GetSafeFilename(fileName);
		}

		static string GenerateRandomFileName()
		{
			return string.Format("att_{0}", Guid.NewGuid().ToString("N"));
		}

		public static MailboxAddress GetSenderOrFrom(this MimeMessage message)
		{
			if (message.Sender != null)
			{
				return message.Sender;
			}

			if (message.From.Count > 0)
			{
				return message.From.Mailboxes.First();
			}

			return null;
		}
	}
}
