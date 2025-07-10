using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJPM_DeleteReasonCode()
		{
			PrepareRefCusCodeList();

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var messageSending = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = ZString.Empty
			};
			AssertHasMessageError(messageSending.JPM_DeleteReasonCodeInfo, ValidationConstants.MessageSending.DeleteReasonCodeIsRequired);

			messageSending = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = "6"
			};
			AssertHasMessageError(messageSending.JPM_DeleteReasonCodeInfo, "The code you have selected is not in the list.");
			messageSending.JPM_DeleteReasonCode = "1";
			AssertNoMessageError(messageSending.JPM_DeleteReasonCodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJPM_DeleteReasonText()
		{
			PrepareRefCusCodeList();

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var messageSending = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = "5"
			};
			AssertHasMessageError(messageSending.JPM_DeleteReasonTextInfo, ValidationConstants.MessageSending.ADeleteReasonMustBeSupplied);

			messageSending.JPM_DeleteReasonText = "Other Reason";
			AssertHasMessageError(messageSending.JPM_DeleteReasonTextInfo, ValidationConstants.MessageSending.ADeleteReasonMustBeSpecific);

			messageSending.JPM_DeleteReasonText = "好";
			AssertHasError(messageSending.JPM_DeleteReasonTextInfo, "Delete Reason Text only accepts Western European languages characters.");

			messageSending.JPM_DeleteReasonText = "　"; // CJK Space
			AssertHasError(messageSending.JPM_DeleteReasonTextInfo, "Delete Reason Text only accepts Western European languages characters.");

			messageSending.JPM_DeleteReasonText = "this is test";
			AssertNoMessageErrors(messageSending.JPM_DeleteReasonTextInfo);
		}

		public void TestCheckJPM_ActionCode()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			messageSending.JPM_Send = true;
			AssertEquals(true, messageSending.JPM_ActionCodeInfo.ReadOnly);
			messageSending.JPM_ActionCode = ZString.Empty;
			AssertNoNotifications(messageSending.JPM_ActionCodeInfo);

			var actionCodeList = new ActionCode[] { ActionCode.AmendingAdd, ActionCode.AmendingDelete, ActionCode.AmendingUpdate };
			foreach (var actionCode in actionCodeList)
			{
				bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				messageSending = new MessageSendingObject(bill, actionCode, TestSendingAction);
				messageSending.JPM_Send = true;
				AssertEquals(false, messageSending.JPM_ActionCodeInfo.ReadOnly);
				messageSending.JPM_ActionCode = ZString.Empty;
				AssertHasMessageErrorContaining(messageSending.JPM_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ListValidation.InvalidCodeMessageError);
				messageSending.JPM_ActionCode = "!";
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ListValidation.InvalidCodeMessageError);
				messageSending.JPM_ActionCode = AFRSendingActionCodeList.Codes.Add;
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertHasMessageError(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInstead);
				bill.JPB_ReleaseStatus = ZString.Empty;
				messageSending.JPM_ActionCode = AFRSendingActionCodeList.Codes.Add;
				AssertNoMessageError(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInstead);

				messageSending.JPM_Send = false;
				messageSending.JPM_ActionCode = ZString.Empty;
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ListValidation.InvalidCodeMessageError);

				messageSending.JPM_ActionCode = "!";
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ListValidation.InvalidCodeMessageError);
				if (actionCode == ActionCode.AmendingAdd)
				{
					messageSending.JPM_ActionCode = AFRSendingActionCodeList.Codes.Add;
					AssertNoMessageError(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInstead);
				}
			}

			CombineAssertions(() =>
			{
				TestSendingAction.HasATDBeenSent = true;
				messageSending.JPM_ActionCode = AFRSendingActionCodeList.Codes.Add;
				messageSending.JPM_Send = true;
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);
				messageSending.JPM_Send = false;
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);

				messageSending.JPM_ActionCode = AFRSendingActionCodeList.Codes.Update;
				messageSending.JPM_Send = true;
				AssertHasMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);
				messageSending.JPM_Send = false;
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);

				messageSending.JPM_ActionCode = AFRSendingActionCodeList.Codes.Delete;
				messageSending.JPM_Send = true;
				AssertHasMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);
				messageSending.JPM_Send = false;
				AssertNoMessageErrorContaining(messageSending.JPM_ActionCodeInfo, ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);
			});
		}

		public void TestCheckJPM_ReleaseStatus()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			messageSending.JPM_Send = true;
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			messageSending.Validation.ValidateJPM_ReleaseStatus();
			AssertHasMessageError(messageSending.JPM_ReleaseStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction);

			bill.JPB_ReleaseStatus = ZString.Empty;
			messageSending.Validation.ValidateJPM_ReleaseStatus();
			AssertNoMessageError(messageSending.JPM_ReleaseStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction);

			messageSending.JPM_Send = false;
			messageSending.Validation.ValidateJPM_ReleaseStatus();
			AssertNoMessageError(messageSending.JPM_ReleaseStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction);

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			messageSending.Validation.ValidateJPM_ReleaseStatus();
			AssertNoMessageError(messageSending.JPM_ReleaseStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction);

			messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			messageSending.Validation.ValidateJPM_ReleaseStatus();
			AssertNoMessageError(messageSending.JPM_ReleaseStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction);
		}

		public void TestCheckJPM_BillOfLadingNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			messageSending.JPM_Send = true;
			messageSending.Validation.ValidateJPM_BillOfLadingNumber();
			AssertHasErrorContaining(messageSending.JPM_BillOfLadingNumberInfo, ValidationConstants.MessageSending.BOLIsEmpty(messageSending.JPM_BillOfLadingNumberInfo.HumanReadableName));

			messageSending.JPM_Send = false;
			messageSending.Validation.ValidateJPM_BillOfLadingNumber();
			AssertNoErrorContaining(messageSending.JPM_BillOfLadingNumberInfo, ValidationConstants.MessageSending.BOLIsEmpty(messageSending.JPM_BillOfLadingNumberInfo.HumanReadableName));

			bill.JPB_BillNumber = "J07JTESTA01";
			messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			messageSending.JPM_Send = true;
			messageSending.Validation.ValidateJPM_BillOfLadingNumber();
			AssertNoErrorContaining(messageSending.JPM_BillOfLadingNumberInfo, ValidationConstants.MessageSending.BOLIsEmpty(messageSending.JPM_BillOfLadingNumberInfo.HumanReadableName));

			messageSending.JPM_Send = false;
			messageSending.Validation.ValidateJPM_BillOfLadingNumber();
			AssertNoErrors(messageSending.JPM_BillOfLadingNumberInfo);

			bill.JPB_BillNumber = "J07JTESTA01Â";
			messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			messageSending.JPM_Send = true;
			messageSending.Validation.ValidateJPM_BillOfLadingNumber();
			AssertHasErrorContaining(messageSending.JPM_BillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumberMessageForSending("J07JTESTA01Â"));

			messageSending.JPM_Send = false;
			messageSending.Validation.ValidateJPM_BillOfLadingNumber();
			AssertNoErrors(messageSending.JPM_BillOfLadingNumberInfo);
		}

		public void TestCheckJPB_Calc_ESDT()
		{
			using (var inbondDetailInitiator = new InBondDetailInitiatorTestHelper())
			{
				var header = Factory.New<JPAFRHeader>();
				var bill = header.Bills.AddNew();
				bill.InBondDetailInitiator = inbondDetailInitiator;
				bill.JPB_Calc_ESDT = ZDateTime.UtcNow.AddDays(-10);
				var messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
				messageSending.JPM_Send = true;
				messageSending.Validation.ValidateJPM_Calc_ESDT();
				AssertHasMessageError(messageSending.JPM_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

				messageSending.JPM_Send = false;
				messageSending.Validation.ValidateJPM_Calc_ESDT();
				AssertNoMessageError(messageSending.JPM_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

				bill.JPB_Calc_ESDT = ZDateTime.UtcNow.AddDays(+10);
				messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
				messageSending.JPM_Send = true;
				messageSending.Validation.ValidateJPM_Calc_ESDT();
				AssertNoMessageError(messageSending.JPM_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

				messageSending.JPM_Send = false;
				messageSending.Validation.ValidateJPM_Calc_ESDT();
				AssertNoMessageError(messageSending.JPM_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);
			}
		}

		public void TestCheckJPB_Calc_EFDT()
		{
			using (var inbondDetailInitiator = new InBondDetailInitiatorTestHelper())
			{
				var header = Factory.New<JPAFRHeader>();
				var bill = header.Bills.AddNew();
				bill.InBondDetailInitiator = inbondDetailInitiator;
				bill.JPB_Calc_EFDT = ZDateTime.UtcNow.AddDays(-10);
				var messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
				messageSending.JPM_Send = true;
				messageSending.Validation.ValidateJPM_Calc_EFDT();
				AssertHasMessageError(messageSending.JPM_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

				messageSending.JPM_Send = false;
				messageSending.Validation.ValidateJPM_Calc_EFDT();
				AssertNoMessageError(messageSending.JPM_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

				bill.JPB_Calc_EFDT = ZDateTime.UtcNow.AddDays(+10);
				messageSending = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
				messageSending.JPM_Send = true;
				messageSending.Validation.ValidateJPM_Calc_EFDT();
				AssertNoMessageError(messageSending.JPM_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

				messageSending.JPM_Send = false;
				messageSending.Validation.ValidateJPM_Calc_EFDT();
				AssertNoMessageError(messageSending.JPM_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);
			}
		}

		MessageSendingAction TestSendingAction
		{
			get { return testSendingAction ?? (testSendingAction = new MessageSendingAction(Factory.New<JPAFRHeader>(), ActionCode.AmendingAdd)); }
		}
		MessageSendingAction testSendingAction;

		void PrepareRefCusCodeList()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			TestCaseHelper.ClearTable(RefCusCodeListAttribute.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "AFR Delete Reason");
			var jp1dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "1", startDate, endDate);
			jp1dr.ZZD_Description = "Cancelation of Loading";
			var jp2dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "3", startDate, endDate);
			jp2dr.ZZD_Description = "Change of B/L Number";
			var jp3dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "4", startDate, endDate);
			jp3dr.ZZD_Description = "Misregistration";
			var jp4dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "5", startDate, endDate);
			jp4dr.ZZD_Description = "Other Reason";
			universalReferenceTestHelper.CreateCusCodeListAttribute(jp4dr.PK, RefCusCodeListAttributeTypes.Codes.FreeTextRequired,
				ZString.Empty);
			Factory.Save();
		}
	}
}
