using CargoWise.EntityFramework;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmEventValidation
//
//    This class should be used for overriding validation in AutoStmEventValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmEventValidation : AutoStmEventValidation
	{
		public StmEventValidation(AutoStmEvent parent) : base(parent)
		{
		}

		protected override void CheckSE_Desc()
		{
			base.CheckSE_Desc();
			TranslatableDataFieldAttribute.Validate(Parent.SE_DescInfo);
		}
	}
}
