//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKREntryHeaderDetailsViewValidation
//
//    This class should be used for overriding validation in AutoKREntryHeaderDetailsViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class KREntryHeaderDetailsViewValidation : AutoKREntryHeaderDetailsViewValidation
	{
		public KREntryHeaderDetailsViewValidation(AutoKREntryHeaderDetailsView parent) : base(parent)
		{
		}
	}
}
