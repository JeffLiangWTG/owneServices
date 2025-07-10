using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.CryptoUtilities.SMIME;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = MailManager.Res;

namespace Enterprise.MailManager
{
	public class OutgoingMailCreator : OutgoingBaseMailCreator<OutgoingMailCreator>
	{
		protected OutgoingMailCreator()
		{
		}
	}

	public class OutgoingBaseMailCreator<T> : IOutgoingMailManager, IOutgoingMIMEManager where T : class
	{
		protected OutgoingBaseMailCreator()
		{
		}
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		public static readonly T Instance = GetInstance();

		static T GetInstance()
		{
			return Activator.CreateInstance(typeof(T), true) as T;
		}

		void IOutgoingMailManager.Create(ITransactionParticipant factory, EmailDef emailToSend)
		{
			BusinessObjectFactory actualFactory = (BusinessObjectFactory)factory;
			Create_Core(actualFactory, emailToSend, "");
		}

		void IOutgoingMailManager.Create(ITransactionParticipant factory, EmailDef emailToSend, Guid recipientGroupPk, IGroupSourceLocator locator)
		{
			BusinessObjectFactory actualFactory = (BusinessObjectFactory)factory;
			var groupDetail = AddGroupRecipientsToEmail(actualFactory, emailToSend, recipientGroupPk, locator);
			Create_Core(actualFactory, emailToSend, groupDetail);
		}

		void IOutgoingMailManager.CreateAndSave(EmailDef emailToSend)
		{
			CreateAndSave_Core(emailToSend, null);
		}

		void IOutgoingMailManager.CreateAndSave(EmailDef emailToSend, BusinessObjectFactory factory)
		{
			CreateAndSave_Core(emailToSend, factory);
		}

		void IOutgoingMailManager.CreateAndSaveToPostmasterGroup(EmailDef emailToSend)
		{
			BusinessObjectFactory fact = new BusinessObjectFactory();
			GlbGroup postMastersGroup = GetPostMasterGroup(fact);
			var groupDetail = AddGroupRecipientsToEmail(fact, emailToSend, postMastersGroup.PK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			CreateAndSave_Core(emailToSend, groupDetail: groupDetail);
		}

		void IOutgoingMailManager.CreateAndAttachToEDocs(ITransactionParticipant factory, EmailDef emailToSend, BusinessObject bizOToAttach, string fileName, string docType, string description)
		{
			var mailItem = Create_Core((BusinessObjectFactory)factory, emailToSend, string.Empty);

			if (bizOToAttach is IDocManagerSupport docManagerSupport)
			{
				using (var emailStream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					mailItem.SaveEntireEmailAsEml(new UnclosableStreamWrapper(emailStream));
					emailStream.Position = 0;

					docManagerSupport.DocManagerInfo.AddFileOrDocument(emailStream, fileName, docType, description: description);
					docManagerSupport.DocManagerInfo.Save();
				}
			}
		}

		GlbGroup GetPostMasterGroup(BusinessObjectFactory factory)
		{
			GlbGroup postMastersGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMastersGroup == null)
			{
				BusinessObjectFactory tmpFactory = new BusinessObjectFactory();
				GlbGroup tmp = tmpFactory.LoadTop1<GlbGroup>(new ZQuery(Enterprise.ZArchitecture.Schema.GlbGroupSchema.GG_Code, "PMG"));
				if (tmp != null)
				{
					tmp.GG_Code += "1";
					tmp.Factory.Save();
				}

				tmp = tmpFactory.NewWithPrimaryKey<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				tmp.GG_Code = "PMG";
				tmp.GG_Desc = (NoResString)"Post Masters";
				tmp.GG_IsSystemDefined = false;
				tmp.GG_IsActive = true;
				tmp.Factory.Save();
				postMastersGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			}
			return postMastersGroup;
		}

		void IOutgoingMailManager.CreateAndSaveToPostmasterGroup(EmailDef emailToSend, BusinessObjectFactory factoryInWhichToSaveLATER)
		{
			GlbGroup postMastersGroup = GetPostMasterGroup(factoryInWhichToSaveLATER);
			var groupDetail = AddGroupRecipientsToEmail(factoryInWhichToSaveLATER, emailToSend, postMastersGroup.PK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			Create_Core(factoryInWhichToSaveLATER, emailToSend, groupDetail);
		}

		void IOutgoingMailManager.CreateAndSave(EmailDef emailToSend, Guid recipientGroupPk, IGroupSourceLocator locator)
		{
			var groupDetail = AddGroupRecipientsToEmail(emailToSend, recipientGroupPk, locator);
			CreateAndSave_Core(emailToSend, groupDetail: groupDetail);
		}

		void IOutgoingMailManager.CreateAndSaveToCompanyNotificationGroup(string subject, string body, EmailContentTypes emailContentType)
		{
			string groupDetail;
			CreateAndSaveWithCopy(subject, body, new EmailGroupUtility().GetCompanyNotificationGroupEmails(out groupDetail).Cast<string>().ToArray(), null, GetGroupDetail(GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup).Location, groupDetail), emailContentType);
		}

		void IOutgoingMailManager.CreateAndSaveWithCopyToCompanyNotificationGroup(string subject, string body, string[] toRecipients, bool copyToNotificationGroupIfRecipientsNotEmpty, EmailContentTypes emailContentType)
		{
			if (toRecipients == null || toRecipients.Length == 0)
			{
				((IOutgoingMailManager)this).CreateAndSaveToCompanyNotificationGroup(subject, body, emailContentType);
			}
			else if (copyToNotificationGroupIfRecipientsNotEmpty)
			{
				CreateAndSaveWithCopy(subject, body, toRecipients, new EmailGroupUtility().GetCompanyNotificationGroupEmails().Cast<string>().ToArray(), "", emailContentType);
			}
			else
			{
				CreateAndSaveWithCopy(subject, body, toRecipients, null, "", emailContentType);
			}
		}

		void CreateAndSaveWithCopy(string subject, string body, string[] toRecipients, string[] ccRecipients, string groupDetail, EmailContentTypes emailContentType = null)
		{
			var emailToSend = new EmailDef();
			emailToSend.ContentType = emailContentType;

			emailToSend.Recipients.Clear();
			AddRecipientCore(emailToSend, toRecipients, RecipientDef.RecipientTypes.TO);

			if (ccRecipients != null && ccRecipients.Length > 0)
			{
				AddRecipientCore(emailToSend, ccRecipients, RecipientDef.RecipientTypes.CC);
			}

			emailToSend.Subject = subject;
			emailToSend.Body = body;
			CreateAndSave_Core(emailToSend, groupDetail: groupDetail);
		}

		void IOutgoingMailManager.CreateAndSaveSimple(string subject, string body, string recipient)
		{
			if (string.IsNullOrWhiteSpace(recipient))
			{
				throw new EmailHasNoRecipientsException(Res.GetString("{86AA573D-268B-487F-A282-70FBA7E252F5}", "Email (Subject: '{0}') must have a recipient", subject));
			}

			EmailDef emailToSend = new EmailDef();
			AddRecipientCore(emailToSend, GetEmailsFromOutlookFormatEmailAddresses(recipient), RecipientDef.RecipientTypes.TO);
			emailToSend.Subject = subject;
			emailToSend.Body = body;

			CreateAndSave_Core(emailToSend);
		}

		void IOutgoingMIMEManager.CreateAndSaveMIME(MIMEMessage message, string sender, string recipient, string subject)
		{
			CreateAndSaveMIME_Core(message, sender, recipient, subject);
		}

		void IOutgoingMIMEManager.CreateAndSaveSignedMIME(string innerAttachmentName, byte[] innerAttachmentContents, string contentType, string contentDisposition, string sender, string recipient, string subject, Store signingStore, Certificate encryptionCert)
		{
			MIMEMessage outerMessage;
			SingleAttachmentMIMEMessage innerMessage = new SingleAttachmentMIMEMessage(innerAttachmentName, innerAttachmentContents, contentType, contentDisposition);

			if (signingStore != null && encryptionCert != null)
			{
				SignedSMIMEMessage signedMessage = new SignedSMIMEMessage(innerMessage, signingStore);
				outerMessage = new EnvelopedSMIMEMessage(signedMessage, encryptionCert);
			}
			else if (signingStore != null && encryptionCert == null)
			{
				outerMessage = new SignedSMIMEMessage(innerMessage, signingStore);
			}
			else if (signingStore == null && encryptionCert != null)
			{
				outerMessage = new EnvelopedSMIMEMessage(innerMessage, encryptionCert);
			}
			else
			{
				outerMessage = innerMessage;
			}

			CreateAndSaveMIME_Core(outerMessage, sender, recipient, subject);
		}

#if DEBUG

		public List<EmailDef> EmailsCreated
		{
			get { return fEmailsCreated; }
		}

		protected readonly List<EmailDef> fEmailsCreated = new List<EmailDef>();

		public Guid LastCreatedMailGuid;

#endif

		#region Implemementation

		protected void CreateAndSave_Core(EmailDef emailToSend, BusinessObjectFactory factory = null, string groupDetail = "")
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}
			Create_Core(factory, emailToSend, groupDetail);
			factory.Save();
		}

		protected virtual MailItem Create_Core(BusinessObjectFactory factory, EmailDef emailToSend, string groupDetail)
		{
			ValidateEmail(emailToSend, groupDetail);

			var newItem = NewMailItem(factory, emailToSend);

#if DEBUG
			if (Globals.IsTest)
			{
				LastCreatedMailGuid = newItem.PK.ToGuid();
				fEmailsCreated.Add(emailToSend);
			}
#endif

			return newItem;
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
#if DEBUG
		internal
#endif
		void ValidateEmail(EmailDef emailToSend, string groupDetail)
		{
			if (emailToSend == null)
			{
				throw new ArgumentNullException(nameof(emailToSend));
			}

			if (emailToSend.FromAddress.Length == 0)
			{
				throw new EmailHasNoFromAddressException("The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One.");
			}

			if (emailToSend.Recipients.Count == 0 && emailToSend.CCRecipients.Count == 0 && emailToSend.BCCRecipients.Count == 0)
			{
				throw new EmailHasNoRecipientsException(string.Format(Culture.Invariant, "Email (Subject: '{0}'{1}) must have at least one recipient, CC or BCC", emailToSend.Subject, string.IsNullOrWhiteSpace(groupDetail) ? "" : ", For Group: " + groupDetail));
			}

			if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(emailToSend.FromAddress))
			{
				throw new EmailNotCompleteException("Invalid email address for sender '" + emailToSend.FromAddress + "'");
			}

			if (emailToSend.ContentType == EmailContentTypes.HTML && emailToSend.Body.Length > 0)
			{
				AnalyzeTableHeaders(emailToSend.Body);
			}
		}

		void AnalyzeTableHeaders(string htmlBody)
		{
			Dictionary<int, int> tableHeaderRowIndexes = new Dictionary<int, int>();
			bool exit = false;
			bool wrongTagFound = false;
			int lastIndex = -1;
			while (!exit && lastIndex + 1 < htmlBody.Length)
			{
				int index = htmlBody.Replace('"', '\'').IndexOf("'tableheadings'", lastIndex + 1);
				int closingTagIndex = htmlBody.IndexOf("</tr>", index + 1);
				if (closingTagIndex == -1)
				{
					closingTagIndex = htmlBody.Length - 1;
				}

				int wrongTagIndex = index >= 0 && closingTagIndex > index ? htmlBody.IndexOf("<td", index, closingTagIndex - index) : -1;
				if (wrongTagIndex >= 0)
				{
					wrongTagFound = true;
				}

				if (index < 0 || wrongTagFound)
				{
					exit = true;
				}

				lastIndex = index;
			}
			if (wrongTagFound)
			{
				string message = (NoResString)"Wrong table header tag found in email. You should use <th> tags inside row with \"tableheadings\" class.\r\nMessage Body: " + htmlBody;
				bool isDebug = Globals.IsDebugMode;
				if (isDebug)
				{
					throw new ArgumentException(message);
				}
				else
				{
					Globals.Message.ShowDeveloperErrorAlways("WrongTableHeaderTag", message, "Wrong Table Header Tag");
				}
			}
		}

		string AddGroupRecipientsToEmail(EmailDef emailToAddRecipients, Guid recipientGroupPk, IGroupSourceLocator locator)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return AddGroupRecipientsToEmail(factory, emailToAddRecipients, recipientGroupPk, locator);
		}

		string AddGroupRecipientsToEmail(BusinessObjectFactory factory, EmailDef emailToAddRecipients, Guid recipientGroupPk, IGroupSourceLocator locator)
		{
			var isGroupAll = (recipientGroupPk == Core.Constants.Groups.AllPK);
			var emailGroupUtility = new EmailGroupUtility();

			Argument.NotNull(locator, "GroupSourceLocator");
			if (emailToAddRecipients == null)
			{
				throw new ArgumentNullException(nameof(emailToAddRecipients));
			}

			var recipientGroup = factory.Load<GlbGroup>(recipientGroupPk);

			if (recipientGroup != null)
			{
				bool atLeastOneRecipientHasEmailAddressSetup = false;

				foreach (GlbStaff staff in recipientGroup.Staff)
				{
					if (staff.GS_IsActive && !staff.GS_EmailAddress.IsEmpty)
					{
						if (isGroupAll && emailGroupUtility.IsHostNotificationEmail(staff.GS_EmailAddress))
						{
							continue;
						}

						atLeastOneRecipientHasEmailAddressSetup = true;
						if (!emailToAddRecipients.Recipients.Contains(staff.GS_EmailAddress))
						{
							AddRecipientCore(emailToAddRecipients, new string[] { staff.GS_EmailAddress }, RecipientDef.RecipientTypes.TO);
						}
					}
				}

				if (!atLeastOneRecipientHasEmailAddressSetup && emailToAddRecipients.Recipients.Count == 0)
				{
					List<string> emptyGroups = new List<string>();
					StringCollection recipients = GetWiderGroupRecipients(emptyGroups);

					string message = string.Format(
						(NoResString)"This email could not be delivered to the requested group {0}. Please add a member with an email address to this group or specify an email address on an existing member.",
						locator.Location) + "\n\r" + string.Join("\n\r", emptyGroups.ToArray()) + "\n\r";
					if (recipients.Count > 0)
					{
						string originalEmailAsString = GetEmailAsString(emailToAddRecipients);
						string filename = string.IsNullOrEmpty(emailToAddRecipients.Subject) ? "OriginalMessage" : emailToAddRecipients.Subject;

						var suffix = emailToAddRecipients.ContentType == EmailContentTypes.HTML ? ".html" : ".txt";
						var displayName = new ZString(filename).Left(AutoMailDBAttachments.Schema.MA_FileNameMaxLength - suffix.Length) + suffix;
						emailToAddRecipients.Attachments.Add(new AttachmentDef(displayName, System.Text.Encoding.ASCII.GetBytes(originalEmailAsString)));

						emailToAddRecipients.Body = Res.GetString("6ffe72f6-dd18-4d6d-9deb-af6dbd83d5e3", "{0}\r\nOriginal message attached.", message);

						string[] recipientsArray = new string[recipients.Count];
						recipients.CopyTo(recipientsArray, 0);
						AddRecipientCore(emailToAddRecipients, recipientsArray, RecipientDef.RecipientTypes.TO);
					}
					else
					{
						throw new EmailHasNoRecipientsException(string.Format(Culture.Invariant, "Email (Subject: '{0}') {1}", emailToAddRecipients.Subject, message));
					}
				}
			}
			return GetGroupDetail(locator.Location, recipientGroup == null ? string.Empty : Res.GetString("{B73E62CA-9D4E-461E-A265-F333106241E4}", "'{0} - {1}'", recipientGroup.GG_Code, recipientGroup.GG_Desc));
		}

		string GetGroupDetail(string location, string groupData)
		{
			return string.Format(Culture.Invariant, "{0} {1}", location, groupData).TrimEnd();
		}

		string GetEmailAsString(EmailDef email)
		{
			string emailAsString = Res.GetString("da75af42-188d-4918-9535-5d0c5ac90c25", "From: {0}", email.FromAddress) + "\r\n";
			emailAsString += Res.GetString("8b0fa4c9-68a1-41ea-9994-830210dc8114", "Subject: {0}", email.Subject) + "\r\n";
			emailAsString += Res.GetString("358b260f-b2da-482d-8bdf-5feb13f9bec1", "Body:\r\n{0}", email.Body);

			return emailAsString;
		}

		StringCollection GetWiderGroupRecipients(List<string> emptyGroups)
		{
			EmailGroupUtility util = new EmailGroupUtility();
			StringCollection recipients = util.PostMasters;
			if (recipients.Count == 0)
			{
				emptyGroups.Add(Res.GetString("d654de58-9240-4640-b76f-7562cb305dac", "This email could not be delivered to the Post Masters Group as it is either empty or no members have an email address specified."));
				recipients = util.AdminStaff;
			}
			if (recipients.Count == 0)
			{
				emptyGroups.Add(Res.GetString("1f2b352f-827e-4044-be4a-5b840a519cd9", "This email could not be delivered to Admin as there is either no Admin (Is Controller flag on Staff form should be ticked) or none have an email address specified."));
				recipients = util.AllStaff;
			}
			if (recipients.Count == 0)
			{
				emptyGroups.Add(Res.GetString("d672e153-38d5-4990-ae8a-fa541d403e27", "This email could not be delivered to All Staff as there are no staff at all or none have an email address specified."));
			}
			return recipients;
		}

#if DEBUG
		internal
#endif
		string[] GetEmailsFromOutlookFormatEmailAddresses(string outlookFormatEmailAddresses)
		{
			return outlookFormatEmailAddresses.Split(';');
		}

		void CreateAndSaveMIME_Core(MIMEMessage message, string sender, string recipient, string subject)
		{
			message.SetTag((NoResString)"From", sender);
			message.SetTag((NoResString)"To", recipient);
			message.SetTag((NoResString)"Subject", subject);
			message.SetTag((NoResString)"Importance", "HIGH");
			message.SetTag("X-Priority", "1");

			CreateAndSaveMIMERaw(message.RawMIMEString, TimeSpan.Zero);
		}

#if DEBUG
		internal
#endif
		void CreateAndSaveMIMERaw(string rawMIMEString, TimeSpan timeToDelaySendingEmail)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			MailItem newItem = GetNewMailItem(factory);
			newItem.RawMIMEString = rawMIMEString;

			newItem.MI_Direction = MailDirection.Transmit;
			newItem.MI_SendDateTime = ZDateTime.UtcNow.Add(timeToDelaySendingEmail);
			newItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			newItem.MI_Status = MailStatus.Queued;
			if (newItem.MailRecipients.Count > 0)
			{
				if (newItem.MailRecipients[0].MR_AckAttempt == 0)
				{
					newItem.SendAttempts = 1;
				}
			}

			factory.Save();

#if DEBUG
			LastCreatedMailGuid = newItem.PK.ToGuid();
			fEmailsCreated.Add(null);
#endif
		}

		public MailItem NewMailItem(BusinessObjectFactory factory, EmailDef email)
		{
			MailItem newItem = GetNewMailItem(factory);

			newItem.MI_Direction = MailDirection.Transmit;
			newItem.MI_SendDateTime = ZDateTime.UtcNow;
			newItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			newItem.SendAttempts = 1;
			newItem.MI_QueueWithLowPriority = email.QueueWithLowPriority;

			newItem.MI_ContentType = email.ContentType.ContentTypeCode;

			newItem.MI_Body = email.Body;

			if (!string.IsNullOrEmpty(email.FromDisplayName))
			{
				newItem.MI_From = QuoteDisplayName(email.FromDisplayName) + " <" + email.FromAddress + ">";
			}
			else
			{
				newItem.MI_From = email.FromAddress;
			}

			if (email.SenderStaffPK.IsValid)
			{
				newItem.MI_SenderStaffID = email.SenderStaffPK.ToString();
			}

			if (email.BusinessEntityID.IsValid)
			{
				newItem.MI_BusinessEntityID = email.BusinessEntityID.ToString();
			}

			if (!string.IsNullOrEmpty(email.BusinessEntityTableCode))
			{
				newItem.MI_BusinessEntityTableCode = email.BusinessEntityTableCode;
			}

			if (!string.IsNullOrEmpty(email.BusinessEntityJobNumber))
			{
				newItem.MI_BusinessEntityjobNumber = email.BusinessEntityJobNumber;
			}

			if (!string.IsNullOrEmpty(email.DocumentName))
			{
				newItem.MI_DocumentName = email.DocumentName;
			}

			if (!string.IsNullOrEmpty(email.ListUnsubscribe))
			{
				newItem.MI_ListUnsubscribe = email.ListUnsubscribe;
			}

			foreach (var header in email.Headers)
			{
				newItem.SetHeaderItem(header.Key, header.Value);
			}

			newItem.MI_ReplyTo = ((ZString)email.ReplyTo).Left(newItem.MI_ReplyToInfo.MaxLength);
			newItem.MI_Subject = ((ZString)email.Subject).Left(newItem.MI_SubjectInfo.MaxLength);

			newItem.MI_Status = MailStatus.Queued;

			if (email.Recipients.Count == 0 && email.CCRecipients.Count == 0 && email.BCCRecipients.Count == 1)
			{
				newItem.AddRecipientFromRecipientDef(email.BCCRecipients[0], MailRecipient.RecipientTypes.TO);
			}
			else
			{
				foreach (RecipientDef to in email.Recipients)
				{
					newItem.AddRecipientFromRecipientDef(to, MailRecipient.RecipientTypes.TO);
				}

				foreach (RecipientDef cCEmail in email.CCRecipients)
				{
					newItem.AddRecipientFromRecipientDef(cCEmail, MailRecipient.RecipientTypes.CC);
				}

				foreach (RecipientDef bCCEmail in email.BCCRecipients)
				{
					newItem.AddRecipientFromRecipientDef(bCCEmail, MailRecipient.RecipientTypes.BCC);
				}
			}

			newItem.AddFooterForOverriddenRecipients();

			foreach (AttachmentDef attach in email.Attachments)
			{
				var myAttachment = newItem.MailAttachments.AddNew();
				myAttachment.MA_Data = attach.Data;
				myAttachment.MA_Encoding = "B64";
				myAttachment.MA_FileName = ((ZString)attach.DisplayName).Left(myAttachment.MA_FileNameInfo.MaxLength);
			}

			return newItem;
		}

		protected virtual void AddRecipientCore(EmailDef emailDef, string[] emails, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForUserCommunication(emails, type);
		}

		protected virtual MailItem GetNewMailItem(BusinessObjectFactory factory)
		{
			return factory.New<MailItem>();
		}

		/// <summary>
		/// Display name must be quoted when part of a mailbox string
		/// if it contains anything other than letter, digit, space or a few allowed punctuation characters.
		/// See RFC 2822.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1000: Do not declare static members on generic types")]
		public static string QuoteDisplayName(string displayName)
		{
			if (displayName.Length > 0 &&
				displayName[0] != '"' &&
				displayName.Any(c => !IsValidDisplayNameCharacter(c)))
			{
				displayName = '"' + displayName + '"';
			}

			return displayName;
		}

		internal const string ValidDisplayNameSpecials = @"!#$%&'*+-/=?^_`{|}~";

		static bool IsValidDisplayNameCharacter(char c)
		{
			return char.IsWhiteSpace(c) ||
				(c >= '0' && c <= '9') ||
				(c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z') ||
				ValidDisplayNameSpecials.IndexOf(c) >= 0;
		}

		#endregion
	}
}
