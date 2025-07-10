//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDiagnosticCriteriaInvestigationItemLinkValidation
//
//    This class should be used for overriding validation in AutoDiagnosticCriteriaInvestigationItemLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DiagnosticCriteriaInvestigationItemLinkValidation : AutoDiagnosticCriteriaInvestigationItemLinkValidation
	{
		public DiagnosticCriteriaInvestigationItemLinkValidation(AutoDiagnosticCriteriaInvestigationItemLink parent) : base(parent)
		{
		}
	}
}
