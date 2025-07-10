using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIWebSalesInquiryEmailHelper
	{
		#region FieldNames

		public static class FieldNames
		{
			public const string SalesInquiryHtmlLink = "(*SalesInquiryHtmlLink*)";
			public const string CompanyName = "(*CompanyName*)";
			public const string FullName = "(*FullName*)";
			public const string Address1 = "(*Address1*)";
			public const string Address2 = "(*Address2*)";
			public const string City = "(*City*)";
			public const string State = "(*State*)";
			public const string PostCode = "(*PostCode*)";
			public const string Country = "(*Country*)";
			public const string Email = "(*Email*)";
			public const string Mobile = "(*Mobile*)";
			public const string WorkPhone = "(*WorkPhone*)";
			public const string JobTitle = "(*JobTitle*)";
			public const string JobRole = "(*JobRole*)";
			public const string TypeOfBusiness = "(*TypeOfBusiness*)";
			public const string ReasonForRequest = "(*ReasonForRequest*)";
			public const string CompanySize = "(*CompanySize*)";
			public const string AdditionalInformation = "(*AdditionalInformation*)";
		}

		#endregion

		public EDIWebSalesInquiryEmailHelper(EDIWebSalesInquiry inquiry)
		{
			this.inquiry = inquiry;
		}

		readonly EDIWebSalesInquiry inquiry;
		const string approvalEmailTemplateResourceName = "Enterprise.Client.EDI.MasterFiles.OrgColdCallRegister.SalesInquiry.RegistrationNotificationEmailTemplate.htm";

		public HtmlEmailDef CreateEmail(GlbStaffManyToManyCollection staffs)
		{
			HtmlEmailDef result = new HtmlEmailDef();

			string[] emails = staffs.Cast<GlbStaff>().Where(s => !s.GS_EmailAddress.IsEmpty).Select(s => s.GS_EmailAddress.ToString()).ToArray();
			result.AddRecipientForUserCommunication(emails);

			result.FromDisplayName = "ediProd System";
			result.FromAddress = "PleaseDoNotReply@wisetechglobal.com";
			result.Subject = "CargoWise My Account User Registration";

			string populatedHtmlContent = GetPopulatedHtmlContent();
			result.LoadHtmlUsingTemplate(populatedHtmlContent);

			return result;
		}

		string GetPopulatedHtmlContent()
		{
			string templateContent;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			using (Stream stream = executingAssembly.GetManifestResourceStream(approvalEmailTemplateResourceName))
			using (StreamReader streamReader = new StreamReader(stream))
			{
				templateContent = streamReader.ReadToEnd();
			}
			StringBuilder builder = new StringBuilder(templateContent);
			PopulateCommonValues(builder);
			return builder.ToString();
		}

		void PopulateCommonValues(StringBuilder builder)
		{
			builder.Replace(FieldNames.SalesInquiryHtmlLink, inquiry.HtmlLink);
			builder.Replace(FieldNames.CompanyName, inquiry.O1_CompanyName);
			builder.Replace(FieldNames.FullName, inquiry.O1_ContactName);
			builder.Replace(FieldNames.Address1, inquiry.O1_Address1);
			builder.Replace(FieldNames.Address2, inquiry.O1_Address2);
			builder.Replace(FieldNames.City, inquiry.O1_City);
			builder.Replace(FieldNames.State, inquiry.O1_State);
			builder.Replace(FieldNames.PostCode, inquiry.O1_PostCode);
			builder.Replace(FieldNames.Country, inquiry.O1_PortOrCountry);
			builder.Replace(FieldNames.Email, inquiry.O1_Email);
			builder.Replace(FieldNames.Mobile, inquiry.O1_Mobile);
			builder.Replace(FieldNames.WorkPhone, inquiry.O1_Phone);
			builder.Replace(FieldNames.JobTitle, inquiry.JobTitle);
			builder.Replace(FieldNames.JobRole, inquiry.JobRole);
			builder.Replace(FieldNames.TypeOfBusiness, inquiry.TypeOfBusinessSelectionsAsText);
			builder.Replace(FieldNames.ReasonForRequest, inquiry.ReasonForRequestingAccess);
			builder.Replace(FieldNames.CompanySize, inquiry.CompanySize);
			builder.Replace(FieldNames.AdditionalInformation, inquiry.AdditionalInformation);
		}
	}
}
