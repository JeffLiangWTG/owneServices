using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01CONSV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class QueryT2LPOUSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new QueryT2LPOUSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, "V", CSVClearance, "2023-07-06T11:25:49", "2023-12-27T10:07:49", ProofStatus);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register T2L (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeT2l + "</td></tr>" +
				"<tr><td>Register JEC (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeJec + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Request Date:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr>" +
				"<tr><td>Registration Date:</td><td>&nbsp;&nbsp;</td><td>27-12-2023</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Proof Status:</td><td>&nbsp;&nbsp;</td><td>" + ProofStatus + "</td></tr></table>";

			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretationText, messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new Iep01Cons();
			var error1 = SetFunctionalError("14", "/CC511C/ExportOperation/LRN", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "PRLSV00000012-ES89890010F");
			var error2 = SetFunctionalError("17", "/CC511C/ExportOperation/MRN", "No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.", "23ES00999912345678");
			declarationResponse.ErrorData = new Collection<ErrorCon> { error1, error2 };

			var messagePrettyFormatter = new QueryT2LPOUSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC511C/ExportOperation/LRN</td><td>No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>14</td><td>/CC511C/ExportOperation/LRN</td><td>No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>17</td><td>/CC511C/ExportOperation/MRN</td><td>No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.</td><td>23ES00999912345678</td></tr>" +
				"<tr><td>17</td><td>/CC511C/ExportOperation/MRN</td><td>No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.</td><td>23ES00999912345678</td></tr></table>", messageInterpretationText);
		}

		public void TestRegisterT2LDeclaration()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Register T2L (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeT2l + "</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Register T2l Declaration NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Register T2l Declaration when it's in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestRegisterJECDeclaration()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Register JEC (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeJec + "</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Register JEC Declaration NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Register JEC Declaration when it's in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, "V", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, "N", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, "R", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestRequestDate()
		{
			var declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Request Date:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Request Date NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, "2023-07-06T11:25:49", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Request Date when date is in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestRegistrationDate()
		{
			var declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Registration Date:</td><td>&nbsp;&nbsp;</td><td>27-12-2023</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Registration Date NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, "2023-12-27T10:07:49", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Registration Date when date is in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestProofStatus()
		{
			var declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Proof Status:</td><td>&nbsp;&nbsp;</td><td>" + ProofStatus + "</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Proof Status NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(MRNCodeT2l, MRNCodeJec, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ProofStatus);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Proof Status when status is in the response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		Iep01Cons SetAcceptedResponseData(ZString t2lMRN, ZString jecMRN, ZString circuit, ZString csvClearance, ZString requestDate, ZString registrationDate, ZString proofStatus)
		{
			var response = new Iep01Cons();
			response.ProofData = new ProofDataTd()
			{
				Mrnt2L = t2lMRN,
				Mrnjec = jecMRN,
				RiskAnalysisResultCode = circuit,
				Csvt2L = csvClearance,
				RequestDate = requestDate,
				RegistrationDate = registrationDate,
				ProofStatus = proofStatus
			};
			return response;
		}

		ErrorCon SetFunctionalError(ZString errorCode, ZString errorPointer, ZString reason, ZString wrongValue)
		{
			var errorCon = new ErrorCon();
			var error = new ErrorTdEs();
			error.ErrorCode = errorCode;
			error.ErrorPointer = errorPointer;
			error.ErrorReason = reason;
			error.OriginalAttributeValue = wrongValue;
			errorCon.Error.Add(error);
			errorCon.Error.Add(error);
			return errorCon;
		}

		ZString GetAcceptedInterpretationText(Iep01Cons response)
		{
			var messagePrettyFormatter = new QueryT2LPOUSMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		protected const string MRNCodeT2l = "23ES009999L00026M6";
		protected const string MRNCodeJec = "23ES009999M0003573";
		protected const string CSVClearance = "379CJX9DEWWYLP4Z";
		protected readonly ZDateTime RequestDate = new ZDateTime(2023, 07, 06, 11, 25, 49);
		protected readonly ZDateTime RegistrationDate = new ZDateTime(2023, 12, 27, 10, 07, 49);
		protected const string ProofStatus = "Saldado";
	}
}
