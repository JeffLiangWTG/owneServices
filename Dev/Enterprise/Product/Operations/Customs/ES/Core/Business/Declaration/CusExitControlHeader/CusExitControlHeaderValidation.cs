namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusExitControlHeaderValidation : EU.Business.CusExitControlHeaderValidation
	{
		public CusExitControlHeaderValidation(CusExitControlHeader parent) : base(parent)
		{
		}

		protected new CusExitControlHeader Parent => (CusExitControlHeader)base.Parent;

		protected override void CheckCEH_CustomsProfile()
		{
			base.CheckCEH_CustomsProfile();

			CertificateHelper.CheckCustomsProfile(Parent.CEH_CustomsProfileInfo, Parent.CEH_CustomsProfile, Parent.CustomsAgent);
		}
	}
}
