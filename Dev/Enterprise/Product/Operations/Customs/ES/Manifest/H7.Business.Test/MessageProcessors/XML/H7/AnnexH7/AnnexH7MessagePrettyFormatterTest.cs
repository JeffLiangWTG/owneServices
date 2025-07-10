using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.TDV1;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	class AnnexH7MessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AnnexH7MessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var expectedInterpretationWithoutAdditionalInfo =
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>0000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Operación Correcta</td></tr></table>";

			var expectedAdditionalInfo =
				"<H4>Additional Information</H4>" +
				"<table border=\"0\"><tr><td>Document CSV ID:</td><td>&nbsp;&nbsp;</td><td>SMPU4X96QKZZTZG9</td></tr></table>" +
				"<table border=\"0\"><tr><td>Clearance Request:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";

			CombineAssertions(() =>
			{
				var response = CreateResponseData(csvId: "VRMNB3HSMAFTYUDR", csvDoc: "SMPU4X96QKZZTZG9", clearanceRequest: "S");
				AssertEquals("Expected message interpretation with additional information", expectedInterpretationWithoutAdditionalInfo + expectedAdditionalInfo, GetMessageDetailsAccepted(response));

				response = CreateResponseData(csvId: "VRMNB3HSMAFTYUDR");
				AssertEquals("Expected message interpretation without additional information", expectedInterpretationWithoutAdditionalInfo, GetMessageDetailsAccepted(response));
			});
		}

		public void TestCreateMessageDetailsAccepted_CsvId()
		{
			var expectedInterpretation = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>";

			CombineAssertions(() =>
			{
				var response = CreateResponseData();
				AssertNotContains("CSV ID is NOT included", "<td>CSV ID:</td>", GetMessageDetailsAccepted(response));

				response = CreateResponseData(csvId: "VRMNB3HSMAFTYUDR");
				AssertContains("CSV ID is included", expectedInterpretation, GetMessageDetailsAccepted(response));
			});
		}

		public void TestCreateMessageDetailsAccepted_AdditionalInformation()
		{
			var expectedInterpretation = "<H4>Additional Information</H4>";

			CombineAssertions(() =>
			{
				var response = CreateResponseData();
				AssertNotContains("Additional Information is NOT included when NO InformacionAdicionalRespuesta", expectedInterpretation, GetMessageDetailsAccepted(response));

				response = CreateResponseData(csvDoc: "SMPU4X96QKZZTZG9");
				AssertContains("Additional Information is included when there is CSVdelDocumentoEnviado", expectedInterpretation, GetMessageDetailsAccepted(response));

				response = CreateResponseData(clearanceRequest: "S");
				AssertContains("Additional Information is included when there is SolicitudDespacho", expectedInterpretation, GetMessageDetailsAccepted(response));

				response = CreateResponseData(csvDoc: "", clearanceRequest: "");
				AssertNotContains("Additional Information is NOT included when Valor is empty", expectedInterpretation, GetMessageDetailsAccepted(response));
			});
		}

		public void TestCreateMessageDetailsAccepted_DocumentCsvId()
		{
			var expectedInterpretation = "<table border=\"0\"><tr><td>Document CSV ID:</td><td>&nbsp;&nbsp;</td><td>SMPU4X96QKZZTZG9</td></tr></table>";

			CombineAssertions(() =>
			{
				var response = CreateResponseData();
				AssertNotContains("Document CSV ID is NOT included", "<td>Document CSV ID:</td>", GetMessageDetailsAccepted(response));

				response = CreateResponseData(csvDoc: "SMPU4X96QKZZTZG9");
				AssertContains("Document CSV ID is included", expectedInterpretation, GetMessageDetailsAccepted(response));
			});
		}

		public void TestCreateMessageDetailsAccepted_ClearanceRequest()
		{
			var expectedInterpretation = "<table border=\"0\"><tr><td>Clearance Request:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";

			CombineAssertions(() =>
			{
				var response = CreateResponseData();
				AssertNotContains("Clearance Request is NOT included", "<td>Clearance Request:</td>", GetMessageDetailsAccepted(response));

				response = CreateResponseData(clearanceRequest: "S");
				AssertContains("Clearance Request is included", expectedInterpretation, GetMessageDetailsAccepted(response));
			});
		}

		public void TestCreateMessageDetailsRejected()
		{
			var expectedInterpretation =
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>NNNN</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Error</td></tr></table>";

			var response = CreateResponseData(responseCode: RejectedResponseCode, responseDescription: "Error");
			var messagePrettyFormatter = new AnnexH7MessagePrettyFormatter(response);

			AssertEquals(expectedInterpretation, messagePrettyFormatter.CreateMessageDetailsAccepted());
		}

		ZString GetMessageDetailsAccepted(EnvioDeDocumentosV1Sal response)
		{
			var messagePrettyFormatter = new AnnexH7MessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		EnvioDeDocumentosV1Sal CreateResponseData(string responseCode = AcceptedResponseCode, string responseDescription = "Operación Correcta", string csvId = "", string csvDoc = null, string clearanceRequest = null)
		{
			var response = new EnvioDeDocumentosV1Sal();
			response.CodigoRespuesta = responseCode;
			response.DescripcionRespuesta = responseDescription;
			response.CsVdeDeclaracionElectronica = csvId;

			if (csvDoc != null || clearanceRequest != null)
			{
				var additionalResponseInfos = new Collection<InfoAdicionalTd>();

				if (csvDoc != null)
				{
					var additionalResponseInfoCSVDoc = new InfoAdicionalTd()
					{
						NombreEtiqueta = "CSVdelDocumentoEnviado",
						Valor = csvDoc
					};

					additionalResponseInfos.Add(additionalResponseInfoCSVDoc);
				}

				if (clearanceRequest != null)
				{
					var additionalResponseInfoRequestDispatch = new InfoAdicionalTd()
					{
						NombreEtiqueta = "SolicitudDespacho",
						Valor = clearanceRequest
					};

					additionalResponseInfos.Add(additionalResponseInfoRequestDispatch);
				}

				response.InformacionAdicionalRespuesta = additionalResponseInfos;
			}

			return response;
		}

		const string AcceptedResponseCode = "0000";
		const string RejectedResponseCode = "NNNN";
	}
}
