using System;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Messaging.Business.HttpXmlMessaging.Testing
{
	[TestedType(typeof(HttpXmlEDIMessage))]
	sealed class HttpXmlEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNumberFountainNumbersAndFillInPlaceHoldersDoesNothingForHttpXmlEDIMessage()
		{
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
			message.EM_ReceiveTransmit = Direction.Transmit;
			ErrorReporter.Instance.Clear();
			Factory.Save();
			AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestMessageNumberIsPopulatedForInboundMessages()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			// Act
			Factory.Save();

			// Assert
			Assert(!message.EM_MessageNum.IsEmpty);
		}

		public void TestShouldShowInterpretation()
		{
			var message = Factory.New<HttpXmlEDIMessage>();
			AssertEquals("ShouldShowInterpretation", false, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberIsPopulatedForReceivedMessages()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;

			// Act
			Factory.Save();

			// Assert
			Assert(!message.EM_MessageNum.IsEmpty);
		}

		public void TestMessageNumberFountainIsLossy()
		{
			Assert("Number fountain for EM_MessageNumber column should be lossy", !Env.NumberFountains.HttpXmlEDIMessageNumber.EnsureConsistentSequence);
		}

		public void TestMessageNumberFountainIsResetBetweenTests()
		{
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
			Factory.Save();
			AssertEquals("Message number starts at 1", "00000000000000000001", message.EM_MessageNum);

			FountainTestListener.Instance.AfterEachTest(DateTime.Now);
			var newMessage = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
			Factory.Save();
			AssertEquals("Message number reset to 1 after test", "00000000000000000001", newMessage.EM_MessageNum);
		}

		public void TestMessageNumberIsPopulatedForReceivedMessages_OnlyOnce()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;

			// Act
			Factory.Save();
			var original = message.EM_MessageNum;
			Factory.Save();

			// Assert
			AssertEquals(original, message.EM_MessageNum);
		}

		public void TestHttpXmlEdiMessage_EM_MessageText_Access_Get_ProducesErrorReport()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();

			// Act
			var text = message.EM_MessageText;

			// Assert
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(AutoEDIMessage.Schema.EM_MessageText + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestHttpXmlEdiMessage_EM_MessageText_Access_Set_ProducesErrorReport()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();

			// Act
			message.EM_MessageText = "A";

			// Assert
			AssertEquals(5, ErrorReporter.TotalErrorCount);
			AssertEquals(AutoEDIMessage.Schema.EM_MessageText + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestHttpXmlEdiMessage_EM_MessageInterpretation_Access_Get_ProducesErrorReport()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();

			// Act
			var text = message.EM_MessageInterpretation;

			// Assert
			AssertEquals(2, ErrorReporter.TotalErrorCount);
			AssertEquals(AutoEDIMessage.Schema.EM_MessageText + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestHttpXmlEdiMessage_EM_MessageInterpretation_Access_Set_ProducesErrorReport()
		{
			// Arrange
			var message = Factory.NewWithValidTestData<HttpXmlEDIMessage>();

			// Act
			message.EM_MessageInterpretation = "A";

			// Assert
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("EM_MessageInterpretation should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
