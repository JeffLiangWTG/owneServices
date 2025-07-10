//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingMatchPivotValidation
//
//    This class should be used for overriding validation in AutoNettingMatchPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingMatchPivotValidation : AutoNettingMatchPivotValidation
	{
		public NettingMatchPivotValidation(AutoNettingMatchPivot parent)
			: base(parent)
		{
		}
	}
}
