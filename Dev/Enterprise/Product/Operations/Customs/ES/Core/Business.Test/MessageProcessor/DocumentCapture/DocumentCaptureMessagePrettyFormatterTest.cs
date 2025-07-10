using System;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DocumentCaptureMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new DocumentCaptureMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var fileName = "fileNameText";
			var mockeResponse = new AttachedDocument();
			mockeResponse.FileName = fileName;

			var messagePrettyFormatter = new DocumentCaptureMessagePrettyFormatter(mockeResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals("<H3>Document " + fileName + " received and saved on eDocs.</H3>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var mockTestHelper = new Mock<AttachedDocument>();
			var messagePrettyFormatter = new DocumentCaptureMessagePrettyFormatter(mockTestHelper.Object);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>EDI Message response processing for requested document failed.</H3>", messageInterpretationText);
		}
	}
}
