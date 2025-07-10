//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKREntryCustomsBillsViewValidation
//
//    This class should be used for overriding validation in AutoKREntryCustomsBillsViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class KREntryCustomsBillsViewValidation : AutoKREntryCustomsBillsViewValidation
	{
		public KREntryCustomsBillsViewValidation(AutoKREntryCustomsBillsView parent) : base(parent)
		{
		}
	}
}

