//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingSystemExchangeRateValidation
//
//    This class should be used for overriding validation in AutoNettingSystemExchangeRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingSystemExchangeRateValidation : AutoNettingSystemExchangeRateValidation
	{
		public NettingSystemExchangeRateValidation(AutoNettingSystemExchangeRate parent) : base(parent)
		{
		}
	}
}
