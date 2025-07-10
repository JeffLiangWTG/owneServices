using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageMessageFunctionTest : TestCaseWithFactory
	{
		public void TestMembersForFunction()
		{
			var presentationNotificationMessageFunction = new PresentationNotificationMessageFunction();
			AssertEquals(TemporaryStorageMessageTypeList.Codes.PresentationNotification, presentationNotificationMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, presentationNotificationMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, presentationNotificationMessageFunction.SentCustomsStatus);

			var preLodgedTSDMessageFunction = new PreLodgedTSDMessageFunction();
			AssertEquals(TemporaryStorageMessageTypeList.Codes.PreLodgedTSD, preLodgedTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, preLodgedTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, preLodgedTSDMessageFunction.SentCustomsStatus);

			var combinedTSDMessageFunction = new CombinedTSDMessageFunction();
			AssertEquals(TemporaryStorageMessageTypeList.Codes.CombinedTSD, combinedTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, combinedTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, combinedTSDMessageFunction.SentCustomsStatus);

			var transferNotificationTSDMessageFunction = new TransferNotificationTSDMessageFunction();
			AssertEquals(TemporaryStorageMessageTypeList.Codes.TransferNotificationTSD, transferNotificationTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, transferNotificationTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, transferNotificationTSDMessageFunction.SentCustomsStatus);

			var deconsolidationNotificationTSDMessageFunction = new DeconsolidationNotificationTSDMessageFunction();
			AssertEquals(TemporaryStorageMessageTypeList.Codes.DeconsolidationNotificationTSD, deconsolidationNotificationTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, deconsolidationNotificationTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, deconsolidationNotificationTSDMessageFunction.SentCustomsStatus);

			var amendmentRequestTSDMessageFunction = new AmendmentRequestTSDMessageFunction();
			AssertEquals(TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, amendmentRequestTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, amendmentRequestTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, amendmentRequestTSDMessageFunction.SentCustomsStatus);

			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD;
			sendingObject.VOCReason = "ABCD";
			var invalidationRequestTSDMessageFunction = new InvalidationRequestTSDMessageFunction(sendingObject);
			AssertEquals("ABCD", invalidationRequestTSDMessageFunction.VOCReason);
			AssertEquals(TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD, invalidationRequestTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, invalidationRequestTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, invalidationRequestTSDMessageFunction.SentCustomsStatus);
		}
	}
}
