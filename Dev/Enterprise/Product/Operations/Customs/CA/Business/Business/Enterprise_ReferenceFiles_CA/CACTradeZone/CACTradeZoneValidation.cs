//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACTradeZoneValidation
//
//    This class should be used for overriding validation in AutoCACTradeZoneValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACTradeZoneValidation : AutoCACTradeZoneValidation
	{
		public CACTradeZoneValidation(AutoCACTradeZone parent) : base(parent)
		{
		}
	}
}
