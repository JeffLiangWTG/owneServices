using CargoWise.Customs.DE.MessageContracts.NCTS;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSAuthorisationProvider : INCTSAuthorisation
	{
		public static NCTSAuthorisationProvider NewOrNull(CusAuthorizationUsage authorizationUsage) => authorizationUsage != null ? new NCTSAuthorisationProvider(authorizationUsage) : null;

		NCTSAuthorisationProvider(CusAuthorizationUsage authorizationUsage)
		{
			this.authorizationUsage = authorizationUsage;
		}

		public string Type => authorizationUsage.CustomsCode;

		public string ReferenceNumber => authorizationUsage.AGC_Number;

		readonly CusAuthorizationUsage authorizationUsage;
	}
}
