using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEJECV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PresentationT2LPOUSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new PresentationT2LPOUSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData("2020-10-20T10:50:30", "21ES00999912345678", "V", "ABCDEFGHIJKLMNOP", "3AG5G6SSCJJ93NML");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new IejecSalType();
			var error1 = SetFunctionalError("14", "/CC511C/ExportOperation/LRN", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "PRLSV00000012-ES89890010F");
			var error2 = SetFunctionalError("17", "/CC511C/ExportOperation/MRN", "No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.", "23ES00999912345678");
			declarationResponse.Error = new Collection<ErrorTdEs> { error1, error2 };

			var messagePrettyFormatter = new PresentationT2LPOUSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC511C/ExportOperation/LRN</td><td>No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>17</td><td>/CC511C/ExportOperation/MRN</td><td>No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.</td><td>23ES00999912345678</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsError_EmptyFields()
		{
			var declarationResponse = new IejecSalType();
			var error1 = SetFunctionalError("14", ZString.Empty, "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", ZString.Empty);
			declarationResponse.Error = new Collection<ErrorTdEs> { error1 };

			var messagePrettyFormatter = new PresentationT2LPOUSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>&nbsp;</td><td>No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "V", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "R", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "3AG5G6SSCJJ93NML");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Release date NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData("2020-10-20T10:50:30", ZString.Empty, "V", ZString.Empty, "3AG5G6SSCJJ93NML");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Release date when date and csvCode are in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData("2020-10-20T10:50:30", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Release date NOT included when date is in the response but csvCode is not", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVElectronicDeclaration()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Electronic Declaration data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "ABCDEFGHIJKLMNOP", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Electronic Declaration data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		IejecSalType SetResponseData(ZString acceptanceDate, ZString mrn, ZString circuit, ZString declarationCSV, ZString csvCode)
		{
			var response = new IejecSalType();
			response.Message = new MessageTd()
			{
				PreparationDateAndTime = acceptanceDate
			};
			response.Mrnjec = mrn;
			response.RiskAnalysisResultCode = circuit;
			response.Csvjec = csvCode;
			response.CsVeDeclaration = declarationCSV;
			return response;
		}

		ZString GetAcceptedInterpretationText(IejecSalType response)
		{
			var messagePrettyFormatter = new PresentationT2LPOUSMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		ErrorTdEs SetFunctionalError(ZString errorCode, ZString errorPointer, ZString reason, ZString wrongValue)
		{
			var error = new ErrorTdEs();
			error.ErrorCode = errorCode;
			error.ErrorPointer = errorPointer;
			error.ErrorReason = reason;
			error.OriginalAttributeValue = wrongValue;
			return error;
		}
	}
}
