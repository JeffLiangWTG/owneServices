using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using MimeKit;

namespace Enterprise.EConversation.ServiceTasks
{
	public class Email
	{
		public Email(byte[] eml)
		{
			this.eml = eml;
			message = MimeMessageExtensions.CreateMessageFromEml(eml);
		}

		public MimeMessage Message
		{
			get
			{
				return message;
			}
		}
		public string From
		{
			get
			{
				return message.From.Count > 0 ? message.From[0].ToString() : string.Empty;
			}
		}

		public string SenderNameAddress
		{
			get => ToNameAddress(message.GetSenderOrFrom());
		}

		public string SenderAddress
		{
			get => message.GetSenderOrFrom()?.Address ?? string.Empty;
		}

		public string SenderName
		{
			get => message.GetSenderOrFrom()?.Name ?? string.Empty;
		}

		public string Subject
		{
			get { return message.Subject ?? string.Empty; }
		}

		public ZDateTime EmailDate
		{
			get { return message.Date != DateTimeOffset.MinValue ? new ZDateTime(message.Date.LocalDateTime) : ZDateTime.Invalid; }
		}

		public string Body
		{
			get
			{
				var result = message.TextBody;
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}

				var html2Text = new HtmlToTextUtility();
				result = html2Text.GetPlainText(HtmlBody);
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}

				return string.Empty;
			}
		}

		public string HtmlBody
		{
			get { return message.HtmlBody ?? string.Empty; }
		}
		public string LatestMessageInBody
		{
			get
			{
				if (string.IsNullOrEmpty(latestMessageInBody))
				{
					latestMessageInBody = GetAndCleanRecentReply();
				}
				return latestMessageInBody;
			}
		}
		string latestMessageInBody;

		public string GetEml()
		{
			return Encoding.UTF8.GetString(eml) ?? string.Empty;
		}

		public byte[] GetEmlBytes()
		{
			return eml ?? Array.Empty<byte>();
		}

		public bool IsEmailProcessingSkipped;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public bool SaveEml(string fileName, out string errorMessage)
		{
			try
			{
				File.WriteAllText(fileName, GetEml());
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				errorMessage = ex.Message;
				return false;
			}

			errorMessage = null;
			return true;
		}

		public virtual string GetFirstAttachmentOrVisualText()
		{
			return GetFirstAttachmentOrVisualText(null);
		}

		public string GetFirstAttachmentOrVisualText(string password)
		{
			return GetAttachmentOrVisualText(0, password);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		public string GetAttachmentOrVisualText(int index, string password = null)
		{
			var attachment = GetAttachmentOrVisual(message, index);
			if (attachment is MessagePart messagePartAttachment)
			{
				attachment = FirstAttachment(messagePartAttachment.Message);
			}

			if (attachment != null)
			{
				var attachmentFilename = attachment.GetName();
				if (!string.IsNullOrEmpty(attachmentFilename))
				{
					var extension = Path.GetExtension(attachmentFilename);
					var fileData = attachment.GetData();
					if (extension.EndsWith("zip", StringComparison.OrdinalIgnoreCase))
					{
						StringBuilder sb = new StringBuilder();
						new AttachmentTextAppender().Append(attachmentFilename, fileData, sb, password);
						return sb.ToString();
					}
					else
					{
						return Encoding.ASCII.GetString(fileData);
					}
				}
			}

			return string.Empty;
		}

		static MimeEntity GetAttachmentOrVisual(MimeMessage msg, int index)
		{
			var attachments = msg.GetFullAttachments();

			if (index < attachments.Count())
			{
				return attachments.ElementAt(index);
			}
			return null;
		}

		static MimeEntity FirstAttachment(MimeMessage msg)
		{
			return GetAttachmentOrVisual(msg, 0);
		}

		public int NonVisualCount
		{
			get => message.GetNonVisualAttachments().Count();
		}

		public int VisualCount
		{
			get => message.GetVisualAttachments().Count();
		}

		static string ToNameAddress(MailboxAddress mailbox)
		{
			string result = string.Empty;
			if (mailbox != null)
			{
				if (!string.IsNullOrEmpty(mailbox.Name))
				{
					result += '"' + mailbox.Name + '"';
				}

				if (!string.IsNullOrEmpty(mailbox.Address))
				{
					if (result.Length > 0)
					{
						result += ' ';
					}

					result += '<' + mailbox.Address + '>';
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		string GetAndCleanRecentReply()
		{
			string cleanBody = CleanBody();
			var regexes = new Dictionary<string, RegexOptions>();

			regexes.Add("-+\\s*original\\s+message\\s*-+\\s", RegexOptions.IgnoreCase);
			regexes.Add("-+\\s*forwarded\\s+message\\s*-+\\s", RegexOptions.IgnoreCase);

			regexes.Add("From:\\s*.*\\nSent:\\s*.*\\nTo:", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			regexes.Add("From:\\s*.*\\r\\nSent:\\s*.*\\r\\nTo:", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			regexes.Add("from:\\s*", RegexOptions.IgnoreCase);
			regexes.Add("--reply above this line", RegexOptions.IgnoreCase);
			regexes.Add("reply above this line", RegexOptions.IgnoreCase);
			regexes.Add("\\n.*On.*(\\r\\n)?.*wrote:\\r\\n", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			regexes.Add("\\n.*On.*(\\n)?.*wrote:\\n", RegexOptions.IgnoreCase | RegexOptions.Multiline);

			if (message.Sender != null)
			{
				regexes.Add("From:\\s*" + Regex.Escape(message.Sender.Address), RegexOptions.IgnoreCase);
				regexes.Add("<" + Regex.Escape(message.Sender.Address) + ">", RegexOptions.IgnoreCase);
				regexes.Add(Regex.Escape(message.Sender.Address) + "\\s+wrote:", RegexOptions.IgnoreCase);
			}

			regexes.Add("\\w+:\\s(\\w+\\s)+.*((?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\]))(.*(\\r\\n|\\n).*){1,2}(\\r\\n|\\n).*:\\s.*CS", RegexOptions.Compiled);

			int minIndex = int.MaxValue;
			var searchRangeLength = cleanBody.Length;
			var maxTimeout = RegexTimeout.Add(RegexTimeoutStep);
			var matchingStart = ZDateTime.UtcNow;

			foreach (var regex in regexes)
			{
				var timeout = RegexTimeout;
				while (timeout <= maxTimeout)
				{
					try
					{
						var firstMatch = Regex.Match(cleanBody, regex.Key, regex.Value, timeout);
						if (firstMatch != null && firstMatch.Success && firstMatch.Index > 0 && firstMatch.Index < minIndex)
						{
							minIndex = firstMatch.Index;
							searchRangeLength = firstMatch.Index;
							cleanBody = cleanBody.Substring(0, searchRangeLength);
						}

						break;
					}
					catch (RegexMatchTimeoutException e)
					{
						timeout = timeout.Add(RegexTimeoutStep);

						if ((ZDateTime.UtcNow - matchingStart).TotalSeconds > AggregatedTimeout.TotalSeconds)
						{
							ErrorReporter.ReportOnce(FormattableString.Invariant($"Aggregated Regex Timeout Error while processing email. Subject: {Subject}"), e);
							break;
						}
					}
				}
			}

			return minIndex < cleanBody.Length ? cleanBody.Substring(0, minIndex).TrimEnd() : cleanBody.TrimEnd();
		}

		protected virtual TimeSpan RegexTimeout => TimeSpan.FromSeconds(2);
		TimeSpan RegexTimeoutStep => TimeSpan.FromSeconds(3);

		protected virtual TimeSpan AggregatedTimeout => TimeSpan.FromSeconds(15);

		const int maxBodySize = 10240; // 10 KB
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		const string messageTruncated = "\r\n----------\r\nMessage truncated. See eDocs for a full message.";
		string CleanBody()
		{
			var result = Body;
			if (result.Length > maxBodySize)
			{
				result = result.Substring(0, maxBodySize);
				result += messageTruncated;
			}

			result = result.Replace('’', '\'').Replace('“', '"').Replace('”', '"');

			var regexWithAnchor = new Regex(@"([^\s]+)(\r\n|\n)?(<(((http|ftp|https):\/\/)|www\.)([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-;!]*[\w@?^=%&/~+#-$])?>)");
			var regexNoAnchor = new Regex(@"\s(<*(((http|ftp|https):\/\/)|www\.)([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-;!]*[\w@?^=%&/~+#-$])?>*)");

			result = regexWithAnchor.Replace(result, match =>
			{
				var matchAnchorWithoutBrackets = match.Groups[1].Value.Replace("[", "").Replace("]", "");
				var matchUrlWithoutBrackets = match.Groups[3].Value.Replace("<", "").Replace(">", "");
				return FormattableString.Invariant($"[{matchAnchorWithoutBrackets}]({matchUrlWithoutBrackets})");
			});

			var index = 1;

			result = regexNoAnchor.Replace(result, match =>
			{
				var firstWhiteSpace = match.Value[0];
				var matchWithoutBrackets = match.Value.Replace("<", "").Replace(">", "").Trim();
				return FormattableString.Invariant($"{firstWhiteSpace}[link {index++}]({matchWithoutBrackets})");
			});

			return result;
		}

		readonly MimeMessage message;
		readonly byte[] eml;
	}
}

