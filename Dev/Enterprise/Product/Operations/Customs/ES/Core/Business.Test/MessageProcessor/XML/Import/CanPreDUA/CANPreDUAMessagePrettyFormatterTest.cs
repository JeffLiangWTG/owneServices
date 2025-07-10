using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Sal;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CANPreDUAMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestCreateMessageDetailsAccepted()
		{
			var declarationResponse = new AnulaImportacionV1Sal();

			var messagePrettyFormatter = new CANPreDUAMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals("<H3>Accepted Cancellation</H3>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new AnulaImportacionV1Sal();

			declarationResponse.CodigoRespuesta = "1001";
			declarationResponse.DescripcionRespuesta = "El documento no es valido o no es Predeclaración Incompleta";

			var messagePrettyFormatter = new CANPreDUAMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>Rejected Cancellation</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>1001</td><td>El documento no es valido o no es Predeclaración Incompleta</td></tr>" +
						"</table>", messageInterpretationText);
		}
	}
}

