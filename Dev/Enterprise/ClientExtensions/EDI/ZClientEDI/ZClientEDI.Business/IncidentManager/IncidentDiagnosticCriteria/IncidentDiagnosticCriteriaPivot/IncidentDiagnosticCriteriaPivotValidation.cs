//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentDiagnosticCriteriaPivotValidation
//
//    This class should be used for overriding validation in AutoIncidentDiagnosticCriteriaPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentDiagnosticCriteriaPivotValidation : AutoIncidentDiagnosticCriteriaPivotValidation
	{
		public IncidentDiagnosticCriteriaPivotValidation(AutoIncidentDiagnosticCriteriaPivot parent) : base(parent)
		{
		}
	}
}
