using Enterprise.Client.EDI.UserManagement.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementResponseWrapper
	{
		public UserAgreementResponseData ResponseData { get; set; }
		public EdiUserAgreement Agreement { get; set; }
	}
}
