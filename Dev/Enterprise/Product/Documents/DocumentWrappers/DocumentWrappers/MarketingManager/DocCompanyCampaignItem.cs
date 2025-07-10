using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocCompanyCampaignItem : DocumentWrapper, Integration.DocumentWrappers.IDocCompanyCampaignItem
	{
		protected DocCompanyCampaignItem(GlbCompanyCampaignItem item, BusinessObjectFactory factoryForWrapper)
			: base(item, factoryForWrapper)
		{
		}

		public static DocCompanyCampaignItem New(GlbCompanyCampaignItem item, BusinessObjectFactory factoryForWrapper)
		{
			return item != null ? new DocCompanyCampaignItem(item, factoryForWrapper) : null;
		}

		#region Document Fields

		#region Contact

		[DocumentField("The Contact's full name")]
		public ZString ContactName
		{
			get { return Item.ContactName; }
		}

		[DocumentField("The Contact's first name")]
		public ZString ContactFirstName
		{
			get
			{
				ZString result = ContactName;
				if (!result.IsEmpty)
				{
					return result.Split(new char[] { ' ' })[0];
				}

				return result;
			}
		}

		[DocumentField("Name to appear on the document, for example 'John'. If this is blank, the contact's full name will be displayed")]
		public ZString ContactSalutation
		{
			get
			{
				ZString result = Item.Recipient.Salutation;
				if (result.IsEmpty)
				{
					result = Item.ContactName;
				}
				return result;
			}
		}

		[DocumentField("The Contact's job title")]
		public ZString ContactJobTitle
		{
			get { return Item.Recipient.Title; }
		}

		[DocumentField("The Contact's email address")]
		public ZString ContactEmail
		{
			get { return Item.EmailAddress; }
		}

		[DocumentField("The Contact's phone number")]
		public ZString ContactPhone
		{
			get { return Item.Recipient.Phone; }
		}

		[DocumentField("The Contact's mobile phone number")]
		public ZString ContactMobile
		{
			get { return Item.Recipient.Mobile; }
		}

		[DocumentField("The Contact's fax number")]
		public ZString ContactFax
		{
			get { return Item.Recipient.Fax; }
		}

		[DocumentField("The Contact's Web Access password")]
		public ZString ContactWebPassword
		{
			get
			{
				IPasswordEmailSource recipientWithPassword = Item.Recipient as IPasswordEmailSource;
				return (recipientWithPassword != null) ? recipientWithPassword.Password : ZString.Empty;
			}
		}

		[DocumentField("The Contact's login to Learning Centre")]
		public ZString ContactLearningCentreLogin
		{
			get { return Item.EmailAddress; }
		}

		[DocumentField("The Contact's password to Learning Centre")]
		public ZString ContactLearningCentrePassword
		{
			get
			{
				IPasswordEmailSource recipientWithPassword = Item.Recipient as IPasswordEmailSource;
				return (recipientWithPassword != null) ? recipientWithPassword.Password : ZString.Empty;
			}
		}

		#endregion

		#region Organisation

		[DocumentField("The full name of a Contact's Organisation")]
		public ZString OrganisationName
		{
			get { return GetOrgHeaderPropertyValue(org => org.OH_FullName); }
		}

		[DocumentField("The code of a Contact's Organisation")]
		public ZString OrganisationCode
		{
			get { return GetOrgHeaderPropertyValue(org => org.OH_Code); }
		}

		[DocumentField("The main address of this contact's organisation")]
		public ZString OrganisationMainAddress
		{
			get { return GetOrgHeaderPropertyValue(org => org.MainAddress.AddressAsASingleLine); }
		}

		[DocumentField("The main address of this contact's organisation (line break in html format)")]
		public ZString OrganisationMainAddressInHTMLWithoutCompanyName
		{
			get
			{
				return GetOrgHeaderPropertyValue<ZString>(org =>
					new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false)
						.PostalAddressWithoutCompanyName().Replace("\n", "<br />"));
			}
		}

		delegate T GetOrgHeaderPropertyValueDelegate<T>(OrgHeader org);

		T GetOrgHeaderPropertyValue<T>(GetOrgHeaderPropertyValueDelegate<T> getPropertyValue)
		{
			OrgHeader org = Item.Recipient.Organisation;
			return (org != null) ? getPropertyValue(org) : default(T);
		}

		#endregion

		#region Dates

		[DocumentField("Today's date in short date format eg 28/11/2005")]
		public ZString ShortDate
		{
			get { return ZDateTime.Now.ToShortDateString(); }
		}

		[DocumentField("Today's date in long date format eg Monday, 28 November 2005")]
		public ZString LongDate
		{
			get { return ZDateTime.Now.ToDateTime().ToLongDateString(); }
		}

		[DocumentField("Today's date in long date format, without the day eg, 28 November 2005")]
		public ZString LongDateNoDay
		{
			get { return ZDateTime.Now.ToDateTime().ToString("dd MMMM yyyy"); }
		}

		#endregion

		#region Campaign

		[DocumentField("The Campaign Name")]
		public ZString CampaignName
		{
			get { return Item.CompanyCampaign.G0_CampaignNameMultilingual; }
		}

		[DocumentField("The Campaign ID")]
		public ZString CampaignID
		{
			get { return Item.CompanyCampaign.CampaignID; }
		}

		[DocumentField("The Campaign URL for Voting / Exam / Survey Campaign")]
		public ZString CampaignURL
		{
			get { return Item.VoteExamSurveyCampaignURL; }
		}

		[DocumentField("The link to unsubscribe contact from all campaigns.")]
		public ZString UnsubscribeFromAllCampaignOfthisSenderUrl => Item.GetUnsubscribeUrlString(UnsubscribeType.Sender);

		[DocumentField("The link to unsubscribe contact from campaigns of media category.")]
		public ZString UnsubscribeFromMediaCategoryUrl => Item.GetUnsubscribeUrlString(UnsubscribeType.MediaCategory);

		[DocumentField("The link to unsubscribe contact from campaigns of media type.")]
		public ZString UnsubscribeFromMediaTypeUrl => Item.GetUnsubscribeUrlString(UnsubscribeType.MediaType);

		[DocumentField("The link to unsubscribe contact from campaigns of particular media category and type.")]
		public ZString UnsubscribeFromMediaCategoryAndTypeUrl => Item.GetUnsubscribeUrlString(UnsubscribeType.MediaCategoryAndType);

		[DocumentField("The link to subscribe preference for preconfigured combinations media category and type.")]
		public ZString SubscriptionPreferenceUrl => Item.GetSubscriptionPreferenceUrlString();

		[DocumentField("This link to send a specified type of agreement to the user.")]
		public ZString GetMyAccountUserAgreementUrl(ZString agreementType, ZBool sendAgreementCopy) => Item.GetMyAccountUserAgreementUrl(agreementType, sendAgreementCopy);

		#endregion

		#region Campaign Item

		[DocumentField("The PK of the campaign item")]
		public ZString CampaignItemPk
		{
			get { return Item.PK.ToGuid().ToString("N"); }
		}

		#endregion

		#region Coordinator

		[DocumentField("The Campaign Coordinator's full name")]
		public ZString CampaignCoordinatorName
		{
			get
			{
				ZString result = "";
				if (Item.CompanyCampaign.CampaignCoordinator != null)
				{
					if (!Item.CompanyCampaign.CampaignCoordinator.GS_FriendlyName.IsEmpty)
					{
						result = Item.CompanyCampaign.CampaignCoordinator.GS_FriendlyName;
					}
					else
					{
						result = Item.CompanyCampaign.CampaignCoordinator.GS_FullName;
					}
				}
				return result;
			}
		}

		[DocumentField("The Campaign Coordinator's Email Address")]
		public ZString CampaignCoordinatorEmail
		{
			get { return Item.CompanyCampaign.CampaignCoordinator != null ? Item.CompanyCampaign.CampaignCoordinator.GS_EmailAddress : ZString.Empty; }
		}

		[DocumentField("The Campaign Coordinator's Work Phone Number")]
		public ZString CampaignCoordinatorWorkPhone
		{
			get { return Item.CompanyCampaign.CampaignCoordinator != null ? Item.CompanyCampaign.CampaignCoordinator.GS_WorkPhone_Formatted : ZString.Empty; }
		}

		[DocumentField("The Campaign Coordinator's Mobile Phone Number")]
		public ZString CampaignCoordinatorMobilePhone
		{
			get { return Item.CompanyCampaign.CampaignCoordinator != null ? Item.CompanyCampaign.CampaignCoordinator.GS_MobilePhone_Formatted : ZString.Empty; }
		}

		[DocumentField("The Campaign Coordinator's Job Title")]
		public ZString CampaignCoordinatorTitle
		{
			get { return Item.CompanyCampaign.CampaignCoordinator != null ? Item.CompanyCampaign.CampaignCoordinator.GS_Title : ZString.Empty; }
		}

		#endregion

		#region Manager

		[DocumentField("The Campaign Manager's full name")]
		public ZString CampaignManagerName
		{
			get
			{
				ZString result = "";
				if (Item.CompanyCampaign.CampaignManager != null)
				{
					if (!Item.CompanyCampaign.CampaignManager.GS_FriendlyName.IsEmpty)
					{
						result = Item.CompanyCampaign.CampaignManager.GS_FriendlyName;
					}
					else
					{
						result = Item.CompanyCampaign.CampaignManager.GS_FullName;
					}
				}
				return result;
			}
		}

		[DocumentField("The Campaign Manager's Email Address")]
		public ZString CampaignManagerEmail
		{
			get { return Item.CompanyCampaign.CampaignManager != null ? Item.CompanyCampaign.CampaignManager.GS_EmailAddress : ZString.Empty; }
		}

		[DocumentField("The Campaign Manager's Work Phone Number")]
		public ZString CampaignManagerWorkPhone
		{
			get { return Item.CompanyCampaign.CampaignManager != null ? Item.CompanyCampaign.CampaignManager.GS_WorkPhone_Formatted : ZString.Empty; }
		}

		[DocumentField("The Campaign Manager's Mobile Phone Number")]
		public ZString CampaignManagerMobilePhone
		{
			get { return Item.CompanyCampaign.CampaignManager != null ? Item.CompanyCampaign.CampaignManager.GS_MobilePhone_Formatted : ZString.Empty; }
		}

		[DocumentField("The Campaign Manager's Job Title")]
		public ZString CampaignManagerTitle
		{
			get { return Item.CompanyCampaign.CampaignManager != null ? Item.CompanyCampaign.CampaignManager.GS_Title : ZString.Empty; }
		}

		#endregion

		#region Current Company

		[DocumentField("Your company's name")]
		public ZString CurrentCompanyName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		[DocumentField("Your company's phone number")]
		public ZString CurrentCompanyPhone
		{
			get { return GlbCompany.CurrentCompany.GC_Phone_Formatted; }
		}

		[DocumentField("Your company's fax number")]
		public ZString CurrentCompanyFax
		{
			get { return GlbCompany.CurrentCompany.GC_Fax_Formatted; }
		}

		[DocumentField("Your company's web site")]
		public ZString CurrentCompanyWebSite
		{
			get { return GlbCompany.CurrentCompany.GC_WebAddress; }
		}

		#endregion

		#region Sales Rep

		[DocumentField("The Overall Sales Representative's name for the specified contact's organisation")]
		public ZString SalesRepresentativeName
		{
			get { return SalesRep != null ? SalesRep.GS_FullName : ZString.Empty; }
		}

		[DocumentField("The Overall Sales Representative's job title for the specified contact's organisation")]
		public ZString SalesRepresentativeTitle
		{
			get { return SalesRep != null ? SalesRep.GS_Title : ZString.Empty; }
		}

		[DocumentField("The Overall Sales Representative's email address for the specified contact's organisation")]
		public ZString SalesRepresentativeEmail
		{
			get { return SalesRep != null ? SalesRep.GS_EmailAddress : ZString.Empty; }
		}

		[DocumentField("The Overall Sales Representative's work phone number for the specified contact's organisation")]
		public ZString SalesRepresentativeWorkPhone
		{
			get { return SalesRep != null ? SalesRep.GS_WorkPhone_Formatted : ZString.Empty; }
		}

		[DocumentField("The Overall Sales Representative's work mobile number for the specified contact's organisation")]
		public ZString SalesRepresentativeMobilePhone
		{
			get { return SalesRep != null ? SalesRep.GS_MobilePhone_Formatted : ZString.Empty; }
		}

		GlbStaff SalesRep
		{
			get { return Item.GetAssignedStaff(StaffAssignmentRoles.Codes.SalesRep); }
		}

		#endregion

		#region Email Sender

		[DocumentField("The sender's name that matches the email sender selection process")]
		public ZString EmailSenderName
		{
			get
			{
				return Item.SenderName;
			}
		}

		[DocumentField("The sender's title that matches the email sender selection process")]
		public ZString EmailSenderTitle
		{
			get
			{
				return Item.SenderTitle;
			}
		}

		[DocumentField("The sender's email that matches the email sender selection process")]
		public ZString EmailSenderEmail
		{
			get
			{
				return Item.SenderEmailAddress;
			}
		}

		[DocumentField("The sender's work phone that matches the email sender selection process")]
		public ZString EmailSenderWorkPhone
		{
			get
			{
				return Item.SenderWorkPhone;
			}
		}

		[DocumentField("The sender's mobile phone that matches the email sender selection process")]
		public ZString EmailSenderMobilePhone
		{
			get
			{
				return Item.SenderMobilePhone;
			}
		}

		[DocumentField("Get the email sender staff")]
		public DocStaff EmailSenderStaff
		{
			get
			{
				var staff = Item.GetEmailSenderStaff();
				return staff != null ? DocStaff.New(staff, staff.Factory) : null;
			}
		}

		#endregion

		#region Assigned Staff

		[DocumentField("Get staff assignment for specified role. Replace {role} with a valid organization staff assignment role code, for example: GetStaffAssignment(SAL).")]
		public DocStaff GetStaffAssignment(ZString role)
		{
			var staff = Item.GetAssignedStaff(role);
			return staff != null ? DocStaff.New(staff, staff.Factory) : null;
		}

		#endregion

		#region Exam Link

		[DocumentField("Get URL of online learning centre exam with specified exam ID. Replace {examId} with a valid Exam ID from Learning Centre module. Ensure the exam has a Related Job Skill Test (must be linked from the Job Skill module). If no URL is generated, it may be because the Learning Centre has no Related Job Skill Tests. You will need to add one from the Job Skill module. URLs can only be generated for Recipients who are an Organisation Contact, Job Applicant or Staff member")]
		public ZString RecipientAndExam(ZString examId)
		{
			return RecipientAndExam(examId, "", "");
		}

		[DocumentField("Get URL of online learning centre exam with specified exam ID. Replace {examId} with a valid Exam ID from Learning Centre module. Replace {examSettingsCode} with a proper Version from the related Exam Settings. Ensure the exam has a Related Job Skill Test (must be linked from the Job Skill module). If no URL is generated, it is because the Learning Centre has no Related Job Skill Tests. You will need to add one from the Job Skill module. URLs can only be generated for Recipients who are an Organisation Contact, Job Applicant or Staff Member.")]
		public ZString RecipientAndExam(ZString examId, ZString examSettingsCode)
		{
			return RecipientAndExam(examId, examSettingsCode, "");
		}

		[DocumentField("Get URL of online learning centre exam with specified exam ID. Replace {examId} with a valid Exam ID from Learning Centre module. Replace {examSettingsCode} with a proper Version from the related Exam Settings. Replace {jobSkillCode} with one of the exam-related skill codes. Ensure the exam has a Related Job Skill Test (must be linked from the Job Skill module). If no URL is generated, it is because the Learning Centre has no Related Job Skill Tests. You will need to add one from the Job Skill module. URLs can only be generated for Recipients who are an Organisation Contact, Job Applicant or Staff Member.")]
		public ZString RecipientAndExam(ZString examId, ZString examSettingsCode, ZString skillCode)
		{
			var result = ZString.Empty;
			var recipient = Item.RecipientAsExamUrlRecipient;

			if (recipient != null)
			{
				var language = recipient.Language;
				result = RecipientAndExamWithLanguage(examId, examSettingsCode, skillCode, language);
			}
			return result;
		}

		[DocumentField("Get URL of online learning centre exam with specified exam ID. Replace {examId} with a valid Exam ID from Learning Centre module. Replace {examSettingsCode} with a proper Version from the related Exam Settings. Replace {language} with a valid language from the web page selector. Ensure the exam has a Related Job Skill Test (must be linked from the Job Skill module). If no URL is generated, it is because the Learning Centre has no Related Job Skill Tests. You will need to add one from the Job Skill module. URLs can only be generated for Recipients who are an Organisation Contact, Job Applicant or Staff Member.")]
		public ZString RecipientAndExamWithLanguage(ZString examId, string examSettingsCode, string language)
		{
			return RecipientAndExamWithLanguage(examId, examSettingsCode, "", language);
		}

		[DocumentField("Get URL of online learning centre exam with specified exam ID. Replace {examId} with a valid Exam ID from Learning Centre module. Replace {language} with a valid language from the web page selector. Replace {examSettingsCode} with a proper Version from the related Exam Settings. Replace {jobSkillCode} with one of the exam-related skill codes. Ensure the exam has a Related Job Skill Test (must be linked from the Job Skill module). If no URL is generated, it is because the Learning Centre has no Related Job Skill Tests. You will need to add one from the Job Skill module. URLs can only be generated for Recipients who are an Organisation Contact, Job Applicant or Staff Member.")]
		public ZString RecipientAndExamWithLanguage(ZString examId, string examSettingsCode, string skillCode, string language)
		{
			var result = ZString.Empty;
			var recipient = Item.RecipientAsExamUrlRecipient;

			if (recipient != null)
			{
				if (recipient.HasNonPermittedDuplicateEmail)
				{
					result = Res.GetString("713a93cd-e792-4b62-8e01-e96151a4cd3e", "Exam URL could not be generated for this recipient because there was a duplicate of their email in the database.");
				}
				else
				{
					result = LearningCentreTestUrlHelper.GetTestUrl(examId, recipient, string.Empty, skillCode, language, examSettingsCode);
				}
			}
			return result;
		}

		#endregion

		#region Campaign Custom Field

		[DocumentField("Get campaign custom field value by field name. Replace {fieldName} with name of custom field specified in Workflow Manager module.")]
		public ZString GetCampaignCustomField(ZString fieldName)
		{
			var result = ZString.Empty;
			if (Item.CompanyCampaign != null)
			{
				result = (ZString)BODocDataProvider.GetCustomField(Item.CompanyCampaign, fieldName, null);
			}
			return result;
		}

		#endregion

		#region Link Tracking

		[DocumentField("The trackable link for the given link context and destination URL.")]
		public ZString LinkTracking(ZString contextDisplayName, ZString destinationURL)
		{
			var result = ZString.Empty;
			if (Item.CompanyCampaign != null)
			{
				result = GlbCompanyCampaignLink.GenerateTrackedUrl(Item, contextDisplayName, destinationURL);
			}
			return result;
		}

		[DocumentField("The trackable link for the given link context.")]
		public ZString LinkTracking(ZString contextDisplayName)
		{
			var result = ZString.Empty;
			if (Item.CompanyCampaign != null)
			{
				result = GlbCompanyCampaignLink.GenerateTrackedUrl(Item, contextDisplayName, ZString.Empty);
			}
			return result;
		}

		#endregion

		#region GetContextPK

		[DocumentField("Get the PK of tracked link by its context display name. Replace {contextDisplayName} with the context name of link.")]
		public ZString GetContextPK(ZString contextDisplayName)
		{
			var result = ZString.Empty;
			var campaign = Item.CompanyCampaign;
			if (campaign != null)
			{
				var link = campaign.TrackedLinks.FirstOrDefault(x => x.GCL_Context.EqualsIgnoringCase(contextDisplayName));
				if (link != null)
				{
					result = link.PK.ToGuid().ToString("N");
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Implementation

		GlbCompanyCampaignItem Item
		{
			get { return (GlbCompanyCampaignItem)WrappedObject; }
		}

		public override string ToString()
		{
			return Item.Code;
		}

		#endregion
	}
}
