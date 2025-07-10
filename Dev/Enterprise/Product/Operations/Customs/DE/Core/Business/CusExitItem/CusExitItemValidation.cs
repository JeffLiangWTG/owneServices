using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitItemValidation : EU.Business.CusExitItemValidation
	{
		public CusExitItemValidation(AutoCusExitItem parent) : base(parent)
		{
		}

		protected new CusExitItem Parent => (CusExitItem)base.Parent;

		protected override void CheckCXI_StatusIsValidCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CXI_StatusInfo);
		}
	}
}
