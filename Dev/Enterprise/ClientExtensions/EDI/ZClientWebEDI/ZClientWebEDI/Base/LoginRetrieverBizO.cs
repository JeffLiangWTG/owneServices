using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class LoginRetrieverBizO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LoginRetrieverBizO(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[MaxLength(OrgContact.Schema.OC_EmailMaxLength)]
		public ZString Email
		{
			get { return email; }
			set
			{
				if (email != value)
				{
					var query = new ZQuery(OrgContactSchema.OC_Email, value);
					query.OrderBy = string.Join(" DESC,", OrgContactSchema.Constants.OC_IsActive, OrgContactSchema.Constants.OC_WebAccessEnabled, OrgContactSchema.Constants.OC_ContactName);
					var contacts = new BusinessObjectFactory().Load<OrgContact>(query).OrderByDescending(x => x.Person.HasPassword);

					foreach (var contact in contacts)
					{
						if (contact.OC_IsActive && contact.OC_WebAccessEnabled)
						{
							defaultOrgContact = contact;
							break;
						}
					}

					if (defaultOrgContact == null)
					{
						defaultOrgContact = contacts.FirstOrDefault();
					}
				}

				SetNonPersistentPropertyValue(EmailInfo, ref email, value);
			}
		}

		public ZPropertyInfo EmailInfo => GetZPropertyInfo(nameof(Email));
		ZString email;
		internal OrgContact defaultOrgContact;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public string Retrieve(Global appInstance)
		{
			if (defaultOrgContact == null)
			{
				var message = GenericEmailSentMessage;
				return message;
			}

			if (!defaultOrgContact.OC_WebAccessEnabled || !defaultOrgContact.OC_IsActive)
			{
				var message = GenericEmailSentMessage;
				var resetFailedEmail = new EmailToContactBusinessObject(defaultOrgContact);
				resetFailedEmail.Subject = "Password Reset Failed";
				resetFailedEmail.Body = "This is a deactivated account. Kindly raise a CR9 to request account activation.";
				resetFailedEmail.ToDisplayName = defaultOrgContact.Name;
				resetFailedEmail.ToEmailAddress = defaultOrgContact.Email;
				resetFailedEmail.SendEmail(systemCommunication: true);
				return message;
			}

			var shouldSendMasterPassword = ((IPasswordInstructionEmailSource)defaultOrgContact).ShouldSendMasterPassword;

			var resetPasswordPage = shouldSendMasterPassword ? appInstance.ResetMasterPasswordPage : appInstance.ResetPasswordPage;
			var resetPasswordUrl = FormattableString.Invariant($"{resetPasswordPage}?{appInstance.ResetPasswordKey}=");
			appInstance.SetupSession(null, EventArgs.Empty, false);

			var contactWithoutCompanyInfo = new ContactWithoutCompanyInfo(defaultOrgContact.ContactNameWithoutNumberSuffix, Email, resetPasswordUrl,
				defaultOrgContact.Salutation, defaultOrgContact.ExtraInstruction, defaultOrgContact.OrgCode, defaultOrgContact.EmailOrgCodes, null, shouldSendMasterPassword);

			contactWithoutCompanyInfo.CompanyPKForEmailTemplate = defaultOrgContact.CompanyPKForLogin;

			var result = PasswordInstructionEmailSender.SendPasswordResetEmail(contactWithoutCompanyInfo);
			return result ? GenericEmailSentMessage : "There was an error occurred when sending the email.";
		}

		public const string GenericEmailSentMessage = "If the email address is linked to a valid account, a reset password link has been sent.";
	}
}
