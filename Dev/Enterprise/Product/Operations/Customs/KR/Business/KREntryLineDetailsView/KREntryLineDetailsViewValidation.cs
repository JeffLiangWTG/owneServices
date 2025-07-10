//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKREntryLineDetailsViewValidation
//
//    This class should be used for overriding validation in AutoKREntryLineDetailsViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class KREntryLineDetailsViewValidation : AutoKREntryLineDetailsViewValidation
	{
		public KREntryLineDetailsViewValidation(AutoKREntryLineDetailsView parent) : base(parent)
		{
		}
	}
}
