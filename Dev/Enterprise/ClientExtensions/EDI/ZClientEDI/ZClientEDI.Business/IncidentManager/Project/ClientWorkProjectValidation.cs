//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientWorkProjectValidation
//
//    This class should be used for overriding validation in AutoClientWorkProjectValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientWorkProjectValidation : AutoClientWorkProjectValidation
	{
		public ClientWorkProjectValidation(AutoClientWorkProject parent) : base(parent)
		{
		}
	}
}

