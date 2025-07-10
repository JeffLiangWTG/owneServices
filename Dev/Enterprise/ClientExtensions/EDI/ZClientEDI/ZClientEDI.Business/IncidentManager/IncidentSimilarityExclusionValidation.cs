//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentSimilarityExclusionValidation
//
//    This class should be used for overriding validation in AutoIncidentSimilarityExclusionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentSimilarityExclusionValidation : AutoIncidentSimilarityExclusionValidation
	{
		public IncidentSimilarityExclusionValidation(AutoIncidentSimilarityExclusion parent) : base(parent)
		{
		}
	}
}
