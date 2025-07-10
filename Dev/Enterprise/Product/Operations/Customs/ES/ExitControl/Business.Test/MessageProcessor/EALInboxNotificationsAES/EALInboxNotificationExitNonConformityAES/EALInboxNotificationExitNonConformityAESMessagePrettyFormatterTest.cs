using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeSalidaV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class EALInboxNotificationExitNonConformityAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new EALInboxNotificationExitNonConformityAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", new DateTime(2020, 10, 21, 10, 50, 30), "Comments About Declaration", "NL");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CDISSA - Exit Stopped, Non-Conformity</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Non-Conformity Date:</td><td>&nbsp;&nbsp;</td><td>21-10-2020</td></tr></table>" +
				"<table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Comments About Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not cleared</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new EALInboxNotificationExitNonConformityAESMessagePrettyFormatter(new ComunicaDisconformeSalidaV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestRemarksData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Comments About Declaration</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Remarks data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, "Comments About Declaration", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Remarks data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedSTStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>ST - Stop at Exit</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedSTStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "ST");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test ST Status", expectedSTStatusText, messageInterpretationText);
			});
		}

		ComunicaDisconformeSalidaV1Sal SetResponseData(DateTime acceptanceDate, ZString mrn, DateTime nonConformityDate, ZString remarks, ZString status)
		{
			var response = new ComunicaDisconformeSalidaV1Sal();
			response.PreparationDateAndTime = acceptanceDate;
			response.DatosComunicacion = new DatosComunicacionDisconformeSalidaType()
			{
				Mrn = mrn,
				FechaDisconformeSalida = nonConformityDate,
				OtrasCosasAInformar = remarks,
				EstadoAes = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaDisconformeSalidaV1Sal response)
		{
			var messagePrettyFormatter = new EALInboxNotificationExitNonConformityAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
