//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACTaxRefNumHeaderValidation
//
//    This class should be used for overriding validation in AutoCACTaxRefNumHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRefNumHeaderValidation : AutoCACTaxRefNumHeaderValidation
	{
		public CACTaxRefNumHeaderValidation(AutoCACTaxRefNumHeader parent) : base(parent)
		{
		}
	}
}
