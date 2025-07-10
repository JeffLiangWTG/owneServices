using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.TDV1;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CommonAnnexMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new CommonAnnexMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetAcceptedResponseData("ABCDEFGHIJKLMNOP", "S", "EDQGYLR8BHGKCVR9");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Request Dispatch:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<br><table border=\"0\"><tr><td>CSV Document:</td><td>&nbsp;&nbsp;</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = SetRejectedResponseData("Wrong value");

			var messagePrettyFormatter = new CommonAnnexMessagePrettyFormatter(declarationResponse);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Code:</td><td>&nbsp;&nbsp;</td><td>1021</td></tr></table>" +
				"<table border=\"0\"><tr><td>Error:</td><td>&nbsp;&nbsp;</td><td>Wrong value</td></tr></table>", messageInterpretationText);
		}

		public void TestCSVElectronicDeclaration()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Electronic Declaration NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData("ABCDEFGHIJKLMNOP", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Electronic Declaration when it's in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestRequestDispatch()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Request Dispatch:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Request Dispatch NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, "S", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Request Dispatch when it's in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVDocument()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><H4>CSV Electronic Documents</H4>" +
				"<br><table border=\"0\"><tr><td>CSV Document:</td><td>&nbsp;&nbsp;</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Documents NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, "EDQGYLR8BHGKCVR9");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Documents when it's in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		EnvioDeDocumentosV1Sal SetAcceptedResponseData(ZString declarationCSV, ZString requestDispath, ZString csvDoc)
		{
			var response = new EnvioDeDocumentosV1Sal();
			response.CodigoRespuesta = "0000";
			response.CsVdeDeclaracionElectronica = declarationCSV;
			var additionalResponseInfos = new Collection<InfoAdicionalTd>();
			if (!requestDispath.IsEmpty)
			{
				var additionalResponseInfoRequestDispatch = new InfoAdicionalTd()
				{
					NombreEtiqueta = "SolicitudDespacho",
					Valor = requestDispath
				};
				additionalResponseInfos.Add(additionalResponseInfoRequestDispatch);
			}

			if (!csvDoc.IsEmpty)
			{
				var additionalResponseInfoCSVDoc = new InfoAdicionalTd()
				{
					NombreEtiqueta = "CSVdelDocumentoEnviado",
					Valor = csvDoc
				};
				additionalResponseInfos.Add(additionalResponseInfoCSVDoc);
			}
			response.InformacionAdicionalRespuesta = additionalResponseInfos;
			return response;
		}

		EnvioDeDocumentosV1Sal SetRejectedResponseData(ZString errorDesc)
		{
			var response = new EnvioDeDocumentosV1Sal();
			response.CodigoRespuesta = "1021";
			response.DescripcionRespuesta = errorDesc;
			return response;
		}

		ZString GetAcceptedInterpretationText(EnvioDeDocumentosV1Sal response)
		{
			var messagePrettyFormatter = new CommonAnnexMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
