using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class ContactImportValue
	{
		public ContactImportValue(StaffReport staffReport)
		{
			UserId = staffReport.Code;
			Name = staffReport.Name;
			Email = staffReport.EmailAddress;
			Language = staffReport.LanguageCode;
			WorkPhone = staffReport.WorkPhone;
			WorkPhoneExtension = staffReport.WorkPhoneExtension;
			JobTitle = staffReport.JobTitle;
			BranchCode = staffReport.BranchCode;
			IsUserActive = staffReport.IsActive;
		}

		public ContactImportValue(Xsd.OrgContact xsdContact, ZString staffCode, ZString branchCode)
		{
			UserId = staffCode;
			Name = xsdContact.Name;
			Email = xsdContact.EmailAddress;
			Language = xsdContact.Language;
			WorkPhone = xsdContact.Phone;
			WorkPhoneExtension = xsdContact.PhoneExtension;
			JobTitle = xsdContact.JobTitle;
			BranchCode = branchCode;
			IsUserActive = true;
		}

		public ContactImportValue(GlbStaff staff)
		{
			UserId = staff.GS_Code;
			Name = staff.GS_FullName;
			Email = staff.GS_EmailAddress;
			JobTitle = staff.GS_Title;
			Language = staff.GS_WorkingLanguage;
			WorkPhone = staff.GS_WorkPhone;
			Mobile = staff.GS_MobilePhone;
			BranchCode = staff.HomeBranch?.GB_Code ?? ZString.Empty;
			IsUserActive = staff.GS_IsActive;
		}

		public ContactImportValue(EdiCustomerUserAccount userAccount, string branchCode)
		{
			UserId = userAccount.EUA_UserID;
			Name = userAccount.EUA_FullName;
			Email = userAccount.EUA_Email;
			IsUserActive = userAccount.EUA_IsActive;
			BranchCode = branchCode;
		}

		public ContactImportValue(ITrustedUserInfo userInfo)
		{
			UserId = userInfo.UserId;
			Name = userInfo.FullName;
			Email = userInfo.Email;
			IsUserActive = true;
		}

		public ZString UserId { get; set; }

		public ZString Name
		{
			get => name;
			set => name = value.SubstringSafe(0, OrgContactSchema.OC_ContactName.MaxLength).Trim();
		}
		ZString name;

		public ZString Email
		{
			get => email;
			set => email = value.SubstringSafe(0, OrgContactSchema.OC_Email.MaxLength).Trim();
		}
		ZString email;

		public ZString JobTitle
		{
			get => jobTitle;
			set => jobTitle = value.SubstringSafe(0, OrgContactSchema.OC_Title.MaxLength).Trim();
		}
		ZString jobTitle;

		public ZString Language
		{
			get => language;
			set => language = value.SubstringSafe(0, OrgContactSchema.OC_Language.MaxLength).Trim();
		}
		ZString language;

		public ZString WorkPhone
		{
			get => workPhone;
			set => workPhone = value.SubstringSafe(0, OrgContactSchema.OC_Phone.MaxLength).Trim();
		}
		ZString workPhone;

		public ZString WorkPhoneExtension
		{
			get => workPhoneExtension;
			set => workPhoneExtension = value.SubstringSafe(0, OrgContactSchema.OC_PhoneExtension.MaxLength).Trim();
		}
		ZString workPhoneExtension;

		public ZString Mobile
		{
			get => mobile;
			set => mobile = value.SubstringSafe(0, OrgContactSchema.OC_Mobile.MaxLength).Trim();
		}
		ZString mobile;

		public ZString BranchCode { get; set; }

		public ZBool IsUserActive { get; set; }
	}
}
