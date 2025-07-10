using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using ResponseTd = CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Sal.ResponseTd;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class QueryH7MessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new QueryH7MessagePrettyFormatter(null, null));
			AssertNoExceptionThrown(() => new QueryH7MessagePrettyFormatter(new ConsultaH7V1Sal(), null));
		}

		public void TestCreateMessageDetailsAccepted_NullMrnInfoForBill()
		{
			var expectedInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>";
			var (response, _) = CreateResponseData();
			ZString messageInterpretationText = string.Empty;
			AssertNoExceptionThrown(() => messageInterpretationText = new QueryH7MessagePrettyFormatter(response, null).CreateMessageDetailsAccepted());

			AssertEquals(expectedInterpretationText, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_MessageId()
		{
			var (response, _) = CreateResponseData();
			response.Message.CorrelationIdentifier = "20250117170000000000";

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, null);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedMessageIdInterpretation = "<table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>";
			AssertContains(expectedMessageIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_DeclarationStatus()
		{
			var expectedMapping = new Dictionary<string, string>
			{
				{ "PR", "PR: Pre-declaration completed with/without Customs control." },
				{ "PD", "PD: Declaration pending clearance." },
				{ "DE", "DE: Declaration processed (only 3rd assumption)." },
				{ "LE", "LE: Declaration with release (if prior payment has been made with third assumption or for IOSS, SA assumptions)." },
				{ "AN", "AN: Annulled." },
				{ "H1", "H1: Pending High Value H1 declaration." },
				{ "CP", "CP: SA declaration with payment letter." },
				{ "NL", "NL: No Release." },
				{ "DV", "DV: Returned." },
				{ "IN", "IN: Invalidated by return confirmation in a declaration (EXS/ETD)." },
				{ "U1", "U1: Final status of the H7 after being declared in an H1 and having been cleared, the H1, with clearance approval." },
				{ "AD", "AD: Abandonment/Destruction." },
				{ "A1", "A1: H7 Low Value declared in H1 High Value H1 declaration." },
				{ "RA", "RA: The H7 has had its re-exportation canceled and is pending confirmation with an EXS/ETD in annulled status." },
			};

			var expectedTextTemplate = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>{0}</td></tr></table>";

			var (response, mrnInfo) = CreateResponseData();
			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);

			CombineAssertions(() =>
			{
				foreach (var mapping in expectedMapping)
				{
					mrnInfo.StatusDeclaration = mapping.Key;
					var expectedText = string.Format(expectedTextTemplate, mapping.Value);
					var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
					AssertContains(mapping.Key, expectedText, messageInterpretationText);
				}
			});
		}

		public void TestCreateMessageDetailsAccepted_PresentationDateTime()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.PresentationDate = "20250116";
			mrnInfo.PresentationTime = "180000";

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedPresentationDateTimeInterpretation = "<table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>";
			AssertContains(expectedPresentationDateTimeInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_AcceptanceDateTime()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.DeclarationAcceptanceDate = "20250115";
			mrnInfo.DeclarationAcceptanceTime = "190000";

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedAcceptanceDateTimeInterpretation = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>";
			AssertContains(expectedAcceptanceDateTimeInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_Mrn()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.Mrn = "23ES123456A0000011";

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedMrnInterpretation = "<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>";
			AssertContains(expectedMrnInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_CsvId()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.EdeclarationCsvId = "XIV2468101211111";
			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedCsvIdInterpretation = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>";
			AssertContains(expectedCsvIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_DocumentationRequired()
		{
			var (response, mrnInfo) = CreateResponseData();
			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);

			CombineAssertions(() =>
			{
				mrnInfo.DocumentationRequired = DocumentationRequiredS;
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				var expectedDocumentRequiredInterpretation = "<table border=\"0\"><tr><td>Documentation Required:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";
				AssertContains("Documentation Required S", expectedDocumentRequiredInterpretation, messageInterpretationText);

				mrnInfo.DocumentationRequired = DocumentationRequiredN;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertNotContains("Documentation Required N", messageInterpretationText);

				mrnInfo.DocumentationRequired = string.Empty;
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

			var (response, mrnInfo) = CreateResponseData();
			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);

			CombineAssertions(() =>
			{
				mrnInfo.RiskAnalysisResultCode = RiskAnalysisResultCodeL;
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code L", expectedRiskLText, messageInterpretationText);

				mrnInfo.RiskAnalysisResultCode = MessageFunctionCodeList.Codes.GreenCircuitText;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code V", expectedRiskVText, messageInterpretationText);

				mrnInfo.RiskAnalysisResultCode = MessageFunctionCodeList.Codes.OrangeCircuitText;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code N", expectedRiskNText, messageInterpretationText);

				mrnInfo.RiskAnalysisResultCode = MessageFunctionCodeList.Codes.RedCircuitText;
				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertContains("Risk Analysis Result Code R", expectedRiskRText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAccepted_ReleaseDateTime()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.ReleaseDate = "20250117";
			mrnInfo.ReleaseTime = "200000";

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedReleaseDateTimeInterpretation = "<table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>";
			AssertContains(expectedReleaseDateTimeInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_ReleaseCsvId()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.ReleaseCsvId = "XIV1357911222222";

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedReleaseCsvIdInterpretation = "<table border=\"0\"><tr><td>Release CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV1357911222222</td></tr></table>";
			AssertContains(expectedReleaseCsvIdInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAccepted_Taxes()
		{
			var (response, mrnInfo) = CreateResponseData();
			mrnInfo.CalculationOfTaxes = new CalculationOfTaxesTd();
			mrnInfo.CalculationOfTaxes.CurrencyCode = "EUR";
			mrnInfo.CalculationOfTaxes.Tax = new Collection<Tax2Td>
					{
						new Tax2Td { TaxType = "IVA", PayableTaxAmount = 100, TaxBase = 200 },
						new Tax2Td { TaxType = "IGIC", PayableTaxAmount = 5000, TaxBase = 10000 }
					};

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, mrnInfo);
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
			var (response, _) = CreateResponseData(responseCode: RejectedResponseCode);
			response.Error = new Collection<ErrorTd>
					{
						new ErrorTd { ErrorType = FunctionalErrorType, ErrorCode = 101, ErrorReason = "Error 1", ErrorPointerGoodsItem = 201, ErrorPointerDataElement = 301, ErrorPointerTag = "/ExportOperation/item1",  OriginalAttributeValue = "20ES0099" },
						new ErrorTd { ErrorType = SyntaxErrorType, ErrorCode = 102, ErrorReason = "Error 2", ErrorPointerGoodsItem = 202, ErrorPointerDataElement = 302, ErrorPointerTag = "/ExportOperation/item2",  OriginalAttributeValue = "20ES0088" }
					};

			var messagePrettyFormatter = new QueryH7MessagePrettyFormatter(response, null);
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

		(ConsultaH7V1Sal, MrnInfoTd) CreateResponseData(string responseCode = AcceptedResponseCode)
		{
			var response = new ConsultaH7V1Sal();
			response.Message = new MessageSalTd();
			response.Response = new ResponseTd();
			response.Response.ResponseCode = responseCode;
			var mrnInfo = new MrnInfoTd();
			response.Response.MrnInfo.Add(mrnInfo);

			return (response, mrnInfo);
		}

		const string AcceptedResponseCode = "A";
		const string RejectedResponseCode = "R";

		const string FunctionalErrorType = "F";
		const string SyntaxErrorType = "N";
		const string DocumentationRequiredS = "S";
		const string DocumentationRequiredN = "N";
		const string RiskAnalysisResultCodeL = "L";
	}
}
