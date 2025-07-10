using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PenaltyExemptionSessionalDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPenaltyExemptionCode()
		{
			sessionalData.PenaltyExemptionCode = ZString.Empty;
			sessionalData.Validation.ValidatePenaltyExemptionCode();
			AssertNoMessageErrors(sessionalData.PenaltyExemptionCodeInfo);

			sessionalData.PenaltyExemptionCode = "A";
			sessionalData.Validation.ValidatePenaltyExemptionCode();
			AssertHasMessageErrorContaining(sessionalData.PenaltyExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.PenaltyExemptionCode = "Y";
			sessionalData.Validation.ValidatePenaltyExemptionCode();
			AssertNoMessageErrors(sessionalData.PenaltyExemptionCodeInfo);
		}

		public void TestCheckApplyDutyPenaltyReduction()
		{
			sessionalData.Validation.ValidateApplyDutyPenaltyReduction();
			AssertNoMessageErrors(sessionalData.ApplyDutyPenaltyReductionInfo);

			sessionalData.ApplyDutyPenaltyReduction = "A";
			sessionalData.Validation.ValidateApplyDutyPenaltyReduction();
			AssertHasMessageErrorContaining(sessionalData.ApplyDutyPenaltyReductionInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.ApplyDutyPenaltyReduction = "Y";
			sessionalData.Validation.ValidateApplyDutyPenaltyReduction();
			AssertNoMessageErrors(sessionalData.ApplyDutyPenaltyReductionInfo);
		}
		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionData = new AmendmentSessionalDataCollection(instruction).AddNew();
			amendmentSessionData.CSI_Code = "A";
			sessionalData = amendmentSessionData.PenaltyExemptionSessionalData;
		}
		PenaltyExemptionSessionalData sessionalData;
	}
}
