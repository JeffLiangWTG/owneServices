//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceHeaderExchangeRateLookups
//
//    This class should be used for overriding collections in AutoEdiPriceHeaderExchangeRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderExchangeRateLookups : AutoEdiPriceHeaderExchangeRateLookups
	{
		public EdiPriceHeaderExchangeRateLookups(AutoEdiPriceHeaderExchangeRate parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList GroupCodes => EDIDataRegistry.Instance.StlPriceListExchangeRateGroups.Value;
	}
}


