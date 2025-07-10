using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415MessageSendingObjectValidation))]
	sealed class RF415MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRefundType()
		{
			var messageSendingObject = CreateSendingObject();
			var targetInfo = messageSendingObject.RefundTypeInfo;

			messageSendingObject.ShouldSend = ZBool.False;
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo, MandatoryValidation.MustBeEnteredMessage("Type"));
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "REM", "Enter a valid Type.");
		}

		public void TestCheckOfficeOfDebt()
		{
			var messageSendingObject = CreateSendingObject();
			CreateCustomsOffice();
			var targetInfo = messageSendingObject.OfficeOfDebtInfo;

			messageSendingObject.ShouldSend = ZBool.False;
			messageSendingObject.Validation.ValidateOfficeOfDebt();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo, MandatoryValidation.MustBeEnteredMessage("Office of Debt"));
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "IE000001", "Enter a valid Office of Debt.");
		}

		public void TestCheckOfficeOfResponsibility()
		{
			var messageSendingObject = CreateSendingObject();
			CreateCustomsOffice();
			var targetInfo = messageSendingObject.OfficeOfResponsibilityInfo;

			messageSendingObject.ShouldSend = ZBool.False;
			messageSendingObject.OfficeOfResponsibility = "~";
			messageSendingObject.Validation.ValidateOfficeOfResponsibility();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "IE000001", ListValidation.InvalidCodeError);
		}

		public void TestCheckLegalBasis()
		{
			var messageSendingObject = CreateSendingObject();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType("LBC", "LegalBasisCode");
			var repCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "LBC", "REP", "REP DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var targetInfo = messageSendingObject.LegalBasisInfo;

			messageSendingObject.ShouldSend = ZBool.False;

			messageSendingObject.Validation.ValidateOfficeOfDebt();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);
			messageSendingObject.OfficeOfResponsibility = "~";
			messageSendingObject.Validation.ValidateOfficeOfDebt();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo, MandatoryValidation.MustBeEnteredMessage("Legal Basis"));
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "~", "REP", "Enter a valid Legal Basis.");
		}

		public void TestCheckDescriptionOfGrounds()
		{
			var messageSendingObject = CreateSendingObject();
			var targetInfo = messageSendingObject.DescriptionOfGroundsInfo;

			messageSendingObject.ShouldSend = ZBool.False;
			messageSendingObject.Validation.ValidateOfficeOfDebt();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo, MandatoryValidation.MustBeEnteredMessage("Description of Grounds"));
		}

		public void TestCheckAmount()
		{
			var messageSendingObject = CreateSendingObject();
			var targetInfo = messageSendingObject.AmountInfo;

			messageSendingObject.ShouldSend = ZBool.False;
			messageSendingObject.Amount = -1;
			messageSendingObject.Validation.ValidateAmount();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);
			messageSendingObject.Amount = 0;
			messageSendingObject.Validation.ValidateAmount();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo, MandatoryValidation.ValueCannotBeNegativeMessage("Amount"));
			messageSendingObject.Amount = 0;
			AssertHasError(targetInfo, MandatoryValidation.ValueCannotBeZeroMessage("Amount"));
		}

		RF415MessageSendingObject CreateSendingObject()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.Header.AMA_ApplicationCode = "LV2";
			var expectedMovementReferenceNumber = "1234";
			var cusEntryNumber = CusEntryNumber.New(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, bill.Header.AMA_RN_NKCountry);
			cusEntryNumber.CE_EntryNum = expectedMovementReferenceNumber;
			Factory.Save();

			return new RF415MessageSendingObject(bill);
		}

		void CreateCustomsOffice()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE000001", "IE000001 OFFICE", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "ENQ");
			Factory.Save();
		}
	}
}
