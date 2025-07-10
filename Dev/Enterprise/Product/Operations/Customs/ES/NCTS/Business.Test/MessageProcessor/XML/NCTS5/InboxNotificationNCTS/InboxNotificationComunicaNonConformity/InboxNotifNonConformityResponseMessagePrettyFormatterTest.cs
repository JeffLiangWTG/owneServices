using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaDisconformeParV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class InboxNotifNonConformityResponseMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotifNonConformityResponseMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), mrn: MRNCode, remarks: "Remarks", status: "NL");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CDITPA - Non-conformity Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Remarks</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not Cleared</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotifNonConformityResponseMessagePrettyFormatter(new ComunicaDisconformeParV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestRemarks()
		{
			var declarationResponse = SetResponseData(DateTime.Now);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedRemarks = "<tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Remarks</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Remarks", expectedRemarks, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, remarks: "Remarks");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Remarks", expectedRemarks, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var expectedStatusNLText = "<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not Cleared</td></tr></table>";

			CombineAssertions(() =>
			{
				var declarationResponse = SetResponseData(DateTime.Now, status: Ncts5TransitStatusList.Codes.NotCleared);
				var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test NL Status", expectedStatusNLText, messageInterpretationText);
			});
		}

		ComunicaDisconformeParV1Sal SetResponseData(DateTime receivedDate, string mrn = "", string remarks = "", string status = "")
		{
			var response = new ComunicaDisconformeParV1Sal();
			response.PreparationDateAndTime = receivedDate;
			response.DatosComunicacion = new DatosComunicacionDisconformeParType()
			{
				Mrn = mrn,
				OtrasCosasAInformar = remarks,
				Estado = status,
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(ComunicaDisconformeParV1Sal response)
		{
			var messagePrettyFormatter = new InboxNotifNonConformityResponseMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		const string MRNCode = "22ES000101500647K2";
	}
}
