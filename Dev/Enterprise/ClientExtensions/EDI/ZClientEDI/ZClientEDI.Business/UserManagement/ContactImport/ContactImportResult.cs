using Enterprise.MasterFiles.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class ContactImportResult
	{
		public ContactImportResult(EdiCustomerUserAccount userAccount, OrgContact contact, string errorMessage)
		{
			UserAccount = userAccount;
			Contact = contact;
			ErrorMessage = errorMessage;
		}

		public EdiCustomerUserAccount UserAccount { get; }
		public OrgContact Contact { get; }
		public string ErrorMessage { get; }

		public static string NoOrganizationErrorMessage => Res.GetString("a24a5c7a-91c9-4d4a-b7b0-65a7c00552cf", "Organization could not be found");
		public static string NoEmailAddressErrorMessage => Res.GetString("c64c4a34-8226-4773-8501-22e17b39e2c2", "Cannot import a Contact with no email address");
	}
}
