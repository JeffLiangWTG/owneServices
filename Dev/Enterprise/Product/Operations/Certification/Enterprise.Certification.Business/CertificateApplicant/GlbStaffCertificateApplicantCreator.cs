using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Certification;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Certification.Business
{
	public class GlbStaffCertificateApplicantCreator : IGlbStaffCertificateApplicantCreator
	{
		public static CertificateApplicant LoadOrCreateFromStaff(GlbStaff staff, bool isOnSaving = false)
		{
			Argument.NotNull(staff, "staff");
			Argument.NotNullOrEmpty(staff.GS_EmailAddress, "staff.GS_EmailAddress");

			var factory = staff.Factory;

			var result = GetExistingApplicant(staff);
			if (result == null)
			{
				result = factory.New<CertificateApplicant>();
				PopulateNewApplicant(result, staff);
			}

			if (result.HasChanges && !isOnSaving)
			{
				factory.Save();
			}
			return result;
		}

		public static CertificateApplicant LoadFromStaff(GlbStaff staff)
		{
			Argument.NotNull(staff, "staff");
			Argument.NotNullOrEmpty(staff.GS_EmailAddress, "staff.GS_EmailAddress");

			var result = GetExistingApplicant(staff);
			return result;
		}

		static CertificateApplicant GetExistingApplicant(GlbStaff staff)
		{
			var email = staff.GS_EmailAddressInfo.OriginalValue.IsEmpty ? staff.GS_EmailAddress : staff.GS_EmailAddressInfo.OriginalValue;
			return staff.Factory.LoadFromUniqueKey<CertificateApplicant>(HRJobApplicantSchema.HA_EmailAddress, email);
		}

		static void PopulateNewApplicant(CertificateApplicant applicant, GlbStaff staff)
		{
			applicant.HA_PER = staff.GS_PER;
			applicant.RelatedGlbStaffPK = staff.PK;
			applicant.HA_FullName = staff.GS_FullName.IsEmpty ? new ZString(GlbPerson.EmptyFullName) : staff.GS_FullName;
			applicant.HA_Birthdate = staff.GS_Birthdate;
			applicant.HA_EmailAddress = staff.GS_EmailAddress;
			applicant.HA_MobilePhone = staff.GS_MobilePhone;
			applicant.HA_WorkPhone = staff.GS_WorkPhone;
			applicant.HA_WorkExtension = staff.GS_WorkExtension;
			applicant.HA_FaxNum = staff.GS_FaxNum;
			applicant.HA_HomePhone = staff.GS_HomePhone;
		}

		GlbStaff LoadStaff(ZGuid staffPk)
		{
			var factory = new BusinessObjectFactory();
			return factory.Load<GlbStaff>(staffPk);
		}

		ICertificateApplicant IGlbStaffCertificateApplicantCreator.LoadOrCreateFromStaff(ZGuid staffPk)
		{
			var staff = LoadStaff(staffPk);
			return LoadOrCreateFromStaff(staff);
		}

		ICertificateApplicant IGlbStaffCertificateApplicantCreator.LoadFromStaff(ZGuid staffPk)
		{
			var staff = LoadStaff(staffPk);
			return LoadFromStaff(staff);
		}
	}
}
