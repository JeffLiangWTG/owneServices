using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AuthorisationWrapper : IAuthorisation
	{
		AuthorisationWrapper(CusAuthorizationUsage authorizationUsage)
		{
			this.authorizationUsage = Argument.NotNull(authorizationUsage, nameof(authorizationUsage));
		}
		readonly CusAuthorizationUsage authorizationUsage;

		public static AuthorisationWrapper New(CusAuthorizationUsage authorizationUsage) => authorizationUsage == null ? null : new AuthorisationWrapper(authorizationUsage);

		public string HolderOfTheAuthorisation => holderOfTheAuthorisation ?? (holderOfTheAuthorisation = authorizationUsage.Owner?.GetEuIdentificationNumber());
		string holderOfTheAuthorisation;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = authorizationUsage.EffectiveReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(authorizationUsage.Factory, authorizationUsage.AGC_Code));
		string type;
	}
}
