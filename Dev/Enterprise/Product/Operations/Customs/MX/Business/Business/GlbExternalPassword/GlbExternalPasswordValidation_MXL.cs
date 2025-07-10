using System.Linq;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Business
{
	public class GlbExternalPasswordValidation_MXL : GlbExternalPasswordValidation
	{
		public GlbExternalPasswordValidation_MXL(GlbExternalPassword_MXL parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_MXL Parent => (GlbExternalPassword_MXL)base.Parent;

		MXGlbStaffWrapper glbStaffWrapper => Parent.Staff.GetMXWrapper() as MXGlbStaffWrapper;

		protected override void CheckGP_CertificateAuthority()
		{
			base.CheckGP_CertificateAuthority();

			var certificateAuthority = Parent.GP_CertificateAuthority;
			var certificateAuthorityInfo = Parent.GP_CertificateAuthorityInfo;
			if (!certificateAuthority.IsNumbersOnlyOrEmpty)
			{
				certificateAuthorityInfo.AddError(Res.GetString("108856EC-4863-43AA-A23B-CB9194BBBCAD", "Customs Area must only contain numeric characters."));
			}
			if (!certificateAuthority.IsEmpty && glbStaffWrapper.StaffLicenses.Cast<GlbExternalPassword_MXL>().Count(w => w.GP_CertificateAuthority == certificateAuthority) > 1)
			{
				certificateAuthorityInfo.AddError(Res.GetString("415D36CC-50D2-4261-A32D-9EB4122F4208", "The same Customs Area cannot be entered twice."));
			}
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			var userID = Parent.GP_UserID;
			var userIDInfo = Parent.GP_UserIDInfo;
			if (!userID.IsNumbersOnlyOrEmpty)
			{
				userIDInfo.AddError(Res.GetString("1621E2C8-6224-4E5E-94AC-A6D0A8A35606", "Number must only contain numeric characters."));
			}
			if (!userID.IsEmpty && userID.Length < userIDInfo.MaxLength)
			{
				userIDInfo.AddError(Res.GetString("C69B583E-7756-4CD8-BD8C-7E6DDABDE4DC", "Number has less than {0} characters.", userIDInfo.MaxLength));
			}
			if (!userID.IsEmpty && glbStaffWrapper.StaffLicenses.Cast<GlbExternalPassword_MXL>().Count(w => w.GP_UserID == userID) > 1)
			{
				userIDInfo.AddError(Res.GetString("4CF7B77F-4BE6-4BD6-8967-DEBD1BBA92F8", "The same Customs Area cannot be entered twice for the same Number."));
			}
		}
	}
}
