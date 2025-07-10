//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxGLMovementValidation
//
//    This class should be used for overriding validation in AutoAccTaxGLMovementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxGLMovementValidation : AutoAccTaxGLMovementValidation
	{
		public AccTaxGLMovementValidation(AutoAccTaxGLMovement parent) : base(parent)
		{
		}
	}
}
