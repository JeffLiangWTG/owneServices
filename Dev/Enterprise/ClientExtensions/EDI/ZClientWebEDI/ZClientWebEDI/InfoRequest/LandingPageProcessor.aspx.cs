using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Schema = Enterprise.Client.EDI.MasterFiles.Business.EDIWebSalesInquiry.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class LandingPageProcessor : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected void Page_Load(object sender, EventArgs e)
		{
			ProcessRequest();
		}

		void ProcessRequest()
		{
			EDIWebSalesInquiry inquiry = Factory.New<EDIWebSalesInquiry>();
			PopulateInquiryInfo(inquiry);

			inquiry.Validation.ValidateAll();
			if (!inquiry.HasErrors)
			{
				AppInstance.SetupSession(null, EventArgs.Empty, false);
				Factory.Save();
				string emailSubject = Request[NotificationEmailSubjectFieldName] ?? string.Format("{0} Registration ({1})", inquiry.O1_LeadSource, inquiry.O1_Email);
				inquiry.CreateUserRegistrationNotificationEmail(emailSubject);
				Response.Redirect(SuccessRedirectUrl);
			}
			else
			{
				Response.Redirect(FailRedirectUrl);
			}
		}

		void PopulateInquiryInfo(EDIWebSalesInquiry inquiry)
		{
			var fieldsList = GetFieldList();
			foreach (var fieldInfo in fieldsList)
			{
				var fieldName = fieldInfo.Item1;
				var columnName = fieldInfo.Item2;
				var columnMaxLength = fieldInfo.Item3;

				if (Request[fieldName] != null)
				{
					inquiry[columnName] = new ZString(Request[fieldName]).SubstringSafe(0, columnMaxLength);
				}
			}
		}

		string SuccessRedirectUrl
		{
			get { return Request[SuccessRedirectUrlFieldName] ?? "http://www.wisetechglobal.com/thank-you"; }
		}

		string FailRedirectUrl
		{
			get { return Request[FailRedirectUrlFieldName] ?? "http://www.wisetechglobal.com/"; }
		}

		#region Query String Field Name Constant

		const string InquiryTypeFieldName = "InquiryType";
		const string LeadSourceFieldName = "LeadSource";
		const string CompanyNameFieldName = "CompanyName";
		const string ContactNameFieldName = "FullName";
		const string EmailFieldName = "Email";
		const string WorkPhoneNationalCodeFieldName = "WorkPhoneNationalCode";
		const string WorkPhoneFieldName = "WorkPhone";
		const string WorkPhoneExtensionFieldName = "WorkPhoneExtension";
		const string MobileNationalCodeFieldName = "MobileNationalCode";
		const string MobileFieldName = "Mobile";
		const string Address1FieldName = "AddressLine1";
		const string Address2FieldName = "AddressLine2";
		const string CityFieldName = "City";
		const string StateFieldName = "State";
		const string PostcodeFieldName = "Postcode";
		const string CountryFieldName = "Country";
		const string JobTitleFieldName = "JobTitle";
		const string JobRoleFieldName = "JobRole";
		const string CompanySizeFieldName = "CompanySize";
		const string TypeOfBusinessFieldName = "TypeOfBusiness";
		const string ReasonForRequestingAccessFieldName = "ReasonForRequestingAccess";
		const string AdditionalInfoFieldName = "AdditionalInfo";

		const string NotificationEmailSubjectFieldName = "NotificationEmailSubject";
		const string SuccessRedirectUrlFieldName = "SuccessRedirectUrl";
		const string FailRedirectUrlFieldName = "FailRedirectUrl";

		#endregion

		#region Field and Property Lists

		List<Tuple<string, string, int>> GetFieldList()
		{
			var fields = new List<Tuple<string, string, int>>
			{
				Tuple.Create(InquiryTypeFieldName, Schema.O1_EnquiryType, Schema.O1_EnquiryTypeMaxLength),
				Tuple.Create(LeadSourceFieldName, Schema.O1_LeadSource, Schema.O1_LeadSourceMaxLength),
				Tuple.Create(CompanyNameFieldName, Schema.O1_CompanyName, Schema.O1_CompanyNameMaxLength),
				Tuple.Create(ContactNameFieldName, Schema.O1_ContactName, Schema.O1_ContactNameMaxLength),
				Tuple.Create(EmailFieldName, Schema.O1_Email, Schema.O1_EmailMaxLength),
				Tuple.Create(WorkPhoneNationalCodeFieldName, Schema.WorkPhoneNationalCode, Schema.WorkPhoneNationalCodeMaxLength),
				Tuple.Create(WorkPhoneFieldName, Schema.O1_Phone, Schema.O1_PhoneMaxLength),
				Tuple.Create(WorkPhoneExtensionFieldName, Schema.WorkPhoneExtension, Schema.WorkPhoneExtensionMaxLength),
				Tuple.Create(MobileNationalCodeFieldName, Schema.MobileNationalCode, Schema.MobileNationalCodeMaxLength),
				Tuple.Create(MobileFieldName, Schema.O1_Mobile, Schema.O1_MobileMaxLength),
				Tuple.Create(Address1FieldName, Schema.O1_Address1, Schema.O1_Address1MaxLength),
				Tuple.Create(Address2FieldName, Schema.O1_Address2, Schema.O1_Address2MaxLength),
				Tuple.Create(CityFieldName, Schema.O1_City, Schema.O1_CityMaxLength),
				Tuple.Create(StateFieldName, Schema.O1_State, Schema.O1_StateMaxLength),
				Tuple.Create(PostcodeFieldName, Schema.O1_PostCode, Schema.O1_PostCodeMaxLength),
				Tuple.Create(CountryFieldName, Schema.O1_PortOrCountry, Schema.O1_PortOrCountryMaxLength),
				Tuple.Create(JobTitleFieldName, Schema.JobTitle, Schema.JobTitleMaxLength),
				Tuple.Create(JobRoleFieldName, Schema.JobRole, Schema.JobRoleMaxLength),
				Tuple.Create(CompanySizeFieldName, Schema.CompanySize, Schema.CompanySizeMaxLength),
				Tuple.Create(TypeOfBusinessFieldName, Schema.TypeOfBusinessOther, Schema.TypeOfBusinessOtherMaxLength),
				Tuple.Create(ReasonForRequestingAccessFieldName, Schema.ReasonForRequestingAccessOverwrite, Schema.ReasonForRequestingAccessOverwriteMaxLength),
				Tuple.Create(AdditionalInfoFieldName, Schema.AdditionalInformation, Schema.AdditionalInformationMaxLength)
			};

			return fields;
		}

		#endregion
	}
}
