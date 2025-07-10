using System;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputDocumentSearchResultMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "Chartera Output Document Search Result Message Processor";

	protected override string ApplicationCode => CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchResult;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new CharteraOutputDocumentSearchResultMessageProcessor(Logger);

	[TestUtcOffset(3, 0, 0)]
	public void TestProcess() => CombineAssertions(() =>
	{
		var x = new ZDecimal(12345.678);
		AssertEquals("12345.678", x.ToString());
		AssertRequestProcessing(extraAssertion: company =>
		{
			EventsTestHelper.AssertEventNotAdded(company, Events.ManualDocumentSearch, message: "No MDS event for automatic search request");
		});
	});

	public void TestProcessManualSearchResult()
	{
		AssertRequestProcessing(dsrReference: ReferenceCodes.Manual, extraAssertion: company =>
		{
			EventsTestHelper.AssertEventAdded(company, Events.ManualDocumentSearch, "|CNT=3|REQ=2|TYP=Result");
		});
	}

	public void TestProcessDeclareItSearchResult() => CombineAssertions(() =>
	{
		AssertRequestProcessing(expectedEMStatus: EDIMessage.Status.Warning, expectDeliveryTransactions: false, noDsr: true, extraAssertion: company =>
		{
			EventsTestHelper.AssertEventNotAdded(company, Events.ManualDocumentSearch);
		});
	});

	void AssertRequestProcessing(string dsrReference = null, string expectedEMStatus = EDIMessage.Status.ProcessedOK, bool expectDeliveryTransactions = true, Action<GlbCompany> extraAssertion = null, bool noDsr = false)
	{
		const string documentId1 = "docid1";
		const string documentId2 = "docid2";
		const string documentId3 = "docid3";

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);

		CusPollingTransaction docTransaction3 = null;
		uint docTransaction3ChangeNumber = 0;
		var (company, ediMessage, dsrTransaction, midTransaction) = CreateIncomingMessage(new[] { documentId1, documentId2, documentId3 }, dsrReference: dsrReference);
		if (noDsr)
		{
			dsrTransaction.Delete();
		}
		else
		{
			docTransaction3 = testHelper.AddPollingTransaction(company, ApplicationCode, type: TransactionTypes.DocumentDelivery, transactionID: documentId3, status: StatusCodes.AwaitingResponse);
			docTransaction3ChangeNumber = docTransaction3.LastChangeNumber;
		}
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		MessageProcessor.ProcessMessage(ediMessage);

		AssertEquals("EM_LinkedObject", company, ediMessage.EM_LinkedObject);
		AssertEquals("EM_Status", expectedEMStatus, ediMessage.EM_Status);

		AssertEquals("MID: CPT_Status", StatusCodes.Closed, midTransaction.CPT_Status);
		AssertEquals("MID: CPT_StatusTimeUtc", ZDateTime.UtcNow, midTransaction.CPT_StatusTimeUtc);

		if (!noDsr)
		{
			AssertEquals("DSR: CPT_Status", StatusCodes.Closed, dsrTransaction.CPT_Status);
			AssertEquals("DSR: CPT_StatusTimeUtc", ZDateTime.UtcNow, dsrTransaction.CPT_StatusTimeUtc);
		}

		var docTransactions = Factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_Type, TransactionTypes.DocumentDelivery));
		AssertEquals("# of doc transactions", expectDeliveryTransactions ? 3 : 0, docTransactions.Length);
		if (expectDeliveryTransactions)
		{
			AssertDocTransaction("DOC #1", docTransactions.First(x => x.CPT_TransactionID == documentId1));
			AssertDocTransaction("DOC #2", docTransactions.First(x => x.CPT_TransactionID == documentId2));
			AssertEquals("DOC #3 - Existing transaction not touched", docTransaction3ChangeNumber, docTransaction3.LastChangeNumber);
		}

		void AssertDocTransaction(string assertionMessage, CusPollingTransaction transaction)
		{
			AssertEquals($"{assertionMessage} - CPT_ApplicationCode", ApplicationCodes.CHCustomsCharteraOutput, transaction.CPT_ApplicationCode);
			AssertEquals($"{assertionMessage} - CPT_Type", TransactionTypes.DocumentDelivery, transaction.CPT_Type);
			AssertEquals($"{assertionMessage} - CPT_ParentTableCode", GlbCompanySchema.Constants.Prefix, transaction.CPT_ParentTableCode);
			AssertEquals($"{assertionMessage} - CPT_ParentID", company.PK, transaction.CPT_ParentID);
			AssertEquals($"{assertionMessage} - CPT_Status", StatusCodes.New, transaction.CPT_Status);
			AssertEquals($"{assertionMessage} - CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		}

		extraAssertion?.Invoke(company);
	}

	(GlbCompany company, EDIMessage ediMessage, CusPollingTransaction dsrTransaction, CusPollingTransaction midTransaction) CreateIncomingMessage(string[] documentIds, string dsrReference)
	{
		const string MessageId = "MID123";
		var processId = Guid.NewGuid().ToString();

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var messageText = GetResponseMessage(processId: processId, documentIds: documentIds);
		var (company, ediMessage, midTransaction) = testHelper.CreateGetMessageResponseObjects(ApplicationCode, messageText, messageSubType: MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchResult, cptType: TransactionTypes.MessageId, messageId: MessageId);
		AssertEquals("Pre-condition: incoming EM_ApplicationReference", ZString.Empty, ediMessage.EM_ApplicationReference);
		var dsrTransaction = testHelper.AddPollingTransaction(company, ApplicationCode, type: TransactionTypes.DocumentSearchRequest, transactionID: processId, status: StatusCodes.AwaitingResponse, reference: dsrReference);
		return (company, ediMessage, dsrTransaction, midTransaction);
	}

	protected override string GetResponseMessage() => GetResponseMessage(documentIds: new[] { "DOC123" });

	string GetResponseMessage(string[] documentIds, string processId = null)
	{
		var message = new CargoWise.Customs.CH.MessageDefinitions.Chartera.searchresult_v2.SearchResult()
		{
			ProcessId = processId ?? Guid.NewGuid().ToString(),
			Count = documentIds.Length,
			Documents = documentIds.Select(x => new CargoWise.Customs.CH.MessageDefinitions.Chartera.searchresult_v2.DocumentType()
			{
				DocumentId = x,
				CreatedAt = DateTime.Now,
				DocumentTypeProperty = "XXXXXX",
			}).ToCollection(),
		};
		return XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(message);
	}
}
