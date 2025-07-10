using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public sealed class AsycudaManifestQueryMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestCheckMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			var sendingObject = new AsycudaManifestQueryMessageSendingObject(header);
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

			var sendingObject = new AsycudaManifestQueryMessageSendingObject(header);
			sendingObject.MessageSubType = ZString.Empty;
			sendingObject.Validation.ValidateMessageSubType();
			AssertHasError("Make sure correct error provided when mandatory Message Sub Type is empty", sendingObject.MessageSubTypeInfo, "Please enter a Message Sub Type.");

			sendingObject.MessageSubType = ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest;
			sendingObject.Validation.ValidateMessageSubType();
			AssertNoError("Make sure no error when mandatory Message Sub Type is not empty", sendingObject.MessageSubTypeInfo, "Please enter a Message Sub Type.");
		}

		public void TestCheckManifestNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			var sendingObject = new AsycudaManifestQueryMessageSendingObject(header);
			sendingObject.ManifestNumber = ZString.Empty;
			sendingObject.Validation.ValidateManifestNumber();
			AssertHasMessageError("Make sure correct error mandatory provided when Manifest Number is empty", sendingObject.ManifestNumberInfo, "You have not entered a Manifest Number.");

			sendingObject.ManifestNumber = "123";
			sendingObject.Validation.ValidateManifestNumber();
			AssertNoMessageError("Make sure no error mandatory when  Manifest Number is not empty", sendingObject.ManifestNumberInfo, "You have not entered a Manifest Number.");
		}

		public void TestCheckParentDealNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			var sendingObject = new AsycudaManifestQueryMessageSendingObject(header);
			sendingObject.ParentDealNumber = ZString.Empty;
			sendingObject.Validation.ValidateParentDealNumber();
			AssertHasMessageError("Make sure correct error mandatory provided when Parent Deal Number is empty", sendingObject.ParentDealNumberInfo, "You have not entered a Parent Deal Number.");

			sendingObject.ParentDealNumber = "456";
			sendingObject.Validation.ValidateParentDealNumber();
			AssertNoMessageError("Make sure no error mandatory when Parent Deal Number is not empty", sendingObject.ParentDealNumberInfo, "You have not entered a Parent Deal Number.");
		}
	}
}
