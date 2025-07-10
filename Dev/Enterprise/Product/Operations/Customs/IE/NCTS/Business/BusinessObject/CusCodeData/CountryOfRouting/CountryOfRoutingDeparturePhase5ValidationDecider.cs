using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class CountryOfRoutingDeparturePhase5ValidationDecider : ICountryOfRoutingDeparturePhase5ValidationDecider
	{
		public EU.NCTS.Business.NctsHeader NctsHeader;

		public CountryOfRoutingDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsHeader nctsHeader)
		{
			NctsHeader = nctsHeader;
		}

		public bool IsRuleB1836Active => true;

		public bool IsRuleC0030Active
		{
			get
			{
				if (NctsHeader.MovementHeader is EU.NCTS.Business.NctsDepartureMovementHeader departureMovementHeader)
				{
					if (departureMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR ||
						departureMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T2SM)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool IsRuleC0586Active => true;
	}
}
