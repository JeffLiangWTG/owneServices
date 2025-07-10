//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransLinePayValidation
//
//    This class should be used for overriding validation in AutoAccTransLinePayValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class AccTransLinePayValidation : AutoAccTransLinePayValidation
	{
		public AccTransLinePayValidation(AutoAccTransLinePay parent) : base(parent)
		{
		}
	}
}
