using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupplementaryDeclarantValidation : CusCodeDataValidation
	{
		public SupplementaryDeclarantValidation(SupplementaryDeclarant parent) : base(parent)
		{
		}

		new SupplementaryDeclarant Parent => (SupplementaryDeclarant)base.Parent;

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);
		}
	}
}
