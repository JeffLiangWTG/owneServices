//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmLinkValidation
//
//    This class should be used for overriding validation in AutoStmLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmLinkValidation : AutoStmLinkValidation
	{
		public StmLinkValidation(AutoStmLink parent) : base(parent)
		{
		}
	}
}
