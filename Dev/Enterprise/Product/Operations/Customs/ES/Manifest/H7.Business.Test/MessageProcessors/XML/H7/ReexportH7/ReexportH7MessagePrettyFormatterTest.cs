using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ReexportacionH7V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	class ReexportH7MessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ReexportH7MessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted_DeclarationType()
		{
			var expectedExexportH7TextForInvalidateH7WithExsEtd = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: Invalidate H7 (Re-Export through an EXS/ETD)</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";
			var messageInterpretationTextForInvalidateH7WithExsEtd = GetMessageInterpretationTextByDeclarationType(InvalidateH7WithExsEtd);

			var expectedExexportH7TextForAnnullReExportExsEtd = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>2: Annulation of a Re-Export through an EXS/ETD </td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";
			var messageInterpretationTextForAnnullReExportExsEtd = GetMessageInterpretationTextByDeclarationType(AnnullReExportExsEtd);

			var expectedExexportH7TextForInvalidateH7WithTransit = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>3: Invalidate H7 (Re-Export through a Transit procedure)</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";
			var messageInterpretationTextForInvalidateH7WithTransit = GetMessageInterpretationTextByDeclarationType(InvalidateH7WithTransit);

			var expectedExexportH7TextForAnnulReExportWithTransit = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>4: Annulation of a Re-Export through a Transit procedure</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";
			var messageInterpretationTextForAnnulReExportWithTransit = GetMessageInterpretationTextByDeclarationType(AnnulReExportWithTransit);

			CombineAssertions(() =>
			{
				AssertEquals("Exexport H7 text for InvalidateH7WithExsEtd", expectedExexportH7TextForInvalidateH7WithExsEtd, messageInterpretationTextForInvalidateH7WithExsEtd);
				AssertEquals("Exexport H7 text for AnnullReExportExsEtd", expectedExexportH7TextForAnnullReExportExsEtd, messageInterpretationTextForAnnullReExportExsEtd);
				AssertEquals("Exexport H7 text for InvalidateH7WithTransit", expectedExexportH7TextForInvalidateH7WithTransit, messageInterpretationTextForInvalidateH7WithTransit);
				AssertEquals("Exexport H7 text for AnnulReExportWithTransit", expectedExexportH7TextForAnnulReExportWithTransit, messageInterpretationTextForAnnulReExportWithTransit);
			});
		}

		ZString GetMessageInterpretationTextByDeclarationType(string declarationType)
		{
			var response = CreateResponseData();
			var messagePrettyFormatter = new ReexportH7MessagePrettyFormatter(response);
			response.Response.OperationCode = declarationType;

			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		public void TestCreateMessageDetailsAccepted_MessageId()
		{
			var response = CreateResponseData();
			response.Message.CorrelationIdentifier = "20250117170000000000";

			var messagePrettyFormatter = new ReexportH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedMessageIdInterpretation = "<table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>";
			AssertContains(expectedMessageIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_CsvId()
		{
			var response = CreateResponseData();
			response.EdeclarationCsvId = "XIV2468101211111";

			var messagePrettyFormatter = new ReexportH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedCsvIdInterpretation = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>";
			AssertContains(expectedCsvIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var response = CreateResponseData(responseCode: RejectedResponseCode);
			response.Error = new Collection<ErrorTd>
			{
				new ErrorTd { ErrorType = FunctionalErrorType, ErrorCode = 101, ErrorReason = "Error 1", ErrorPointerGoodsItem = 201, ErrorPointerDataElement = 301, ErrorPointerTag = "/ExportOperation/item1",  OriginalAttributeValue = "20ES0099" },
				new ErrorTd { ErrorType = SyntaxErrorType, ErrorCode = 102, ErrorReason = "Error 2", ErrorPointerGoodsItem = 202, ErrorPointerDataElement = 302, ErrorPointerTag = "/ExportOperation/item2",  OriginalAttributeValue = "20ES0088" }
			};

			var messagePrettyFormatter = new ReexportH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			var expectedMessageInterpretationText = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>101</td><td>301</td><td>/ExportOperation/item1</td><td>Error 1</td><td>201</td><td>20ES0099</td></tr>" +
				"<tr><td>102</td><td>302</td><td>/ExportOperation/item2</td><td>Error 2</td><td>202</td><td>20ES0088</td></tr>" +
				"</table>";

			AssertEquals(expectedMessageInterpretationText, messageInterpretationText);
		}

		ReexportacionH7V1Sal CreateResponseData(string responseCode = AcceptedResponseCode, string operationCode = "")
		{
			var response = new ReexportacionH7V1Sal();
			response.Message = new MessageSalTd();
			response.Response = new ResponseTd();
			response.Response.ResponseCode = responseCode;
			response.Response.OperationCode = operationCode;

			return response;
		}

		const string AcceptedResponseCode = "A";
		const string RejectedResponseCode = "R";
		const string InvalidateH7WithExsEtd = "0";
		const string AnnullReExportExsEtd = "2";
		const string InvalidateH7WithTransit = "3";
		const string AnnulReExportWithTransit = "4";
		const string FunctionalErrorType = "F";
		const string SyntaxErrorType = "N";
	}
}
