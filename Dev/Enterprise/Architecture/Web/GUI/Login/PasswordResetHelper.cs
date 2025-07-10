using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// Object responsible for capturin user login details for sending password reset email
	/// </summary>
	public class PasswordResetHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string CompanyCode = "CompanyCode";
			public const string EmailAddress = "EmailAddress";
		}

		#endregion

		public PasswordResetHelper(BusinessObjectFactory factory, string email)
			: this(factory)
		{
			EmailToRetrieveOrgHeaders = email;
		}

		public PasswordResetHelper(BusinessObjectFactory factory, string email, string orgCodeRestriction)
			: this(factory)
		{
			EmailToRetrieveOrgHeaders = email;
			OrgCodeRestriction = orgCodeRestriction;
		}

		public PasswordResetHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region EmailAddress

		protected ZString fEmailAddress;

		[MaxLength(OrgContact.Schema.OC_EmailMaxLength)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql Sort key word, not need to translate")]
		public ZString EmailAddress
		{
			get => fEmailAddress;
			set
			{
				if (fEmailAddress != value)
				{
					CheckMaximumLength(EmailAddressInfo, value);
					fEmailAddress = value;
					EmailAddressInfo.RefreshBinding();

					defaultOrgContact = null;

					var query = new ZQuery(OrgContactSchema.OC_Email, value);
					query.OrderBy = string.Join(" DESC,", OrgContactSchema.Constants.OC_IsActive, OrgContactSchema.Constants.OC_WebAccessEnabled, OrgContactSchema.Constants.OC_ContactName);
					var contacts = Factory.Load<OrgContact>(query).OrderByDescending(x => x.Person.HasPassword);

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

				if (!IsValidationSuspended)
				{
					ValidateEmailAddress();
				}
			}
		}

		public virtual ZPropertyInfo EmailAddressInfo => GetZPropertyInfo(Schema.EmailAddress);

		public void ValidateEmailAddress()
		{
			EmailAddressInfo.ClearAllNotifications();
			if (EmailAddress.IsEmpty)
			{
				EmailAddressInfo.AddError(Res.GetString("94c6a599-41f6-49e1-8cd8-11a98d06ee2c",
					"Email address is required"));
			}
		}

		internal OrgContact defaultOrgContact;
		protected OrgContact DefaultOrgContact => defaultOrgContact;

		protected OrgContactCollection RelatedContactsWithoutWebAccess
		{
			get
			{
				if (fRelatedContactsWithoutWebAccess == null)
				{
					RefreshRelatedContacts();
				}

				return fRelatedContactsWithoutWebAccess;
			}
		}

		OrgContactCollection fRelatedContactsWithoutWebAccess;

		protected void RefreshRelatedContacts()
		{
			fRelatedContactsWithoutWebAccess = new OrgContactCollection(Factory);

			if (!EmailAddress.IsEmpty)
			{
				var query = new ZQuery(OrgContactSchema.OC_WebAccessEnabled, false);
				query.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_IsActive, false);

				var filter = new ZQuery(OrgContactSchema.OC_Email, SQLComparisonOperator.Equal, EmailAddress);
				filter.AddToFilter(query);

				var lRelatedContactsWithoutWebAccess = new OrgContactCollection(Factory, filter);
				lRelatedContactsWithoutWebAccess.Load();
				fRelatedContactsWithoutWebAccess = lRelatedContactsWithoutWebAccess;
			}
		}

		#endregion

		#region Company Code

		public ZString CompanyCode
		{
			get => companyCode;
			set
			{
				if (companyCode != value)
				{
					companyCode = value;
					CompanyCodeInfo.RefreshBinding();
				}
			}
		}

		ZString companyCode;

		public ZPropertyInfo CompanyCodeInfo => GetZPropertyInfo(Schema.CompanyCode);

		public OrgHeaderCollection OrgHeaders
		{
			get
			{
				if (orgHeaders == null)
				{
					var lorgHeaders = new OrgHeaderCollection(Factory, GetOrgHeadersFilter());
					lorgHeaders.Load();
					orgHeaders = lorgHeaders;
				}

				return orgHeaders;
			}
		}

		OrgHeaderCollection orgHeaders;

		ZQuery GetOrgHeadersFilter()
		{
			if (!string.IsNullOrEmpty(EmailToRetrieveOrgHeaders))
			{
				var orgContactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_OH);
				orgContactSubQuery.AddToFilter(OrgContactSchema.OC_Email, EmailToRetrieveOrgHeaders);
				orgContactSubQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
				orgContactSubQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);

				var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				orgQuery.AddSubQuery(orgContactSubQuery, JoinCondition.And);

				if (!string.IsNullOrEmpty(OrgCodeRestriction))
				{
					orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, OrgCodeRestriction);
				}

				orgQuery.OrderBy = OrgHeaderSchema.Constants.OH_Code;
				return orgQuery;
			}

			var query = new ZQuery();
			query.IsNoResultQuery = true;

			return query;
		}

		public readonly string EmailToRetrieveOrgHeaders;

		public readonly string OrgCodeRestriction;

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEmailAddress();
		}

		ZString CompanyName => Env.Registry.MailboxDisplayName;

		protected void NotifyWebAdminNoWebAccount()
		{
			RefreshRelatedContacts();

			var notification = new EmailDef();
			string language;
			notification.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty),
					WebDataRegistry.Instance.WebAdminsEmailNotificationGroup, out language);

			using (Res.TemporarilySwitchLanguage(language))
			{
				var subject = Res.GetString("7db2f8f0-d5e3-4f46-bd99-228a6653b16f", "{0} Website Password Reset Request: {1}", CompanyName, EmailAddress);
				var footer = "<br />" + Res.GetString("13cf4ec1-c6d7-411d-a9e8-7046168dd027", "{0} Web Administrator Notification", CompanyName) + "<br /><br />";

				var body = new StringBuilder();
				body.Append(Res.GetString("c40161a8-0a66-4122-9e05-1edbf3e122c9", "A user without an active web account has requested a password reset, however this was not delivered because their account is not enabled for web access."));
				body.Append("<br /><br />");
				body.Append(Res.GetString("cb4776dc-1e3c-47b4-84a7-f15aaab3f026", "Their contact details are:"));
				body.Append("<br /><br />");
				body.Append(Res.GetString("c57eb3ef-0aa3-44d6-b635-c7a10c09c9c5", "Name: {0}", RelatedContactsWithoutWebAccess[0].OC_ContactName));
				body.Append("<br />");
				body.Append(Res.GetString("e5c0270b-3b70-4067-b659-e810c784fd22", "Email: {0}", EmailAddress));
				body.Append("<br /><br />");

				foreach (OrgContact contact in RelatedContactsWithoutWebAccess)
				{
					body.AppendFormat(CultureInfo.InvariantCulture, "{0} - {1}<br />", contact.OrganisationCode, contact.Header.OH_FullNameTruncated);
				}

				var companyGuid = Guid.Empty;

				if (RelatedContactsWithoutWebAccess[0].ParentOrg.Branch != null && RelatedContactsWithoutWebAccess[0].ParentOrg.Branch.Company != null)
				{
					companyGuid = RelatedContactsWithoutWebAccess[0].ParentOrg.Branch.Company.PK.ToGuid();
				}

				new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(
						notification,
						subject,
						body.ToString(),
						footer,
						companyGuid);

				if (CompanyName.IsValid && !CompanyName.IsEmpty)
				{
					notification.FromDisplayName = CompanyName;
				}

				try
				{
					Env.OutgoingMailManager.CreateAndSave(notification);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					//if sending of email fails, do nothing and continue operation
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html code should not be translated")]
		protected void NotifyWebAdminUnknownUser()
		{
			string language;
			var notification = new EmailDef();
			notification.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty),
					WebDataRegistry.Instance.WebAdminsEmailNotificationGroup, out language);

			using (Res.TemporarilySwitchLanguage(language))
			{
				var subject = Res.GetString("ef43bce0-1240-4715-b1e5-8b59de9e0846", "{0} Website Password Reset Request", CompanyName);
				var footer = "<br />" + Res.GetString("13cf4ec1-c6d7-411d-a9e8-7046168dd027", "{0} Web Administrator Notification", CompanyName) + "<br /><br />";

				var body = new StringBuilder();

				body.Append(Res.GetString("b11a0f6a-edfd-47f5-bde3-e6ffa3c7e99a", "The following email address was used to try to obtain a web password reset:"));
				body.AppendFormat(CultureInfo.InvariantCulture, @"
<p>
<i>{0}</i>
</p>
", EmailAddress);
				body.Append(Res.GetString("bad31b55-911f-4918-bd97-d729460a743a", "This email address is not registered against any organization contacts within your system. If you recognize this email address, you may wish to set up a web account for this person."));

				new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(notification,
						subject,
						body.ToString(),
						footer,
						Guid.Empty);

				if (CompanyName.IsValid && !CompanyName.IsEmpty)
				{
					notification.FromDisplayName = CompanyName;
				}

				try
				{
					Env.OutgoingMailManager.CreateAndSave(notification);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					//if sending of email is failed, do nothing and continue operation
				}
			}
		}
	}
}
