using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public sealed class AsycudaManifestMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestCheckMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			var sendingObject = new AsycudaManifestMessageSendingObject(header);
			sendingObject.MessageType = ZString.Empty;
			sendingObject.Validation.ValidateMessageType();
			AssertHasError(sendingObject.MessageTypeInfo, "Please enter a Message Type.");

			sendingObject.MessageType = ILMessageTypeList.Codes.MAN;
			sendingObject.Validation.ValidateMessageType();
			AssertNoError(sendingObject.MessageTypeInfo, "Please enter a Message Type.");
		}

		public void TestCheckMessageSubType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			var sendingObject = new AsycudaManifestMessageSendingObject(header);
			sendingObject.MessageSubType = ZString.Empty;
			sendingObject.Validation.ValidateMessageSubType();
			AssertHasError("Make sure correct error provided when mandatory Message Sub Type is empty", sendingObject.MessageSubTypeInfo, "Please enter a Message Sub Type.");

			sendingObject.MessageSubType = ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest;
			sendingObject.Validation.ValidateMessageSubType();
			AssertNoError("Make sure no error when mandatory Message Sub Type is not empty", sendingObject.MessageSubTypeInfo, "Please enter a Message Sub Type.");
		}
	}
}
