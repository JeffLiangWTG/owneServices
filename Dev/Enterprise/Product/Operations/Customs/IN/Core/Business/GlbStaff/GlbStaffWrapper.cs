using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IN;

namespace Enterprise.Customs.IN.Business;

public class GlbStaffWrapper : MasterFiles.Business.GlbStaffWrapper, IINGlbStaffWrapper
{
	protected GlbStaffWrapper(GlbStaff staff)
		: base(staff)
	{
	}

	public static GlbStaffWrapper Get(GlbStaff staff)
	{
		return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapper(staff));
	}

	public static GlbStaffWrapper GetWrapperForCurrentUser()
	{
		return Get(GlbStaff.CurrentUser);
	}

	public GlbLoginPassword LoginPassword => loginPassword ??= GetExternalPasswordOrCreateNew<GlbLoginPassword>(PasswordTypesList.Codes.INC);
	GlbLoginPassword loginPassword;

	public GlbLoginPassword GetLoginPassword() => GetGlbExternalPassword<GlbLoginPassword>(PasswordTypesList.Codes.INC, GlbCompany.CurrentCompany.PK);

	public GlbCertificatePassword CertificatePassword => certificatePassword ??= GetExternalPasswordOrCreateNew<GlbCertificatePassword>(PasswordTypesList.Codes.INX);
	GlbCertificatePassword certificatePassword;

	public GlbCertificatePassword GetCertificatePassword() => GetGlbExternalPassword<GlbCertificatePassword>(PasswordTypesList.Codes.INX, GlbCompany.CurrentCompany.PK);

	T GetExternalPasswordOrCreateNew<T>(ZString passwordType) where T : GlbExternalPassword
	{
		var password = GetGlbExternalPasswordOrCreateNew<T>(passwordType, GlbCompany.CurrentCompany.PK);
		RegisterEditableChildObject(password);
		return password;
	}
}
