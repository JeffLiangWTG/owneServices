using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public static class GlbStaffExtension
{
	public static GenRegCertAccredMaintList GetItalianRegistrationNumberCertificate(this GlbStaff glbStaff)
	{
		Argument.NotNull(glbStaff, nameof(glbStaff));

		return glbStaff.Certificates.GetFirstCertificate(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale);
	}

	public static ZString GetItalianRegistrationNumber(this GlbStaff glbStaff)
	{
		return glbStaff.GetItalianRegistrationNumberCertificate()?.XZ_RefNumber ?? ZString.Empty;
	}

	public static ZString GetSignerFiscalCode(this GlbStaff glbStaff)
	{
		Argument.NotNull(glbStaff, nameof(glbStaff));

		var certificateProvider = new GlbCertificateProvider(glbStaff);
		if (certificateProvider.HasValidAutomaticSignaturePassword)
		{
			return certificateProvider.AutomaticSignaturePassword.GP_Name;
		}
		return glbStaff.GetItalianRegistrationNumber();
	}

	public static ZBool HasValidAutomaticSignaturePassword(this GlbStaff glbStaff)
	{
		Argument.NotNull(glbStaff, nameof(glbStaff));

		var certificateProvider = new GlbCertificateProvider(glbStaff);
		return certificateProvider.HasValidAutomaticSignaturePassword;
	}

#if DEBUG

	public static bool IsSupportUserAndIsNotTestEnviroment(this GlbStaff glbStaff)
	{
		return !Globals.IsTest && glbStaff.IsSupportUser;
	}

#endif
}
