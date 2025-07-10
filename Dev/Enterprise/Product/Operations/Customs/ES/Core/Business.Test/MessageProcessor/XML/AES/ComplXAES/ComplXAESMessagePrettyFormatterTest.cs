using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515X_v514.CC515XV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ComplXAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new ComplXAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData("22ES000101100148B8", "5XP9J5VAV2KCV2VR", "DE", "D");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100148B8</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td>Complementary</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5XP9J5VAV2KCV2VR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>" +
				"<table border=\"0\"><tr><td>Complementary In Term</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, (string)messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cc515Xv1Sal();
			declarationResponse.ControlRespuesta = new Cc515Xv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC515X/ExportOperation/LRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC515X/ExportOperation/LRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new ComplXAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC515X/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>14</td><td>/CC515X/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new Cc515Xv1Sal();
			declarationResponse.ControlRespuesta = new Cc515Xv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError("18", "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515XXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515XXV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(ZString.Empty, "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new ComplXAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515XXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515XXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestComplementaryData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDComplementaryText = "<table border=\"0\"><tr><td>Complementary In Term</td></tr></table>";
			var expectedFComplementaryText = "<table border=\"0\"><tr><td>Complementary Out Term</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Complementary NOT included if it's not in the response", expectedDComplementaryText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "D");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test D Complementary", expectedDComplementaryText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "F");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test F Complementary", expectedFComplementaryText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAWStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			var expectedDEStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
			var expectedDSStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			var expectedPLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "AW", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "DE", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "DS", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DS Status", expectedDSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PL", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PL Status", expectedPLStatusText, messageInterpretationText);
			});
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

		Cc515Xv1Sal SetResponseData(ZString mrn, ZString declarationCSV, ZString status, ZString complementary)
		{
			var response = new Cc515Xv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType515X()
			{
				Mrn = mrn,
				CsvDeclaracionElectronica = declarationCSV,
				EstadoAes = status,
				ComplementariaFueraPlazo = complementary
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cc515Xv1Sal response)
		{
			var messagePrettyFormatter = new ComplXAESMessagePrettyFormatter(response);
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
	}
}
