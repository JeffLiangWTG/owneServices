using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class GlbCompanyCredentialValidation : GlbExternalPasswordValidation
	{
		public GlbCompanyCredentialValidation(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected new GlbCompanyCredential Parent => (GlbCompanyCredential)base.Parent;

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			if (Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.GP_UserIDInfo);
			}
			else if (Parent.GP_UserID.Length < 12 || Parent.GP_UserID.Length > 13)
			{
				Parent.GP_UserIDInfo.AddMessageError(Res.GetString("C78F9957-5812-4C34-B468-0913B0CA3CB2", "Username must be between 12 and 13 characters."));
			}
		}

		protected override void CheckGP_CurrentPassword()
		{
			base.CheckGP_CurrentPassword();

			if (Parent.GP_CurrentPassword.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.GP_CurrentPasswordInfo);
			}
		}

		protected override void CheckCurrentDecryptedPassword()
		{
		}
	}
}
