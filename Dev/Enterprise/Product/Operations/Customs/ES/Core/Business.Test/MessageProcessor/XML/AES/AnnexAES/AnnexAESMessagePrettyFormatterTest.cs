using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCDOCC_v514.CCDOCCV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class AnnexAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new AnnexAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var doc1 = SetDocument("reference1", "EDQGYLR8BHGKCVR9");
			var doc2 = SetDocument("reference2", "3AG5G6SSCJJ93NML");
			var doc3 = SetDocument("reference3", "AAAAAAAAAAAAAAAA");
			var doc4 = SetDocument("reference4", "BBBBBBBBBBBBBBBB");

			var declarationResponse = SetResponseData("21ES00999912345678", "ABCDEFGHIJKLMNOP", "DS", new Collection<CsvDocumentosDoc> { doc1, doc2, doc3, doc4 });
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>reference1</td><td>EDQGYLR8BHGKCVR9</td></tr>" +
				"<tr><td>reference2</td><td>3AG5G6SSCJJ93NML</td></tr>" +
				"<tr><td>reference3</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
				"<tr><td>reference4</td><td>BBBBBBBBBBBBBBBB</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Ccdoccv1Sal();
			declarationResponse.ControlRespuesta = new Ccdoccv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC511C/ExportOperation/LRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC511C/ExportOperation/LRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new AnnexAESMessagePrettyFormatter(declarationResponse);

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
			var declarationResponse = new Ccdoccv1Sal();
			declarationResponse.ControlRespuesta = new Ccdoccv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError("18", "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(ZString.Empty, "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new AnnexAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null);
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

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "DE", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "DS", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DS Status", expectedDSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PL", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PL Status", expectedPLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "AW", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "SA", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test SA Status", expectedSAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PA", null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PA Status", expectedPAStatusText, messageInterpretationText);
			});
		}

		Ccdoccv1Sal SetResponseData(ZString mrn, ZString declarationCSV, ZString status, Collection<CsvDocumentosDoc> docs)
		{
			var response = new Ccdoccv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaTypeDoc()
			{
				Mrn = mrn,
				CsvDeclaracionElectronica = declarationCSV,
				EstadoAes = status,
				CsvDocumentos = docs
			};
			return response;
		}

		CsvDocumentosDoc SetDocument(ZString refernce, ZString csvCode)
		{
			var doc = new CsvDocumentosDoc()
			{
				NumeroReferencia = refernce,
				CsvDocumentoDigitalizado = csvCode,
			};
			return doc;
		}

		ZString GetAcceptedInterpretationText(Ccdoccv1Sal response)
		{
			var messagePrettyFormatter = new AnnexAESMessagePrettyFormatter(response);
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
