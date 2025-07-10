using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsMovementHeaderCustomsOfficeRequirementHelper : EU.NCTS.Business.NctsMovementHeaderCustomsOfficeRequirementHelper
{
	public NctsMovementHeaderCustomsOfficeRequirementHelper(NctsCommonMovementHeader movementHeader) : base(movementHeader)
	{
	}

	protected NctsDepartureMovementHeader DepartureMovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	protected override string GetCacheKeyCombination() => string.Join(".", CountryCodes.Switzerland, base.GetCacheKeyCombination());

	bool IsNationalTransitSwitzerland => DepartureMovementHeader?.IsNationalTransitSwitzerland ?? false;

	protected override CustomsOfficeRequirement GetNCTSOfficeOfDepartureRequirement()
	{
		var requirement = base.GetNCTSOfficeOfDepartureRequirement();
		requirement.IsRecommended = false;
		return requirement;
	}

	protected override CustomsOfficeRequirement GetNCTSOfficeOfTransitRequirement()
	{
		CustomsOfficeRequirement requirement = null;

		if (!IsNationalTransitSwitzerland)
		{
			requirement = base.GetNCTSOfficeOfTransitRequirement();
			requirement.IsMandatory = true;
		}
		return requirement;
	}

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirementsCore()
	{
		var result = base.GetOtherRequirementsCore().ToList();

		if (MovementHeader.Header.IsArrivalMovement)
		{
			result.Add(GetNCTSOfficeOfDepartureForArrivalRequirement());
		}

		return result.WhereNotNull();
	}

	CustomsOfficeRequirement GetNCTSOfficeOfDepartureForArrivalRequirement()
	{
		var requirement = new CustomsOfficeRequirement(
			officeRole: OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
			isMandatory: true,
			isLocalCountryOnly: false,
			friendlyName: OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDeparture)
		{
			OfficeRolesForLookup = new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture }
		};
		return requirement;
	}

	protected override CustomsOfficeRequirement GetNCTSOfficeOfExitForTransitRequirement()
	{
		return IsNationalTransitSwitzerland ? null : base.GetNCTSOfficeOfExitForTransitRequirement();
	}
}
