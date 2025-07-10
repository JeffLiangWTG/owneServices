using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeExporV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationNonConformityAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationNonConformityAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", "Comments About Declaration", "NL");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CDISEX - Non-Conformity Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Comments About Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not cleared</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationNonConformityAESMessagePrettyFormatter(new ComunicaDisconformeExporV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestRemarksData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<br><table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Comments About Declaration</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Remarks data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "Comments About Declaration", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Remarks data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		ComunicaDisconformeExporV1Sal SetResponseData(DateTime acceptanceDate, ZString mrn, ZString remarks, ZString status)
		{
			var response = new ComunicaDisconformeExporV1Sal();
			response.PreparationDateAndTime = acceptanceDate;
			response.DatosComunicacion = new DatosComunicacionDisconformeExporType()
			{
				Mrn = mrn,
				OtrasCosasAInformar = remarks,
				EstadoAes = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaDisconformeExporV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationNonConformityAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
