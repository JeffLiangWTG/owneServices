using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CountryOfRoutingValidationTest : BusinessObjectValidationTestCase
{
	public void TestPhase5Departure_ValidateConditionRuleB1848()
	{
		var errorMsg = "[B1848] In transition period, which is now, no Country/Region of Routing rows must be entered.";

		var countryOfRouting = nctsHeader.CountriesOfRouting.AddNew();
		countryOfRouting.CY_Data = "IT";

		using (SetTransitionPeriod(false))
		{
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			countryOfRouting.Validation.ValidateAll();
			AssertNoRowMessageError("When TP is false, Security : NON", countryOfRouting, errorMsg);
		}

		using (SetTransitionPeriod(true))
		{
			CombineAssertions("When TP is true", () =>
			{
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				countryOfRouting.Validation.ValidateAll();
				AssertNoRowMessageError("Security : ENT", countryOfRouting, errorMsg);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				countryOfRouting.Validation.ValidateAll();
				AssertNoRowMessageError("Security : EXI", countryOfRouting, errorMsg);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				countryOfRouting.Validation.ValidateAll();
				AssertNoRowMessageError("Security : BTH", countryOfRouting, errorMsg);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				countryOfRouting.Validation.ValidateAll();
				AssertHasRowMessageError("Security : NON", countryOfRouting, errorMsg);
			});
		}
	}

	#region Implementation

	IDisposable SetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	#endregion

	#region SetUp

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		movementHeader = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	#endregion
}
