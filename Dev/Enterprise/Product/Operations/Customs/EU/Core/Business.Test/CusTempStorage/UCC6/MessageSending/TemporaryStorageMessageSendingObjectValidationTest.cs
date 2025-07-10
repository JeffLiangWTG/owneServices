using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestCheckVOCReason()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
			sendingObject.VOCReason = ZString.Empty;
			AssertNoError("VOCReason is not mandatory when Message type is not InvalidationRequestTSD", sendingObject.VOCReasonInfo, "Please enter an Invalidation reason.");

			sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD;
			sendingObject.VOCReason = ZString.Empty;
			AssertHasError("VOCReason is mandatory when Message type is InvalidationRequestTSD", sendingObject.VOCReasonInfo, "Please enter an Invalidation reason.");

			sendingObject.VOCReason = "ABCD";
			AssertNoError("VOCReason is mandatory when Message type is InvalidationRequestTSD", sendingObject.VOCReasonInfo, "Please enter an Invalidation reason.");
		}

		public void TestCheckMessageType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.Validation.ValidateMessageType();
			AssertHasError(sendingObject.MessageTypeInfo, "Please enter a Message Type.");

			sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
			AssertNoError(sendingObject.MessageTypeInfo, "Please enter a Message Type.");
			AssertHasError(sendingObject.MessageTypeInfo, "Enter a valid Message Type.");

			header.AMA_MessageStatus = PNTSMessageStatusList.Codes.TechnicalFailure;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			sendingObject.Validation.ValidateMessageType();
			AssertNoError(sendingObject.MessageTypeInfo, "Enter a valid Message Type.");
		}

		public void TestCheckEffectiveDate()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.Date = ZDate.Invalid;
			AssertHasError(sendingObject.DateInfo, "Enter a valid Date.");

			sendingObject.Date = ZDate.Today;
			AssertNoError(sendingObject.DateInfo, "Enter a valid Date.");
		}
	}
}
