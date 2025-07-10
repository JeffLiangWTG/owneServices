//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientOrgImportHistoryValidation
//
//    This class should be used for overriding validation in AutoClientOrgImportHistoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.ScavengingImportServiceTask.Business
{
	public class ClientOrgImportHistoryValidation : AutoClientOrgImportHistoryValidation
	{
		public ClientOrgImportHistoryValidation(AutoClientOrgImportHistory parent) : base(parent)
		{
		}
	}
}
