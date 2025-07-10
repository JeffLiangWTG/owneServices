using System;
using CargoWise.Customs.ES.MessageDefinitions.Xhub.Products.Customs;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CustomsServiceErrorMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new CustomsServiceErrorMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var mockError = new Mock<CommonCustomsServiceError>();

			var messagePrettyFormatter = new CustomsServiceErrorMessagePrettyFormatter(mockError.Object);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals(ZString.Empty, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var mockError = new CommonCustomsServiceError();
			mockError.ErrorType = "400";
			mockError.ErrorDescription = "Unknown service/operation pair: 'XXXXXX222323323'/'T2LReceptionV1'.";

			var messagePrettyFormatter = new CustomsServiceErrorMessagePrettyFormatter(mockError);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>400</td><td>Unknown service/operation pair: 'XXXXXX222323323'/'T2LReceptionV1'.</td></tr>" +
						"</table>", messageInterpretationText);
		}
	}
}
