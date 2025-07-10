using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsCargoDescFeeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBFE_ChargeType()
		{
			fee.Validation.ValidateBFE_ChargeType();
			AssertHasMessageError(fee.BFE_ChargeTypeInfo, "You have not entered a Fee Code.");

			fee.BFE_ChargeType = "1";
			fee.Validation.ValidateBFE_ChargeType();
			AssertHasMessageError(fee.BFE_ChargeTypeInfo, "The code you have selected is not in the list.");

			fee.BFE_ChargeType = HarbourFeeCodes.Codes.V905;
			fee.Validation.ValidateBFE_ChargeType();
			AssertNoMessageError("V905 should be a valid code.", fee.BFE_ChargeTypeInfo, "The code you have selected is not in the list.");

			fee.BFE_ChargeType = ChargeTypesList.Codes.VAT;
			fee.Validation.ValidateBFE_ChargeType();
			AssertNoMessageError("VAT should be a valid code.", fee.BFE_ChargeTypeInfo, "The code you have selected is not in the list.");

			fee.BFE_ChargeType = ChargeTypesList.Codes.DTY;
			fee.Validation.ValidateBFE_ChargeType();
			AssertNoMessageError("DTY should be a valid code.", fee.BFE_ChargeTypeInfo, "The code you have selected is not in the list.");

			fee.BFE_ChargeType = "ADD";
			fee.Validation.ValidateBFE_ChargeType();
			AssertNoMessageError("DTY should be a valid code.", fee.BFE_ChargeTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckBFE_RateOverrideReasonCode()
		{
			fee.BFE_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
			AssertNoMessageError(fee.BFE_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			fee.BFE_RateOverrideReasonCode = "AAA";
			AssertHasMessageError(fee.BFE_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			fee.BFE_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Override;
			AssertNoMessageError(fee.BFE_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var line = nctsHeader.MovementHeader.GoodsItems.AddNew();
			fee = line.Fees.AddNew();
		}

		NctsHeader nctsHeader;
		NctsCargoDescFee fee;
	}
}
