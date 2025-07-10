//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentSimilarityMatrixValidation
//
//    This class should be used for overriding validation in AutoIncidentSimilarityMatrixValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentSimilarityMatrixValidation : AutoIncidentSimilarityMatrixValidation
	{
		public IncidentSimilarityMatrixValidation(AutoIncidentSimilarityMatrix parent) : base(parent)
		{
		}
	}
}
