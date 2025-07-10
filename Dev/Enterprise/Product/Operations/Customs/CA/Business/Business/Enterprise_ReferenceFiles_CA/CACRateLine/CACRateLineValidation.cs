//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACRateLineValidation
//
//    This class should be used for overriding validation in AutoCACRateLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACRateLineValidation : AutoCACRateLineValidation
	{
		public CACRateLineValidation(AutoCACRateLine parent) : base(parent)
		{
		}
	}
}
