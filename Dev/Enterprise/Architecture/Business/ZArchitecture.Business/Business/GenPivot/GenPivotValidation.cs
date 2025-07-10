//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenPivotValidation
//
//    This class should be used for overriding validation in AutoGenPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class GenPivotValidation : AutoGenPivotValidation
	{
		public GenPivotValidation(AutoGenPivot parent)
			: base(parent)
		{
		}
	}
}
