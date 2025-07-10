namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitSealValidation : EU.ExitControl.Business.CusExitSealValidation
	{
		public CusExitSealValidation(CusExitSeal parent) : base(parent)
		{
		}

		protected new CusExitSeal Parent => (CusExitSeal)base.Parent;
	}
}
