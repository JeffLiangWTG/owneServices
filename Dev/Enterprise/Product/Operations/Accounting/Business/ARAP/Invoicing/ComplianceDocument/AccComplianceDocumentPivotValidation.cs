//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceDocumentPivotValidation
//
//    This class should be used for overriding validation in AutoAccComplianceDocumentPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentPivotValidation : AutoAccComplianceDocumentPivotValidation
	{
		public AccComplianceDocumentPivotValidation(AutoAccComplianceDocumentPivot parent) : base(parent)
		{
		}
	}
}
