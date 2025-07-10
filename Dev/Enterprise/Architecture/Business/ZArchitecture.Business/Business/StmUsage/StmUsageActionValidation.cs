//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUsageActionValidation
//
//    This class should be used for overriding validation in AutoStmUsageActionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmUsageActionValidation : AutoStmUsageActionValidation
	{
		public StmUsageActionValidation(AutoStmUsageAction parent) : base(parent)
		{
		}
	}
}
