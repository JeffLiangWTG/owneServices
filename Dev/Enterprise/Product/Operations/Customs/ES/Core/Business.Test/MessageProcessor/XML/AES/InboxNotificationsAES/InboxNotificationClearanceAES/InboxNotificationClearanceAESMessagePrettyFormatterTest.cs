using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteExporV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationClearanceAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationClearanceAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", new DateTime(2021, 02, 20, 05, 50, 30), "ABCDEFGHIJKLMNOP", "D", "DS");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CLEVEX - Clearance Information</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationClearanceAESMessagePrettyFormatter(new ComunicaLevanteExporV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, "3AG5G6SSCJJ93NML", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestReleaseDate()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), "3AG5G6SSCJJ93NML", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Release date when date and csvCode are in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Release date NOT included when date is in the response but csvCode is not", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestExitTypeData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDirectExitTypeText = "<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>";
			var expectedIndirectExitTypeText = "<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Type NOT included if it's not in the response", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "D", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Direct Exit Type", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, "I", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Indirect Exit Type", expectedIndirectExitTypeText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDEStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
			var expectedDSStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			var expectedPLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>";
			var expectedAWStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			var expectedSAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>SA - Effective Exit</td></tr></table>";
			var expectedPAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, "DE");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, "DS");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DS Status", expectedDSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, "PL");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PL Status", expectedPLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, "AW");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, "SA");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test SA Status", expectedSAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, DateTime.Now, ZString.Empty, ZString.Empty, "PA");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PA Status", expectedPAStatusText, messageInterpretationText);
			});
		}

		ComunicaLevanteExporV1Sal SetResponseData(DateTime receivedDate, ZString mrn, DateTime csvClearanceDate, ZString csvCode, ZString directFlag, ZString status)
		{
			var response = new ComunicaLevanteExporV1Sal();
			response.PreparationDateAndTime = receivedDate;
			response.DatosComunicacion = new DatosComunicacionType()
			{
				Mrn = mrn,
				FechaLevante = csvClearanceDate,
				CsvLevanteExportacion = csvCode,
				FlagDirectaIndirecta = directFlag,
				EstadoAes = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaLevanteExporV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationClearanceAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
