using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class ResponseMessageAbnormalityReporterTest : TestCaseWithFactory
	{
		public void TestReportMessageWithAllTheOriginalErrors()
		{
			originalMessage.EM_SendWithMessageErrors = true;
			dummyObj.AddRowMessageError("NOT GOOD ENOUGH DESCRIPTION");
			dummyObj.AddRowMessageError("EMPTY DATE");
			string messageErrors = dummyObj.Notifications.GetMessageErrors().ToUniqueMessageListString();
			AssertEquals(false, string.IsNullOrEmpty(messageErrors));
			isClearForTesting = true;
			ErrorReporter.Clear();
			dummyObj.MarkAsNeedingValidation();
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(ResponseMessageAbnormalityReporter.ClearedWhenMessageWasSendWithErrorKey, ErrorReporter.LastKeyReported);
			string errorMessage = string.Format("{4}{0}Message Errors:{0}{1}{0}Original Message:{0}{2}{0}Response Message:{0}{3}",
								System.Environment.NewLine,
								messageErrors,
								"Hello World",
								"Goodbye World",
								dummyObj.GetType().FullName);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			reporter.GetMessageOverride = GetMessageDelegate;
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(ResponseMessageAbnormalityReporter.ClearedWhenMessageWasSendWithErrorKey, ErrorReporter.LastKeyReported);
			errorMessage = string.Format("{4}{0}Message Errors:{0}{1}{0}Original Message:{0}{2}{0}Response Message:{0}{3}",
								System.Environment.NewLine,
								messageErrors,
								"Yo Hello World",
								"Yo Goodbye World",
								dummyObj.GetType().FullName);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			isClearForTesting = false;
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			reporter.GetReportKeyOverride = GetReportKeyDelegate;
			isClearForTesting = true;
			ErrorReporter.Clear();
			reporter.GetMessageOverride = GetMessageDelegate;
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(GetReportKeyDelegate(ResponseMessageAbnormalityReporter.ClearedWhenMessageWasSendWithErrorKey, responseMessage), ErrorReporter.LastKeyReported);
			errorMessage = string.Format("{4}{0}Message Errors:{0}{1}{0}Original Message:{0}{2}{0}Response Message:{0}{3}",
								System.Environment.NewLine,
								messageErrors,
								"Yo Hello World",
								"Yo Goodbye World",
								dummyObj.GetType().FullName);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportMessageNotClearedWhenItShouldHaveBeenCleared()
		{
			isClearForTesting = false;
			ErrorReporter.Clear();
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(ResponseMessageAbnormalityReporter.NotClearedWhenMessageWasSendWithoutErrorKey, ErrorReporter.LastKeyReported);
			string errorMessage = string.Format("{3}{0}Original Message:{0}{1}{0}Response Message:{0}{2}",
					System.Environment.NewLine,
								"Hello World",
								"Goodbye World",
								dummyObj.GetType().FullName);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			reporter.GetMessageOverride = GetMessageDelegate;
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(ResponseMessageAbnormalityReporter.NotClearedWhenMessageWasSendWithoutErrorKey, ErrorReporter.LastKeyReported);
			errorMessage = string.Format("{3}{0}Original Message:{0}{1}{0}Response Message:{0}{2}",
					System.Environment.NewLine,
								"Yo Hello World",
								"Yo Goodbye World",
								dummyObj.GetType().FullName);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			isClearForTesting = true;
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			reporter.GetReportKeyOverride = GetReportKeyDelegate;
			isClearForTesting = false;
			ErrorReporter.Clear();
			reporter.GetMessageOverride = GetMessageDelegate;
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(GetReportKeyDelegate(ResponseMessageAbnormalityReporter.NotClearedWhenMessageWasSendWithoutErrorKey, responseMessage), ErrorReporter.LastKeyReported);
			errorMessage = string.Format("{3}{0}Original Message:{0}{1}{0}Response Message:{0}{2}",
					System.Environment.NewLine,
								"Yo Hello World",
								"Yo Goodbye World",
								dummyObj.GetType().FullName);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAbnormalityReportershouldNotReportAnythingWhenThereAreActuallyNoErrorsOnTheObjectRegardlessOfWhatEMSendWithErrorSays()
		{
			originalMessage.EM_SendWithMessageErrors = true;
			string declarationMessageErrors = dummyObj.Notifications.GetMessageErrors().ToUniqueMessageListString();
			AssertEquals(true, string.IsNullOrEmpty(declarationMessageErrors));
			isClearForTesting = true;
			ErrorReporter.Clear();
			reporter.Report(dummyObj, originalMessage, responseMessage, IsClear);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
		}

		ZString GetMessageDelegate(EDIMessage message)
		{
			return "Yo " + message.EM_MessageText;
		}

		ZString GetReportKeyDelegate(ZString originalKey, EDIMessage message)
		{
			return originalKey + " Message Type:" + message.EM_MessageType;
		}

		bool isClearForTesting;
		bool IsClear()
		{
			return isClearForTesting;
		}

		DummyBusinessObject dummyObj;
		EDIMessage originalMessage;
		EDIMessage responseMessage;
		ResponseMessageAbnormalityReporter reporter;

		protected override void SetUp()
		{
			base.SetUp();
			dummyObj = Factory.New<DummyBusinessObject>();
			originalMessage = EDIMessageTestFactory.New(Factory);
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageText = "Hello World";
			responseMessage = EDIMessageTestFactory.New(Factory);
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageText = "Goodbye World";
			originalMessage.EM_LinkedObject = dummyObj;
			responseMessage.EM_LinkedObject = dummyObj;
			reporter = new ResponseMessageAbnormalityReporter();
		}
	}
}
