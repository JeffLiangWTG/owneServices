using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaResulSalidaV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationExitResultAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationExitResultAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", "A1", new DateTime(2022, 10, 20, 10, 50, 30), new DateTime(2022, 09, 20, 10, 50, 30), "SA");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CSALID - Exit Result</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A1 - Satisfied</td></tr></table>" +
				"<table border=\"0\"><tr><td>Effective Exit Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Stop at Exit Date:</td><td>&nbsp;&nbsp;</td><td>20-09-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>SA - Effective Exit</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationExitResultAESMessagePrettyFormatter(new ComunicaResulSalidaV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestExitResultData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, null, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedA1ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A1 - Satisfied</td></tr></table>";
			var expectedA2ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>";
			var expectedA4ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A4 - Satisfied</td></tr></table>";
			var expectedB1ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>B1 - Dissatisfied</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Result NOT included if it's not in the response", expectedA1ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "A1", null, null, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A1 Exit Result", expectedA1ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "A2", null, null, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A2 Exit Result", expectedA2ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "A4", null, null, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A4 Exit Result", expectedA4ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "B1", null, null, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test B1 Exit Result", expectedB1ExitResultText, messageInterpretationText);
			});
		}

		public void TestEffectiveExitDate()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, null, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Effective Exit Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Effective Exit date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, new DateTime(2022, 10, 20, 10, 50, 30), null, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Effective Exit date", expectedText, messageInterpretationText);
			});
		}

		public void TestStopAtExitDate()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, null, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Stop at Exit Date:</td><td>&nbsp;&nbsp;</td><td>20-09-2022</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Stop at Exit date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, new DateTime(2022, 09, 20, 10, 50, 30), ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Stop at Exit date", expectedText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, null, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedSAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>SA - Effective Exit</td></tr></table>";
			var expectedSTStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>ST - Stop at Exit</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedSAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, null, "SA");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test SA Status", expectedSAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, null, null, "ST");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test ST Status", expectedSTStatusText, messageInterpretationText);
			});
		}

		ComunicaResulSalidaV1Sal SetResponseData(DateTime receivedDate, ZString mrn, ZString exitResult, DateTime? effectiveExitDate, DateTime? stopAtExitDate, ZString status)
		{
			var response = new ComunicaResulSalidaV1Sal();
			response.PreparationDateAndTime = receivedDate;
			response.DatosComunicacion = new DatosComunicacionResulSalidaType()
			{
				Mrn = mrn,
				ResultadoSalida = exitResult,
				FechaSalidaEfectiva = effectiveExitDate,
				FechaParadaAduanaSalida = stopAtExitDate,
				EstadoAes = status,
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaResulSalidaV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationExitResultAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}

