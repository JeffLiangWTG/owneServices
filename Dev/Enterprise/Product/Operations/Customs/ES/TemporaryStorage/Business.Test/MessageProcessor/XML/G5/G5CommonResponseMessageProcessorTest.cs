using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using CusGuaranteeHeader = Enterprise.Customs.ES.Business.CusGuaranteeHeader;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

public abstract class G5CommonResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESCommonResponseMessageProcessorTest<TResponse, TemporaryStorageHeader, TResponseProvider>
	where TResponse : G5CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage>
	where TResponseProvider : class, ICommonServiceSegment, IResponseCode
	where TPrettyMessage : IMessagePrettyFormatter
{
	public void TestProcessMessageWrongXML()
	{
		SetSentInterchange(temporaryStorageHeader, InterchangeID);
		var message = CreateNewEDIMessage(ApplicationReference, WrongXMLTestFile, InterchangeID);

		ProcessMessageForTest(message);

		AssertG5Declaration(message, emStatus: EDIMessage.Status.Failed, messageSubType: "AAA", expectedMessageStatus: EDIMessage.Status.Failed, expectedMrnNumber: MRNCodeWhenRejectedOrError, messageNum: ZString.Empty);
		AssertLoggerMessagesWhenProcessMessageWrongXML();
	}

	public void TestMessageProcessingError()
	{
		var message = CreateNewEDIMessage("AAAAAAAA", ZString.Empty, InterchangeID, false);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertLoggerMessagesWhenMessageProcessingError();
		});
	}

	public void TestMessageProcessingErrorNoReference()
	{
		var message = CreateNewEDIMessage(ZString.Empty, ZString.Empty, InterchangeID, false);

		Factory.Save();

		ProcessMessageForTest(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertContains("Log has error", "No Application Reference found for message", logger.UserLogStrings[0]);
		});
	}

	public void TestProcessRejectedMessage()
	{
		AddMessageProcessAndAssertResult_RejectedMessage();
	}

	public void TestProcessMessageAcceptedWithSegmentIdTooLong()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown("No exception expected since SementId is cut to 35 chars", () => ProcessMessageForTest(message));

			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = ApplicationReference;

		message = CreateNewEDIMessage(ApplicationReference, "Message text", InterchangeID, false);
		message.EM_LinkedObject = temporaryStorageHeader;

		sentInterchange = SetSentInterchange(temporaryStorageHeader, InterchangeID);
	}
	protected TemporaryStorageHeader temporaryStorageHeader;
	protected TestEdiMessage message;
	protected EDIInterchange sentInterchange;

	void AssertLoggerMessagesWhenProcessMessageWrongXML()
	{
		AssertContains("logger", "Unable to read message text from message", GetAllConcatenatedUserLogStrings());
	}

	void AssertLoggerMessagesWhenMessageProcessingError()
	{
		AssertContains("Log has error", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
	}

	protected void AssertG5Declaration(TestEdiMessage message, string messageSubType, ZDateTime? expectedMrnIssueDate = null, string expectedCircuit = "", string expectedMrnNumber = "", string expectedDsdtMRN = "", string expectedCsvClearance = "", string expectedMessageInterpretation = "", string emStatus = "RCV", string expectedCustomsStatus = "", string expectedMessageStatus = "", string messageNum = "")
	{
		CombineAssertions(() => {
			if (!expectedMrnNumber.IsNullOrEmpty())
			{
				var cusEntryNumberMRN = GetCusEntryNumber(temporaryStorageHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, expectedMrnNumber);

				AssertNotNull("cusEntryNumberMRN should not be null", cusEntryNumberMRN);

				if (cusEntryNumberMRN != null)
				{
					AssertEquals("cusEntryNumberMRN.CE_EntryStatus", expectedCircuit, cusEntryNumberMRN.CE_EntryStatus);
					AssertEquals("cusEntryNumberMRN.CE_IssueDate", expectedMrnIssueDate == null ? ZDateTime.Empty : expectedMrnIssueDate, cusEntryNumberMRN.CE_IssueDate);
					AssertEquals("cusEntryNumberMRN.CE_EntryNum", expectedMrnNumber, cusEntryNumberMRN.CE_EntryNum);
				}
			}

			if (!expectedCsvClearance.IsNullOrEmpty())
			{
				var cusEntryNumberClearance = GetCusEntryNumber(temporaryStorageHeader, CusEntryNumberTypes.Spain.ClearanceCSV, expectedCsvClearance);

				AssertNotNull("cusEntryNumberClearance should not be null", cusEntryNumberClearance);

				if (cusEntryNumberClearance != null)
				{
					AssertEquals("cusEntryNumberClearance.CE_EntryNum", expectedCsvClearance, cusEntryNumberClearance.CE_EntryNum);
				}
			}

			if (!expectedDsdtMRN.IsNullOrEmpty())
			{
				var cusEntryNumberSummary = GetCusEntryNumber(temporaryStorageHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, expectedDsdtMRN);

				AssertNotNull("cusEntryNumberSummary should not be null", cusEntryNumberSummary);

				if (cusEntryNumberSummary != null)
				{
					AssertEquals("cusEntryNumberSummary.CE_EntryNum", expectedDsdtMRN, cusEntryNumberSummary.CE_EntryNum);
				}
			}

			AssertEquals("Message Status", expectedMessageStatus, temporaryStorageHeader.AMA_MessageStatus);
			AssertEquals("Customs Status", expectedCustomsStatus, temporaryStorageHeader.CustomsStatus);

			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, messageNum: messageNum, messageSubType: messageSubType, loggerDesc: ZString.Empty);
		});
	}

	protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
	{
		if (businessObject is TemporaryStorageHeader temporaryStorageHeader)
		{
			AssertEquals("MessageStatus", "FAL", temporaryStorageHeader.AMA_MessageStatus);
		}

		var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
		AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
		AssertContains("logger exception", "There is an error in XML document ", concatenatedUserLogStrings);
		AssertContains("EM_MessageInterpretation",
				string.Format("<H3>Processor Failure</H3><br>" +
				"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
				"<H4>Exception: There is an error in XML document ", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
				, message.EM_MessageInterpretation);
	}

	protected void AssertGuaranteeTransactionForExpedition(ZString transactionReference, ZDateTime acceptanceDate, ZDecimal tranValue, ZString mrn, string commentSuffix = "")
	{
		var transactions = temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == "TRA");
		var transaction = transactions.FirstOrDefault();
		var expectedComment = "G5X LRNTest. MRN: " + mrn + commentSuffix;
		CombineAssertions(() =>
		{
			AssertEquals("Transaction is created", 1, transactions.Count());
			AssertEquals("Transaction Date", acceptanceDate, transaction.CPL_TransactionDate);
			AssertEquals("Transaction Type", "TRA", transaction.CPL_TransactionType);
			AssertEquals("Reference", transactionReference, transaction.CPL_Reference);
			AssertEquals("Value", tranValue, transaction.CPL_TranValue);
			AssertEquals("Comment", expectedComment, transaction.CPL_Comment);
			AssertEquals("Status", "CON", transaction.CPL_TransactionStatus);
		});
	}

	protected void AddMessageProcessAndAssertResult_RejectedMessage()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetRejectedTestFile(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Error Type</strong></td><td><strong>Place</strong></td><td><strong>Goods Item Number</strong></td></tr>" +
			"<tr><td>600</td><td>El mensaje es erroneo.</td><td>F</td><td>GoodsItem</td><td>1</td></tr>" +
			"<tr><td>900</td><td>El mensaje enviado no cumple el esquema.</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";

		AssertG5Declaration(message, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, messageNum: MessageNum, messageSubType: "REJ");
	}

	protected CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header, ZString entryType, ZString entryNum)
	{
		var queryCEN = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNum);
		queryCEN.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		queryCEN.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK);
		return Factory.LoadTop1<CusEntryNumber>(queryCEN);
	}

	protected void SetUpGuaranteeData(bool shouldAddOBLTransaction = true, bool shouldAddCONTransaction = true, string conTransactionReference = "", decimal conTransactionTranValue = -20m, bool shouldHaveSameDeclarantAndConsignee = true, bool shouldHaveLocationInPremises = true, decimal bondAmount = 30m)
	{
		base.SetUp();

		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		temporaryStorageHeader.Bills.RemoveAndDeleteAll();
		var masterBill = temporaryStorageHeader.Bills.AddNew();
		masterBill.ABL_BolType = "BOL";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = orgHeader.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_BondNumber = "Test1";
		tsGuarantee.PW_Override = true;
		tsGuarantee.PW_BondAmount = bondAmount;

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = shouldHaveLocationInPremises ? "TSLoc" : "LOCATION";

		var orgHDeclarant = Factory.New<OrgHeader>();
		orgHDeclarant.OH_Code = "AAA";
		var orgADeclarant = orgHDeclarant.Addresses.AddNew();
		orgADeclarant.Address1 = "Declarant address";
		temporaryStorageHeader.AMA_OA_Declarant = orgADeclarant.PK;
		masterBill.ABL_OA_Consignee = shouldHaveSameDeclarantAndConsignee ? orgADeclarant.PK : orgAddress.PK;

		if (shouldAddOBLTransaction)
		{
			guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
		}

		if (shouldAddCONTransaction)
		{
			guarantee.AddTransaction(conTransactionReference, "SUM REFERENCE", ZString.Empty, ZString.Empty, conTransactionTranValue, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
		}
	}

	protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var tmpStorageHeader = Factory.New<TemporaryStorageHeader>();
			tmpStorageHeader.AMA_JobReference = "TEST";

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(tmpStorageHeader, interchangeID);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationReference = "TEST";
			var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			responseInterchange.ContainedMessages.Add(message);
			return message;
		}
	}

	protected virtual ZString MRNCodeWhenRejectedOrError => ZString.Empty;

	protected const string MessageNum = "G52024207";

	protected abstract string GetRejectedTestFile();

	protected abstract string GetAcceptanceTestFileWithLongSegmentId();
}
