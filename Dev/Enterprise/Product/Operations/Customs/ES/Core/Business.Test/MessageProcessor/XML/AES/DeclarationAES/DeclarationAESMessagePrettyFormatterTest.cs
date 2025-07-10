using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515C_v514.CC515CV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DeclarationAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new DeclarationAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", "V", "R", new DateTime(2021, 02, 20, 05, 50, 30), "ABCDEFGHIJKLMNOP", "3AG5G6SSCJJ93NML", "D", "DS");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cc515Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc515Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC511C/ExportOperation/LRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC511C/ExportOperation/LRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new DeclarationAESMessagePrettyFormatter(declarationResponse);

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
			var declarationResponse = new Cc515Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc515Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError("18", "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(ZString.Empty, "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new DeclarationAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCircuitAEATData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "V", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "N", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "R", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCircuitATCData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit ATC NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, "V", null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit ATC", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, "N", null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit ATC", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, "R", null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit ATC", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestReleaseDate()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Release date NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, "3AG5G6SSCJJ93NML", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Release date when date and csvCode are in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Release date NOT included when date is in the response but csvCode is not", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, "3AG5G6SSCJJ93NML", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestExitTypeData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDirectExitTypeText = "<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>";
			var expectedIndirectExitTypeText = "<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Type NOT included if it's not in the response", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, "D", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Direct Exit Type", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, "I", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Indirect Exit Type", expectedIndirectExitTypeText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDEStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
			var expectedDSStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			var expectedPLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>";
			var expectedAWStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			var expectedSAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>SA - Effective Exit</td></tr></table>";
			var expectedPAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, "DE");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, "DS");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DS Status", expectedDSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, "PL");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PL Status", expectedPLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, "AW");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, "SA");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test SA Status", expectedSAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, "PA");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PA Status", expectedPAStatusText, messageInterpretationText);
			});
		}

		Cc515Cv1Sal SetResponseData(DateTime acceptanceDate, ZString mrn, ZString circuitAEAT, ZString circuitATC, DateTime? csvClearanceDate, ZString declarationCSV, ZString csvCode, ZString directFlag, ZString status)
		{
			var response = new Cc515Cv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType515C()
			{
				Mrn = mrn,
				FechaAdmision = acceptanceDate,
				CircuitoAeat = circuitAEAT,
				CircuitoAtc = circuitATC,
				FechaLevante = csvClearanceDate,
				CsvDeclaracionElectronica = declarationCSV,
				CsvLevanteExportacion = csvCode,
				FlagDirectaIndirecta = directFlag,
				EstadoAes = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cc515Cv1Sal response)
		{
			var messagePrettyFormatter = new DeclarationAESMessagePrettyFormatter(response);
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
