//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACRateValidation
//
//    This class should be used for overriding validation in AutoCACRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACRateValidation : AutoCACRateValidation
	{
		public CACRateValidation(AutoCACRate parent) : base(parent)
		{
		}
	}
}
