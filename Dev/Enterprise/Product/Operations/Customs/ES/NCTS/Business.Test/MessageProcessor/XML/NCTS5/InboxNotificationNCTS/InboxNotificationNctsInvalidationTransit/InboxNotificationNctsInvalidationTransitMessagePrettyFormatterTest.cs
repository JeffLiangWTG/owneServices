using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaInvaliTranV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class InboxNotificationNctsInvalidationTransitMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationNctsInvalidationTransitMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), new DateTime(2020, 10, 20), mrn: "21ES00999912345678", invalidationStartedByCustoms: Flag.Item1, invalidationReason: "Reason AAAAA", status: "PI");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAT - Invalidation Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Invalidation Started by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Reason AAAAA</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationNctsInvalidationTransitMessagePrettyFormatter(new ComunicaInvaliTranV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestInvalidationStatedByCustoms()
		{
			var declarationResponse = SetResponseData(DateTime.Today, DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedInvalidation1Text = "<tr><td>Invalidation Started by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr>";
			var expectedInvalidation0Text = "<tr><td>Invalidation Started by Customs:</td><td>&nbsp;&nbsp;</td><td>NO</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Invalidation Started by Customs if it's not in the response = 1", expectedInvalidation1Text, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Now, invalidationStartedByCustoms: Flag.Item1);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Invalidation Started by Customs if it's in the response = 1", expectedInvalidation1Text, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, invalidationStartedByCustoms: Flag.Item0);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Invalidation Started by Customs if it's in the response = 0", expectedInvalidation0Text, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(DateTime.Now, DateTime.Now);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedStatusPIText = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";
			var expectedStatusIGText = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IG - Invalidated by Guarantee</td></tr></table>";
			var expectedStatusIVText = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response with PI", expectedStatusPIText, messageInterpretationText);
				AssertNotContains("Test Status NOT included if it's not in the response with IG", expectedStatusIGText, messageInterpretationText);
				AssertNotContains("Test Status NOT included if it's not in the response with IV", expectedStatusIVText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, DateTime.Now, status: Ncts5TransitStatusList.Codes.PreDeclarationInvalidated);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PI Status", expectedStatusPIText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, DateTime.Now, status: Ncts5TransitStatusList.Codes.InvalidatedByGuarantee);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IG Status", expectedStatusIGText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, DateTime.Now, status: Ncts5TransitStatusList.Codes.Invalidated);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IV Status", expectedStatusIVText, messageInterpretationText);
			});
		}

		ComunicaInvaliTranV1Sal SetResponseData(DateTime receivedDate, DateTime invalidationDate, string mrn = "", Flag invalidationStartedByCustoms = Flag.Item0, string invalidationReason = "", string status = "", string circuit = "")
		{
			var response = new ComunicaInvaliTranV1Sal();
			response.PreparationDateAndTime = receivedDate;
			response.DatosComunicacion = new DatosComunicacionInvaliTranType()
			{
				Mrn = mrn,
				InvalidacionIniciadaPorLaAduana = invalidationStartedByCustoms,
				JustificacionDeLaInvalidacion = invalidationReason,
				FechaInvalidacion = invalidationDate,
				Estado = status,
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaInvaliTranV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationNctsInvalidationTransitMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
