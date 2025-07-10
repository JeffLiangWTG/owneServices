//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisDocumentTypeValidation
//
//    This class should be used for overriding validation in AutoCMRAqisDocumentTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisDocumentTypeValidation : AutoCMRAqisDocumentTypeValidation
	{
		public CMRAqisDocumentTypeValidation(AutoCMRAqisDocumentType parent)
			: base(parent)
		{
		}
	}
}
