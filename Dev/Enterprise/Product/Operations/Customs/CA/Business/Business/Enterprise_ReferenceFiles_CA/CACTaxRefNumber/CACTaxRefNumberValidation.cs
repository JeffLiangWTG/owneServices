//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACTaxRefNumberValidation
//
//    This class should be used for overriding validation in AutoCACTaxRefNumberValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRefNumberValidation : AutoCACTaxRefNumberValidation
	{
		public CACTaxRefNumberValidation(AutoCACTaxRefNumber parent) : base(parent)
		{
		}
	}
}
