using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.ES.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public static class EmailHelper
	{
		public static ZString GetCurrentUserMainEmail()
		{
			var firstMainUserEmail = GlbStaff.CurrentUser.EmailAddresses.FirstOrDefault(e => e.GSE_Type == Constants.EmailFromAddressTypes.Codes.Main && !e.GSE_EmailAddress.IsEmpty);

			return firstMainUserEmail != null ? firstMainUserEmail.GSE_EmailAddress : ZString.Empty;
		}

		public static ZString GetDeclEmailAddrFromRegistry()
		{
			var clearanceEmailRecipient = ESCustomsDataRegistry.Instance.CustomsClearanceEmailRecipient.Value;
			return string.IsNullOrWhiteSpace(clearanceEmailRecipient) ? Env.Registry.MailboxEmailAddress : clearanceEmailRecipient;
		}
	}
}
