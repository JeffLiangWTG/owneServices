//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDiagnosticCriteriaInvestigationResultValidation
//
//    This class should be used for overriding validation in AutoDiagnosticCriteriaInvestigationResultValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DiagnosticCriteriaInvestigationResultValidation : AutoDiagnosticCriteriaInvestigationResultValidation
	{
		public DiagnosticCriteriaInvestigationResultValidation(AutoDiagnosticCriteriaInvestigationResult parent) : base(parent)
		{
		}
	}
}
