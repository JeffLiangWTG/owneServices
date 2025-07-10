using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public class EmailFormatter
	{
		public EmailFormatter(GlbStaff user)
		{
			UpdateUserDetails(user);
			var branch = GlbBranch.CurrentBranch ?? user?.HomeBranch;
			if (branch != null)
			{
				UpdateBranchDetails(branch);
				UpdateCompanyDetails(branch.Company);
			}
		}

		public void UpdateDocumentName(ZString documentName)
		{
			MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.DocumentName] = documentName;
		}

		public void UpdateScheduledTaskDescription(ZString scheduledTaskDescription)
		{
			MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.ScheduledTaskDescription] = scheduledTaskDescription;
		}

		public void UpdateCompanyNameToBrandName(ZString brandName)
		{
			MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName] = brandName;
		}

		public ZString GetEmailSubjectLine(EmailSubjectFieldCollection subjectFields)
		{
			ZString result = ZString.Empty;
			ZString fieldsSeparator = EmailFormat.Separator;
			foreach (EmailSubjectField subjectField in subjectFields)
			{
				result += !MappedEmailFields.ContainsKey(subjectField.Code) ? "" : MappedEmailFields[subjectField.Code] + fieldsSeparator;
			}
			return result.Trim(fieldsSeparator.ToString().ToCharArray());
		}

		public ZString GetEmailSignature(EmailSignatureFieldCollection signatureFields)
		{
			ZString result = ZString.Empty;
			foreach (EmailSignatureField signatureField in signatureFields)
			{
				result += GetStringWithNewLine(MappedEmailFields.ContainsKey(signatureField.Code) ? MappedEmailFields[signatureField.Code] : ZString.Empty);
			}
			result += EmailFormat.Disclaimer.IsEmpty ? "" : System.Environment.NewLine + EmailFormat.Disclaimer;
			return result.Trim(System.Environment.NewLine.ToCharArray());
		}

		public ZString GetDocumentName()
		{
			return MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.DocumentName];
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string PhoneHeading = "Phone: ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string FaxHeading = "Fax: ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string MobileHeading = "Mobile: ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string EmailHeading = "Email: ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string WebHeading = "Web: ";

		internal ZString GetStringWithHeading(ZString heading, ZString aString)
		{
			return aString.IsEmpty ? "" : heading + aString;
		}

		protected void UpdateCompanyDetails(GlbCompany company)
		{
			if (company != null)
			{
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName] = company.GC_Name;
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyAddress] = GetCompanyAddress(company);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyPhone] = GetStringWithHeading(PhoneHeading, company.GC_Phone);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyFax] = GetStringWithHeading(FaxHeading, company.GC_Fax);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyEmail] = GetStringWithHeading(EmailHeading, company.GC_Email);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyWeb] = GetStringWithHeading(WebHeading, company.GC_WebAddress);
			}
		}

		protected internal void UpdateBranchDetails(GlbBranch branch)
		{
			if (branch != null)
			{
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchName] = branch.GB_BranchName;
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchCode] = branch.GB_Code;
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchAddress] = GetBranchAddress(branch);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchPhone] = GetStringWithHeading(PhoneHeading, branch.GB_Phone);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchFax] = GetStringWithHeading(FaxHeading, branch.GB_Fax);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchEmail] = GetStringWithHeading(EmailHeading, branch.GB_Email);
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchWeb] = GetStringWithHeading(WebHeading, branch.GB_WebAddress);
			}
		}

		protected void UpdateUserDetails(GlbStaff user)
		{
			if (user != null)
			{
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserName] = user.GS_FullName;
				MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserTitle] = user.GS_Title;
				if (user.GS_PublishWorkPhone)
				{
					MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserWorkPhone] = GetStringWithHeading(PhoneHeading, user.GS_WorkPhone);
				}
				if (user.GS_PublishFaxNum)
				{
					MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserFax] = GetStringWithHeading(FaxHeading, user.GS_FaxNum);
				}
				if (user.GS_PublishMobilePhone)
				{
					MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserMobile] = GetStringWithHeading(MobileHeading, user.GS_MobilePhone);
				}
				if (user.GS_PublishEmailAddress)
				{
					MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserEmail] = GetStringWithHeading(EmailHeading, user.GS_EmailAddress);
				}
			}
		}

		#region EmailFormat
		internal EmailFormat EmailFormat => emailFormat ?? (emailFormat = DocumentsDataRegistry.Instance.EmailFormat.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

		EmailFormat emailFormat;
		#endregion

		#region MappedEmailFields
		internal Dictionary<ZString, ZString> MappedEmailFields
		{
			get
			{
				if (fMappedEmailFields == null)
				{
					fMappedEmailFields = new Dictionary<ZString, ZString>();
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.CompanyAddress, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.CompanyPhone, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.CompanyFax, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.CompanyEmail, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.CompanyWeb, ZString.Empty);

					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchName, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchCode, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchAddress, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchPhone, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchFax, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchEmail, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.BranchWeb, ZString.Empty);

					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.UserName, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.UserTitle, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.UserWorkPhone, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.UserFax, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.UserMobile, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.UserEmail, ZString.Empty);

					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.DocumentName, ZString.Empty);
					fMappedEmailFields.Add(Core.Constants.EmailFormat.EmailFieldCodes.ScheduledTaskDescription, ZString.Empty);
				}
				return fMappedEmailFields;
			}
		}
		Dictionary<ZString, ZString> fMappedEmailFields;
		#endregion

		internal ZString GetCompanyAddress(GlbCompany company)
		{
			ZString result = ZString.Empty;
			if (company != null)
			{
				result = new AddressFormatter(Factory, "", company.GC_Address1, company.GC_Address2, company.GC_City, company.GC_State, company.GC_PostCode, (NoResString)company.GC_RN_NKCountryCode, true).PostalAddress();
			}
			return result;
		}

		internal ZString GetBranchAddress(GlbBranch branch)
		{
			ZString result = ZString.Empty;
			if (branch != null)
			{
				result = new AddressFormatter(Factory, "", branch.GB_Address1, branch.GB_Address2, branch.GB_City, branch.GB_State, branch.GB_PostCode, (NoResString)"", true).PostalAddress();
			}
			return result;
		}

		ZString GetStringWithNewLine(ZString aString)
		{
			ZString result = ZString.Empty;
			if (!aString.Trim().IsEmpty)
			{
				result = aString + System.Environment.NewLine;
			}
			return result;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
