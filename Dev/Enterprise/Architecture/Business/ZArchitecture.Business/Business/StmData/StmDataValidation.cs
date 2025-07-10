//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDataValidation
//
//    This class should be used for overriding validation in AutoStmDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmDataValidation : AutoStmDataValidation
	{
		public StmDataValidation(AutoStmData parent) : base(parent)
		{
		}
	}
}
