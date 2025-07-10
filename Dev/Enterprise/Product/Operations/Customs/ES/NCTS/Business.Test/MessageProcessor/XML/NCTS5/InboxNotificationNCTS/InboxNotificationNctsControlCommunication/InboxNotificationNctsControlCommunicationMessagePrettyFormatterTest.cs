using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaControlesParV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class InboxNotificationNctsControlCommunicationMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationNctsControlCommunicationMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), mrn: "21ES00999912345678", status: Ncts5TransitStatusList.Codes.PendingDispatch, circuit: MessageFunctionCodeList.Codes.OrangeCircuitText);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CCOTPA - Controls Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationNctsControlCommunicationMessagePrettyFormatter(new ComunicaControlesParV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedStatusText = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, status: Ncts5TransitStatusList.Codes.PendingDispatch);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PD Status", expectedStatusText, messageInterpretationText);
			});
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetResponseData(DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, circuit: MessageFunctionCodeList.Codes.GreenCircuitText);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, circuit: MessageFunctionCodeList.Codes.OrangeCircuitText);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, circuit: MessageFunctionCodeList.Codes.RedCircuitText);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		ComunicaControlesParV1Sal SetResponseData(DateTime acceptanceDate, string mrn = "", string status = "", string circuit = "")
		{
			var response = new ComunicaControlesParV1Sal();
			response.PreparationDateAndTime = acceptanceDate;
			response.DatosComunicacion = new DatosComunicacionControlesParType()
			{
				Mrn = mrn,
				CircuitoExpedicion = circuit,
				Estado = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaControlesParV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationNctsControlCommunicationMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
