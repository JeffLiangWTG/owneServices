//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmLoginFailureLogValidation
//
//    This class should be used for overriding validation in AutoStmLoginFailureLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmLoginFailureLogValidation : AutoStmLoginFailureLogValidation
	{
		public StmLoginFailureLogValidation(AutoStmLoginFailureLog parent) : base(parent)
		{
		}
	}
}
