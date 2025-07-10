using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsMovementHeaderCustomsOfficeRequirementHelper))]
sealed class NctsMovementHeaderCustomsOfficeRequirementHelperTest : NctsMovementHeaderCustomsOfficeRequirementHelperAbstractTest<NctsMovementHeaderCustomsOfficeRequirementHelper>
{
	protected override string ExpectedCacheKey => "CH.NctsMovementHeaderCustomsOfficeRequirementHelper.OtherRequirements.D..NC5";

	protected override IEnumerable<CustomsOfficeRequirement> ExpectedOtherRequirements
	{
		get
		{
			var officeRequirements = base.ExpectedOtherRequirements;
			var officeOfTransit = officeRequirements.Single(o => o.OfficeRole == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			officeOfTransit.IsMandatory = true;
			var officeOfDeparture = officeRequirements.Single(o => o.OfficeRole == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			officeOfDeparture.IsRecommended = false;
			officeOfDeparture.IsMandatory = !header.IsPhase5;
			return officeRequirements;
		}
	}

	protected override IEnumerable<ZString> CustomsOfficeRequirementWithRoles() => new ZString[]
	{
		OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
		OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit,
		OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit
	};

	public void TestOtherRequirements_NationalTransitSwitzerland()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;

		AssertEquals("Count of OtherRequirements", 2, officeHelper.OtherRequirements.Count());

		foreach (var role in new[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination })
		{
			AssertCustomsOfficeRequirementWithRole(role);
		}
	}

	public void TestOtherRequirements_Arrival()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		NctsArrivalMovementHeader movementHeader = nctsHeader.ArrivalMovementHeader;
		movementHeader.CustomsOffices.RemoveAndDeleteAll();
		officeHelper = movementHeader.CustomsOfficeRequirementHelper;
		AssertContainsExactElementsInAnyOrder("All Office Roles"
			, new[]
			{
				OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
				OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival,
				OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
			},
			officeHelper.OtherRequirements.Select(x => x.OfficeRole));
	}

	protected override void SetUp()
	{
		base.SetUp();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
	}
}
