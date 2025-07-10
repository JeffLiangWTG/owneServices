using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.AES
{
	public class AuthorisationProvider : IAuthorisation
	{
		public AuthorisationProvider(CusAuthorizationUsage authorizationUsage)
		{
			this.authorizationUsage = Argument.NotNull(authorizationUsage, nameof(authorizationUsage));
		}
		readonly CusAuthorizationUsage authorizationUsage;

		public string IdentificationType => EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(authorizationUsage.Factory, authorizationUsage.AGC_Code);

		public string UCR => authorizationUsage.AGC_Number;

		public string AuthorisatonHolder => authorizationUsage.Owner.GetEoriDetails();
	}
}
