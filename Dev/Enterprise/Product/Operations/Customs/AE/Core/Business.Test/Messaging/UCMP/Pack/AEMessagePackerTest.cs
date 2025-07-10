using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AE.Registry;
using Enterprise.Edifact.Generic.V4;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.AEManifest;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(AEMessagePacker))]
sealed class AEMessagePackerTest : UniversalCustomsEDIMessagePackerTest<AEMessagePacker>
{
	public void TestValidMessage_CUSDOC()
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateMessage();
		testMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.DOCSUC;
		SetupExternalControllers(true);

		using (ObjectFactory.Substitute(manifestController))
		{
			messagePacker.Pack(testMessage, interchange, logger);
			AssertEquals("TEST MESSAGE'", interchange.EI_BodyText);
		}
	}

	public void TestUnkownMessageParent_CUSCAR() => CombineAssertions(() =>
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateMessage();
		testMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
		SetupExternalControllers(false);
		using (ObjectFactory.Substitute(manifestController))
		{
			var errorMsg = messagePacker.Pack(testMessage, interchange, logger);
			var expectedErrorText = "Unknown message parent for message 0000001 AEC CAR.";
			AssertEquals("Error Log Text", expectedErrorText, logger.Logs.First(x => x.Type == Integration.LogType.Error).Message);
			AssertEquals("Error Text", expectedErrorText, errorMsg);
		}
	});

	public void TestValidMessage_CUSCAR() => CombineAssertions(() =>
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateMessage();
		testMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
		SetupExternalControllers(true);

		using (ObjectFactory.Substitute(manifestController))
		{
			var errorMsg = messagePacker.Pack(testMessage, interchange, logger);
			AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
			AssertEquals("EI_InterchangeNum", ZString.Empty, interchange.EI_InterchangeNum);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_IsActive", expected: true, interchange.EI_IsActive);
			AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			AssertEquals("EI_To", AEConstants.Messaging.MessageRecipient, interchange.EI_To);
			AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals("EI_ApplicationCode", testMessage.EM_ApplicationCode, interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", testMessage.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals("Success Log Text", $"Message({testMessage.EM_MessageNum}) AEMessagePacker finished successfully.", logger.Logs.First(x => x.Type == Integration.LogType.Information).Message);
			AssertEquals("Packer Output Text", string.Empty, errorMsg);

			var expectedBodyText = @"UNB+UNOB+++<<DATE>>:<TM>+<<AE-INT-NUM>>'
TEST MESSAGE'
UNZ+1+<<AE-INT-NUM>>'";
			AssertEquals("EI_BodyText", expectedBodyText, interchange.EI_BodyText);
		}
	});

	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestPlaceHoldersAssignedOnSave_CUSCAR()
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateMessage();
		testMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;

		SetupExternalControllers(true);
		using (ObjectFactory.Substitute(manifestController))
		{
			messagePacker.Pack(testMessage, interchange, logger);
			interchange.OnSaving();

			AssertEquals("EI_InterchangeNum", "012345B0000001", interchange.EI_InterchangeNum);
			var expectedText = @"UNB+UNOB+++20240701:0123+012345B0000001'
TEST MESSAGE'
UNZ+1+012345B0000001'";
			AssertEquals("Interchange Body Text placeholders are assigned", expectedText, interchange.EI_BodyText);
		}
	}

	public void TestUnkownMessageParent_CONTRL() => CombineAssertions(() =>
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateMessage();
		testMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CONTRL;

		var errorMsg = messagePacker.Pack(testMessage, interchange, logger);

		var expectedErrorText = "Unknown message parent for message 0000001 AEC CTL.";
		AssertEquals("Error Log Text", expectedErrorText, logger.Logs.First(x => x.Type == Integration.LogType.Error).Message);
		AssertEquals("Error Text", expectedErrorText, errorMsg);
	});

	public void TestValidMessage_CONTRL() => CombineAssertions(() =>
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateCONTRLMessage();

		var errorMsg = messagePacker.Pack(testMessage, interchange, logger);
		AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
		AssertEquals("EI_InterchangeNum", ZString.Empty, interchange.EI_InterchangeNum);
		AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("EI_IsActive", expected: true, interchange.EI_IsActive);
		AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
		AssertEquals("EI_To", AEConstants.Messaging.MessageRecipient, interchange.EI_To);
		AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
		AssertEquals("EI_ApplicationCode", testMessage.EM_ApplicationCode, interchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", testMessage.EM_MessageType, interchange.EI_InterchangeType);
		AssertEquals("Success Log Text", $"Message({testMessage.EM_MessageNum}) AEMessagePacker finished successfully.", logger.Logs.First(x => x.Type == Integration.LogType.Information).Message);
		AssertEquals("Packer Output Text", string.Empty, errorMsg);

		var senderId = AECustomsRegistry.Instance.NAICServiceProviderCode.Value;
		var expectedBodyText = @"UNB+UNOB:4::2:02+SERPRID::FORFFID:LOCFFID+UAENAIC+<<DATE>>:<TM>+<<AE-INT-NUM>>++++++0'
TEST MESSAGE'
UNZ+1+<<AE-INT-NUM>>'";
		AssertEquals("EI_BodyText", expectedBodyText, interchange.EI_BodyText);
	});

	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestPlaceHoldersAssignedOnSave_CONTRL() => CombineAssertions(() =>
	{
		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var messagePacker = new AEMessagePacker();
		var testMessage = CreateCONTRLMessage();

		messagePacker.Pack(testMessage, interchange, logger);
		interchange.OnSaving();

		AssertEquals("EI_InterchangeNum", "012345B0000001", interchange.EI_InterchangeNum);
		var expectedText = @"UNB+UNOB:4::2:02+SERPRID::FORFFID:LOCFFID+UAENAIC+20240701:0123+012345B0000001++++++0'
TEST MESSAGE'
UNZ+1+012345B0000001'";
		AssertEquals("Interchange Body Text placeholders are assigned", expectedText, interchange.EI_BodyText);
	});

	protected override string ApplicationCode => EDIMessage.ApplicationCodes.UnitedArabEmirates;

	EDIMessage CreateMessage()
	{
		var message = Factory.New<AEEDIMessage>();
		message.EM_MessageNum = "0000001";
		message.EM_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		message.EM_MessageText = $"TEST MESSAGE'";
		return message;
	}

	EDIMessage CreateCONTRLMessage()
	{
		var message = Factory.New<AEEDIMessage>();
		message.EM_MessageNum = "0000001";
		message.EM_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		message.EM_MessageText = $"TEST MESSAGE'";
		message.EM_MessageType = AEConstants.Messaging.MessageTypes.CONTRL;

		var requestInterchange = Factory.New<EDIInterchange>();
		requestInterchange.EI_BodyText = @"UNB+UNOB:4::2:02+UAENAIC+SERPRID::FORFFID:LOCFFID+20240516:0840+084059B3333333'
UNH+123H456+CUSRES'
UNZ+1+084059B3333333'
";
		var requestMessage = Factory.New<EDIMessage>();
		requestMessage.EM_EI = requestInterchange.PK;

		message.EM_EM_RequestMessage = requestMessage.PK;
		return message;
	}

	void SetupExternalControllers(bool segmentProviderResult)
	{
		var headerSegment = new UNBSegment();
		headerSegment.SyntaxIdentifier.SyntaxIdentifier = "UNOB";
		headerSegment.DateTimeOfPreparation.Date = AEConstants.Messaging.Placeholders.DateOfCreation;
		headerSegment.DateTimeOfPreparation.Time = AEConstants.Messaging.Placeholders.TimeOfCreation;
		headerSegment.InterchangeControlReference = AEConstants.Messaging.Placeholders.InterchangeNumber;

		var mockInterchangeSegmentProvider = new Mock<IInterchangeSegmentProvider>();
		mockInterchangeSegmentProvider.Setup(x => x.TryGetHeaderSegment(It.IsAny<BusinessObject>(), out headerSegment)).Returns(segmentProviderResult);

		var mockManifestController = new Mock<IManifestController>();
		mockManifestController.Setup(x => x.InterchangeSegmentProvider).Returns(mockInterchangeSegmentProvider.Object);

		manifestController = mockManifestController.Object;
	}
	IManifestController manifestController;
}
