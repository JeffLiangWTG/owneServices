//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmModuleFilterValidation
//
//    This class should be used for overriding validation in AutoStmModuleFilterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	using CargoWise.EntityFramework;

	public class StmModuleFilterValidation : AutoStmModuleFilterValidation
	{
		public StmModuleFilterValidation(AutoStmModuleFilter parent) : base(parent)
		{
		}

		protected override void CheckS9_FilterName()
		{
			base.CheckS9_FilterName();
			TranslatableDataFieldAttribute.Validate(Parent.S9_FilterNameInfo);
		}
	}
}
