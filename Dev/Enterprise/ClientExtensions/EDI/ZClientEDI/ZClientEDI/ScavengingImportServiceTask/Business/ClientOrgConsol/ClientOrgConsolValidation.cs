//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientOrgConsolValidation
//
//    This class should be used for overriding validation in AutoClientOrgConsolValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.ScavengingImportServiceTask.Business
{
	public class ClientOrgConsolValidation : AutoClientOrgConsolValidation
	{
		public ClientOrgConsolValidation(AutoClientOrgConsol parent) : base(parent)
		{
		}
	}
}
