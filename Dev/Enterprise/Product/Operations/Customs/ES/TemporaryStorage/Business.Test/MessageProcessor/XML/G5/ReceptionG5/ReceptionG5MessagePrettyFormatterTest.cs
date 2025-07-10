using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DG;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5RecNotifV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing
{
	class ReceptionG5MessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new ReceptionG5MessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData("20240228151809", "24ES009999Y00097Y8", ChannelDe.V, "24ES00999880000464", "VQVE4H9SDGK2JERP", "5EA9589535D0AF79");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 15:18:09</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00097Y8</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000464</td></tr></table>" +
				"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000046</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>VQVE4H9SDGK2JERP</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.N, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.R, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestDSDTData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999898765432</td></tr></table>" +
				"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984876543</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test DSDT data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, "24ES00999898765432", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DSDT data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, ZString.Empty, ZString.Empty, "5EA9589535D0AF79");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVElectronicDeclaration()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>VQVE4H9SDGK2JERP</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Electronic Declaration data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ChannelDe.V, ZString.Empty, "VQVE4H9SDGK2JERP", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Electronic Declaration data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new G5RecNotifV1Sal();
			var error1 = SetFunctionalError("10", "El mensaje enviado no cumple el esquema.", ErrorTypeDe.F, "GoodItem", 1);
			var error2 = SetFunctionalError("15", "Error en mensaje.", ErrorTypeDe.N, ZString.Empty);
			declarationResponse.Rejected = new RejectedDg()
			{
				ResponseCode = ResponseCodeDe.Re,
				Errors = new System.Collections.ObjectModel.Collection<ErrorDe> { error1, error2 }
			};

			var messagePrettyFormatter = new ReceptionG5MessagePrettyFormatter(declarationResponse);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Error Type</strong></td><td><strong>Place</strong></td><td><strong>Goods Item Number</strong></td></tr>" +
				"<tr><td>10</td><td>El mensaje enviado no cumple el esquema.</td><td>F</td><td>GoodItem</td><td>1</td></tr>" +
				"<tr><td>15</td><td>Error en mensaje.</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		G5RecNotifV1Sal SetResponseData(ZString acceptanceDate, ZString mrn, ChannelDe circuit, ZString dsdtMrn, ZString declarationCSV, ZString csvCode)
		{
			var response = new G5RecNotifV1Sal();
			response.EnvelopeG5 = new EnvelopeG5SalDg()
			{
				PreparationDate = acceptanceDate
			};
			response.Accepted = new AcceptedDg()
			{
				ResponseCode = ResponseCodeDe.Ac,
				Mrn = mrn,
				Channel = circuit,
				TsAtDestination = dsdtMrn,
				NotificationCsv = declarationCSV,
				ReleaseCsv = csvCode
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(G5RecNotifV1Sal response)
		{
			var messagePrettyFormatter = new ReceptionG5MessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		ErrorDe SetFunctionalError(ZString code, ZString reason, ErrorTypeDe errorType, ZString place, int itemNumber = 0)
		{
			var error = new ErrorDe();
			error.Code = code;
			error.Description = reason;
			error.Type = errorType;
			error.Pointer = place;
			if (itemNumber != 0)
			{
				error.GoodsItemNumber = itemNumber;
			}

			return error;
		}
	}
}
