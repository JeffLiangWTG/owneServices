//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewMatchGroupValidation
//
//    This class should be used for overriding validation in AutoViewMatchGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ViewMatchGroupValidation : AutoViewMatchGroupValidation
	{
		public ViewMatchGroupValidation(AutoViewMatchGroup parent)
			: base(parent)
		{
		}
	}
}
