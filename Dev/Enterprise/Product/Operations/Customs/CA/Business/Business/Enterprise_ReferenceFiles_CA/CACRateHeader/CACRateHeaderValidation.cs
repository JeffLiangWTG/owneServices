//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACRateHeaderValidation
//
//    This class should be used for overriding validation in AutoCACRateHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACRateHeaderValidation : AutoCACRateHeaderValidation
	{
		public CACRateHeaderValidation(AutoCACRateHeader parent) : base(parent)
		{
		}
	}
}
