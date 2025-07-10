using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public class GlbExternalPasswordWithCertificateValidation : MasterFiles.Business.GlbExternalPasswordWithCertificateValidation
	{
		public GlbExternalPasswordWithCertificateValidation(GlbExternalPasswordWithCertificate parent) : base(parent)
		{
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			ListValidation.MessageErrorIfInvalidCode(Parent.GP_UserIDInfo, ValidationCaptions.GlbILExternalPassword.IncorrectUser);
		}

		protected override bool IsCertificateMandatory => false;

		protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => false;
	}
}
