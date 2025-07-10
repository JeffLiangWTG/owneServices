//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmSystemDefinedFieldValidation
//
//    This class should be used for overriding validation in AutoStmSystemDefinedFieldValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldValidation : AutoStmSystemDefinedFieldValidation
	{
		public StmSystemDefinedFieldValidation(AutoStmSystemDefinedField parent) : base(parent)
		{
		}
	}
}
