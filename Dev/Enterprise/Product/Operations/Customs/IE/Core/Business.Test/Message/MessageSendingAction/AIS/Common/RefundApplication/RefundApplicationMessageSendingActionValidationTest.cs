using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class RefundApplicationMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMessageType()
		{
			CreateSendingAction();
			var targetInfo = sendingAction.MessageTypeInfo;

			sendingAction.ShouldSend = ZBool.True;
			sendingAction.MessageType = "!@#";
			AssertNoErrors("No validation against MessageType", targetInfo);
			sendingAction.MessageType = ZString.Empty;
			AssertNoErrors("No validation against MessageType", targetInfo);
		}

		public void TestCheckRefundType()
		{
			CreateSendingAction();
			var targetInfo = sendingAction.RefundTypeInfo;

			sendingAction.ShouldSend = ZBool.False;
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, "Please enter");

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "REM");
		}

		public void TestCheckOfficeOfDebt()
		{
			CreateCustomsOffice();
			CreateSendingAction();
			var targetInfo = sendingAction.OfficeOfDebtInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Validation.ValidateOfficeOfDebt();
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, "Please enter");

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "IE000001");
		}

		public void TestCheckOfficeOfResponsibility()
		{
			CreateCustomsOffice();
			CreateSendingAction();
			var targetInfo = sendingAction.OfficeOfResponsibilityInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.OfficeOfResponsibility = "~";
			sendingAction.Validation.ValidateOfficeOfResponsibility();
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, ListValidation.InvalidCodeError);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "IE000001");
		}

		public void TestCheckLegalBasis()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "LegalBasisCode");
			var repCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "REP", "REP DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			CreateSendingAction();
			var targetInfo = sendingAction.LegalBasisInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Validation.ValidateOfficeOfDebt();
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, "Please enter");
			sendingAction.OfficeOfResponsibility = "~";
			sendingAction.Validation.ValidateOfficeOfDebt();
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, ListValidation.InvalidCodeError);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "REP");
		}

		public void TestCheckDescriptionOfGrounds()
		{
			CreateSendingAction();
			var targetInfo = sendingAction.DescriptionOfGroundsInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Validation.ValidateOfficeOfDebt();
			AssertNoErrorContaining("No error if action is not selected for sending.", targetInfo, "Please enter");

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
		}

		public void TestCheckAmount()
		{
			CreateSendingAction();
			var targetInfo = sendingAction.AmountInfo;

			sendingAction.ShouldSend = ZBool.False;
			sendingAction.Amount = -1;
			sendingAction.Validation.ValidateAmount();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);
			sendingAction.Amount = 0;
			sendingAction.Validation.ValidateAmount();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			sendingAction.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);
			sendingAction.Amount = 0;
			AssertHasErrorContaining("Amount = 0", targetInfo, MandatoryValidation.ValueCannotBeZero);
			sendingAction.Amount = 1;
			AssertNoErrorContaining("Amount > 0", targetInfo, MandatoryValidation.ValueCannotBeZero);
		}

		void CreateSendingAction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
		}

		CusEntryHeader entryHeader;
		RefundApplicationMessageSendingAction sendingAction;

		void CreateCustomsOffice()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE000001", "IE000001 OFFICE", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			Factory.Save();
		}
	}
}
