using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSJobDeclarationValidation : EU.EMCS.Business.EMCSJobDeclarationValidation
	{
		public EMCSJobDeclarationValidation(EMCSJobDeclaration parent) : base(parent) { }

		EMCSJobDeclaration Declaration => Parent as EMCSJobDeclaration;

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			MandatoryValidation.CheckEntered(Parent.JE_CustomsProfileInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JE_CustomsProfileInfo);
			if (Declaration.Credential == null)
			{
				Parent.JE_CustomsProfileInfo.AddWarning(Res.GetString("F603603C-CE3A-489B-9237-8FF14FFB0EB7", "Credential status does not appear to be valid."));
			}
		}
	}
}
