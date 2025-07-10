using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIWebSalesInquiry : SalesEnquiry, IObsoleteValidation
	{
		public EDIWebSalesInquiry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EDIWebSalesInquiryLookups Lookups
		{
			get { return new EDIWebSalesInquiryLookups(this); }
		}

		public new EDIWebSalesInquiryValidation Validation
		{
			get { return (EDIWebSalesInquiryValidation)GetNewValidation(); }
		}

		protected override OrgColdCallRegisterValidation GetNewValidation()
		{
			return new EDIWebSalesInquiryValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
		}

		#region HTML Link

		public string HtmlLink
		{
			get { return "<a href=\"" + HtmlUrl + "\">" + O1_LeadUniqueReference + "</a>"; }
		}

		string HtmlUrl
		{
			get { return ShowEditFormUrlHandler.Instance.Create(ControllerIDs.SalesEnquiry, PK); }
		}

		#endregion

		#region Schema

		public new class Schema : SalesEnquiry.Schema
		{
			public const string WorkPhoneNationalCode = "WorkPhoneNationalCode";
			public const string WorkPhoneExtension = "WorkPhoneExtension";
			public const string MobileNationalCode = "MobileNationalCode";
			public const string JobTitle = "JobTitle";
			public const string JobRole = "JobRole";
			public const string CompanySize = "CompanySize";
			public const string TypeOfBusinessOther = "TypeOfBusinessOther";
			public const string ReasonForRequestingAccessOverwrite = "ReasonForRequestingAccessOverwrite";
			public const string AdditionalInformation = "AdditionalInformation";

			public const int WorkPhoneNationalCodeMaxLength = 4;
			public const int WorkPhoneExtensionMaxLength = 6;
			public const int MobileNationalCodeMaxLength = 4;
			public const int JobTitleMaxLength = 50;
			public const int JobRoleMaxLength = 50;
			public const int CompanySizeMaxLength = 50;
			public const int TypeOfBusinessOtherMaxLength = 300;
			public const int ReasonForRequestingAccessOverwriteMaxLength = 300;
			public const int AdditionalInformationMaxLength = 5000;
		}

		#endregion

		#region Properties

		#region O1_Address1

		public ZBool O1_Address1_IsMandatory
		{
			get { return IsMyAccountRequest; }
		}

		#endregion

		#region O1_PostCode

		public ZBool O1_PostCode_IsMandatory
		{
			get { return IsMyAccountRequest; }
		}

		#endregion

		#region Work Phone National Code

		[CargoWise.ComponentModel.MaxLength(Schema.WorkPhoneNationalCodeMaxLength)]
		public ZString WorkPhoneNationalCode
		{
			get { return workPhoneNationalCode; }
			set { SetNonPersistentPropertyValue(WorkPhoneNationalCodeInfo, ref workPhoneNationalCode, value); }
		}
		ZString workPhoneNationalCode;

		public ZPropertyInfo WorkPhoneNationalCodeInfo
		{
			get { return GetZPropertyInfo(Schema.WorkPhoneNationalCode); }
		}

		#endregion

		#region Work Phone Extension

		[CargoWise.ComponentModel.MaxLength(Schema.WorkPhoneExtensionMaxLength)]
		public ZString WorkPhoneExtension
		{
			get { return workPhoneExtension; }
			set { SetNonPersistentPropertyValue(WorkPhoneExtensionInfo, ref workPhoneExtension, value); }
		}
		ZString workPhoneExtension;

		public ZPropertyInfo WorkPhoneExtensionInfo
		{
			get { return GetZPropertyInfo(Schema.WorkPhoneExtension); }
		}

		#endregion

		#region Mobile National Code

		[CargoWise.ComponentModel.MaxLength(Schema.MobileNationalCodeMaxLength)]
		public ZString MobileNationalCode
		{
			get { return mobileNationalCode; }
			set { SetNonPersistentPropertyValue(MobileNationalCodeInfo, ref mobileNationalCode, value); }
		}
		ZString mobileNationalCode;

		public ZPropertyInfo MobileNationalCodeInfo
		{
			get { return GetZPropertyInfo(Schema.MobileNationalCode); }
		}

		#endregion

		#region Job Title

		[CargoWise.ComponentModel.MaxLength(Schema.JobTitleMaxLength)]
		public ZString JobTitle
		{
			get { return jobTitle; }
			set { SetNonPersistentPropertyValue(JobTitleInfo, ref jobTitle, value); }
		}
		ZString jobTitle;

		public ZPropertyInfo JobTitleInfo
		{
			get { return GetZPropertyInfo(Schema.JobTitle); }
		}

		public ZBool JobTitle_IsMandatory
		{
			get { return IsMyAccountRequest; }
		}

		#endregion

		#region Job Role

		[CargoWise.ComponentModel.MaxLength(Schema.JobRoleMaxLength)]
		public ZString JobRole
		{
			get { return jobRole; }
			set { SetNonPersistentPropertyValue(JobRoleInfo, ref jobRole, value); }
		}
		ZString jobRole;

		public ZPropertyInfo JobRoleInfo
		{
			get { return GetZPropertyInfo(Schema.JobRole); }
		}

		public ZBool JobRole_IsMandatory
		{
			get { return IsMyAccountRequest; }
		}

		#endregion

		#region Company Size

		[CargoWise.ComponentModel.MaxLength(Schema.CompanySizeMaxLength)]
		public ZString CompanySize
		{
			get { return companySize; }
			set { SetNonPersistentPropertyValue(CompanySizeInfo, ref companySize, value); }
		}
		ZString companySize;

		public ZPropertyInfo CompanySizeInfo
		{
			get { return GetZPropertyInfo(Schema.CompanySize); }
		}

		public ZBool CompanySize_IsMandatory
		{
			get { return IsMyAccountRequest; }
		}

		#endregion

		#region Type Of Business

		[CargoWise.ComponentModel.MaxLength(Schema.TypeOfBusinessOtherMaxLength)]
		public ZString TypeOfBusinessOther
		{
			get { return typeOfBusinessOther; }
			set { SetNonPersistentPropertyValue(TypeOfBusinessOtherInfo, ref typeOfBusinessOther, value); }
		}
		ZString typeOfBusinessOther;

		public ZPropertyInfo TypeOfBusinessOtherInfo
		{
			get { return GetZPropertyInfo(Schema.TypeOfBusinessOther); }
		}

		public List<IBindableBooleanItem> TypeOfBusinessSelections
		{
			get
			{
				if (typeOfBusinessSelections == null)
				{
					typeOfBusinessSelections = new List<IBindableBooleanItem>();
					foreach (ICodeDescription businessType in Lookups.TypeOfBusinessList)
					{
						typeOfBusinessSelections.Add(new BusinessTypeOption(businessType.Description));
					}
				}
				return typeOfBusinessSelections;
			}
		}
		List<IBindableBooleanItem> typeOfBusinessSelections;

		public ZBool TypeOfBusiness_IsMandatory
		{
			get { return IsMyAccountRequest; }
		}

		public ZString TypeOfBusinessSelectionsAsText
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (IBindableBooleanItem businessType in TypeOfBusinessSelections)
				{
					if (businessType.BoolValue)
					{
						builder.Append(businessType.Text);
						builder.Append(", ");
					}
				}
				builder.Append(TypeOfBusinessOther);
				return builder.ToString().Trim().TrimEnd(',');
			}
		}

		#endregion

		#region Reason For Requesting Access

		[CargoWise.ComponentModel.MaxLength(Schema.ReasonForRequestingAccessOverwriteMaxLength)]
		public ZString ReasonForRequestingAccessOverwrite
		{
			get { return reasonForRequestingAccessOverwrite; }
			set { SetNonPersistentPropertyValue(ReasonForRequestingAccessOverwriteInfo, ref reasonForRequestingAccessOverwrite, value); }
		}
		ZString reasonForRequestingAccessOverwrite;

		public ZPropertyInfo ReasonForRequestingAccessOverwriteInfo
		{
			get { return GetZPropertyInfo(Schema.ReasonForRequestingAccessOverwrite); }
		}

		public List<IBindableBooleanItem> ReasonForRequestingAccessSelections
		{
			get
			{
				if (reasonForRequestingAccessSelections == null)
				{
					reasonForRequestingAccessSelections = new List<IBindableBooleanItem>();
					foreach (ICodeDescriptionBool reason in Lookups.ReasonForRequestingAccessList)
					{
						reasonForRequestingAccessSelections.Add(new RequestReasonOption(reason));
					}
				}
				return reasonForRequestingAccessSelections;
			}
		}
		List<IBindableBooleanItem> reasonForRequestingAccessSelections;

		RequestReasonOption SelectedRequestReasonOption
		{
			get
			{
				foreach (RequestReasonOption option in ReasonForRequestingAccessSelections)
				{
					if (option.BoolValue)
					{
						return option;
					}
				}
				return null;
			}
		}

		public ZString ReasonForRequestingAccess
		{
			get
			{
				var selectedOption = SelectedRequestReasonOption;
				return selectedOption != null ? selectedOption.Text : ReasonForRequestingAccessOverwrite;
			}
		}

		public bool IsMyAccountRequest
		{
			get
			{
				var selectedOption = SelectedRequestReasonOption;
				return selectedOption != null && selectedOption.IsMyAccount;
			}
		}

		#endregion

		#region Additional Information

		[CargoWise.ComponentModel.MaxLength(Schema.AdditionalInformationMaxLength)]
		public ZString AdditionalInformation
		{
			get { return additionalInformation; }
			set { SetNonPersistentPropertyValue(AdditionalInformationInfo, ref additionalInformation, value); }
		}
		ZString additionalInformation;

		public ZPropertyInfo AdditionalInformationInfo
		{
			get { return GetZPropertyInfo(Schema.AdditionalInformation); }
		}

		#endregion

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			CopyNonPersistentValues();
			PopulateInfoNote();

			base.OnFactorySavingBeforeTransactionCore();
		}

		void CopyNonPersistentValues()
		{
			WorkPhoneNationalCode = WorkPhoneNationalCode.StartsWith("00") ? WorkPhoneNationalCode.Replace("00", "+") : WorkPhoneNationalCode;
			var phone = ZString.Format("{0} {1}", WorkPhoneNationalCode, O1_Phone);
			if (!WorkPhoneExtension.IsEmpty)
			{
				phone += string.Format("<{0}>", WorkPhoneExtension);
			}
			if (phone.Length <= OrgColdCallRegister.Schema.O1_PhoneMaxLength)
			{
				O1_Phone = phone;
			}

			MobileNationalCode = MobileNationalCode.StartsWith("00") ? MobileNationalCode.Replace("00", "+") : MobileNationalCode;
			var mobile = ZString.Format("{0} {1}", MobileNationalCode, O1_Mobile);
			if (mobile.Length <= OrgColdCallRegister.Schema.O1_PhoneMaxLength)
			{
				O1_Mobile = mobile;
			}

			O1_LeadCalledDate = ZDateTime.UtcNow;
			O1_LeadSourcePerson = GlbStaff.CurrentUser.GS_LoginName.SubstringSafe(0, OrgColdCallRegister.Schema.O1_LeadSourcePersonMaxLength);

			if (O1_LeadSource.IsEmpty)
			{
				var leadType = IsMyAccountRequest ? SalesInquiryConstants.MyAccountRequestLeadType : Constants.Sales.LeadType.Website;
				if (Lookups.Source_ActiveList.ContainsCode(leadType))
				{
					O1_LeadSource = leadType;
				}
			}

			O1_OpportunitySourceDetails = ReasonForRequestingAccess.SubstringSafe(0, OrgColdCallRegister.Schema.O1_OpportunitySourceDetailsMaxLength);
		}

		void PopulateInfoNote()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Job Title: " + JobTitle).AppendLine();
			builder.AppendLine("Job Role: " + JobRole).AppendLine();
			builder.AppendLine("Type of Business: " + TypeOfBusinessSelectionsAsText).AppendLine();
			builder.AppendLine("Company Size: " + CompanySize).AppendLine();
			builder.AppendLine("Additional Information: " + AdditionalInformation);

			EnquiryNotesContent = ZBlob.FromUTF8(builder.ToString());
		}

		#endregion

		#region Emails

		public void CreateUserRegistrationNotificationEmail(string overrideEmailSubject = "")
		{
			ZGuid notificationGroupPK = new ZGuid(EDIDataRegistry.Instance.UserRegistrationNotificationGroup.Value);
			GlbGroup notificationGroup = Factory.Load<GlbGroup>(notificationGroupPK);
			if (notificationGroup != null)
			{
				EDIWebSalesInquiryEmailHelper helper = new EDIWebSalesInquiryEmailHelper(this);
				HtmlEmailDef anonymousUserRegistrationEmail;
				if (notificationGroup.Staff.Count > 0)
				{
					anonymousUserRegistrationEmail = helper.CreateEmail(notificationGroup.Staff);
					anonymousUserRegistrationEmail.Body = InsertNotificationGroupMessage(anonymousUserRegistrationEmail.Body);
				}
				else
				{
					GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					anonymousUserRegistrationEmail = helper.CreateEmail(postMastersGroup.Staff);
					anonymousUserRegistrationEmail.Body = InsertPostMasterMessage(anonymousUserRegistrationEmail.Body);
				}
				if (!string.IsNullOrEmpty(overrideEmailSubject))
				{
					anonymousUserRegistrationEmail.Subject = overrideEmailSubject;
				}
				Env.OutgoingMailManager.CreateAndSave(anonymousUserRegistrationEmail);
			}
		}

		string InsertNotificationGroupMessage(string body)
		{
			var name = EDIDataRegistry.Instance.UserRegistrationNotificationGroup.Name;
			return body.Insert(body.IndexOf("</body>"), "<p class=\"style2\">You are receiving this email because:<br>&emsp;-&emsp;You are a member of CargoWise My Account User Registration Notification Group</p><br>");
		}

		string InsertPostMasterMessage(string body)
		{
			var name = EDIDataRegistry.Instance.UserRegistrationNotificationGroup.Name;
			var category = EDIDataRegistry.Instance.UserRegistrationNotificationGroup.Category.Replace("/", " &#10141; ");
			return body.Insert(body.IndexOf("</body>"), string.Format("<p class=\"style2\">You are receiving this email because:<br>&emsp;-&emsp;Group {0} (defined in Registry &#10141; {1} &#10141; {2}) has no recipients</p><br>",
				name, category, name));
		}

		#endregion

		internal class RequestReasonOption : NonPersistentBusinessObject, IBindableBooleanItem
		{
			internal RequestReasonOption(ICodeDescriptionBool reason)
			{
				this.text = reason.Description;
				this.IsMyAccount = reason.Bool;
			}

			public readonly ZBool IsMyAccount;

			#region IBindableBooleanItem Members

			public ZBool BoolValue
			{
				get { return boolValue; }
				set { SetNonPersistentPropertyValue(BoolValueInfo, ref boolValue, value); }
			}

			public ZPropertyInfo BoolValueInfo
			{
				get { return GetZPropertyInfo(nameof(BoolValue)); }
			}

			public ZString Text
			{
				get { return text; }
			}

			ZBool boolValue;
			readonly ZString text;

			#endregion
		}

		internal class BusinessTypeOption : NonPersistentBusinessObject, IBindableBooleanItem
		{
			internal BusinessTypeOption(ZString text)
			{
				this.text = text;
			}

			#region IBindableBooleanItem Members

			public ZBool BoolValue
			{
				get { return boolValue; }
				set { SetNonPersistentPropertyValue(BoolValueInfo, ref boolValue, value); }
			}

			public ZPropertyInfo BoolValueInfo
			{
				get { return GetZPropertyInfo(nameof(BoolValue)); }
			}

			public ZString Text
			{
				get { return text; }
			}

			ZBool boolValue;
			readonly ZString text;

			#endregion
		}

		public static class SalesInquiryConstants
		{
			public const string MyAccountRequestLeadType = "WEX";
			public const string CargoWiseInquiryType = "CWI";
			public const string TranslogixInquiryType = "TLI";
		}
	}
}
