using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateBM_InBondEntryType()
	{
		const string notExpectedError = "A TIR declaration requires a guarantee of type B (TIR). (R0900)";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("No Bond Type", false, departureMovement.BM_InBondEntryTypeInfo.HasMessageError(notExpectedError));

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertEquals("Bond Type B", false, departureMovement.BM_InBondEntryTypeInfo.HasMessageError(notExpectedError));
		});
	}

	public void TestCheckBM_TOLCarrierCode()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			departureMovement.Validation.ValidateBM_TOLCarrierCode();
			AssertEquals("Skip Validation for BM_TOLCarrierCode when empty", false, departureMovement.BM_TOLCarrierCodeInfo.Notifications.ContainsNotificationContaining(MandatoryValidation.YouHaveNotEntered));

			departureMovement.BM_TOLCarrierCode = "Z!";
			departureMovement.Validation.ValidateBM_TOLCarrierCode();
			AssertEquals("Skip Validation for BM_TOLCarrierCode when invalid", false, departureMovement.BM_TOLCarrierCodeInfo.Notifications.Contains(ListValidation.InvalidCodeMessageError.ToString()));
		});
	}
}
