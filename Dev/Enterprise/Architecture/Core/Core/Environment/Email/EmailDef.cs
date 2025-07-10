using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class EmailDef
	{
		#region Constructors

		public EmailDef()
		{
			IUser currentUser = EnvProxy.Instance.CurrentUser;

			var fullName = string.Empty;
			var emailAddress = string.Empty;
			var isSystemAccount = false;

			if ((currentUser != null) && !currentUser.IsBatchProcessor)
			{
				senderStaffPK = currentUser.PK;
				fullName = currentUser.FullName;
				emailAddress = currentUser.EmailAddress;
				isSystemAccount = currentUser.IsSystemAccount;
			}

			SetEmailInfo(fullName, emailAddress, isSystemAccount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public EmailDef(Guid senderGlbStaffPK)
		{
			senderStaffPK = senderGlbStaffPK;

			var fullName = string.Empty;
			var emailAddress = string.Empty;
			var isSystemAccount = false;

			if (senderGlbStaffPK != Guid.Empty)
			{
				using (DbCommand query = Db.Connection.Command(
					"SELECT " + GlbStaffSchema.Constants.GS_FullName + ", "
					+ GlbStaffSchema.Constants.GS_EmailAddress + ", "
					+ GlbStaffSchema.Constants.GS_IsSystemAccount
					+ " FROM " + GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName
					+ " WHERE " + GlbStaffSchema.Constants.PK + " = @GS_PK")) // Structured Query Language
				{
					query.AddParameterBasedOnDbColumn("@GS_PK", senderGlbStaffPK, GlbStaffSchema.PK);

					using (var reader = query.ExecuteReader())
					{
						if (reader.Read())
						{
							fullName = reader.GetString(0);
							emailAddress = reader.GetString(1);
							isSystemAccount = reader.GetBoolean(2);
						}
						else
						{
							Globals.Message.ShowDeveloperException(new ArgumentException("GS_PK not found", nameof(senderGlbStaffPK)));
						}
					}
				}
			}

			SetEmailInfo(fullName, emailAddress, isSystemAccount);
		}

		void SetEmailInfo(string fullName, string emailAddress, bool isSystemAccount)
		{
			DataRegistry registry = EnvProxy.Instance.Registry;

			if (registry?.AllowEmailsToBeSentFromUsersAddress ?? false)
			{
				if (!isSystemAccount)
				{
					FromDisplayName = fullName;
				}

				FromAddress = emailAddress;
			}

			if (string.IsNullOrEmpty(FromAddress))
			{
				FromAddress = GetDefaultFromEmailAddress();
			}

			if (string.IsNullOrEmpty(FromDisplayName))
			{
				FromDisplayName = registry?.MailboxDisplayName ?? string.Empty;
			}
		}

		#endregion

		#region methods

		#region GetDefaultFromEmailAddress

		string GetDefaultFromEmailAddress()
		{
			string result = string.Empty;
			DataRegistry registry = EnvProxy.Instance.Registry;
			if (registry != null)
			{
				string smtpDefaultReturnEmailAddress = registry.SMTPDefaultReturnEmailAddress;
				if (!string.IsNullOrEmpty(smtpDefaultReturnEmailAddress))
				{
					result = smtpDefaultReturnEmailAddress;
				}
				else
				{
					result = registry.MailboxEmailAddress;
				}
			}
			return result;
		}

		#endregion

		#region Add Recipients

		#region Add Group Recipients

		public void AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Guid groupPK, IRegistryItem groupLocation)
		{
			string language;
			AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupPK, groupLocation, out language);
		}

		public void AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Guid groupPK, IRegistryItem groupLocation, out string language)
		{
			AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupPK, ResString.GetMultilingualString("33caf23f-7b8d-4e3c-9457-e05e97ca8edd", "System Registry: {0}", ((IMultilingualRegistryItem)groupLocation).LocationMultilingual), out language);
		}

		public void AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Guid groupPK, MultilingualString groupLocation)
		{
			string language;
			AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupPK, groupLocation, out language);
		}

		public void AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Guid groupPK, MultilingualString groupLocation, out string language)
		{
			var util = new EmailGroupUtility();
			var recipients = util.GetGroupEmailCollection(groupPK, false, out language);
			if (recipients.Count > 0)
			{
				foreach (string recipient in recipients)
				{
					AddRecipientForUserCommunication(recipient, RecipientDef.RecipientTypes.TO);
				}
				using (Res.TemporarilySwitchLanguage(language))
				{
					FooterText += Res.GetString("41cc003d-97cf-4bc0-a9a3-500282f6504a", "You have received this email because you are a member of the staff group defined at {0}.", groupLocation);
				}
			}
			else
			{
				recipients = util.GetGroupEmailCollection(GetGlbGroupAllPK(), false, out language);
				if (recipients.Count > 0)
				{
					foreach (string recipient in recipients)
					{
						AddRecipientForUserCommunication(recipient, RecipientDef.RecipientTypes.TO);
					}
					using (Res.TemporarilySwitchLanguage(language))
					{
						FooterText += Res.GetString("4337a15b-7471-46f4-b076-68c2f8bb3d1f", "This email is sent to the group defined at {0}. Since there are no valid email addresses set up in this group this email has been sent to all users.", groupLocation);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		public static Guid GetGlbGroupAllPK()
		{
			string sqlText = string.Format(@"
				SELECT {0}
				FROM {1}
				WHERE {2} = @code",
				GlbGroupSchema.PK.Name,
				GlbGroupSchema.Constants.TableName,
				GlbGroupSchema.GG_Code.Name);
			return (Guid)Db.Connection.ExecuteScalar(sqlText, cmd => cmd.AddParameterBasedOnDbColumn("@code", Constants.Groups.ALL, GlbGroupSchema.GG_Code)); // User class uses complex SQL scripts that can't be accomplished by using Business Objects
		}

		#endregion

		#region AddRecipientForUserCommunication

		public void AddRecipientForUserCommunication(string email)
		{
			AddRecipientForUserCommunication(email, RecipientDef.RecipientTypes.TO);
		}

		public void AddRecipientForUserCommunication(StringCollection emails)
		{
			AddRecipientForUserCommunication(emails, RecipientDef.RecipientTypes.TO);
		}

		public void AddRecipientForUserCommunication(StringCollection emails, RecipientDef.RecipientTypes type)
		{
			foreach (string email in emails)
			{
				AddRecipientForUserCommunication(email, type);
			}
		}

		public void AddRecipientForUserCommunication(string[] emails)
		{
			AddRecipientForUserCommunication(emails, RecipientDef.RecipientTypes.TO);
		}

		public void AddRecipientForUserCommunication(string[] emails, RecipientDef.RecipientTypes type)
		{
			foreach (string email in emails)
			{
				AddRecipientForUserCommunication(email, type);
			}
		}

		public void AddRecipientForUserCommunication(string email, RecipientDef.RecipientTypes type)
		{
			AddRecipient(email, type, false);
		}

		#endregion

		#region AddRecipientForSystemCommunication

		public void AddRecipientForSystemCommunication(string email, bool ignoreSystemEmailDestinationOverride = false)
		{
			AddRecipientForSystemCommunication(email, RecipientDef.RecipientTypes.TO, ignoreSystemEmailDestinationOverride);
		}

		public void AddRecipientForSystemCommunication(string[] emails, bool ignoreSystemEmailDestinationOverride = false)
		{
			AddRecipientForSystemCommunication(emails, RecipientDef.RecipientTypes.TO, ignoreSystemEmailDestinationOverride);
		}

		public void AddRecipientForSystemCommunication(string[] emails, RecipientDef.RecipientTypes type, bool ignoreSystemEmailDestinationOverride = false)
		{
			foreach (string email in emails)
			{
				AddRecipientForSystemCommunication(email, type, ignoreSystemEmailDestinationOverride);
			}
		}

		public void AddRecipientForSystemCommunication(string email, RecipientDef.RecipientTypes type, bool ignoreSystemEmailDestinationOverride = false)
		{
			AddRecipient(email, type, true, ignoreSystemEmailDestinationOverride);
		}

		#endregion

		internal void AddRecipient(string email, RecipientDef.RecipientTypes type, bool isSystem, bool ignoreSystemEmailDestinationOverride = false)
		{
			Func<RecipientDef, bool> dupCheck = r => string.Compare(r.Email, email, StringComparison.CurrentCultureIgnoreCase) != 0;

			if (RecipientsCore.All(dupCheck) && CCRecipientsCore.All(dupCheck) && BCCRecipientsCore.All(dupCheck))
			{
				var recipient = new RecipientDef(email, isSystem, ignoreSystemEmailDestinationOverride);

				switch (type)
				{
					case RecipientDef.RecipientTypes.TO:
						RecipientsCore.Add(recipient);
						break;
					case RecipientDef.RecipientTypes.CC:
						CCRecipientsCore.Add(recipient);
						break;
					case RecipientDef.RecipientTypes.BCC:
						BCCRecipientsCore.Add(recipient);
						break;
				}
			}
		}

		#endregion

		#endregion

		#region Properties

		internal static Guid CurrentUserPK
		{
			get
			{
				IUser currentUser = EnvProxy.Instance.CurrentUser;
				return (currentUser == null) || currentUser.IsBatchProcessor ? Guid.Empty : currentUser.PK;
			}
		}

		#region Sender Staff PK

		public ZGuid SenderStaffPK => senderStaffPK;
		readonly Guid senderStaffPK;

		#endregion

		#region ContentType

		public EmailContentTypes ContentType
		{
			get
			{
				if (fContentType == null)
				{
					fContentType = EmailContentTypes.PlainText;
				}
				return fContentType;
			}
			set { fContentType = value; }
		}
		EmailContentTypes fContentType;

		#endregion

		#region Headers

		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		#endregion

		#region Body

		public string Body
		{
			get
			{
				string result = fBody;

				if (!string.IsNullOrEmpty(FooterText))
				{
					result = result + "\r\n\r\n" + FooterText;
				}

				return result;
			}
			set
			{
				fBody = value;
			}
		}

		protected string fBody = "";

		#endregion

		#region From Address

		public string FromAddress
		{
			get { return fFrom; }
			set
			{
				if (value.Contains("<") && value.Contains(">"))
				{
					int lastIndexOfLT = value.LastIndexOf('<');
					int firstIndexOfGT = value.IndexOf('>', lastIndexOfLT);
					if (firstIndexOfGT > lastIndexOfLT)
					{
						fFrom = value.Substring(lastIndexOfLT + 1, firstIndexOfGT - 1 - lastIndexOfLT).Trim();
						if (lastIndexOfLT > 0)
						{
							FromDisplayName = value.Substring(0, lastIndexOfLT).Trim();
						}
					}
				}
				else
				{
					fFrom = value.Trim();
				}
			}
		}

		string fFrom = "";

		#endregion

		#region BusinessEntity ID

		public ZGuid BusinessEntityID
		{
			get { return businessEntityID; }
			private set { businessEntityID = value; }
		}

		protected ZGuid businessEntityID;

		#endregion

		#region BusinessEntity TableCode

		public string BusinessEntityTableCode
		{
			get { return businessEntityTableCode; }
			private set { businessEntityTableCode = value; }
		}
		protected string businessEntityTableCode = "";

		#endregion

		#region BusinessEntityJobNumber

		public string BusinessEntityJobNumber
		{
			get { return businessEntityJobNumber; }
			private set { businessEntityJobNumber = value; }
		}
		protected string businessEntityJobNumber = "";

		#endregion

		public void SetupBusinessEntityInfo(BusinessObject sourceBusinessObject)
		{
			if (sourceBusinessObject != null)
			{
				BusinessEntityID = sourceBusinessObject.PK;
				BusinessEntityTableCode = sourceBusinessObject.TablePrefix;
			}
		}

		public void SetupBusinessEntityInfo(ZGuid sourceBusinessObjectPk, string sourceBusinessObjectTableCode, string sourceBusinessObjectJobNumber)
		{
			BusinessEntityID = sourceBusinessObjectPk;
			BusinessEntityTableCode = sourceBusinessObjectTableCode;
			BusinessEntityJobNumber = sourceBusinessObjectJobNumber;
		}

		#region QueueWithLowPriority

		public bool QueueWithLowPriority { get; set; }

		#endregion

		#endregion

		#region Document Name

		public string DocumentName
		{
			get { return documentName; }
			set { documentName = value; }
		}
		string documentName = "";

		#endregion

		#region List-Unsubscribe

		public string ListUnsubscribe { get; set; } = "";

		#endregion

		#region AttachHeaderAndFooterImages

		public void AttachHeaderImage(ImageRegistryItem headerImage)
		{
			if (headerImage.Value != null)
			{
				MemoryStream imageStream = new MemoryStream();
				headerImage.Value.Save(imageStream, ImageFormat.Jpeg);
				Attachments.Add(new AttachmentDef("Banner.jpg", imageStream.ToArray()));
			}
		}

		public void AttachFooterImage(ImageRegistryItem footerImage)
		{
			if (footerImage.Value != null)
			{
				MemoryStream imageStream = new MemoryStream();
				footerImage.Value.Save(imageStream, ImageFormat.Jpeg);
				Attachments.Add(new AttachmentDef("Footer.jpg", imageStream.ToArray()));
			}
		}

		#endregion

		#region Priority

		public enum PriorityFlag
		{
			Medium,
			Low,
			High
		}

		public PriorityFlag Priority
		{
			get { return fPriority; }
			set { fPriority = value; }
		}

		PriorityFlag fPriority;

		#endregion

		#region Recipient Collection

		public string Subject = "";
		public string FromDisplayName = ""; // From display name.
		public string ReplyTo;
		public readonly AttachmentDefCollection Attachments = new AttachmentDefCollection();
		public string FooterText
		{
			get
			{
				if (fFooterText == null)
				{
					fFooterText = string.Empty;
				}

				return fFooterText;
			}
			set { fFooterText = value; }
		}
		string fFooterText;

		RecipientDefReadonlyCollection recipients;
		public RecipientDefReadonlyCollection Recipients
		{
			get
			{
				if (recipients == null)
				{
					recipients = new RecipientDefReadonlyCollection(RecipientsCore);
				}
				return recipients;
			}
		}

		RecipientDefReadonlyCollection ccRecipients;
		public RecipientDefReadonlyCollection CCRecipients
		{
			get
			{
				if (ccRecipients == null)
				{
					ccRecipients = new RecipientDefReadonlyCollection(CCRecipientsCore);
				}
				return ccRecipients;
			}
		}

		RecipientDefReadonlyCollection bccRecipients;
		public RecipientDefReadonlyCollection BCCRecipients
		{
			get
			{
				if (bccRecipients == null)
				{
					bccRecipients = new RecipientDefReadonlyCollection(BCCRecipientsCore);
				}
				return bccRecipients;
			}
		}

		internal List<RecipientDef> RecipientsCore = new List<RecipientDef>();
		readonly List<RecipientDef> CCRecipientsCore = new List<RecipientDef>();
		readonly List<RecipientDef> BCCRecipientsCore = new List<RecipientDef>();

		#endregion

		#region Save

		public TempFile SaveAsEml()
		{
			var message = new MimeMessage();
			var bodyBuilder = new BodyBuilder();

			for (int i = 0; i < Recipients.Count; i++)
			{
				message.To.Add(new MailboxAddress(null, Recipients[i].Email));
			}

			var senderAddress = FromAddress;
			if (string.IsNullOrWhiteSpace(senderAddress))
			{
				senderAddress = "sender@SenderAddress.com";
			}
			var mailboxAddress = new MailboxAddress(FromDisplayName, senderAddress);
			message.Sender = mailboxAddress;
			message.From.Add(mailboxAddress);
			message.Subject = Subject;
			bodyBuilder.HtmlBody = Body;

			foreach (var img in Attachments.Cast<AttachmentDef>())
			{
				bodyBuilder.LinkedResources.Add(img.DisplayName, img.Data).ContentId = img.DisplayName;
			}
			message.Body = bodyBuilder.ToMessageBody();

			var tempfile = TempFile.NewWithExtension("eml");
			message.WriteTo(tempfile.Filename);

			return tempfile;
		}

		#endregion

		#region Test
#if DEBUG

		public string ToStringForTesting()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("To: " + Recipients.RecipientsAsDelimitedString());
			AppendWithTitleIfNotEmpty(result, "CC", CCRecipients.RecipientsAsDelimitedString());
			AppendWithTitleIfNotEmpty(result, "BCC", BCCRecipients.RecipientsAsDelimitedString());
			result.Append("Subject: " + Subject);
			result.Append("---- Body Text ----");
			result.Append(Body);
			return result.ToStringWithNewLineBetweenAppends();
		}

		void AppendWithTitleIfNotEmpty(ZStringBuilder builder, string title, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				builder.Append(title + ": " + value);
			}
		}

#endif
		#endregion
	}
}
