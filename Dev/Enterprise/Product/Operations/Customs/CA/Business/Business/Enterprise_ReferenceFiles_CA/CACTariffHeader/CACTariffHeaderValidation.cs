//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACTariffHeaderValidation
//
//    This class should be used for overriding validation in AutoCACTariffHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACTariffHeaderValidation : AutoCACTariffHeaderValidation
	{
		public CACTariffHeaderValidation(AutoCACTariffHeader parent) : base(parent)
		{
		}
	}
}
