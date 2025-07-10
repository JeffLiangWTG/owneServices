//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRCustomsShipRegisterValidation
//
//    This class should be used for overriding validation in AutoCMRCustomsShipRegisterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCustomsShipRegisterValidation : AutoCMRCustomsShipRegisterValidation
	{
		public CMRCustomsShipRegisterValidation(AutoCMRCustomsShipRegister parent)
			: base(parent)
		{
		}
	}
}
