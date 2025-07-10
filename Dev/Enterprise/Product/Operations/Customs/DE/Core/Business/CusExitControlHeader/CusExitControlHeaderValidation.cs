using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitControlHeaderValidation : EU.Business.CusExitControlHeaderValidation
	{
		public CusExitControlHeaderValidation(CusExitControlHeader parent) : base(parent)
		{
		}

		public new CusExitControlHeader Parent => (CusExitControlHeader)base.Parent;

		protected override void CheckCEH_CustomsOffice()
		{
			base.CheckCEH_CustomsOffice();

			ListValidation.MessageErrorIfInvalidCode(Parent.CEH_CustomsOfficeInfo);
		}
	}
}
