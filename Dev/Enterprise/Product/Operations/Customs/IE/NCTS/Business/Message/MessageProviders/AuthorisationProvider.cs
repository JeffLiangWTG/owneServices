using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CusAuthorizationUsage = Enterprise.Customs.EU.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class AuthorisationProvider : IAuthorisation
	{
		readonly CusAuthorizationUsage authorisation;

		public AuthorisationProvider(CusAuthorizationUsage authorisation)
		{
			this.authorisation = Argument.NotNull(authorisation, nameof(authorisation));
		}

		public string IdentificationType
		{
			get
			{
				var customsValue = authorisation.CustomsCode;
				if (customsValue.IsEmpty)
				{
					customsValue = authorisation.AGC_Code;
				}
				return customsValue;
			}
		}

		public string ReferenceNumber => authorisation.AGC_Number;
	}
}
