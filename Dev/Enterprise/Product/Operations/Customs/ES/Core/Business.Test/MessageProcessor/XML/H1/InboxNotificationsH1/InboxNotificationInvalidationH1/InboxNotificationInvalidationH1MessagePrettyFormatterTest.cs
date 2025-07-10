using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ComunicaAnulacionV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ComunicaAnulacion;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing;

public class InboxNotificationInvalidationH1MessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationInvalidationH1MessagePrettyFormatter(null));
	}

	public void TestCreateMessageDetailsAcceptedCompleteDeclaration_InvalidatedByCustoms_WithMRN()
	{
		var declarationResponse = SetResponseData("2022-12-02T13:11:35", ZString.Empty, "24ES009999I001H2R9", true, "2024-12-02T13:11:35", "2024-12-02T13:11:35", "Invalidation Reason Text");
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Preparation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2022, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidation Reason Text</td></tr></table>";
		AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
	}

	public void TestCreateMessageDetailsAcceptedCompleteDeclaration_InvalidatedByCustoms_WithRegistrationNumber()
	{
		var declarationResponse = SetResponseData("2022-12-02T13:11:35", "24ES009999I001H2R9", ZString.Empty, true, "2024-12-02T13:11:35", "2024-12-02T13:11:35", "Invalidation Reason Text");
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Preparation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2022, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (CRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidation Reason Text</td></tr></table>";
		AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
	}

	public void TestCreateMessageDetailsAcceptedCompleteDeclaration_NotInvalidatedByCustoms_WithMRN()
	{
		var declarationResponse = SetResponseData("2022-12-02T13:11:35", ZString.Empty, "24ES009999I001H2R9", false, "2024-12-02T13:11:35", "2024-12-02T13:11:35", "Invalidation Reason Text");
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Preparation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2022, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>NO</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidation Reason Text</td></tr></table>";
		AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
	}

	public void TestCreateMessageDetailsRejected()
	{
		var messagePrettyFormatter = new InboxNotificationInvalidationH1MessagePrettyFormatter(new ComunicaAnulacionV1Sal());
		var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
		AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
	}

	public void TestInvalidationByCustomsData()
	{
		var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty);
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedText = "<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>NO</td></tr></table>";

		CombineAssertions(() =>
		{
			AssertContains("Test Invalidation by Customs included if flag is 0 in the response", expectedText, messageInterpretationText);

			expectedText = "<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>";
			declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, true, ZString.Empty, ZString.Empty, ZString.Empty);
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Invalidation by Customs included if flag is 1 in the response", expectedText, messageInterpretationText);
		});
	}

	public void TestCustomsRegistrationNumberData()
	{
		var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty);
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedText = "<table border=\"0\"><tr><td>Register (CRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>";

		CombineAssertions(() =>
		{
			AssertNotContains("Test Customs Registration Number data NOT included if it's not in the response", expectedText, messageInterpretationText);

			declarationResponse = SetResponseData(ZString.Empty, "24ES009999I001H2R9", ZString.Empty, true, ZString.Empty, ZString.Empty, ZString.Empty);
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Customs Registration Number data", expectedText, messageInterpretationText);
		});
	}

	public void TestMRNData()
	{
		var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty);
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedText = "<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>";

		CombineAssertions(() =>
		{
			AssertNotContains("Test MRN data NOT included if it's not in the response", expectedText, messageInterpretationText);

			declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "24ES009999I001H2R9", true, ZString.Empty, ZString.Empty, ZString.Empty);
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test MRN data", expectedText, messageInterpretationText);
		});
	}

	public void TestInvalidationReasonData()
	{
		var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, false, ZString.Empty, null, ZString.Empty);
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

		var expectedAcceptanceText = "<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidation Reason Text</td></tr></table>";

		CombineAssertions(() =>
		{
			AssertNotContains("Test Invalidation Reason data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

			declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, false, ZString.Empty, null, "Invalidation Reason Text");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Invalidation Reason data", expectedAcceptanceText, messageInterpretationText);
		});
	}

	ComunicaAnulacionV1Sal SetResponseData(ZString preparationDate, ZString customsRegistrationNumber, ZString mrn, ZBool invalidatedByCustoms, ZString invalidationDate, ZString invalidationRequestedDate, ZString reason)
	{
		var response = new ComunicaAnulacionV1Sal();
		response.Message = new MessageTypeD()
		{
			PreparationDateAndTime = preparationDate
		};
		response.ComunicaAnulacion = new ComunicaAnulacionTypeD()
		{
			ImportOperation = new MCciOperationType23()
			{
				CustomsRegistrationNumber = customsRegistrationNumber,
				Mrn = mrn,
				InvalidationInitiatedByCustoms = invalidatedByCustoms ? "1" : "0",
				InvalidationDecisionDateAndTime = invalidationDate,
				InvalidationRequestDateAndTime = invalidationRequestedDate,
				InvalidationJustification = reason
			}
		};
		
		return response;
	}

	ZString GetAcceptedInterpretationText(ComunicaAnulacionV1Sal response)
	{
		var messagePrettyFormatter = new InboxNotificationInvalidationH1MessagePrettyFormatter(response);
		return messagePrettyFormatter.CreateMessageDetailsAccepted();
	}
}
