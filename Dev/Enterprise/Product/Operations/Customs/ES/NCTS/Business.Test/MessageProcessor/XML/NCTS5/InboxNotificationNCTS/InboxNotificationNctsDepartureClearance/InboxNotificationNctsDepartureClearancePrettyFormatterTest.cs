using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaLevanteParV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class InboxNotificationNctsDepartureClearancePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationNctsDepartureClearanceMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), mrn: "21ES00999912345678", csvClearanceDate: new DateTime(2021, 02, 21, 05, 50, 30), csvCode: "ABCDEFGHIJKLMNOP", status: Ncts5TransitStatusList.Codes.Dispatch, circuit: MessageFunctionCodeList.Codes.GreenCircuitText, limitDate: new DateTime(2021, 02, 22, 05, 50, 30));
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CLETPA - Clearance Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>22-02-2021</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>21-02-2021</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationNctsDepartureClearanceMessagePrettyFormatter(new ComunicaLevanteParV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDEStatusText = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, status: Ncts5TransitStatusList.Codes.Dispatch);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);
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

		ComunicaLevanteParV1Sal SetResponseData(DateTime acceptanceDate, string mrn = "", DateTime? csvClearanceDate = null, string csvCode = "", string status = "", string circuit = "", DateTime? limitDate = null)
		{
			var response = new ComunicaLevanteParV1Sal();
			response.PreparationDateAndTime = acceptanceDate;
			response.DatosComunicacion = new DatosComunicacionLevanteParType()
			{
				Mrn = mrn,
				CircuitoExpedicion = circuit,
				CsVdeDat = csvCode,
				Estado = status
			};
			if (csvClearanceDate != null)
			{
				response.DatosComunicacion.FechaLevante = (DateTime)csvClearanceDate;
			}
			if (limitDate != null)
			{
				response.DatosComunicacion.FechaLimiteLlegada = (DateTime)limitDate;
			}
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaLevanteParV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationNctsDepartureClearanceMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
