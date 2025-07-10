//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceDocumentLineValidation
//
//    This class should be used for overriding validation in AutoAccComplianceDocumentLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentLineValidation : AutoAccComplianceDocumentLineValidation
	{
		public AccComplianceDocumentLineValidation(AutoAccComplianceDocumentLine parent) : base(parent)
		{
		}
	}
}
