using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class AuthorisationProvider : IAuthorisation
	{
		readonly CusAuthorizationUsage cusAuthorizationUsage;

		public AuthorisationProvider(CusAuthorizationUsage cusAuthorizationUsage, ZInt sequenceNumber)
		{
			this.cusAuthorizationUsage = Argument.NotNull(cusAuthorizationUsage, nameof(cusAuthorizationUsage));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Type
		{
			get
			{
				switch (cusAuthorizationUsage.AGC_Code)
				{
					case Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit:
						return Constants.AuthorisationTypes.C522;
					case Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir:
						return Constants.AuthorisationTypes.C520;
					case Constants.CusPermitHeaderTypes.ACR:
						return Constants.AuthorisationTypes.C521;
					case Constants.CusPermitHeaderTypes.SSE:
						return Constants.AuthorisationTypes.C523;
					case Constants.CusPermitHeaderTypes.TransitOperation:
						return Constants.AuthorisationTypes.C524;
					default:
						return string.Empty;
				}
			}
		}

		public string ReferenceNumber => cusAuthorizationUsage.AGC_Number;
	}
}
