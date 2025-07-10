//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisCommodityValidation
//
//    This class should be used for overriding validation in AutoCMRAqisCommodityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisCommodityValidation : AutoCMRAqisCommodityValidation
	{
		public CMRAqisCommodityValidation(AutoCMRAqisCommodity parent)
			: base(parent)
		{
		}
	}
}
