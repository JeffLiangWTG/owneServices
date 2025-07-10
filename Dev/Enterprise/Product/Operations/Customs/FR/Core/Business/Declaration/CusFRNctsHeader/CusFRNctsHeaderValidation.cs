using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class CusFRNctsHeaderValidation : AutoCusFRNctsHeaderValidation
	{
		public CusFRNctsHeaderValidation(AutoCusFRNctsHeader parent) : base(parent)
		{
		}

		protected new CusFRNctsHeader Parent => (CusFRNctsHeader)base.Parent;

		protected override void CheckCFN_DetailedDepartureStatusCode()
		{
			base.CheckCFN_DetailedDepartureStatusCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CFN_DetailedDepartureStatusCodeInfo);
		}

		protected override void CheckCFN_NatureOfSeals()
		{
			base.CheckCFN_NatureOfSeals();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CFN_NatureOfSealsInfo);
		}
	}
}
