//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRCodeListsValidation
//
//    This class should be used for overriding validation in AutoCMRCodeListsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCodeListsValidation : AutoCMRCodeListsValidation
	{
		public CMRCodeListsValidation(AutoCMRCodeLists parent)
			: base(parent)
		{
		}
	}
}
