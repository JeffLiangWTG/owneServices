using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.CustomerService;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business
{
	public class StaffContactValueObjectHelper : IStaffContactConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a translatable string")]
		public static class QueryStringKeys
		{
			public const string LicenceCode = "LicenceCode";
			// Add this const because both spells are right and it is used by some services
			public const string LicenseCode = "LicenseCode";
			public const string DatabaseNumber = "DatabaseNumber";
			public const string ContactData = "ContactData";
			public const string LandingPageId = "LandingPageId";
			public const string Password = "Pw";
			public const string Module = "Module";
			public const string SubModule = "SubModule";
			public const string ReferenceId = "ReferenceId";
			public const string IncidentNumber = "Incident";
			public const string StaffCode = "StaffCode";
			public const string HomeBranchCode = "HomeBranchCode";
		}

		public StaffContactValueObjectHelper()
		{
		}

		public static Xsd.OrgContact StaffToContact(GlbStaff staff)
		{
			Xsd.OrgContact result = new Xsd.OrgContact();
			result.Name = GetStaffNameForXsd(staff);
			var pattern = "[^0-9() +-]";

			if (!staff.GS_MobilePhone.Contains(viewDeniedString, StringComparison.CurrentCultureIgnoreCase) && !Regex.IsMatch(staff.GS_MobilePhone, pattern))
			{
				result.Mobile = staff.GS_MobilePhone;
			}

			if (!staff.GS_WorkPhone.Contains(viewDeniedString, StringComparison.CurrentCultureIgnoreCase) && !Regex.IsMatch(staff.GS_WorkPhone, pattern))
			{
				result.Phone = staff.GS_WorkPhone;
				result.PhoneExtension = staff.GS_WorkExtension;
			}

			if (!staff.GS_EmailAddress.Contains(viewDeniedString, StringComparison.CurrentCultureIgnoreCase))
			{
				result.EmailAddress = staff.GS_EmailAddress.Trim();
			}

			result.JobTitle = staff.GS_Title;
			result.Language = staff.GS_WorkingLanguage;

			return result;
		}

		static public string GetStaffNameForXsd(GlbStaff staff)
		{
			ZString result = staff.GS_FullName;
			if (!staff.GS_NameSuffix.IsEmpty)
			{
				result += " " + staff.GS_NameSuffix;
			}

			if (!result.IsWesternEuropeanOrEmpty)
			{
				result = staff.GS_LoginName;
			}

			return result.Trim();
		}

		#region To / From SecureQueryString

		public SecureQueryString CurrentStaffAndRegistrationToSecuredQueryString()
		{
			var queryString = new SecureQueryString();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var homeBranch = GlbStaff.CurrentUser.HomeBranch ?? GlbStaff.CurrentUser.LastLogonBranch;

			queryString[QueryStringKeys.DatabaseNumber] = registrationKey.DatabaseNumber.ToString(CultureInfo.InvariantCulture);
			queryString[QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(StaffToContact(GlbStaff.CurrentUser));
			queryString[QueryStringKeys.LicenceCode] = Env.CurrentCompany.GetLicenceCode();
			queryString[QueryStringKeys.Password] = registrationKey.Password;
			queryString[QueryStringKeys.StaffCode] = GlbStaff.CurrentUser.GS_Code;
			queryString[QueryStringKeys.HomeBranchCode] = homeBranch?.GB_Code ?? ZString.Empty;

			return queryString;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		const string viewDeniedString = "** VIEW DENIED";
	}
}
