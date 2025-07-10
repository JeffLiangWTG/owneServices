using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	public class TemporaryStorageMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestCheckMessageType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;

			var sendingObject = new TemporaryStorageMessageSendingObject(header);

			CombineAssertions("When entry status is REG or ACC and message type is T15", () =>
			{
				const string messageError = "TS315 should not be sent when entry status is REG – Registered or ACC – Accepted.";
				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.Declaration;

				header.CustomsStatus = AISEntryStatusList.Codes.Registered;
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, messageError);

				header.CustomsStatus = AISEntryStatusList.Codes.Accepted;
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, messageError);

				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, messageError);
			});

			var ts313MessageError = "TS313 should only be sent when entry status is REG – Registered.";
			CombineAssertions("When entry status is not REG and message type is T13", () =>
			{
				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.Amendment;

				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, ts313MessageError);

				header.CustomsStatus = AISEntryStatusList.Codes.Registered;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, ts313MessageError);
			});

			var ts314MessageError = "TS314 should only be sent when entry status is REG – Registered.";
			CombineAssertions("When entry status is not REG and message type is T14", () =>
			{
				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.Invalidation;

				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, ts314MessageError);

				header.CustomsStatus = AISEntryStatusList.Codes.Registered;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, ts314MessageError);
			});

			var ts332MessageError = "TS332 should only be sent when entry status is REG – Registered.";
			CombineAssertions("When entry status is not REG and message type is T32", () =>
			{
				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.PresentationNotification;

				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, ts332MessageError);

				header.CustomsStatus = AISEntryStatusList.Codes.Registered;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, ts332MessageError);
			});

			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			CombineAssertions("No error report when UCC5", () =>
			{
				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.Amendment;
				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, ts313MessageError);

				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.Invalidation;
				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, ts314MessageError);
				header.CustomsStatus = AISEntryStatusList.Codes.Registered;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, ts314MessageError);

				sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.PresentationNotification;
				header.CustomsStatus = "ABC";
				sendingObject.Validation.ValidateMessageType();
				AssertHasMessageError(sendingObject.MessageTypeInfo, ts332MessageError);
				header.CustomsStatus = AISEntryStatusList.Codes.Registered;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError(sendingObject.MessageTypeInfo, ts332MessageError);
			});
		}
	}
}
