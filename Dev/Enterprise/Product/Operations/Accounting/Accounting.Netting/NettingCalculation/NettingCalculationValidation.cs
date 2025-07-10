//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingCalculationValidation
//
//    This class should be used for overriding validation in AutoNettingCalculationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingCalculationValidation : AutoNettingCalculationValidation
	{
		public NettingCalculationValidation(AutoNettingCalculation parent) : base(parent)
		{
		}
	}
}
