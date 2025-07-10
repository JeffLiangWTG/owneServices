using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using TdRespuesta = CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Sal.TdRespuesta;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CancelDVDMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new CancelDVDMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData("PNS6NA3WMAUC4J8W");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Cancellation</H3>" +
				"<table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

			AssertEquals("Expected Accepted cancellation message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new AnulaPdcVinculacionV1Sal();

			var error1 = SetError("6322", "El identificador del operador no es un EORI valido ", "CABECERA.ED_3_16_NumIdentifDepositante", ZString.Empty, ZString.Empty);
			var error2 = SetError("1234", "Error Description", "ErrorTag", "1", "Wrong Value");
			declarationResponse.Errores = new Collection<TdError> { error1, error2 };

			var messagePrettyFormatter = new CancelDVDMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>6322</td><td>El identificador del operador no es un EORI valido </td><td>CABECERA.ED_3_16_NumIdentifDepositante</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr></table>", messageInterpretationText);
		}

		public void TestCSVElectronicDeclarationData()
		{
			var declarationResponse = SetResponseData(ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<H3>Accepted Cancellation</H3><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Electronic Declaration data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData("PNS6NA3WMAUC4J8W");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Electronic Declaration data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		ZString GetAcceptedInterpretationText(AnulaPdcVinculacionV1Sal response)
		{
			var messagePrettyFormatter = new CancelDVDMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		AnulaPdcVinculacionV1Sal SetResponseData(ZString declarationCSV)
		{
			var response = new AnulaPdcVinculacionV1Sal();
			response.Respuesta = new TdRespuesta()
			{
				CsvDeclaracionElectronica = declarationCSV
			};
			return response;
		}

		TdError SetError(ZString errorCode, ZString errorDescription, ZString errorLocation, ZString goodsItem, ZString wrongValue)
		{
			var error = new TdError();
			error.CodigoError = errorCode;
			error.DescripcionError = errorDescription;
			error.EtiquetaConError = errorLocation;
			error.NumeroOrdenPartidaConError = goodsItem;
			error.ValorErroneo = wrongValue;
			return error;
		}
	}
}
