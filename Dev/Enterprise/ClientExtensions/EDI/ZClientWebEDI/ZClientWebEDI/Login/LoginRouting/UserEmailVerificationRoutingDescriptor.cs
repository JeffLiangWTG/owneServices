using System;
using System.Net;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserEmailVerificationRoutingDescriptor : ILoginRoutingDescriptor
	{
		public UserEmailVerificationRoutingDescriptor(Uri originalUrl, LicenceDatabase database, EdiCustomerUserAccount userAccount)
		{
			this.originalUrl = originalUrl;
			this.database = database;
			this.userAccount = userAccount;
		}
		readonly Uri originalUrl;
		readonly LicenceDatabase database;
		readonly EdiCustomerUserAccount userAccount;

		internal const string EmailVerificationSentUrlKey = "evs";
		internal const string EmailVerificationAddressUrlKey = "eva";
		public const string VerifyTestUserAccountTokenKey = "VerifyTestUserAccount";

		bool IsEmailSentSuccessful { get; set; }

		public bool IsRoutingRequired
		{
			get
			{
				if (database == null || userAccount == null)
				{
					return false;
				}
				return userAccount.EUA_IsEmailVerificationRequired;
			}
		}

		public Uri RoutingUrl => BuildEmailVerificationSentUrl(userAccount.EUA_Email, IsEmailSentSuccessful);

		public void RoutingAction()
		{
			if (!userAccount.EUA_Email.IsEmpty)
			{
				IsEmailSentSuccessful = SendVerificationEmail();
			}
		}

		bool SendVerificationEmail()
		{
			bool isSuccessful = false;

			var companyName = new ZString(Env.Registry.MailboxDisplayName);
			var scope = FormattableString.Invariant($"{VerifyTestUserAccountTokenKey}:{originalUrl?.ToString() ?? string.Empty}");
			var info = new AccessTokenInfo(scope, userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, info, TimeSpan.FromDays(1), 1);

			var url = FormattableString.Invariant($"{EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/')}/Login/EmailVerification.aspx?token={token}");

			var template = EDIDataRegistry.Instance.EmailVerificationNotificationMessageTemplate.Value;
			var parser = new DocumentParser<EdiCustomerUserAccountEmailWrapper, DocEdiCustomerUserAccount>(userAccount.Factory);
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

			email.AddRecipientForUserCommunication(userAccount.EUA_Email);

			try
			{
				Env.OutgoingMailManager.CreateAndSave(email);
				isSuccessful = true;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("UserEmailVerificationRoutingDescriptor: Failed to send verification email", e.Message, e);
			}

			return isSuccessful;
		}

		static Uri BuildEmailVerificationSentUrl(string emailAddress, ZBool sendSuccess)
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(EmailVerificationSentUrlKey, sendSuccess.ToString());
			secureQueryString.Add(EmailVerificationAddressUrlKey, emailAddress);

			var path = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/EmailSentNotification.aspx?qdata=" + WebUtility.UrlEncode(secureQueryString.ToString());
			var builder = new UriBuilder(path) { Port = -1 };
			return builder.Uri;
		}
	}
}
