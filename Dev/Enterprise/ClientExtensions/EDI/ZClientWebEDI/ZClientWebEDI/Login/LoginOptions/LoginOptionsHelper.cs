using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class LoginOptionsHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LoginOptionsHelper(BusinessObjectFactory factory, OrgContact contact, EdiCustomerUserAccount userAccount) : base(factory)
		{
			SetContactAndUserAccountValues(contact?.PK ?? ZGuid.Empty, userAccount?.PK ?? ZGuid.Empty);
		}

		public LoginOptionsHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		bool VerifyPasswordCore(ZString password)
		{
			var result = false;

			if (Contact != null)
			{
				var person = Contact.Person;
				if (person != null && !person.PER_PasswordHash.IsEmpty)
				{
					result = person.VerifyPassword(password);
				}
				else
				{
					result = Contact.VerifyPassword(password);
				}
			}

			return result;
		}

		public bool VerifyPassword(ZString password)
		{
			var result = VerifyPasswordCore(password);

			if (result)
			{
				PostPasswordVerificationAction();
			}
			return result;
		}

		protected virtual void PostPasswordVerificationAction()
		{
			UserAccount?.ActivateContactRelationshipAndSave();

			var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, Contact.PK);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsContactRelationshipActive, false);
			var otherUserAccountsToActivate = Factory.Load<EdiCustomerUserAccount>(query);
			foreach (var userAccount in otherUserAccountsToActivate)
			{
				if (userAccount.IsAwaitingActivation)
				{
					userAccount?.ActivateContactRelationshipAndSave();
				}
			}
		}

		public bool SendEmailVerification(Uri originalUrl)
		{
			if (Contact != null)
			{
				var originalUrlString = originalUrl != null ? originalUrl.IsAbsoluteUri ? originalUrl.AbsoluteUri : originalUrl.OriginalString : string.Empty;
				var scope = GetScopeForEmailVerification(originalUrlString);
				var info = new AccessTokenInfo(scope, UserAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, info, TimeSpan.FromDays(1), 1);

				return SendEmailVerification(token, Contact.Person.PER_EmailAddress, Contact, UserAccount);
			}

			return false;
		}

		protected virtual string GetScopeForEmailVerification(string originalUrlString)
		{
			return originalUrlString;
		}

		protected bool SendEmailVerification(string token, string recipient, OrgContact contact, EdiCustomerUserAccount userAccount)
		{
			ZString companyName = Env.Registry.MailboxDisplayName;

			var url = FormattableString.Invariant($"{EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/')}/Login/EmailVerification.aspx?token={token}");

			var template = EDIDataRegistry.Instance.EmailVerificationNotificationMessageTemplate.Value;
			var parser = new DocumentParser<EdiCustomerUserAccountEmailWrapper, DocEdiCustomerUserAccount>(contact?.Factory ?? userAccount.Factory);

			var wrapper = new EdiCustomerUserAccountEmailWrapper(userAccount, url);

			var email = new HtmlEmailDef
			{
				FromDisplayName = companyName,
				Subject = parser.Parse(wrapper, template.EmailSubject),
				ContentType = EmailContentTypes.HTML
			};

			email.LoadHtmlUsingTemplate(parser.Parse(wrapper, template.EmailBody));

			if (companyName.IsValid && !companyName.IsEmpty)
			{
				email.FromDisplayName = companyName;
			}

			email.AddRecipientForUserCommunication(recipient);

			try
			{
				Env.OutgoingMailManager.CreateAndSave(email);
				return true;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("LoginOptionsHelper: Failed to send verification email", e.Message, e);
			}

			return false;
		}

		#region Login Hash Cookie

		public void WriteLoginHashCookie(string companyCode, string userName)
		{
			WebApplicationLoginHelper.WriteLoginHashToCookie(companyCode, userName);
		}

		#endregion

		#region Implements

		public void SetContactAndUserAccountValues(OrgContact contact, EdiCustomerUserAccount userAccount)
		{
			UserAccount = userAccount;

			if (contact != null)
			{
				Contact = contact;
				EmailAddress = Contact.Person.PER_EmailAddress;
				MaskedEmailAddress = MaskEmailAddress(EmailAddress);
				HasPassword = !Contact.Person.PER_PasswordHash.IsEmpty || !Contact.OC_PasswordHash.IsEmpty;
			}
		}

		void SetContactAndUserAccountValues(ZGuid contactPK, ZGuid userAccountPK)
		{
			Contact = Factory.Load<OrgContact>(contactPK);
			UserAccount = Factory.Load<EdiCustomerUserAccount>(userAccountPK);

			if (Contact?.Person != null && UserAccount != null)
			{
				EmailAddress = Contact.Person.PER_EmailAddress;
				MaskedEmailAddress = MaskEmailAddress(EmailAddress);

				HasPassword = !Contact.Person.PER_PasswordHash.IsEmpty || !Contact.OC_PasswordHash.IsEmpty;
			}
		}

		protected static string MaskEmailAddress(string email)
		{
			if (string.IsNullOrEmpty(email) || !EmailAddressValidation.IsEmailAddressValidAndNotEmpty(email))
			{
				return string.Empty;
			}

			var maskedEmail = "";
			var pattern = @"(.{2})(.*)(@)(.{1})(.*)";
			var match = Regex.Match(email, pattern);

			if (match.Groups.Count == 6)
			{
				maskedEmail = string.Join("", match.Groups.OfType<Group>().Skip(1).Select(
					(s, i) => i == 0 || i == 2 || i == 3 ? s.Value : new string(
					'*', 5)));
			}
			else
			{
				maskedEmail = string.Join("", email.Take(2)).PadRight(5, '*');
			}

			return maskedEmail;
		}

		#endregion Implements

		#region Properties

		public virtual bool IsValid => Contact != null && UserAccount != null;
		public ZString EmailAddress { get; protected set; }
		public ZString MaskedEmailAddress { get; protected set; }
		public ZString ContactRelationshipStatus => UserAccount?.EUA_ContactRelationshipStatus ?? "";
		public bool HasPassword { get; protected set; }
		public OrgContact Contact { get; protected set; }
		public EdiCustomerUserAccount UserAccount { get; protected set; }

		#endregion Properties
	}
}
