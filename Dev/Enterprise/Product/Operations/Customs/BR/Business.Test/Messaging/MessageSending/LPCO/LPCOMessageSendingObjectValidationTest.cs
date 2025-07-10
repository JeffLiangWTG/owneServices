using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LPCOMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckShouldSend()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "PRT";

			var header = Factory.NewWithValidTestData<CusLPCOHeader>();
			var lpco = new LPCOMessageSendingObjectParent(header);

			var messageSending = new LPCOMessageSendingObject(lpco);

			header.CPH_Number = "1234";
			messageSending.ShouldSend = true;
			AssertHasErrorContaining(messageSending.ShouldSendInfo, "Original message cannot be resent because this LPCO already contains a LPCO Number.");

			header.CPH_Number = ZString.Empty;
			messageSending.Validation.ValidateShouldSend();
			AssertNoErrorContaining(messageSending.ShouldSendInfo, "Original message cannot be resent because this LPCO already contains a LPCO Number.");

			header.CPH_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			messageSending.Validation.ValidateShouldSend();
			AssertHasErrorContaining(messageSending.ShouldSendInfo, "There is still a message waiting for response. Please wait until the message is responded.");

			header.CPH_MessageStatus = BRMessageStatusList.Codes.Accepted;
			messageSending.Validation.ValidateShouldSend();
			AssertNoErrorContaining(messageSending.ShouldSendInfo, "There is still a message waiting for response. Please wait until the message is responded.");
		}
	}
}
