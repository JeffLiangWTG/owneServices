using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DeclaComplemVinculV2Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.TD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXDVDMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new ComplXDVDMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData("6", "Description", "22ES009999D04136R3", "PNS6NA3WMAUC4J8W");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(6) Description</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

			AssertEquals("Expected Accepted message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new DeclaComplemVinculV2Sal();

			var error1 = SetError("6322", "El identificador del operador no es un EORI valido ", "CABECERA.ED_3_16_NumIdentifDepositante", null, ZString.Empty);
			var error2 = SetError("1234", "Error Description", "ErrorTag", 1, "Wrong Value");
			declarationResponse.Error = new Collection<ErrorTd> { error1, error2 };

			var messagePrettyFormatter = new ComplXDVDMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>6322</td><td>El identificador del operador no es un EORI valido </td><td>CABECERA.ED_3_16_NumIdentifDepositante</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr></table>", messageInterpretationText);
		}

		public void TestDescriptionData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(6) Description</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Description data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData("6", "Description", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Description data", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "Description", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Description data NOT included if code is not in response", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestMRNData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test MRN data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "22ES009999D04136R3", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test MRN data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVElectronicDeclarationData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Electronic Declaration data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "PNS6NA3WMAUC4J8W");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Electronic Declaration data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		ZString GetAcceptedInterpretationText(DeclaComplemVinculV2Sal response)
		{
			var messagePrettyFormatter = new ComplXDVDMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		DeclaComplemVinculV2Sal SetResponseData(ZString declarationCode, ZString declarationDescription, ZString mrn, ZString declarationCSV)
		{
			var response = new DeclaComplemVinculV2Sal()
			{
				CodigoOperacionRegistrada = declarationCode,
				DescripOperacionRegistrada = declarationDescription,
				NumeroReferenciaDvd = mrn,
				CsVdeDeclaracionElectronica = declarationCSV
			};
			return response;
		}

		ErrorTd SetError(ZString errorCode, ZString errorDescription, ZString errorLocation, int? goodsItem, ZString wrongValue)
		{
			var error = new ErrorTd();
			error.CodigoError = errorCode;
			error.DescripcionError = errorDescription;
			error.EtiquetaConError = errorLocation;
			error.NumeroOrdenPartidaConError = goodsItem;
			error.ValorErroneo = wrongValue;
			return error;
		}
	}
}
