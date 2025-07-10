//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDiagnosticCriteriaInvestigationResultLookups
//
//    This class should be used for overriding collections in AutoDiagnosticCriteriaInvestigationResultLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DiagnosticCriteriaInvestigationResultLookups : AutoDiagnosticCriteriaInvestigationResultLookups
	{
		public DiagnosticCriteriaInvestigationResultLookups(AutoDiagnosticCriteriaInvestigationResult parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Results => new DiagnosticCriteriaInvestigationResults();
	}
}
