using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsDepartureMovementHeaderLookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups
{
	public NctsDepartureMovementHeaderLookups(EU.NCTS.Business.NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	protected override bool IsOfficeValidForOfficeCodeList(EU.NCTS.Business.NctsEuOfficeCode office) => validOfficeCodesForOfficeCodeList.Contains(office.CY_Code);

	static readonly ImmutableHashSet<string> validOfficeCodesForOfficeCodeList = new HashSet<string>
	{
		OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
		OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit,
		OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit
	}.ToImmutableHashSet();
}
