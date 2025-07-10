using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC514C_v514.CC514CV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CancelAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new CancelAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData("ABCDEFGHIJKLMNOP", "AW", new DateTime(2020, 10, 20, 10, 50, 30));
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Cancellation</H3>" +
				"<br><table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cc514Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc514Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC511C/ExportOperation/LRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC511C/ExportOperation/LRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new CancelAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC511C/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>14</td><td>/CC511C/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new Cc514Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc514Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError("18", "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(ZString.Empty, "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new CancelAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCancellationDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<br><table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Cancellation date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, new DateTime(2020, 10, 20, 10, 50, 30));
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Cancellation date", expectedText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAWStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			var expectedCAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>";
			var expectedIVStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "AW", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "CA", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CA Status", expectedCAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "IV", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IV Status", expectedIVStatusText, messageInterpretationText);
			});
		}

		Cc514Cv1Sal SetResponseData(ZString declarationCSV, ZString status, DateTime? invalidationDate)
		{
			var response = new Cc514Cv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType514C()
			{
				CsvDeclaracionElectronica = declarationCSV,
				EstadoAes = status,
				FechaInvalidacion = invalidationDate
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cc514Cv1Sal response)
		{
			var messagePrettyFormatter = new CancelAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		FunctionalErrorType SetFunctionalError(ZString errorCode, ZString errorDescription, ZString errorLocation, ZString wrongValue, ZString reason)
		{
			var error = new FunctionalErrorType();
			error.ErrorCode = errorCode;
			error.ErrorDescription = errorDescription;
			error.ErrorPointer = errorLocation;
			error.OriginalAttributeValue = wrongValue;
			error.ErrorReason = reason;
			return error;
		}

		XmlErrorType SetXMLError(ZString errorCode, ZString errorLineNumber, ZString errorColumnNumber, ZString errorText, ZString wrongValue, ZString location)
		{
			var error = new XmlErrorType();
			error.ErrorCode = errorCode;
			error.ErrorLineNumber = errorLineNumber;
			error.ErrorColumnNumber = errorColumnNumber;
			error.ErrorText = errorText;
			error.OriginalAttributeValue = wrongValue;
			error.ErrorPointer = location;
			return error;
		}
	}
}
