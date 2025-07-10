using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(NctsHeaderCustomsOfficeRequirementHelper))]
sealed class NctsHeaderCustomsOfficeRequirementHelperTest : EU.NCTS.Business.Testing.NctsHeaderCustomsOfficeRequirementHelperAbstractTest<NctsHeaderCustomsOfficeRequirementHelper>
{
	protected override string ExpectedCacheKey => string.Join(".", CountryCodes.Italy, base.ExpectedCacheKey);

	protected override IEnumerable<CustomsOfficeRequirement> ExpectedOtherRequirements
	{
		get
		{
			var officeRequirements = base.ExpectedOtherRequirements;
			SetExpectedOfficeOfDepartureFriendlyName(officeRequirements);
			return officeRequirements;
		}
	}
	void SetExpectedOfficeOfDepartureFriendlyName(IEnumerable<CustomsOfficeRequirement> officeRequirements)
	{
		var officeOfDeparture = officeRequirements.Single(x => x.OfficeRole == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		officeOfDeparture.FriendlyName = "NCTS Office of departure/presentation";
	}
}
