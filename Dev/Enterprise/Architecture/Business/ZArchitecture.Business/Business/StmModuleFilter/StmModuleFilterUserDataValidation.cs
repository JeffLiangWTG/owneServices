//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmModuleFilterUserDataValidation
//
//    This class should be used for overriding validation in AutoStmModuleFilterUserDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmModuleFilterUserDataValidation : AutoStmModuleFilterUserDataValidation
	{
		public StmModuleFilterUserDataValidation(AutoStmModuleFilterUserData parent) : base(parent)
		{
		}
	}
}
