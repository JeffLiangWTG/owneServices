using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class RefundSessionalDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRefundCauseCode()
		{
			sessionalData.CSI_Procedure = ZString.Empty;
			sessionalData.Validation.ValidateRefundCauseCode();
			AssertNoMessageErrors(sessionalData.RefundCauseCodeInfo);

			sessionalData.CSI_Procedure = "20";
			sessionalData.Validation.ValidateRefundCauseCode();
			AssertHasMessageErrorContaining(sessionalData.RefundCauseCodeInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.CSI_Procedure = "01";
			sessionalData.Validation.ValidateRefundCauseCode();
			AssertNoMessageErrors(sessionalData.RefundCauseCodeInfo);
		}

		public void TestCheckRefundReasonCode()
		{
			sessionalData.CSI_IssuerType = ZString.Empty;
			sessionalData.Validation.ValidateRefundReasonCode();
			AssertNoMessageErrors(sessionalData.RefundReasonCodeInfo);

			sessionalData.CSI_IssuerType = "05";
			sessionalData.Validation.ValidateRefundReasonCode();
			AssertHasMessageErrorContaining(sessionalData.RefundReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.CSI_IssuerType = "01";
			sessionalData.Validation.ValidateRefundReasonCode();
			AssertNoMessageErrors(sessionalData.RefundReasonCodeInfo);
		}

		public void TestCheckRefundType()
		{
			sessionalData.CSI_SubType = ZString.Empty;
			sessionalData.Validation.ValidateRefundType();
			AssertNoMessageErrors(sessionalData.RefundTypeInfo);

			sessionalData.CSI_SubType = "Z";
			sessionalData.Validation.ValidateRefundType();
			AssertHasMessageErrorContaining(sessionalData.RefundTypeInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.CSI_SubType = "A";
			sessionalData.Validation.ValidateRefundType();
			AssertNoMessageErrors(sessionalData.RefundTypeInfo);
		}

		public void TestCheckRefundRequestYN()
		{
			sessionalData.CSI_Code = ZString.Empty;
			sessionalData.Validation.ValidateRefundRequestYN();
			AssertNoMessageErrors(sessionalData.RefundRequestYNInfo);

			sessionalData.CSI_Code = "X";
			sessionalData.Validation.ValidateRefundRequestYN();
			AssertHasMessageErrorContaining(sessionalData.RefundRequestYNInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.CSI_Code = "Y";
			sessionalData.Validation.ValidateRefundRequestYN();
			AssertNoMessageErrors(sessionalData.RefundRequestYNInfo);
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionData = new AmendmentSessionalDataCollection(instruction).AddNew();
			amendmentSessionData.CSI_Code =	DutyTaxCorrectionCodeList.Codes.C;
			sessionalData = new RefundSessionalDataCollection(instruction, amendmentSessionData.PK).AddNew();
			sessionalData.CSI_CSI_SupportingInfo = amendmentSessionData.PK;
		}
		RefundSessionalData sessionalData;
	}
}
