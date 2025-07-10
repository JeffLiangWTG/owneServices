using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	class DeclarationH7MessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DeclarationH7MessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted_DeclarationType()
		{
			var expectedH7AdmissionText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: H7 admission</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";

			var expectedPreH7PresentationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>1: Pre-H7 presentation</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";

			var response = CreateResponseData();
			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);

			CombineAssertions(() =>
			{
				response.Response.OperationCode = H7AdmissionDeclarationType;
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertEquals("H7 admission", expectedH7AdmissionText, messageInterpretationText);

				response.Response.OperationCode = PreH7DeclarationType;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertEquals("Pre-H7 presentation", expectedPreH7PresentationText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAccepted_PresentationDateTime()
		{
			var response = CreateResponseData();
			response.PresentationDate = "20250116";
			response.PresentationTime = "180000";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedPresentationDateTimeInterpretation = "<table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>";
			AssertContains(expectedPresentationDateTimeInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_AcceptanceDateTime()
		{
			var response = CreateResponseData();
			response.DeclarationAcceptanceDate = "20250115";
			response.DeclarationAcceptanceTime = "190000";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedAcceptanceDateTimeInterpretation = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>";
			AssertContains(expectedAcceptanceDateTimeInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_MessageId()
		{
			var response = CreateResponseData();
			response.Message.CorrelationIdentifier = "20250117170000000000";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedMessageIdInterpretation = "<table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>";
			AssertContains(expectedMessageIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_Mrn()
		{
			var response = CreateResponseData();
			response.Mrn = "23ES123456A0000011";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedMrnInterpretation = "<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>";
			AssertContains(expectedMrnInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_CsvId()
		{
			var response = CreateResponseData();
			response.EdeclarationCsvId = "XIV2468101211111";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedCsvIdInterpretation = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>";
			AssertContains(expectedCsvIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var response = CreateResponseData();
			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);

			CombineAssertions(() =>
			{
				response.DocumentationRequired = DocumentationRequiredS;
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				var expectedDocumentRequiredInterpretation = "<table border=\"0\"><tr><td>Documentation Required:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";
				AssertContains("Documentation Required S", expectedDocumentRequiredInterpretation, messageInterpretationText);

				response.DocumentationRequired = DocumentationRequiredN;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertNotContains("Documentation Required N", messageInterpretationText);

				response.DocumentationRequired = string.Empty;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertNotContains("Documentation Required Empty", messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAccepted_RiskAnalysisResultCode()
		{
			var expectedRiskLText = "<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>L: ENS in latency period</td></tr></table>";
			var expectedRiskVText = "<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>V: <strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedRiskNText = "<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>N: <strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRiskRText = "<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>R: <strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			var response = CreateResponseData();
			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);

			CombineAssertions(() =>
			{
				response.RiskAnalysisResultCode = RiskAnalysisResultCodeL;
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code L", expectedRiskLText, messageInterpretationText);

				response.RiskAnalysisResultCode = MessageFunctionCodeList.Codes.GreenCircuitText;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code V", expectedRiskVText, messageInterpretationText);

				response.RiskAnalysisResultCode = MessageFunctionCodeList.Codes.OrangeCircuitText;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code N", expectedRiskNText, messageInterpretationText);

				response.RiskAnalysisResultCode = MessageFunctionCodeList.Codes.RedCircuitText;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code R", expectedRiskRText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAccepted_ReleaseDateTime()
		{
			var response = CreateResponseData();
			response.ReleaseDate = "20250117";
			response.ReleaseTime = "200000";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedReleaseDateTimeInterpretation = "<table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>";
			AssertContains(expectedReleaseDateTimeInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_ReleaseCsvId()
		{
			var response = CreateResponseData();
			response.ReleaseCsvId = "XIV1357911222222";

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedReleaseCsvIdInterpretation = "<table border=\"0\"><tr><td>Release CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV1357911222222</td></tr></table>";
			AssertContains(expectedReleaseCsvIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_Taxes()
		{
			var response = CreateResponseData();
			response.CalculationOfTaxes = new CalculationOfTaxesTd();
			response.CalculationOfTaxes.CurrencyCode = "EUR";
			response.CalculationOfTaxes.Tax = new Collection<Tax2Td>
			{
				new Tax2Td { TaxType = "IVA", PayableTaxAmount = 100, TaxBase = 200 },
				new Tax2Td { TaxType = "IGIC", PayableTaxAmount = 5000, TaxBase = 10000 }
			};

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedTaxInterpretation = "<br><H2>Taxes and fees data</H2>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>EUR</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IVA</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>100</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>200</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>EUR</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IGIC</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>5000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>10000</td></tr></table>";

			AssertContains(expectedTaxInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var response = CreateResponseData(responseCode: RejectedResponseCode);
			response.Error = new Collection<ErrorTd>
			{
				new ErrorTd { ErrorType = FunctionalErrorType, ErrorCode = 101, ErrorReason = "Error 1", ErrorPointerGoodsItem = 201, ErrorPointerDataElement = 301, ErrorPointerTag = "/ExportOperation/item1",  OriginalAttributeValue = "20ES0099" },
				new ErrorTd { ErrorType = SyntaxErrorType, ErrorCode = 102, ErrorReason = "Error 2", ErrorPointerGoodsItem = 202, ErrorPointerDataElement = 302, ErrorPointerTag = "/ExportOperation/item2",  OriginalAttributeValue = "20ES0088" }
			};

			var messagePrettyFormatter = new DeclarationH7MessagePrettyFormatter(response);
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

		AltaH7V1Sal CreateResponseData(string responseCode = AcceptedResponseCode, string operationCode = "")
		{
			var response = new AltaH7V1Sal();
			response.Message = new MessageSalTd();
			response.Response = new ResponseTd();
			response.Response.ResponseCode = responseCode;
			response.Response.OperationCode = operationCode;

			return response;
		}

		const string AcceptedResponseCode = "A";
		const string RejectedResponseCode = "R";
		const string H7AdmissionDeclarationType = "0";
		const string PreH7DeclarationType = "1";
		const string FunctionalErrorType = "F";
		const string SyntaxErrorType = "N";
		const string DocumentationRequiredS = "S";
		const string DocumentationRequiredN = "N";
		const string RiskAnalysisResultCodeL = "L";
	}
}
