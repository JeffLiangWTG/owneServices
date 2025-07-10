//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentSimilarityTfIdfValidation
//
//    This class should be used for overriding validation in AutoIncidentSimilarityTfIdfValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentSimilarityTfIdfValidation : AutoIncidentSimilarityTfIdfValidation
	{
		public IncidentSimilarityTfIdfValidation(AutoIncidentSimilarityTfIdf parent) : base(parent)
		{
		}
	}
}
