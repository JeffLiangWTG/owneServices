//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingFXOfferValidation
//
//    This class should be used for overriding validation in AutoNettingFXOfferValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingFXOfferValidation : AutoNettingFXOfferValidation
	{
		public NettingFXOfferValidation(AutoNettingFXOffer parent) : base(parent)
		{
		}
	}
}
