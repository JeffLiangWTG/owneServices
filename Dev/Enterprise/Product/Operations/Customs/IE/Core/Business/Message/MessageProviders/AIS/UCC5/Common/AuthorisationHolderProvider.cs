using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class AuthorisationHolderProvider : IAuthorisationHolder
	{
		public static AuthorisationHolderProvider New(CusAuthorizationUsage cusAuthorizationUsage) => new AuthorisationHolderProvider(cusAuthorizationUsage);

		AuthorisationHolderProvider(CusAuthorizationUsage cusAuthorizationUsage)
		{
			Code = cusAuthorizationUsage.AGC_Code;
			ID = cusAuthorizationUsage.Owner.GetEORI();
		}

		public string Code { get; }

		public string ID { get; }
	}
}
