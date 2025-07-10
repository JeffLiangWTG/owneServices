using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteSalidaV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class EALInboxNotificationExitClearanceAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new EALInboxNotificationExitClearanceAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", new DateTime(2021, 02, 20, 05, 50, 30), "ABCDEFGHIJKLMNOP", "DS");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CLEVSA - Exit Clearance Information</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new EALInboxNotificationExitClearanceAESMessagePrettyFormatter(new ComunicaLevanteSalidaV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedPSStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
			var expectedREStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received and Under Control</td></tr></table>";
			var expectedRQStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RQ - Pending to UE</td></tr></table>";
			var expectedRZStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RZ - Rejected deviation</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedPSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "PS");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PS Status", expectedPSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "RE");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RE Status", expectedREStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "RQ");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RQ Status", expectedRQStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "RZ");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RZ Status", expectedRZStatusText, messageInterpretationText);
			});
		}

		ComunicaLevanteSalidaV1Sal SetResponseData(DateTime acceptanceDate, ZString mrn, DateTime csvClearanceDate, ZString csvCode, ZString status)
		{
			var response = new ComunicaLevanteSalidaV1Sal();
			response.PreparationDateAndTime = acceptanceDate;
			response.DatosComunicacion = new DatosComunicacionLevanteSalidaType()
			{
				Mrn = mrn,
				FechaLevanteSalida = csvClearanceDate,
				CsvLevanteSalida = csvCode,
				EstadoAes = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaLevanteSalidaV1Sal response)
		{
			var messagePrettyFormatter = new EALInboxNotificationExitClearanceAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
