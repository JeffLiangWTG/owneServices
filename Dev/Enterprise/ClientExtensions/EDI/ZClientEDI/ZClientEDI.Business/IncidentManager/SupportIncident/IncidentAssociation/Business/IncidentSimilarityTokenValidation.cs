//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentSimilarityTokenValidation
//
//    This class should be used for overriding validation in AutoIncidentSimilarityTokenValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentSimilarityTokenValidation : AutoIncidentSimilarityTokenValidation
	{
		public IncidentSimilarityTokenValidation(AutoIncidentSimilarityToken parent) : base(parent)
		{
		}
	}
}
