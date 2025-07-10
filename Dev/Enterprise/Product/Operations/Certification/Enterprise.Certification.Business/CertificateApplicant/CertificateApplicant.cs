using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Certification;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Certification.Business
{
	public class CertificateApplicant : HRJobApplicant, ICertificateApplicant
	{
		public CertificateApplicant(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ApprovingStaffPK

		public string ApprovedBy
		{
			get
			{
				if (approvedBy == null)
				{
					ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, approvedByReferencePrefix);
					StmALog approvedByLog = Logs.MostRecentLogByEventTime(AutoEvents.EditedARecord, query);
					if (approvedByLog != null)
					{
						Match match = Regex.Match(approvedByLog.SL_Reference, approvedByReferenceRegexPattern);
						approvedBy = (match.Success && match.Groups.Count > 1) ? match.Groups[1].Value : "";
					}
					else
					{
						approvedBy = "";
					}
				}
				return approvedBy;
			}
			set { approvedBy = value; }
		}

		GlbStaff ApprovingStaff => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ApprovedBy);

		string approvedBy;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string approvedByReferencePrefix = "Approved by ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string approvedByReferenceFormat = "Approved by {0} ({1})";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string approvedByReferenceRegexPattern = @"^Approved by (?:.+) \((.+)\)$";

		#endregion

		#region RelatedOrgContactPK

		public ZGuid RelatedOrgContactPK
		{
			get
			{
				if (relatedOrgContactPK == null)
				{
					ZGuid contactPK = ZGuid.Empty;
					ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, relatedOrgContactReferencePrefix);
					StmALog createdFromContactLog = Logs.MostRecentLogByEventTime(AutoEvents.EditedARecord, query);
					if (createdFromContactLog != null)
					{
						Match match = Regex.Match(createdFromContactLog.SL_Reference, relatedOrgContactReferenceRegexPattern);
						if (match.Success && match.Groups.Count > 1)
						{
							ZGuid.TryParse(match.Groups[1].Value, out contactPK);
						}
					}
					relatedOrgContactPK = contactPK;
				}
				return relatedOrgContactPK.Value;
			}
			set { relatedOrgContactPK = value; }
		}

		OrgContact RelatedOrgContact => Factory.Load<OrgContact>(RelatedOrgContactPK);

		ZGuid? relatedOrgContactPK;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string relatedOrgContactReferencePrefix = "Related Contact: ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string relatedOrgContactReferenceFormat = "Related Contact: {0} ({1}) | ID: {2}";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string relatedOrgContactReferenceRegexPattern = @"^Related Contact: (?:.+) \((?:.+)\) \| ID: (.+)$";

		#endregion

		#region RelatedGlbStaffPK

		public ZGuid RelatedGlbStaffPK
		{
			get
			{
				if (relatedGlbStaffPK == null)
				{
					var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, relatedGlbStaffReferencePrefix);
					var createdFromStaffLog = Logs.MostRecentLogByEventTime(AutoEvents.EditedARecord, query);
					if (createdFromStaffLog != null)
					{
						var match = Regex.Match(createdFromStaffLog.SL_Reference, relatedGlbStaffReferenceRegexPattern);
						if (match.Success && match.Groups.Count > 1)
						{
							if (ZGuid.TryParse(match.Groups[1].Value, out var staffPK))
							{
								relatedGlbStaffPK = staffPK;
							}
						}
					}

					if (relatedGlbStaffPK == null)
					{
						relatedGlbStaffPK = ZGuid.Empty;
					}
				}

				return relatedGlbStaffPK.Value;
			}
			set => relatedGlbStaffPK = value;
		}

		GlbStaff RelatedGlbStaff => Factory.Load<GlbStaff>(RelatedGlbStaffPK);

		ZGuid? relatedGlbStaffPK;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string relatedGlbStaffReferencePrefix = "Related Staff: ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string relatedGlbStaffReferenceFormat = "Related Staff: {0} ({1}) | ID: {2}";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		const string relatedGlbStaffReferenceRegexPattern = @"^Related Staff: (?:.+) \((?:.+)\) \| ID: (.+)$";

		#endregion

		#region  IsLearningCenterUser

		public override ZBool IsLearningCenterUser => base.IsLearningCenterUser || IsApprovedAnonymousUser || IsOrgContactRelatedUser || IsGlbStaffRelatedUser;

		#endregion

		public new CertificateApplicantValidation Validation => (CertificateApplicantValidation)base.Validation;

		protected override HRJobApplicantValidation GetNewValidation()
		{
			return new CertificateApplicantValidation(this);
		}

		#region Saving / Saved logic

		public override bool IsSavedByFactory => base.IsSavedByFactory && CanSaveUserToDb;

		bool CanSaveUserToDb => IsInDatabase
						|| RecruiterDataRegistry.Instance.AllowAnonymousLearningCentreUserRegistration.Value
						|| IsApprovedAnonymousUser
						|| IsOrgContactRelatedUser
						|| IsGlbStaffRelatedUser;

		bool IsApprovedAnonymousUser => ApprovingStaff != null;

		bool IsOrgContactRelatedUser => RelatedOrgContact != null;

		bool IsGlbStaffRelatedUser => RelatedGlbStaff != null;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsInDatabase)
			{
				if (!CanSaveUserToDb)
				{
					if (!approvalEmailSent)
					{
						CreateAnonymousUserRegistrationEmail(true, false);
						approvalEmailSent = true;
					}
				}
				else
				{
					shouldSendNotificationEmail = !(IsOrgContactRelatedUser || IsGlbStaffRelatedUser);
				}
			}
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				if (IsApprovedAnonymousUser)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, string.Format(approvedByReferenceFormat, ApprovingStaff.GS_FullName, ApprovingStaff.GS_Code));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (IsOrgContactRelatedUser)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, string.Format(relatedOrgContactReferenceFormat, RelatedOrgContact.OC_ContactName, RelatedOrgContact.OrganisationCode, RelatedOrgContact.PK));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (IsGlbStaffRelatedUser)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, string.Format(relatedGlbStaffReferenceFormat, RelatedGlbStaff.GS_FullName, RelatedGlbStaff.GS_Code, RelatedGlbStaff.PK));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (shouldSendNotificationEmail)
				{
					if (RecruiterDataRegistry.Instance.AllowAnonymousLearningCentreUserRegistration.Value)
					{
						CreateAnonymousUserRegistrationEmail(false, true);
					}
					shouldSendNotificationEmail = false;
				}
			}
		}

		bool approvalEmailSent;
		bool shouldSendNotificationEmail;

		#endregion

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new CertificateApplicantUniqueIndexFailureHandler(this); }
		}

		public static CertificateApplicant GetNewCertificateApplicantForSpecialUser(BusinessObjectFactory factory, string username)
		{
			CertificateApplicant result = factory.New<CertificateApplicant>();
			result.HA_FullName = username;
			return result;
		}

		#region CreateAnonymousUserRegistrationEmail

		void CreateAnonymousUserRegistrationEmail(bool isApprovalEmail, bool shouldSave)
		{
			ZGuid notificationGroupPK = new ZGuid(RecruiterDataRegistry.Instance.LearningCentreUserRegistrationNotificationGroup.Value);
			GlbGroup notificationGroup = Factory.Load<GlbGroup>(notificationGroupPK);
			if (notificationGroup != null)
			{
				AnonymousUserRegistrationEmailCreator emailCreator = new AnonymousUserRegistrationEmailCreator(this, isApprovalEmail);
				BusinessObjectFactory factory = (shouldSave) ? new BusinessObjectFactory() : Factory;
				foreach (GlbStaff staffToNotify in notificationGroup.Staff)
				{
					if (!staffToNotify.GS_EmailAddress.IsEmpty)
					{
						HtmlEmailDef anonymousUserRegistrationEmail = emailCreator.Create(staffToNotify);
						Env.OutgoingMailManager.Create(factory, anonymousUserRegistrationEmail);
					}
				}

				if (shouldSave)
				{
					factory.Save();
				}
			}
		}

		class AnonymousUserRegistrationEmailCreator
		{
			public AnonymousUserRegistrationEmailCreator(CertificateApplicant applicant, bool requiresApproval)
			{
				this.applicant = applicant;
				this.requiresApproval = requiresApproval;
			}

			public HtmlEmailDef Create(GlbStaff approvingStaff)
			{
				HtmlEmailDef result = new HtmlEmailDef();
				result.AddRecipientForUserCommunication(approvingStaff.GS_EmailAddress);
				if (!string.IsNullOrEmpty(applicant.FromAddress))
				{
					result.FromAddress = applicant.FromAddress;
				}
				if (!string.IsNullOrEmpty(applicant.FromDisplayName))
				{
					result.FromDisplayName = applicant.FromDisplayName;
				}

				result.Subject = Res.GetString("b3f9b7ee-118e-4c75-8009-bcd7f3532592", "{0} WebAccreditation Anonymous User Registration", BrandingFactory.Instance.ProductName);
				string populatedHtmlContent = GetPopulatedHtmlContent(approvingStaff);
				string cssElements = GetCssElements(populatedHtmlContent);
				string bodySection = GetBodySection(populatedHtmlContent);
				if (requiresApproval)
				{
					result.Attachments.Add(new AttachmentDef(BrandingFactory.Instance.ProductName + (NoResString)" WebAccreditation - Approve Anonymous User.htm", Encoding.ASCII.GetBytes(populatedHtmlContent)));
				}
				result.LoadHtmlUsingTemplate(bodySection, cssElements);
				return result;
			}

			string GetPopulatedHtmlContent(GlbStaff approvingStaff)
			{
				SecureQueryString[ApprovedCertificateApplicantCreator.FieldNames.ApprovingStaffCode] = approvingStaff.GS_Code;
				if (!commonValuesPopulated)
				{
					PopulateCommonValues();
				}

				string result = Builder.ToString();
				if (requiresApproval)
				{
					result = result.Replace(ApprovedCertificateApplicantCreator.FieldNames.UserDetailsAsQueryString, WebUtility.HtmlEncode(SecureQueryString.ToString()));
				}
				return result;
			}

			void PopulateCommonValues()
			{
				Builder.Replace("(*Caption*)", GetCaption());
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Title, applicant.HA_Title);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.FullName, applicant.HA_FullName);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.NameSuffix, applicant.HA_NameSuffix);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.DOB, applicant.HA_Birthdate.ToShortDateString());
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Gender, applicant.Lookups.Genders.GetDescriptionFromCode(applicant.HA_Gender), applicant.HA_Gender);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Address1, applicant.HA_UserAddress1);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Address2, applicant.HA_UserAddress2);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.City, applicant.HA_City);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.State, applicant.HA_State);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.PostCode, applicant.HA_Postcode);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Country, (!applicant.HA_RN_NKCountry.IsEmpty) ? applicant.Country.Description : ZString.Empty, applicant.HA_RN_NKCountry);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Email, applicant.HA_EmailAddress);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Mobile, applicant.HA_MobilePhone);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Home, applicant.HA_HomePhone);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Fax, applicant.HA_FaxNum);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Work, applicant.HA_WorkPhone);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Extension, applicant.HA_WorkExtension);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.PassportNo, applicant.HA_Passport);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.LicenseNo, applicant.HA_DriversLicenseNumber);
				PopulateValue(ApprovedCertificateApplicantCreator.FieldNames.Nationality, (!applicant.HA_RN_NKNationalityCodeISO.IsEmpty) ? applicant.Person.NationalityCodeISO.Description : ZString.Empty, applicant.HA_RN_NKNationalityCodeISO);

				if (!requiresApproval)
				{
					Builder.Replace(@"<tr><td align=""center"" colspan=""2""><br /><input type=""submit"" value=""Approve""/><!--CannotSubmit--></td></tr>", "");
				}
				else
				{
					string url = string.Format((NoResString)"{0}/AllAccess/CreateUser.aspx", WebDataRegistry.Instance.WebCertificationUrl.Value.TrimEnd('/'));
					Builder.Replace("(*HttpPostUrl*)", url);
				}

				commonValuesPopulated = true;
			}

			bool commonValuesPopulated;

			string GetCaption()
			{
				string result;

				if (requiresApproval)
				{
					result = Res.GetString("FF7B3FA9-00FE-4112-9E95-3C0FD30B3BF6", "Anonymous Registration has been requested for the following user.") + BrHtmlElement + "\r\n" +
							 Res.GetString("813E6F71-F7E6-45a6-AA3F-E27F87E9F63D", "Once approved, an email containing a randomly generated password will be sent to the user.") + BrHtmlElement + "\r\n" +
							 Res.GetString("0583F78D-7DBD-4f97-ADF3-79E78F713183", "You are receiving this email because you are a member of {0} WebAccreditation User Registration Notification Group", BrandingFactory.Instance.ProductName);
				}
				else
				{
					result = Res.GetString("442af9c9-94eb-45a7-8eab-01ebd967dd9b", "The following anonymous user account has been created. No action is required.");
				}

				return result;
			}

			internal const string BrHtmlElement = "<br />";

			void PopulateValue(string keyName, string valueForDisplay)
			{
				PopulateValue(keyName, valueForDisplay, valueForDisplay);
			}

			void PopulateValue(string keyName, string valueForDisplay, string valueForQueryString)
			{
				Builder.Replace(keyName, valueForDisplay);
				if (!string.IsNullOrEmpty(valueForQueryString))
				{
					SecureQueryString.Add(keyName, valueForQueryString);
				}
			}

			string GetCssElements(string htmlContent)
			{
				return GetHtmlSection(htmlContent, (NoResString)"<style type=\"text/css\">(.+)</style>");
			}

			string GetBodySection(string htmlContent)
			{
				string result = GetHtmlSection(htmlContent, (NoResString)"<body>(.+)</body>");
				result = result.Replace("<!--CannotSubmit-->", "<br/><span style='color:red'>Please approve from the HTML file attached if the Approve button above is disabled by your email client</span>");
				return result;
			}

			string GetHtmlSection(string htmlContent, string regexPattern)
			{
				Match cssElementsMatch = Regex.Match(htmlContent, regexPattern, RegexOptions.Singleline);
				return (cssElementsMatch.Success && cssElementsMatch.Groups.Count > 1) ? cssElementsMatch.Groups[1].Value : "";
			}

			StringBuilder Builder => builder ?? (builder = new StringBuilder(TemplateContent));

			SecureQueryString SecureQueryString => secureQueryString ?? (secureQueryString = new SecureQueryString());

			string TemplateContent
			{
				get
				{
					if (templateContent == null)
					{
						Assembly executingAssembly = Assembly.GetExecutingAssembly();
						using (Stream stream = executingAssembly.GetManifestResourceStream(approvalEmailTemplateResourceName))
						using (StreamReader streamReader = new StreamReader(stream))
						{
							templateContent = streamReader.ReadToEnd();
						}
					}
					return templateContent;
				}
			}

			StringBuilder builder;
			SecureQueryString secureQueryString;
			string templateContent;

			readonly CertificateApplicant applicant;
			readonly bool requiresApproval;

			const string approvalEmailTemplateResourceName = "Enterprise.Certification.Business.CertificateApplicant.RegistrationApprovalEmailTemplate.htm";
		}

		#endregion

		protected override bool AllowCompulsorySkillRating => false;
	}
}
