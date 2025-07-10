using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using MimeKit.Utils;
using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	[DebuggerDisplay("Mail({MI_Status} - {MI_Direction} - \"{MI_Subject}\")")]
	public class MailItem : AutoMailDBItems, IMailItem
	{
		public MailItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static void MarkMailItemAsFailed(ZGuid mailItemPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MailItem mailItem = factory.Load<MailItem>(mailItemPK);
			if (mailItem != null)
			{
				mailItem.MI_Status = MailStatus.Failed;
				factory.Save();
			}
		}

		public override ZDateTime MI_SystemCreateTimeUtc
		{
			get { return new ZDateTime(base.MI_SystemCreateTimeUtc, DateTimeKind.Utc); }
			set
			{
				base.MI_SystemCreateTimeUtc = value;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return MI_Subject.IsEmpty ? MI_From : MI_Subject; }
		}

		public ZByte SendAttempts
		{
			set
			{
				foreach (IMailRecipient recipient in MailRecipients)
				{
					recipient.MR_AckAttempt = value;
				}
			}
		}

		public string RawMIMEString
		{
			get
			{
				return IsMIMEEmail ? RawEmailString : null;
			}
			set
			{
				if (!MI_Body.IsEmpty || !MI_Header.IsEmpty || (MailRecipients.Count > 0) || (MailAttachments.Count > 0))
				{
					throw new Exception("You can't set the string because it can only be done with a blank mail item.");
				}
				string rawMIMEText = value;
				int headerEnd = rawMIMEText.IndexOf("\r\n\r\n") + 2;
				int bodyStart = headerEnd + 2;
				string headerText = rawMIMEText.Substring(0, headerEnd);
				string bodyText = rawMIMEText.Substring(bodyStart);

				MI_SendDateTime = ZDateTime.UtcNow;
				MI_ReceivedDateTime = ZDateTime.UtcNow;

				MI_Header = headerText;
				MI_Body = bodyText;
			}
		}

		public IEnumerable<byte> RawEmailBytes
		{
			get { return Encoding.UTF8.GetBytes(RawEmailString); }
		}

		#region AllRecipients

		public ZString AllRecipients
		{
			get
			{
				ZString separator = ";";
				StringBuilder resultBuilder = new StringBuilder();
				foreach (IMailRecipient recipient in MailRecipients)
				{
					resultBuilder.Append(recipient.EmailAddress + separator);
				}
				if (resultBuilder.Length == 0)
				{
					return "";
				}

				return resultBuilder.ToString(0, resultBuilder.Length - 1);
			}
		}

		public ZPropertyInfo AllRecipientsInfo
		{
			get { return GetZPropertyInfo(nameof(AllRecipients)); }
		}

		#endregion

		[List("Lookups.StatusCodes")]
		public override ZString MI_Status
		{
			get { return base.MI_Status; }
			set { base.MI_Status = value; }
		}

		[List("Lookups.Directions")]
		public override ZString MI_Direction
		{
			get { return base.MI_Direction; }
			set { base.MI_Direction = value; }
		}

		public ZDateTime HeaderReceivedTimeUtc
		{
			get
			{
				if (!headerReceivedTimeUtc.IsValid)
				{
					headerReceivedTimeUtc = GetUTCDateTimeFromString(GetHeaderItem((NoResString)"Received"));
					if (!headerReceivedTimeUtc.IsValid)
					{
						headerReceivedTimeUtc = GetUTCDateTimeFromString(GetHeaderItem((NoResString)"Date"));
					}

					if (!headerReceivedTimeUtc.IsValid)
					{
						headerReceivedTimeUtc = MI_ReceivedDateTime;
					}
				}
				return headerReceivedTimeUtc;
			}
		}
		ZDateTime headerReceivedTimeUtc;

#if DEBUG
		public void ResetReceivedUTCTimeForTesting()
		{
			headerReceivedTimeUtc = ZDateTime.Empty;
		}
#endif

		readonly string[] months = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		ZDateTime GetUTCDateTimeFromString(string inputString)
		{
			ZDateTime result = ZDateTime.Empty;

			Regex r = new Regex(@"(?<day>\d{1,2}) (?<month>\w{3}) (?<year>\d{4}) (?<hour>\d{1,2}):(?<minute>\d{1,2}):(?<second>\d{1,2}) (?<offsetsign>[+-])(?<offsethours>\d{2})(?<offsetminutes>\d{2})",
				RegexOptions.Compiled);

			if (r.IsMatch(inputString))
			{
				int day;
				int.TryParse(r.Match(inputString).Result("${day}"), out day);

				string monthString = r.Match(inputString).Result("${month}").ToUpper();
				int month = Array.IndexOf(months, monthString) + 1;
				if (month < 1)
				{
					int.TryParse(monthString, out month);
				}

				int year;
				int.TryParse(r.Match(inputString).Result("${year}"), out year);

				int hour;
				int.TryParse(r.Match(inputString).Result("${hour}"), out hour);

				int minute;
				int.TryParse(r.Match(inputString).Result("${minute}"), out minute);

				int second;
				int.TryParse(r.Match(inputString).Result("${second}"), out second);

				string offsetSign = r.Match(inputString).Result("${offsetsign}");

				int offsetHours;
				int.TryParse(r.Match(inputString).Result("${offsethours}"), out offsetHours);

				int offsetMinutes;
				int.TryParse(r.Match(inputString).Result("${offsetminutes}"), out offsetMinutes);

				result = new ZDateTime(year, month, day, hour, minute, second);
				result = result.AddHours(offsetSign == "-" ? offsetHours : -offsetHours);
				result = result.AddMinutes(offsetSign == "-" ? offsetMinutes : -offsetMinutes);
			}
			return result;
		}

		public string RawEmailString
		{
			get
			{
				if (MI_Header.Length > 0 && MI_Header.EndsWith("\r\n"))
				{
					return MI_Header + "\r\n" + MI_Body;
				}
				else
				{
					return MI_Header + "\r\n\r\n" + MI_Body;
				}
			}
		}

		public bool IsSingleAttachmentMIMEEmail
		{
			get
			{
				if (!IsMIMEEmail)
				{
					return false;
				}

				if (!MI_Body.IsEmpty)
				{
					return false;
				}

				if (MailAttachments.Count != 1)
				{
					return false;
				}

				return true;
			}
		}

		public bool IsMIMEEmail
		{
			get
			{
				ZString mimeVersion = GetHeaderItem("Mime-Version");
				return (mimeVersion != ZString.Empty);
			}
		}

		public string GetMimeMessageBody() => this.BuildMimeMessage().TextBody;

		public ZString GetHeaderItem(ZString index)
		{
			Regex headerRegex = new Regex(@"(^|(\r\n))" + Regex.Escape(index) + @"\s*:\s*(?<value>.*?)($|\r\n($|\S))", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			Match match = headerRegex.Match(MI_Header);
			if (match.Success)
			{
				return match.Groups["value"].Value.Replace("\r\n", "");
			}
			else
			{
				return ZString.Empty;
			}
		}

		public string[] GetAllHeaderKeys()
		{
			ArrayList result = new ArrayList();
			string[] headerLines = ((string)MI_Header).Split("\n".ToCharArray());
			for (int i = 0; i < headerLines.Length; i++)
			{
				if (headerLines[i].IndexOf(":") != -1)
				{
					result.Add(headerLines[i].Substring(0, headerLines[i].IndexOf(":")));
				}
			}
			return (string[])result.ToArray(typeof(string));
		}

		public void SetHeaderItem(ZString index, ZString value)
		{
			Regex headerRegex = new Regex(@"(?<=^|(\r\n))" + Regex.Escape(index) + @"\s*:.*?($|\r\n(?=$|\S))", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			string newHeader = value.IsEmpty ? "" : index + ": " + value + "\r\n";
			if (headerRegex.IsMatch(MI_Header))
			{
				MI_Header = headerRegex.Replace(MI_Header, newHeader);
			}
			else
			{
				MI_Header += newHeader;
			}
		}

		public override void Delete()
		{
			for (int i = MailRecipients.Count - 1; i >= 0; i--)
			{
				MailRecipients[i].Delete();
			}
			for (int i = MailAttachments.Count - 1; i >= 0; i--)
			{
				MailAttachments[i].Delete();
			}
			base.Delete();
		}

		public void CopyValuesFrom(MailItem sourceItem)
		{
			base.CopyValuesFrom(sourceItem);
			CopyRecipientsFrom(sourceItem);
			CopyAttachmentsFrom(sourceItem);
		}

		public void CopyRecipientsFrom(MailItem sourceItem)
		{
			foreach (MailRecipient sourceRecipient in sourceItem.MailRecipientsCore)
			{
				MailRecipient newRecipient = Factory.New<MailRecipient>();
				newRecipient.CopyValuesFrom(sourceRecipient);
				newRecipient.MR_MI = PK;
				MailRecipientsCore.Add(newRecipient);
			}
		}

		public void CopyAttachmentsFrom(MailItem sourceItem)
		{
			foreach (MailAttachment sourceAttachment in sourceItem.MailAttachments)
			{
				MailAttachment newAttachment = Factory.New<MailAttachment>();
				newAttachment.CopyValuesFrom(sourceAttachment);
				newAttachment.MA_MI = PK;
				MailAttachments.Add(newAttachment);
			}
		}

		public int SizeOfBodyAndAttachments
		{
			get
			{
				int result = 0;
				foreach (MailAttachment attachment in this.MailAttachments)
				{
					result += attachment.MA_Data.Length;
				}
				result += MI_Body.Length;
				return result;
			}
		}

		#region BodyFirst2K

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public ZString BodyFirst2K
		{
			get
			{
				if (fBodyFirst2K == null)
				{
					var command = Db.Connection.Command("SELECT SUBSTRING(" + Schema.MI_Body + ", 1, 2048) FROM " + Schema.TableName +
															" WHERE " + Schema.PK + " = @PK");
					command.AddParameterBasedOnDbColumn("@PK", PK.ToGuid(), MailDBItemsSchema.PK);

					object result = command.ExecuteScalar();
					if (result == null)
					{
						fBodyFirst2K = new ZString("");
					}
					else
					{
						fBodyFirst2K = new ZString((string)result);
					}
				}
				return (ZString)fBodyFirst2K;
			}
		}

		object fBodyFirst2K;

		public ZPropertyInfo BodyFirst2KInfo
		{
			get { return GetZPropertyInfo(nameof(BodyFirst2K)); }
		}

		#endregion

		#region SaveAttachments

		public
#if DEBUG
			virtual
#endif
			void SaveAttachmentsTo(MailAttachment[] attachments, string destinationDir)
		{
			foreach (MailAttachment attachment in attachments)
			{
				attachment.SaveTo(destinationDir);
			}
		}

		public void SaveAllAttachmentsTo(string destinationDir)
		{
			SaveAttachmentsTo((MailAttachment[])MailAttachments.ToArray(typeof(MailAttachment)), destinationDir);
		}

		public
#if DEBUG
			virtual
#endif
			void SaveEntireEmailAsEml(Stream targetStream)
		{
			using (targetStream)
			{
				this.BuildMimeMessage().WriteTo(targetStream);
			}
		}

		#endregion

		#region ExtractAttachments
#if DEBUG

		public void ExtractAttachments()
		{
			ExtractAttachments(MimeMessageExtensions.CreateMessageFromEml(RawEmailBytes.ToArray()));
		}

		void ExtractAttachments(MimeMessage message)
		{
			var attachments = message.GetFullAttachments();
			foreach (var attachment in attachments)
			{
				var fileName = attachment.GetName();
				if (!string.IsNullOrEmpty(fileName))
				{
					var mailAttachment = MailAttachments.AddNew();
					mailAttachment.MA_Data = attachment.GetData();
					mailAttachment.MA_FileName = fileName;
				}
			}
		}

#endif
		#endregion

		public bool HasContent
		{
			get
			{
				if (!MI_Body.IsEmpty)
				{
					return true;
				}

				if (MailAttachments.Count > 0)
				{
					foreach (MailAttachment attachment in MailAttachments)
					{
						if (!attachment.MA_Data.IsEmpty)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		public X509Certificate2 SigningCertificate
		{
			get
			{
				return fSigningCertificate;
			}
		}
		X509Certificate2 fSigningCertificate;

		[Serializable]
		public class NoContentException : ApplicationException
		{
			public NoContentException()
			{
			}

#if NETFRAMEWORK
			protected NoContentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		public ZString GetDecodedEmailText(Store decryptionStore, Certificate trustPointCertificate, string signersName, bool decryptInNewTask)
		{
			if (!HasContent)
			{
				throw new NoContentException();
			}

			string rawEmailBody;
			if (MailAttachments.Count > 0)
			{
				byte[] attachmentBytes = MailAttachments[0].MA_Data;
				rawEmailBody = System.Convert.ToBase64String(attachmentBytes);
			}
			else
			{
				rawEmailBody = RawEmailString.Substring(RawEmailString.IndexOf("\r\n\r\n") + 4);
			}

			if (decryptionStore != null)
			{
				Decryptor decryptor = new Decryptor(decryptionStore, rawEmailBody, SystemDataRegistry.Instance.UseWindowsCertificateStore.Value);
				if (decryptInNewTask)
				{
					using var cts = new CancellationTokenSource();
					var decrypTask = Task.Run(() => decryptor.RunDecrypt(cts.Token), cts.Token);
					bool executionFinished = decrypTask.Wait(SystemDataRegistry.Instance.MessageDecryptTimeout.Value * 60000);
					if (!executionFinished)
					{
						cts.Cancel();
						throw new System.Security.Cryptography.CryptographicException(Res.GetString("b547203e-c35a-4549-8aa5-7078b3062344", "Timeout decrypting message"));
					}
				}
				else
				{
					decryptor.RunDecrypt();
				}
				if (decryptor.Ex != null)
				{
					throw decryptor.Ex;
				}
				string signedSMIMEString = decryptor.SignedSMIMEString;
				rawEmailBody = signedSMIMEString.Substring(signedSMIMEString.IndexOf("\r\n\r\n") + 4);
			}

			if (trustPointCertificate != null && signersName != null)
			{
				try
				{
					var verifiedMessage = trustPointCertificate.VerifyBytes(rawEmailBody, signersName);
					rawEmailBody = verifiedMessage.Content;
					fSigningCertificate = verifiedMessage.SigningCertificate;
				}
				catch (FormatException ex)
				{
					throw new MailItemDecodeFail(Res.GetString("e96699be-57f4-4297-92e3-b5ee6990f74c", "Decoding mail item from '{0}' failed", signersName), rawEmailBody, ex);
				}
			}
			return rawEmailBody;
		}

		#region Properties

		#region Calculated Local Time Properties

		public ZDateTime CalcReceivedDateTimeLocal
		{
			get
			{
				return (MI_ReceivedDateTime.IsValid) ?
					Env.Time.GetLocalTimeFromUtc(MI_ReceivedDateTime.ToDateTime()) :
					ZDateTime.Empty;
			}
		}

		public ZPropertyInfo CalcReceivedDateTimeLocalInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CalcReceivedDateTimeLocal)); }
		}

		public ZDateTime CalcSendDateTimeLocal
		{
			get
			{
				return (MI_SendDateTime.IsValid) ?
					Env.Time.GetLocalTimeFromUtc(MI_SendDateTime.ToDateTime()) :
					ZDateTime.Empty;
			}
		}

		public ZPropertyInfo CalcSendDateTimeLocalInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CalcSendDateTimeLocal)); }
		}

		#endregion

		#region Bounce Back Properties

		public ZBool OverrideMailSettingForBounceBackCapturing => !MI_BusinessEntityID.IsEmpty;

		public ZString MI_BusinessEntityID
		{
			get => GetHeaderItem(MI_BusinessEntityIDHeaderIndex);
			set => SetHeaderItem(MI_BusinessEntityIDHeaderIndex, value);
		}

		public ZString MI_BusinessEntityTableCode
		{
			get => GetHeaderItem(MI_BusinessEntityTableCodeHeaderIndex);
			set => SetHeaderItem(MI_BusinessEntityTableCodeHeaderIndex, value);
		}

		public ZString MI_BusinessEntityjobNumber
		{
			get => GetHeaderItem(MI_BusinessEntityJobNumberHeaderIndex);
			set => SetHeaderItem(MI_BusinessEntityJobNumberHeaderIndex, value);
		}

		public ZString MI_DocumentName
		{
			get => BounceEmailParser.DecodeDocumentName(GetHeaderItem(MI_DocumentNameHeaderIndex));
			set => SetHeaderItem(MI_DocumentNameHeaderIndex, BounceEmailParser.EncodeDocumentName(value));
		}

		public ZString EncodedDocumentName => GetHeaderItem(MI_DocumentNameHeaderIndex);

		public ZString MI_SenderStaffID
		{
			get => GetHeaderItem(MI_SenderStaffIDHeaderIndex);
			set => SetHeaderItem(MI_SenderStaffIDHeaderIndex, value);
		}

		public ZString MI_ListUnsubscribe
		{
			get => GetHeaderItem(MI_ListUnsubscribeHeaderIndex);
			set => SetHeaderItem(MI_ListUnsubscribeHeaderIndex, value);
		}

		internal const string MI_BusinessEntityIDHeaderIndex = BounceEmailParser.BounceEmailConstants.BusinessEntityIDKey;
		internal const string MI_BusinessEntityTableCodeHeaderIndex = BounceEmailParser.BounceEmailConstants.BusinessEntityTableCodeKey;
		internal const string MI_BusinessEntityJobNumberHeaderIndex = BounceEmailParser.BounceEmailConstants.BusinessEntityJobNumberKey;
		internal const string MI_DocumentNameHeaderIndex = BounceEmailParser.BounceEmailConstants.DocumentNameKey;
		internal const string MI_SenderStaffIDHeaderIndex = BounceEmailParser.BounceEmailConstants.SenderStaffIDKey;
		internal const string MI_ReturnPathHeaderIndex = BounceEmailParser.BounceEmailConstants.ReturnPathKey;
		internal const string MI_ListUnsubscribeHeaderIndex = "List-Unsubscribe";

		#endregion

		#region Overridden Properties

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded index value")]
		public override ZString MI_Subject
		{
			get { return base.MI_Subject; }
			set
			{
				base.MI_Subject = value;
				SetHeaderItem("Subject", value);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-code header item")]
		public override ZString MI_From
		{
			get { return base.MI_From; }
			set
			{
				if (value.StartsWith("<", StringComparison.Ordinal) && value.EndsWith(">", StringComparison.Ordinal))
				{
					value = value.TrimStart('<').TrimEnd('>');
				}

				base.MI_From = value;
				SetHeaderItem("From", ReformatEmailAddress(value));
			}
		}

		public override ZString MI_ReplyTo
		{
			get { return base.MI_ReplyTo; }
			set
			{
				if (value.StartsWith("<", StringComparison.Ordinal) && value.EndsWith(">", StringComparison.Ordinal))
				{
					value = value.TrimStart('<').TrimEnd('>');
				}

				base.MI_ReplyTo = value;
				SetHeaderItem("ReplyTo", ReformatEmailAddress(value));
			}
		}

		string ReformatEmailAddress(string emailAddress)
		{
			string result = emailAddress.Trim(' ');
			int addressStart = result.IndexOf('<');
			int addressEnd = result.LastIndexOf('>');
			if (addressStart > 0 && addressEnd > addressStart)
			{
				string displayName = result.Substring(0, addressStart);
				string address = result.Substring(addressStart, addressEnd - addressStart + 1);

				displayName = displayName.TrimStart('"', ' ');
				displayName = displayName.TrimEnd(' ', '\\', '"');
				Regex re = new Regex(@"(?<!\\)([""\r\.,()])");
				displayName = re.Replace(displayName, @"\$1");

				result = @"""" + displayName + @""" " + address;
			}

			return result;
		}

		/// <summary>
		/// The FROM field in the email sometimes contains other info e.g. '"John Smith" <jsmith@test.com>'
		/// make sure we only get the email address i.e. 'jsmith@test.com'
		/// </summary>
		public string GetFromEmailAddress()
		{
			return GetFromEmailAddress(MI_From);
		}

		string GetFromEmailAddress(string fromInformation)
		{
			if (MailboxAddress.TryParse(fromInformation, out var mailboxAddress))
			{
				return mailboxAddress.Address ?? string.Empty;
			}

			return string.Empty;
		}

		internal string TruncateSenderFromInfo(string headerFrom, int maxLength)
		{
			if (headerFrom.Length <= maxLength)
			{
				return headerFrom;
			}
			else
			{
				var emailAddress = GetFromEmailAddress(headerFrom);

				if (emailAddress.Length > maxLength)
				{
					throw new InvalidOperationException($"Email address is longer than max length. headerFrom:{headerFrom}, maxLength:{maxLength}");
				}
				else if(emailAddress.Length > maxLength - 2)
				{
					return emailAddress;    // If property has no space for angle brackets, sender name will be omitted so the existing of angle brackets is not important
				}

				emailAddress = "<" + emailAddress + ">";
				var sender = headerFrom.Substring(0, headerFrom.Length - emailAddress.Length);
				var truncatedSender = sender.Substring(0, maxLength - emailAddress.Length);

				return truncatedSender + emailAddress;
			}
		}

		ZString TryDecode(ZString headerItem)
		{
			ZString value;

			try
			{
				value = Rfc2047.DecodeText(Encoding.UTF8.GetBytes(headerItem));
			}
			catch (Exception ex) when (ex.Source.StartsWith(nameof(MimeKit)))
			{
				value = headerItem;
			}

			return value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded header item")]
		public override ZString MI_Header
		{
			get
			{
				if (DataRegistry.Instance.RemoveNonAsciiCharactersInEmailMessageHeader)
				{
					return base.MI_Header.ReplaceNonBreakingSpaceWithNormalSpace().RemoveNonASCIICharacters();
				}
				else
				{
					return base.MI_Header.ReplaceNonBreakingSpaceWithNormalSpace().RemoveUnsupportedPunctuations();
				}
			}
			set
			{
				if (DataRegistry.Instance.RemoveNonAsciiCharactersInEmailMessageHeader)
				{
					base.MI_Header = value.ReplaceNonBreakingSpaceWithNormalSpace().RemoveNonASCIICharacters();
				}
				else
				{
					base.MI_Header = value.ReplaceNonBreakingSpaceWithNormalSpace().RemoveUnsupportedPunctuations();
				}
				if (MI_From.IsEmpty)
				{
					ZString headerFrom = GetHeaderItem("From");

					if (!headerFrom.IsEmpty)
					{
						headerFrom = TryDecode(headerFrom);

						base.MI_From = TruncateSenderFromInfo(headerFrom, MI_FromInfo.MaxLength);
					}
				}

				if (MI_ReplyTo.IsEmpty)
				{
					ZString headerReplyTo = GetHeaderItem("ReplyTo");

					if (!headerReplyTo.IsEmpty)
					{
						headerReplyTo = TryDecode(headerReplyTo);
						base.MI_ReplyTo = TruncateSenderFromInfo(headerReplyTo, MI_ReplyToInfo.MaxLength);
					}
				}

				if (MI_Subject.IsEmpty)
				{
					ZString headerSubject = GetHeaderItem("Subject");

					if (!headerSubject.IsEmpty)
					{
						headerSubject = TryDecode(headerSubject);
						base.MI_Subject = headerSubject.Left(MI_SubjectInfo.MaxLength);
					}
				}

				string sentDateTimeString = GetHeaderItem("Date").ToString();
				DateTime sentDateTime;
				if (DateTime.TryParse(sentDateTimeString, out sentDateTime) &&
					sentDateTime >= ZDateTime.MinSmallDateTimeValue && sentDateTime <= ZDateTime.MaxSmallDateTimeValue)
				{
					MI_SendDateTime = new ZDateTime(sentDateTime.ToUniversalTime());
				}

				if (MailRecipients.Count == 0)
				{
					if (InternetAddressList.TryParse(GetHeaderItem(HeaderId.To.ToHeaderName()), out var toRecipients))
					{
						AddRecipients(toRecipients.Mailboxes, MailRecipient.RecipientTypes.TO);
					}

					if (InternetAddressList.TryParse(GetHeaderItem(HeaderId.Cc.ToHeaderName()), out var ccRecipients))
					{
						AddRecipients(ccRecipients.Mailboxes, MailRecipient.RecipientTypes.CC);
					}
				}
			}
		}

#if DEBUG
		internal
#endif
			void AddRecipients(IEnumerable<MailboxAddress> recipients, MailRecipient.RecipientTypes recipientType)
		{
			foreach (var recipient in recipients)
			{
				var myRecipient = AddRecipientCore(recipient.RenderAddressWithBrackets(), MailRecipient.RecipientTypes.TO);
				if (myRecipient != null)
				{
					myRecipient.MR_DeliveredTime = ZDateTime.Empty;
					myRecipient.MR_LastAttempt = ZDateTime.Empty;
					myRecipient.MR_RecipientType = recipientType.ToString();
				}
			}
		}

		#endregion

		#endregion

		#region Related Business Objects
		public virtual MailAttachmentCollection MailAttachments
		{
			get
			{
				if (fMailAttachments == null)
				{
					fMailAttachments = new MailAttachmentCollection(this, Factory);
					fMailAttachments.Load();
				}
				return fMailAttachments;
			}
		}

		public MailRecipientReadonlyCollection MailRecipients
		{
			get
			{
				if (fMailRecipients == null)
				{
					fMailRecipients = new MailRecipientReadonlyCollection(MailRecipientsCore);
				}
				return fMailRecipients;
			}
		}

#if DEBUG
		protected
#endif
			MailRecipientCollection MailRecipientsCore
		{
			get
			{
				if (fMailRecipientsCore == null)
				{
					fMailRecipientsCore = new MailRecipientCollection(this, Factory);
					fMailRecipientsCore.Load();
				}
				return fMailRecipientsCore;
			}
		}

		public MailRecipientCollection MailRecipientsForGUIBinding
		{
			get
			{
				if (fMailRecipientsForGUIBinding == null)
				{
					fMailRecipientsForGUIBinding = new MailRecipientCollection(this, Factory);
					fMailRecipientsForGUIBinding.Load();
					fMailRecipientsForGUIBinding.CountChanged += new CollectionCountChangedEventHandler(fMailRecipientsForGUIBinding_CountChanged);
				}
				return fMailRecipientsForGUIBinding;
			}
		}

		void fMailRecipientsForGUIBinding_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				AddRecipientCore(((MailRecipient)e.BizObject).EmailAddress, MailRecipient.RecipientTypes.TO);
			}
			else
			{
				MailRecipientsCore.Remove(e.BizObject);
			}
		}

		public void RemoveAndDeleteAllMailRecipients()
		{
			MailRecipients.RemoveAndDeleteAll();
		}

		#region creation of emails

		protected virtual IMailRecipient AddRecipientCore(string email, MailRecipient.RecipientTypes type)
		{
			return AddRecipientForUserCommunication(email, type);
		}

		#region email with override

		public IMailRecipient AddRecipientForUserCommunication()
		{
			return AddRecipientForUserCommunication("");
		}

		public IMailRecipient AddRecipientForUserCommunication(string email)
		{
			return AddRecipientForUserCommunication(email, MailRecipient.RecipientTypes.TO);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public IMailRecipient AddRecipientForUserCommunication(string email, MailRecipient.RecipientTypes type)
		{
			if (!IsHostingSupportEmail(email))
			{
				if (!string.IsNullOrEmpty(Env.Registry.EmailDestinationOverride))
				{
					overriddenRecipients.Add(email);
					email = Env.Registry.EmailDestinationOverride;
					if (MailRecipientsCore.ContainsRecipientWithEmail(email))
					{
						return null;
					}
				}
				else if (!ShouldAddRecipientWhenEmailDestinationIsNotOverriden())
				{
					var loggedInUser = EnvProxy.Instance.CurrentUser;
					if (loggedInUser != null && !string.IsNullOrEmpty(loggedInUser.EmailAddress))
					{
						email = loggedInUser.EmailAddress;
					}
					else
					{
						overriddenRecipients.Add(email);
						return null;
					}
				}
			}

			if (email.IsNullOrEmpty())
			{
				var errorMessage = new StringBuilder();
				errorMessage.AppendLine("Recipient Email Address is empty when added to MailItem");
				errorMessage.AppendLine($"MailItem PK: {PK}");
				errorMessage.AppendLine($"MailRecipient.RecipientTypes is {type}");
				ErrorReporter.ReportOnce($"Recipient Email Address can not be empty. Email and recipient are created by Factory : {Factory.NameForDebugging}", errorMessage.ToString());
			}
			return MailRecipientsCore.AddNew(email, type);
		}

		internal HashSet<string> overriddenRecipients = new HashSet<string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "HTML tag, no need to translate")]
		public void AddFooterForOverriddenRecipients()
		{
			if (overriddenRecipients.Any())
			{
				string originalRecipientsMessage = Res.GetString("84f86a63-947d-4b71-9d6f-987eb52a7cd4",
					"(This message was redirected to {0} as it was sent from a non-production system. Originally the email was addressed to {1}.)",
					Env.Registry.EmailDestinationOverride, string.Join(", ", overriddenRecipients));
				if (this.MI_ContentType == EmailContentTypes.HTML.ContentTypeCode)
				{
					this.MI_Body = this.MI_Body + "<br />" + originalRecipientsMessage;
				}
				else if (this.MI_ContentType == EmailContentTypes.PlainText.ContentTypeCode)
				{
					this.MI_Body = this.MI_Body + System.Environment.NewLine + originalRecipientsMessage;
				}
				else
				{
					string htmlExistsPattern = "(\\r\\nX-ALT-DESC;)";
					var match = Regex.Match(MI_Body, htmlExistsPattern);
					if (match.Success)
					{
						string bodyEndPattern = "(</BODY></HTML>)";
						MI_Body = Regex.Replace(MI_Body, bodyEndPattern, originalRecipientsMessage + "<BR />" + "$1");
						string descriptionEndPattern = "(\\r\\nX-ALT-DESC;)";
						MI_Body = Regex.Replace(MI_Body, descriptionEndPattern, originalRecipientsMessage + "\\N" + "$1");
					}
					else
					{
						string descriptionEndPattern = "(\\r\\nBEGIN:VALARM)";
						MI_Body = Regex.Replace(MI_Body, descriptionEndPattern, originalRecipientsMessage + "$1");
					}
				}
			}
		}

		bool IsHostingSupportEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return false;
			}

			if (!EnvProxy.IsHostedWithCargowise)
			{
				return false;
			}

			return email.Equals(EnvProxy.Instance.Registry.HostedNotificationsEmailOverride, StringComparison.OrdinalIgnoreCase);
		}

		bool ShouldAddRecipientWhenEmailDestinationIsNotOverriden()
		{
			bool shouldAddRecipient = true;

			if (!EnvProxy.Instance.IsProductionSystem)
			{
				shouldAddRecipient = false;
			}

#if DEBUG
			if (!MailItem.SkipDebugCheck)
			{
				shouldAddRecipient = Globals.IsTest;
			}
#endif
			return shouldAddRecipient;
		}

#if DEBUG
		[ThreadStatic]
		static bool SkipDebugCheck;

		public static IDisposable SkipDebugCheckForShouldAddRecipientTesting(bool skip)
		{
			return new DebugCheckForTestingSkipper(skip);
		}

		sealed class DebugCheckForTestingSkipper : IDisposable
		{
			public DebugCheckForTestingSkipper(bool skip)
			{
				SkipDebugCheck = skip;
			}

			public void Dispose()
			{
				SkipDebugCheck = false;
			}
		}
#endif

		#endregion

		#region without override

		public IMailRecipient AddRecipientForSystemCommunication(string email, bool ignoreSystemEmailDestinationOverride = false)
		{
			return AddRecipientForSystemCommunication(email, MailRecipient.RecipientTypes.TO, ignoreSystemEmailDestinationOverride);
		}

		public IMailRecipient AddRecipientForSystemCommunication(string email, MailRecipient.RecipientTypes type, bool ignoreSystemEmailDestinationOverride = false)
		{
			if (!ignoreSystemEmailDestinationOverride && Env.Registry.SystemEmailDestinationOverride && !string.IsNullOrEmpty(Env.Registry.EmailDestinationOverride))
			{
				overriddenRecipients.Add(email);
				email = Env.Registry.EmailDestinationOverride;
				if (MailRecipientsCore.ContainsRecipientWithEmail(email))
				{
					return null;
				}
			}
			return MailRecipientsCore.AddNew(email, type);
		}

		#endregion

		#region from recipientdef

		public IMailRecipient AddRecipientFromRecipientDef(RecipientDef originalRecipient, MailRecipient.RecipientTypes type)
		{
			if (originalRecipient.IsForSystemCommunication)
			{
				return AddRecipientForSystemCommunication(originalRecipient.Email, type, originalRecipient.IgnoreSystemEmailDestinationOverride);
			}
			else
			{
				return AddRecipientForUserCommunication(originalRecipient.Email, type);
			}
		}

		#endregion

		#endregion

		public string BodyTextDecoded
		{
			get
			{
				string result;
				if (MI_Header.ToUpper().IndexOf("QUOTED-PRINTABLE") != -1)
				{
					result = new CryptoUtilities.SMIME.QuotedPrintableDecoder().Decode(MI_Body);
				}
				else
				{
					result = MI_Body;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		MailAttachmentCollection fMailAttachments;
		MailRecipientReadonlyCollection fMailRecipients;
		MailRecipientCollection fMailRecipientsCore;
		MailRecipientCollection fMailRecipientsForGUIBinding;

		#endregion
	}

	class Decryptor
	{
		public Decryptor(Store decryptionStore, string rawEmailBody, bool decryptUsingWindowsCertificateStore)
		{
			this.decryptUsingWindowsCertificateStore = decryptUsingWindowsCertificateStore;
			this.decryptionStore = decryptionStore;
			this.rawEmailBody = rawEmailBody;
		}
		readonly Store decryptionStore;
		readonly string rawEmailBody;
		readonly bool decryptUsingWindowsCertificateStore;

		public string SignedSMIMEString { get; set; }
		public Exception Ex { get; set; }

		public void RunDecrypt(CancellationToken token = new CancellationToken())
		{
			SignedSMIMEString = string.Empty;
			Ex = null;
			try
			{
				SignedSMIMEString = decryptUsingWindowsCertificateStore ?
					decryptionStore.DecryptUsingWindowsCertificateStore(rawEmailBody, token) :
					decryptionStore.DecryptUsingEnterpriseCertificateStore(rawEmailBody, token);
				token.ThrowIfCancellationRequested();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Ex = e;
			}
		}
	}
}
