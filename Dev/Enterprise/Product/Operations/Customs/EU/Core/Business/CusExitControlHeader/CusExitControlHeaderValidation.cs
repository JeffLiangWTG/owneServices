//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusExitControlHeaderValidation
//
//    This class should be used for overriding validation in AutoCusExitControlHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitControlHeaderValidation : AutoCusExitControlHeaderValidation
	{
		public CusExitControlHeaderValidation(AutoCusExitControlHeader parent) : base(parent)
		{
		}

		protected override void CheckCEH_ReferenceNumber()
		{
			base.CheckCEH_ReferenceNumber();
			MandatoryValidation.CheckEntered(Parent.CEH_ReferenceNumberInfo);
		}
	}
}
