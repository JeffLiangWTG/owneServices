using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class SetPersonalEmailHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SetPersonalEmailHelper(BusinessObjectFactory factory, BasePage sourcePage) : base(factory)
		{
			SourcePage = sourcePage;
		}

		readonly BasePage SourcePage;

		#region SaveEmail

		public (bool, string) SaveEmail(OrgContact contact, string emailAddress)
		{
			if (string.IsNullOrEmpty(emailAddress))
			{
				return (false, Res.GetString("6846add3-ef9b-4c6e-b55d-489d37b6c1e0", "The personal email address cannot be empty."));
			}

			if (emailAddress.Length > GlbPersonSchema.PER_EmailAddress.MaxLength)
			{
				return (false, Res.GetString("385ce615-65a0-4004-95ad-2fe9858d2d10", "The length of personal email address exceeds the max length {0}", GlbPersonSchema.PER_EmailAddress.MaxLength));
			}

			if (!EmailAddressValidation.IsEmailAddressValid(emailAddress))
			{
				return (false, Res.GetString("56369fdd-0228-4f05-a14a-14063769bbcf", "The personal email address is invalid.", emailAddress));
			}

			if (contact?.Person != null)
			{
				foreach (var personContact in contact.Person.ContactCollection.Cast<OrgContact>())
				{
					if (emailAddress.Equals(personContact.OC_Email, StringComparison.OrdinalIgnoreCase))
					{
						return (false, Res.GetString("4e1d39a4-9406-4624-96b5-9f9095fc08f3", "The entered email is already set as a login email. A personal recovery email must be different.", emailAddress));
					}
				}
			}

			var result = SendConfirmEmail(contact, emailAddress);

			if (!result)
			{
				return (false, Res.GetString("ec1a98f6-4453-4185-80ca-c07a04c36052", "There was a problem sending the confirmation email. Please check the email address and try again. If problems persist, contact your system administrator."));
			}
			else
			{
				return (true, Res.GetString("2a497580-4932-43c1-a75e-ad69d34a9ed8", "An email was sent to {0} to confirm personal email.", emailAddress));
			}
		}

		bool SendConfirmEmail(OrgContact contact, string emailAddress)
		{
			var companyName = (ZString)Env.Registry.MailboxDisplayName;
			var url = GenerateRegisterPersonalEmailUrl(contact.PK.ToGuid(), emailAddress);

			var body = new ZStringBuilder();

			body.AppendLine(Res.GetString("67d1cec9-4c0e-484a-9105-e8d32476a73f", "Dear {0}", contact.ContactNameWithoutNumberSuffix));
			body.AppendLine();

			body.AppendLine(Res.GetString("E44EF2D4-D4FE-47E4-9F27-710AA69A82E8", "A request was recently submitted to associate your personal email with your account via the My Account Portal."));
			body.AppendLine(Res.GetString("5a2fc38c-fba9-45eb-89c8-0eac2e172284", "Click the confirmation button to complete this action."));
			body.AppendLine();

			body.AppendLine(GetConfirmPersonalEmailButton(url));

			body.AppendLine(Res.GetString("f50b6035-09e2-4c8a-a18f-6fae2d207227", "If you are having troubles with the button, you can also paste the following URL into your browser to finish the operation.", url));
			body.AppendLine(FormattableString.Invariant($"<a href='{url}'>{url}</a>"));
			body.AppendLine();

			body.AppendLine(Res.GetString("582df807-c9fa-4ec7-9371-caf32069c935", "{0}Note: This confirmation link is only valid for the next 24 hours.{1}", "<b>", "</b>"));
			body.AppendLine();

			body.AppendLine(Res.GetString("56e68815-6158-43b9-a5ef-2a7aefacfa5b", "If you did not request this, please contact us immediately and do not click on this link."));
			body.AppendLine();

			var email = new EmailDef();
			email.Subject = Res.GetString("f1e631db-ece3-4ec8-a2ea-1f266f2ee8f8", "Personal Email Confirmation");

			new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(
			email,
			string.Empty,
			new ZString(body.ToString()).NormaliseWhitespaceCharactersForHtml(),
			Res.GetString("bb753f5e-bc27-4574-b033-4ab4f6a35d07", "Regards, {0}WiseTech Global Team{0}", "<br/>"),
			contact.CompanyPKForLogin);

			if (companyName.IsValid && !companyName.IsEmpty)
			{
				email.FromDisplayName = companyName;
			}

			email.AddRecipientForUserCommunication(emailAddress);

			try
			{
				Env.OutgoingMailManager.CreateAndSave(email);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("SetPersonalEmailHelper: Failed to send confirmation email", e.Message, e);
				return false;
			}

			return true;
		}

		string GenerateRegisterPersonalEmailUrl(Guid orgContactPK, string emailAddress)
		{
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo(emailAddress, orgContactPK, OrgContactSchema.Constants.Prefix), TimeSpan.FromHours(24), 1);
			return FormattableString.Invariant($"{SourcePage.AppInstance.RegisterPersonalEmailPage}?{SourcePage.AppInstance.RegisterKey}={token}");
		}

		string GetConfirmPersonalEmailButton(string url)
		{
			var buttonText = Res.GetString("a38b45e1-c0fc-465a-8aee-b3da7925001b", "Confirm Personal Email");

			var result = new ZStringBuilder();
			result.Append("<!--[if mso]>");
			result.Append(FormattableString.Invariant($"<v:roundrect href='{url}' style='mso-wrap-style:none; mso-position-horizontal:center' arcsize='10%' strokecolor='#00a8e1' strokeweight='0px' fillcolor='#00a8e1'>"));
			result.Append("<v:textbox style='mso-fit-shape-to-text:true'>");
			result.Append(FormattableString.Invariant($"<center style='color:#ffffff; font-family:sans-serif; font-size:11px; font-weight:bold;'>{buttonText}</center>"));
			result.Append("</v:textbox></v:roundrect>");
			result.Append("<![endif]-->");
			result.Append("<![if !mso]>");
			result.Append("<table cellspacing='0' cellpadding='0'>");
			result.Append("<tr><td align='center' width='150' height='30' style='-webkit-border-radius: 4px; -moz-border-radius: 4px; border-radius: 4px; color: #ffffff; display: block; background-color:#00a8e1!important'>");
			result.Append(FormattableString.Invariant($"<a href='{url}' style='font-size:11px; font-weight:bold; font-family:sans-serif; text-decoration:none; line-height:25px; width:100%; display:inline-block; border:1px solid transparent;'>"));
			result.Append(FormattableString.Invariant($"<span style='color:#ffffff;'>{buttonText}</span>"));
			result.Append("</a></td></tr></table>");
			result.Append("<![endif]>");
			return result.ToString();
		}

		#endregion
	}
}
