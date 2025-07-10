using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNMessageSendingObjectValidation))]
	sealed class PBNMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMessageType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(MessageSendingObject.MessageTypeInfo, "XXX", PBNMessageTypes.Codes.CreatePBN);
		}

		PBNMessageSendingObject MessageSendingObject => messageSendingObject ??= new PBNMessageSendingObject(Factory.New<AsycudaManifestHeader>());
		PBNMessageSendingObject messageSendingObject;
	}
}
