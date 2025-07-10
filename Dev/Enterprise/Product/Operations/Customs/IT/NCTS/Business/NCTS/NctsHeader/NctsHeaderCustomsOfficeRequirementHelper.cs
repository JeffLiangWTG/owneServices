using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderCustomsOfficeRequirementHelper : EU.NCTS.Business.NctsHeaderCustomsOfficeRequirementHelper
{
	public NctsHeaderCustomsOfficeRequirementHelper(EU.NCTS.Business.NctsHeader header) : base(header)
	{
	}

	protected override string GetCacheKeyCombination() => string.Join(".", CountryCodes.Italy, base.GetCacheKeyCombination());

	string ITNctsOfficeOfDepartureFriendlyName => Res.GetString("D178E15B-EEFD-4FA8-B871-9E858FC6DCFD", "NCTS Office of departure/presentation");

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirementsCore()
	{
		var result = base.GetOtherRequirementsCore();

		var officeOfDeparture = result.SingleOrDefault(x => x.OfficeRole == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		if (officeOfDeparture != null)
		{
			officeOfDeparture.FriendlyName = ITNctsOfficeOfDepartureFriendlyName;
		}

		return result;
	}
}
