using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class DepositRefundApplicationMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckExportMovementReferenceNumber_Mandatory()
		{
			var targetInfo = sendingAction.ExportMovementReferenceNumberInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Validation.ValidateExportMovementReferenceNumber();
			AssertNoErrorContaining(targetInfo, "Please enter");

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
		}

		public void TestCheckExportMovementReferenceNumber_Length()
		{
			var targetInfo = sendingAction.ExportMovementReferenceNumberInfo;
			var messageErrorLength = "Export MRN should be exactly 18 characters long.";

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.ExportMovementReferenceNumber = "17CharsLongString";
			AssertNoErrorContaining(targetInfo, messageErrorLength);

			sendingAction.ShouldSend = ZBool.True;
			sendingAction.ExportMovementReferenceNumber = "17CharsLongString";
			AssertHasErrorContaining(targetInfo, messageErrorLength);
			sendingAction.ExportMovementReferenceNumber = "18_CharsLongString";
			AssertNoErrorContaining(targetInfo, messageErrorLength);
		}

		public void TestCheckExportMovementReferenceNumber_Alphanumeric()
		{
			var targetInfo = sendingAction.ExportMovementReferenceNumberInfo;
			var messageError = "should be alphanumeric only";

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.ExportMovementReferenceNumber = "!&%";
			AssertNoErrorContaining(targetInfo, messageError);

			sendingAction.ShouldSend = ZBool.True;
			sendingAction.ExportMovementReferenceNumber = "!&%";
			AssertHasErrorContaining(targetInfo, messageError);
			sendingAction.ExportMovementReferenceNumber = "IAmAlphanumeric123";
			AssertNoErrorContaining(targetInfo, messageError);
		}

		public void TestCheckExportDate()
		{
			var targetInfo = sendingAction.ExportDateInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Validation.ValidateExportDate();
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, "Please enter");

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
		}

		public void TestCheckCustomsDuty()
		{
			var targetInfo = sendingAction.CustomsDutyInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.CustomsDuty = -1;
			sendingAction.Validation.ValidateCustomsDuty();
			AssertNoErrors("No error if negative value and action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
		}

		public void TestCheckVat()
		{
			var targetInfo = sendingAction.VatInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Vat = -1;
			sendingAction.Validation.ValidateVat();
			AssertNoErrors("No error if negative value and action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
		}

		public void TestCheckOtherDuties()
		{
			var targetInfo = sendingAction.OtherDutiesInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.OtherDuties = -1;
			sendingAction.Validation.ValidateOtherDuties();
			AssertNoErrors("No error if negative value and action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
		}

		public void TestCheckOutstandingBalance()
		{
			var targetInfo = sendingAction.OutstandingBalanceInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.OutstandingBalance = -1;
			sendingAction.Validation.ValidateOutstandingBalance();
			AssertNoErrors("No error if negative value and action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
		}

		public void TestCheckAmountOfDepositRefund()
		{
			var targetInfo = sendingAction.AmountOfDepositRefundInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.AmountOfDepositRefund = -1;
			sendingAction.Validation.ValidateAmountOfDepositRefund();
			AssertNoErrors("No error if negative value and action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
		}

		public void TestCheckPayerEori()
		{
			var targetInfo = sendingAction.PayerEoriInfo;
			var messageError = "should be alphanumeric only";

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.PayerEori = "!&%";
			AssertNoErrorContaining(targetInfo, messageError);

			sendingAction.ShouldSend = ZBool.True;
			sendingAction.PayerEori = "!&%";
			AssertHasErrorContaining(targetInfo, messageError);
			sendingAction.PayerEori = "IAmAlphanumeric12";
			AssertNoErrorContaining(targetInfo, messageError);
		}

		public void TestCheckPayerEori_RuleBR8070()
		{
			var targetInfo = sendingAction.PayerEoriInfo;
			var messageError = "[BR8070] Payer EORI for Refund must equal Declarant EORI or empty.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "1234567890";
			Factory.Save();

			var declarant = orgHeader.Addresses.AddNew();

			sendingAction.ShouldSend = ZBool.True;
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			sendingAction.PayerEori = "!&%";
			AssertHasErrorContaining("ShouldSend is true, PayerEori not equal to Declarant's EORI", targetInfo, messageError);
			sendingAction.PayerEori = "IE1234567890";
			AssertNoErrorContaining("ShouldSend is true, PayerEori equal to Declarant's EORI", targetInfo, messageError);

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.PayerEori = "!&%";
			AssertNoErrorContaining("ShouldSend is false, PayerEori not equal to Declarant's EORI", targetInfo, messageError);
			sendingAction.PayerEori = "IE1234567890";
			AssertNoErrorContaining("ShouldSend is false, PayerEori equal to Declarant's EORI", targetInfo, messageError);
		}

		public void TestCheckPeriodForDischarge()
		{
			var targetInfo = sendingAction.PeriodForDischargeInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.PeriodForDischarge = -1;
			sendingAction.Validation.ValidatePeriodForDischarge();
			AssertNoErrors("No error if negative value and action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			sendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		DepositRefundApplicationMessageSendingAction sendingAction;
	}
}
