using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class FeedbackMessageUnpackerTest : TestCaseWithFactory
	{
		public void TestFeedBackMessageCreated()
		{
			var loggerMock = new Mock<ILoggingInformation>();
			var interchange = CreateTestInterchange(ILMessageTypeList.Codes.MAN, ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_Interchange.xml"));
			var unpacker = new FeedbackMessageUnpacker<ILMAN171ResponseMessage>();
			var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
			var correlationId = ZGuid.BrettsGuid.ToString();
			var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, correlationId, null, null, loggerMock.Object);

			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("Single Message created", 1, unpackResult.EdiMessages.Count);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Application Code", EDIMessage.ApplicationCodes.ILCustoms, message.EM_ApplicationCode);
				AssertEquals("Direction", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("Status", Messaging.Integration.EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("Message Type", ILMessageTypeList.Codes.MAN, message.EM_MessageType);
				AssertEquals("Message SubType", "171", message.EM_MessageSubType);
				AssertEquals("Message Number", "237", message.EM_MessageNum);
				AssertEquals("Message Number", message.EM_GB, interchange.EI_GB);
				AssertEquals("Message Text", message.EM_MessageText, messageBody);
				AssertEquals("Message External Reference Number", message.EM_ApplicationReference, correlationId);
				AssertSame("Message linked to interchange", message, interchange.ContainedMessages.Single());
			});
		}

		public void TestMessageNumberTruncation()
		{
			var interchange = CreateTestInterchange(ILMessageTypeList.Codes.MAN, ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_Interchange.xml"));
			interchange.EI_InterchangeNum = "220230019374294293842394872394873298";

			var loggerMock = new Mock<ILoggingInformation>();
			var unpacker = new FeedbackMessageUnpacker<ILMAN171ResponseMessage>();
			var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
			var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, ZString.Empty, null, null, loggerMock.Object);

			var message = unpackResult.EdiMessages.Single();
			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("Message Number Truncated", "22023001937429429384239487239487329", message.EM_MessageNum);
			});
		}

		public void TestMessageHasNText()
		{
			using var emmessageDataActive = SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var loggerMock = new Mock<ILoggingInformation>();
			var interchange = CreateTestInterchange(ILMessageTypeList.Codes.MAN, ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_Interchange.xml"));
			var unpacker = new FeedbackMessageUnpacker<ILMAN171ResponseMessage>();
			var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
			var correlationId = ZGuid.BrettsGuid.ToString();
			var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, correlationId, null, null, loggerMock.Object);

			var message = unpackResult.EdiMessages.Single();

			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				var shouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed = (bool)message.GetType().GetProperty("ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(message);
				Assert("NText should be used", shouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
			});
		}

		ILEDIInterchange CreateTestInterchange(string messageType, string bodyTextEmbeddedResourcePath)
		{
			var interchange = Factory.NewWithValidTestData<ILEDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;
			interchange.EI_InterchangeType = messageType;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = Messaging.Integration.ReceiveTransmitList.Codes.Receive;
			interchange.EI_Status = ILEDIInterchange.Status.Queued;
			interchange.EI_BodyText = new EmbeddedResourceRetriever().GetString(bodyTextEmbeddedResourcePath);
			return interchange;
		}
	}
}
