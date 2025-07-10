using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsFrOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Date()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = true;
			var frOfficeCode = departureMovement.CustomsOffices.AddNew();
			frOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			frOfficeCode.CY_Data = "FR000060";
			frOfficeCode.CY_Date = ZDateTime.Empty;

			AssertHasMessageErrorContaining(frOfficeCode.CY_DateInfo, preLodgedError);

			frOfficeCode.CY_Date = ZDateTime.Now.AddDays(5);
			AssertNoMessageErrorContaining(frOfficeCode.CY_DateInfo, preLodgedError);

			frOfficeCode.CY_Date = ZDateTime.Now.AddDays(31);
			AssertHasMessageErrorContaining(frOfficeCode.CY_DateInfo, preLodgedError);

			frOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			frOfficeCode.Validation.ValidateCY_Date();
			AssertNoMessageErrorContaining(frOfficeCode.CY_DateInfo, preLodgedError);

			frOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			frOfficeCode.Validation.ValidateCY_Date();
			AssertHasMessageErrorContaining(frOfficeCode.CY_DateInfo, preLodgedError);

			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = false;
			frOfficeCode.Validation.ValidateCY_Date();
			AssertNoMessageErrorContaining(frOfficeCode.CY_DateInfo, preLodgedError);
		}

		const string preLodgedError = "The office of departure date cannot be empty and must less than 30 days in the future for pre-lodged departures";
	}
}
