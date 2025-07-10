using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class MercosulForeignDeclarationValidation : Customs.Business.CusSupportingInfoValidation
	{
		public MercosulForeignDeclarationValidation(MercosulForeignDeclaration parent) : base(parent)
		{
		}

		public new MercosulForeignDeclaration Parent => (MercosulForeignDeclaration)base.Parent;

		protected override void CheckCSI_RN_NKCountryCode()
		{
			base.CheckCSI_RN_NKCountryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_RN_NKCountryCodeInfo);
		}
	}
}
