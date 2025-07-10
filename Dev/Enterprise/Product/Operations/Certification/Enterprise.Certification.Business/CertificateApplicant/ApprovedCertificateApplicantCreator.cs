using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Certification.Business
{
	public class ApprovedCertificateApplicantCreator
	{
		#region FieldNames

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		public static class FieldNames
		{
			public const string QueryStringKey = "v";
			public const string UserDetailsAsQueryString = "(*UserDetailsAsQueryString*)";
			public const string Title = "(*Title*)";
			public const string FullName = "(*FullName*)";
			public const string NameSuffix = "(*NameSuffix*)";
			public const string DOB = "(*DOB*)";
			public const string Gender = "(*Gender*)";
			public const string Address1 = "(*Address1*)";
			public const string Address2 = "(*Address2*)";
			public const string City = "(*City*)";
			public const string State = "(*State*)";
			public const string PostCode = "(*PostCode*)";
			public const string Country = "(*Country*)";
			public const string Email = "(*Email*)";
			public const string Mobile = "(*Mobile*)";
			public const string Home = "(*Home*)";
			public const string Fax = "(*Fax*)";
			public const string Work = "(*Work*)";
			public const string Extension = "(*Extension*)";
			public const string SecurityQuestion = "(*SecurityQuestion*)";
			public const string SecurityAnswer = "(*SecurityAnswer*)";
			public const string PassportNo = "(*PassportNo*)";
			public const string LicenseNo = "(*LicenseNo*)";
			public const string Nationality = "(*Nationality*)";
			public const string ApprovingStaffCode = "ApprovingStaffCode";
		}

		#endregion

		public CertificateApplicant CreateAndSaveNewApplicant(BusinessObjectFactory factory, SecureQueryString queryString, INotificationHandler notifier)
		{
			CertificateApplicant result = null;

			CertificateApplicant applicant = FindExistingApplicant(factory, queryString);
			if (applicant == null)
			{
				applicant = Create(factory, queryString);
				try
				{
					factory.Save();
					result = applicant;
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex, notifier);
				}
			}
			else
			{
				notifier.ReportInformation(
					GetCannotCreateDuplicateUserMessage(queryString[FieldNames.FullName], queryString[FieldNames.Email], applicant),
					CannotCreateDuplicateUserCaption);
			}

			return result;
		}

		#region Duplicate User Message

		public static string CannotCreateDuplicateUserCaption => Res.GetString("efe6b01d-4022-4a4e-88f7-daa1a6d7793b", "User Already Exists");

		public static string GetCannotCreateDuplicateUserMessage(string fullName, string emailAddress, CertificateApplicant existingApplicant)
		{
			Argument.NotNull(existingApplicant, "existingApplicant");

			return Res.GetString("6bb88ba7-c00c-45da-8d39-bf83a8dc2aba", "The anonymous user ({0} - {1}) you are trying to approve already exists in the database. Previously approved by {2}.", fullName, emailAddress, existingApplicant.ApprovedBy);
		}

		#endregion

		CertificateApplicant FindExistingApplicant(BusinessObjectFactory factory, SecureQueryString queryString)
		{
			string emailAddress = queryString[FieldNames.Email];
			return (!string.IsNullOrEmpty(emailAddress))
				? factory.LoadTop1<CertificateApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, emailAddress))
				: null;
		}

		CertificateApplicant Create(BusinessObjectFactory factory, SecureQueryString queryString)
		{
			CertificateApplicant applicant;

			string approvingStaffCode = queryString[FieldNames.ApprovingStaffCode];
			GlbStaff approvingStaff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, approvingStaffCode);
			if (approvingStaff != null)
			{
				applicant = factory.New<CertificateApplicant>();
				applicant.HA_Title = queryString[FieldNames.Title];
				applicant.HA_FullName = queryString[FieldNames.FullName];
				applicant.HA_NameSuffix = queryString[FieldNames.NameSuffix];
				ZDateTime dob;
				if (ZDateTime.TryParseExact(queryString[FieldNames.DOB], out dob, ZDateTime.ShortDateFormat))
				{
					applicant.HA_Birthdate = dob.Date;
				}
				applicant.HA_Gender = queryString[FieldNames.Gender];
				applicant.HA_UserAddress1 = queryString[FieldNames.Address1];
				applicant.HA_UserAddress2 = queryString[FieldNames.Address2];
				applicant.HA_City = queryString[FieldNames.City];
				applicant.HA_State = queryString[FieldNames.State];
				applicant.HA_Postcode = queryString[FieldNames.PostCode];
				applicant.HA_RN_NKCountry = queryString[FieldNames.Country];
				applicant.HA_EmailAddress = queryString[FieldNames.Email];
				applicant.HA_MobilePhone = queryString[FieldNames.Mobile];
				applicant.HA_HomePhone = queryString[FieldNames.Home];
				applicant.HA_FaxNum = queryString[FieldNames.Fax];
				applicant.HA_WorkPhone = queryString[FieldNames.Work];
				applicant.HA_WorkExtension = queryString[FieldNames.Extension];
				applicant.HA_Passport = queryString[FieldNames.PassportNo];
				applicant.HA_DriversLicenseNumber = queryString[FieldNames.LicenseNo];
				applicant.HA_RN_NKNationalityCodeISO = queryString[FieldNames.Nationality];
				applicant.ApprovedBy = approvingStaffCode;
			}
			else
			{
				applicant = null;
			}

			return applicant;
		}
	}
}
