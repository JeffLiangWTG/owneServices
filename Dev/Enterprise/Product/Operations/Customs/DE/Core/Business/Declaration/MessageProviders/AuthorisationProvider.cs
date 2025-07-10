using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public class AuthorisationProvider : IAuthorisation
	{
		internal static AuthorisationProvider[] New(CusEntryInstruction entryInstruction)
		{
			return entryInstruction.CusAuthorizationUsages
				.Select(x => New(EUUniversalLookupsHelper.GetUCCAuthorizationCodeCustomsValue(x.Factory, x.AGC_Code), x.AGC_Number)).ToArray();
		}

		internal static AuthorisationProvider New(ZString type, ZString referenceNumber) => new AuthorisationProvider(type, referenceNumber);

		AuthorisationProvider(ZString type, ZString referenceNumber)
		{
			Type = type;
			ReferenceNumber = referenceNumber;
		}

		public string Type { get; }

		public string ReferenceNumber { get; }

		public string HolderOfAuthorisation => string.Empty;
	}
}
