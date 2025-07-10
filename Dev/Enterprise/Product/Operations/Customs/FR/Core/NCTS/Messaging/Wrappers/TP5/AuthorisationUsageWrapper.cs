using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class AuthorisationUsageWrapper : IAuthorisation
	{
		AuthorisationUsageWrapper(EU.NCTS.Business.CusAuthorizationUsage authorizationUsage)
		{
			this.authorizationUsage = Argument.NotNull(authorizationUsage, nameof(authorizationUsage));
		}
		readonly EU.NCTS.Business.CusAuthorizationUsage authorizationUsage;

		public static AuthorisationUsageWrapper New(EU.NCTS.Business.CusAuthorizationUsage authorizationUsage) => authorizationUsage == null ? null : new AuthorisationUsageWrapper(authorizationUsage);

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = authorizationUsage.EffectiveReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = EU.Business.EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(authorizationUsage.Factory, authorizationUsage.AGC_Code));
		string type;
	}
}
