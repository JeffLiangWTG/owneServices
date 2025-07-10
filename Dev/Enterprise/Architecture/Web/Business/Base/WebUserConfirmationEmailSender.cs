using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebUserConfirmationEmailSender
	{
		public WebUserConfirmationEmailSender(OrgContact contact, string fromNameOverride = null, string fromAddressOverride = null)
		{
			this.contact = contact;
			fromNameForPasswordEmail = fromNameOverride;
			fromAddressForPasswordEmail = fromAddressOverride;
		}

		readonly OrgContact contact;

		GlbPerson Person => contact.Person;

		string ReplyToForPasswordEmail { get; } = Env.Registry.SMTPDefaultReturnEmailAddress;

		Guid? companyPK;
		protected Guid CompanyPK
		{
			get
			{
				if (companyPK == null)
				{
					companyPK = GetCompanyPK(contact);
				}

				return companyPK.Value;
			}
		}

		NotificationEmailTemplate PasswordResetSuccessfullyEmailTemplate
		{
			get
			{
				if (passwordResetSuccessfullyEmailTemplate == null)
				{
					passwordResetSuccessfullyEmailTemplate = WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty);
				}
				return passwordResetSuccessfullyEmailTemplate;
			}
		}
		NotificationEmailTemplate passwordResetSuccessfullyEmailTemplate;

		NotificationEmailTemplate PasswordSetSuccessfullyEmailTemplate
		{
			get
			{
				if (passwordSetSuccessfullyEmailTemplate == null)
				{
					passwordSetSuccessfullyEmailTemplate = WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty);
				}
				return passwordSetSuccessfullyEmailTemplate;
			}
		}
		NotificationEmailTemplate passwordSetSuccessfullyEmailTemplate;

		public void SendConfirmationEmail(PasswordInstructionType instructionType, bool ignoreEmailOverride = false)
		{
			var isMaster = ((IPasswordInstructionEmailSource)contact).ShouldSendMasterPassword;
			if (isMaster)
			{
				SendMasterPasswordSetEmail(instructionType);
			}
			else
			{
				SendPasswordSetEmail(contact, instructionType, ignoreEmailOverride);
			}
		}

		void SendPasswordSetEmail(OrgContact contact, PasswordInstructionType instructionType, bool ignoreEmailOverride = false)
		{
			var parser = DocumentParser.New(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(), contact.Factory);
			string body, subject, footer;
			if (instructionType == PasswordInstructionType.Reset)
			{
				body = parser.Parse(contact, PasswordResetSuccessfullyEmailTemplate.EmailBody);
				subject = parser.Parse(contact, PasswordResetSuccessfullyEmailTemplate.EmailSubject);
				footer = parser.Parse(contact, WebDataRegistry.Instance.PasswordResetEmailFooter.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));
			}
			else
			{
				body = parser.Parse(contact, PasswordSetSuccessfullyEmailTemplate.EmailBody);
				subject = parser.Parse(contact, PasswordSetSuccessfullyEmailTemplate.EmailSubject);
				footer = parser.Parse(contact, WebDataRegistry.Instance.PasswordSetEmailFooter.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));
			}

			var email = new EmailDef() { Subject = subject };
			new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(email, string.Empty, body, footer, CompanyPK);

			email.FromDisplayName = FromNameForPasswordEmail;
			email.FromAddress = FromAddressForPasswordEmail;
			if (!string.IsNullOrWhiteSpace(ReplyToForPasswordEmail))
			{
				email.ReplyTo = ReplyToForPasswordEmail;
			}

			if (ignoreEmailOverride)
			{
				email.AddRecipientForSystemCommunication(contact.OC_Email);
			}
			else
			{
				email.AddRecipientForUserCommunication(contact.OC_Email);
			}

			Env.OutgoingMailManager.CreateAndSave(email);
		}

		void SendMasterPasswordSetEmail(PasswordInstructionType instructionType)
		{
			var contactsToSendTo = Person.ContactCollection.Cast<OrgContact>().Where(x => x.IsValidForWebLogin).GroupBy(x => x.OC_Email.ToLower());

			foreach (var contactGrouping in contactsToSendTo)
			{
				var firstContact = contactGrouping.FirstOrDefault();
				SendMasterPasswordSetEmailCore(instructionType, GetCompanyPK(firstContact), contactGrouping.Key, firstContact);
			}
		}

		void SendMasterPasswordSetEmailCore(PasswordInstructionType instructionType, Guid companyPK, string recipientEmail, OrgContact contactRecipient)
		{
			var template = instructionType == PasswordInstructionType.Reset
				? WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty)
				: WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);

			var email = new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(
				MasterParser.Parse(contactRecipient, template.EmailSubject),
				MasterParser.Parse(contactRecipient, template.EmailBody),
				instructionType == PasswordInstructionType.Reset ?
					WebDataRegistry.Instance.PasswordResetEmailFooter.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty) :
					WebDataRegistry.Instance.PasswordSetEmailFooter.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty),
				companyPK);

			email.FromDisplayName = FromNameForPasswordEmail;
			email.FromAddress = FromAddressForPasswordEmail;
			if (!string.IsNullOrWhiteSpace(ReplyToForPasswordEmail))
			{
				email.ReplyTo = ReplyToForPasswordEmail;
			}

			email.AddRecipientForUserCommunication(recipientEmail);
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		string fromAddressForPasswordEmail;

		string FromAddressForPasswordEmail
		{
			get
			{
				if (string.IsNullOrEmpty(fromAddressForPasswordEmail))
				{
					if (!Env.CurrentUser.IsWebUser && !GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty)
					{
						fromAddressForPasswordEmail = GlbStaff.CurrentUser.GS_EmailAddress;
					}
					else
					{
						fromAddressForPasswordEmail = Env.Registry.EnterpriseMailboxEmailAddress;
					}
				}

				return fromAddressForPasswordEmail;
			}
		}

		Guid GetCompanyPK(OrgContact emailContact)
		{
			if (contact != null && contact.CompanyPKForEmailTemplate != Guid.Empty)
			{
				return contact.CompanyPKForEmailTemplate;
			}

			if (emailContact.ParentOrg.Branch != null && emailContact.ParentOrg.Branch.Company != null)
			{
				return emailContact.ParentOrg.Branch.Company.PK.ToGuid();
			}

			return Guid.Empty;
		}

		string fromNameForPasswordEmail;

		string FromNameForPasswordEmail
		{
			get
			{
				if (string.IsNullOrEmpty(fromNameForPasswordEmail))
				{
					fromNameForPasswordEmail = ((!Env.CurrentUser?.IsWebUser ?? false) && GlbStaff.CurrentUser != null && !GlbStaff.CurrentUser.GS_FullName.IsEmpty)
						? GlbStaff.CurrentUser.GS_FullName.ToString()
						: Env.Registry.MailboxDisplayName;
				}

				return fromNameForPasswordEmail;
			}
		}

		DocumentParser MasterParser
		{
			get
			{
				if (masterParser == null)
				{
					masterParser = DocumentParser.New(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactMasterPasswordInstructionEmail>(), Person.Factory);
				}
				return masterParser;
			}
		}
		DocumentParser masterParser;
	}
}
