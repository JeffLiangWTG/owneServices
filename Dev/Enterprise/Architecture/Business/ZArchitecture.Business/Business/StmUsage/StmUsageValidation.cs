//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUsageValidation
//
//    This class should be used for overriding validation in AutoStmUsageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmUsageValidation : AutoStmUsageValidation
	{
		public StmUsageValidation(AutoStmUsage parent) : base(parent)
		{
		}
	}
}
