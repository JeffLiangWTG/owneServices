//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientADPScoringValidation
//
//    This class should be used for overriding validation in AutoClientADPScoringValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientADPScoringValidation : AutoClientADPScoringValidation
	{
		public ClientADPScoringValidation(AutoClientADPScoring parent)
			: base(parent)
		{
		}
	}
}
