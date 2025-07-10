using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class GlbExternalPasswordValidation_BRS : GlbExternalPasswordValidation
	{
		public GlbExternalPasswordValidation_BRS(GlbExternalPassword_BRS parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_BRS Parent => (GlbExternalPassword_BRS)base.Parent;
		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GP_UserIDInfo);
		}
	}
}
