using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitSummaryMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMessageType()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var messageSendingAction = new ExitSummaryMessageSendingAction(exitDetail);
			ValidationTestHelper.AssertInvalidCodeMessageError(messageSendingAction.MessageTypeInfo, "XX", ExitSummaryMessageTypeList.Codes.Anticipation);
		}
	}
}
