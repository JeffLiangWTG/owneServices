//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewGenericJobValidation
//
//    This class should be used for overriding validation in AutoViewGenericJobValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GenericJob
{
	public class ViewGenericJobValidation : AutoViewGenericJobValidation
	{
		public ViewGenericJobValidation(AutoViewGenericJob parent)
			: base(parent)
		{
		}
	}
}
