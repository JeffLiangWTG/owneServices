using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaInvalidacionV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationInvalidationAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationInvalidationAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData("21ES00999912345678", true, new DateTime(2022, 10, 20, 10, 50, 30), new DateTime(2022, 09, 20, 10, 50, 30), "Invalidation Reason Text", "PI");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAL - Invalidated Declaration</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Invalidation by Customs</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>20-09-2022, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidation Reason Text</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationInvalidationAESMessagePrettyFormatter(new ComunicaInvalidacionV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestInvalidationByCustomsData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<br><table border=\"0\"><tr><td>Invalidation by Customs</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Invalidation by Customs NOT included if flag is 0 in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, true, DateTime.Now, null, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Invalidation by Customs included if flag is 1 in the response", expectedText, messageInterpretationText);
			});
		}

		public void TestInvalidationRequestedDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>20-09-2022, 10:50:30</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Invalidation Requested date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, new DateTime(2022, 09, 20, 10, 50, 30), ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Invalidation Requested date", expectedText, messageInterpretationText);
			});
		}

		public void TestInvalidationReasonData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidation Reason Text</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Invalidation Reason data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, "Invalidation Reason Text", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Invalidation Reason data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedCAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>";
			var expectedPIStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";
			var expectedIVStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedCAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, "CA");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CA Status", expectedCAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, "PI");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PI Status", expectedPIStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, false, DateTime.Now, null, ZString.Empty, "IV");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IV Status", expectedIVStatusText, messageInterpretationText);
			});
		}

		ComunicaInvalidacionV1Sal SetResponseData(ZString mrn, ZBool invalidatedByCustoms, DateTime invalidationDate, DateTime? invalidationRequestedDate, ZString reason, ZString status)
		{
			var response = new ComunicaInvalidacionV1Sal();
			response.DatosComunicacion = new ComunicaInvalidacionType()
			{
				Mrn = mrn,
				InvalidacionIniciadaPorLaAduana = invalidatedByCustoms ? Flag.Item1 : Flag.Item0,
				FechaInvalidacion = invalidationDate,
				FechaHoraSolicitudInvalidacion = invalidationRequestedDate,
				JustificacionDeLaInvalidacion = reason,
				EstadoAes = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaInvalidacionV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationInvalidationAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}

