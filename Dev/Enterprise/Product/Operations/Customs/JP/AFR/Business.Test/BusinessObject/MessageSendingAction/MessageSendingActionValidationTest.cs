using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class MessageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJPM_ETA()
		{
			var header = Factory.New<JPAFRHeader>();
			var messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
			messageSendingAction.Validation.ValidateJPM_ETA();
			AssertNoMessageErrors(messageSendingAction.JPM_ETAInfo);

			header.JPH_ETA = ZDateTime.UtcNow.AddDays(-10);
			messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
			messageSendingAction.Validation.ValidateJPM_ETA();
			AssertHasMessageError(messageSendingAction.JPM_ETAInfo, ValidationConstants.Header.PastDateNotAllowedForETA);

			header.JPH_ETA = ZDateTime.UtcNow.AddDays(10);
			messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
			messageSendingAction.Validation.ValidateJPM_ETA();
			AssertNoMessageErrors(messageSendingAction.JPM_ETAInfo);
		}

		public void TestCheckJPM_MasterBillOfLadingNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
			CombineAssertions("Test IsEmpty validation on Header Bill Number", () =>
			{
				header.JPH_IsShippingLineEntry = true;
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertNoErrors(messageSendingAction.JPM_MasterBillOfLadingNumberInfo);
				header.JPH_IsShippingLineEntry = false;
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.MessageSending.BOLIsEmpty(messageSendingAction.JPM_MasterBillOfLadingNumberInfo.HumanReadableName));
			});

			header.JPH_MasterBillNumber = "TESTMB";
			messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
			messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
			AssertNoErrors(messageSendingAction.JPM_MasterBillOfLadingNumberInfo);

			header.JPH_MasterBillNumber = "TESTMB";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "testBill";
			CombineAssertions("Test Bill Number duplication for NVOCC Job", () =>
			{
				header.JPH_IsShippingLineEntry = true;
				bill2.JPB_BillNumber = "testBill";
				messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.MessageSending.DuplicationBillNumber);
				bill2.JPB_BillNumber = "testBill2";
				messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertNoErrors(messageSendingAction.JPM_MasterBillOfLadingNumberInfo);
			});
			CombineAssertions("Test Bill Number duplication for VOCC Job", () =>
			{
				header.JPH_IsShippingLineEntry = true;
				bill2.JPB_BillNumber = "testBill";
				messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.MessageSending.DuplicationBillNumber);
				bill2.JPB_BillNumber = "testBill2";
				messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertNoErrors(messageSendingAction.JPM_MasterBillOfLadingNumberInfo);
			});
			CombineAssertions("Test Invalid Characters in Master Bill Number", () =>
			{
				header.JPH_IsShippingLineEntry = false;
				header.JPH_MasterBillNumber = "testMBOL1,";
				bill2.JPB_BillNumber = "testBill,";
				messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharMessageForSending(messageSendingAction.JPM_MasterBillOfLadingNumberInfo.HumanReadableName));
			});
			CombineAssertions("Test Invalid Characters in other key header fields", () =>
			{
				header.JPH_IsShippingLineEntry = false;
				header.JPH_MasterBillNumber = "testMBOL1";
				bill2.JPB_BillNumber = "testBill2";
				var testVessel = Factory.NewWithValidTestData<RefVessel>();
				testVessel.RV_RadioCallSign = "CALL[]";
				header.JPH_RL_NKLoading = "JPAB[";
				header.JPH_RL_NKDischarge = "JPAB[";
				header.JPH_LoadingPortSuffix = "[";
				header.JPH_CarrierCode = "SCA[";
				header.JPH_Voyage = "VOY[";
				header.JPH_VesselName = testVessel.RV_Code;
				messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
				messageSendingAction.Validation.ValidateJPM_MasterBillOfLadingNumber();
				AssertEquals(5, messageSendingAction.Notifications.Count());
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharMessageForSending(header.JPH_RL_NKLoadingInfo.HumanReadableName));
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharMessageForSending(header.JPH_LoadingPortSuffixInfo.HumanReadableName));
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharMessageForSending(header.JPH_CarrierCodeInfo.HumanReadableName));
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharMessageForSending(header.JPH_RadioCallSignInfo.HumanReadableName));
				AssertHasError(messageSendingAction.JPM_MasterBillOfLadingNumberInfo, ValidationConstants.Shared.InvalidNACCSCharMessageForSending(header.JPH_VoyageInfo.HumanReadableName));
			});
		}
	}
}
