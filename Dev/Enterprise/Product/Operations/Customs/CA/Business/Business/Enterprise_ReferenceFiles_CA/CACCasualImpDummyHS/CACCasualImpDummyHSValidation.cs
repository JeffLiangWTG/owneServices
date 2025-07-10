//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACCasualImpDummyHSValidation
//
//    This class should be used for overriding validation in AutoCACCasualImpDummyHSValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACCasualImpDummyHSValidation : AutoCACCasualImpDummyHSValidation
	{
		public CACCasualImpDummyHSValidation(AutoCACCasualImpDummyHS parent) : base(parent)
		{
		}
	}
}
