using System;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CharteraOutputDocumentDeliveryResultMessageProcessor))]
public class CharteraOutputDocumentDeliveryResultMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "Chartera Output Document Delivery Result Message Processor";

	protected override string ApplicationCode => CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryResult;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new CharteraOutputDocumentDeliveryResultMessageProcessor(Logger);

	[TestUtcOffset(3, 0, 0)]
	public void TestProcessWithMRN()
	{
		const string MRN = "MRN123";

		BusinessObject job;
		using (DisposableEnvironment.ForBranch(Branch.PK.ToGuid()))
		{
			job = CreateJobWithMRN(MRN).job;
		}

		var (ediMessage, docTransaction, midTransaction) = CreateIncomingMessage(mrn: MRN);
		Factory.Save();

		AssertProcess(job, ediMessage, docTransaction, midTransaction);
	}

	[TestUtcOffset(3, 0, 0)]
	public void TestProcessWithNC084()
	{
		const string MRN = "MRN123";

		BusinessObject job;
		using (DisposableEnvironment.ForBranch(Branch.PK.ToGuid()))
		{
			job = CreateJobWithMRN(MRN).job;
		}

		var receivedEdiMessage = MessageProcessorTestHelper.CreateEDIMessage(
			Factory,
			messageType: MessageTypeCodeList.Codes.MSG,
			messageSubType: MessageSubTypeCodeList.Codes.PassarDocumentNotification,
			applicationCode: ApplicationCodeList.Codes.CHCustomsPassar,
			direction: EDIMessage.Direction.Receive,
			status: EDIMessage.Status.Received,
			applicationReference: "DOC123",
			linkedObject: job);

		var (ediMessage, docTransaction, midTransaction) = CreateIncomingMessage();
		Factory.Save();

		AssertProcess(job, ediMessage, docTransaction, midTransaction);
	}

	void AssertProcess(BusinessObject job, EDIMessage ediMessage, CusPollingTransaction docTransaction, CusPollingTransaction midTransaction) => CombineAssertions(() =>
	{
		AssertNotEquals("Pre-condition: EM_GB", Branch.PK, ediMessage.EM_GB);
		MessageProcessor.ProcessMessage(ediMessage);

		AssertEquals("EM_LinkedObject", job.PK, ediMessage.EM_LinkedObject?.PK);
		AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
		AssertEquals("DOC CPT_Status", StatusCodes.Closed, docTransaction.CPT_Status);
		AssertEquals("DOC CPT_StatusTimeUtc", ZDateTime.UtcNow, docTransaction.CPT_StatusTimeUtc);
		AssertEquals("MID CPT_Status", StatusCodes.Closed, midTransaction.CPT_Status);
		AssertEquals("MID CPT_StatusTimeUtc", ZDateTime.UtcNow, midTransaction.CPT_StatusTimeUtc);
		AssertEquals("EM_GB", Branch.PK, ediMessage.EM_GB);
		AssertEquals("EI_GB", Branch.PK, ediMessage.Interchange?.EI_GB);

		var docManagerSupport = job as IDocManagerSupport;
		var eDoc = docManagerSupport.DocManagerInfo.AllEDocs[0];
		AssertEquals("eDoc.FileName", "sample.pdf", eDoc.FileName);
		AssertEquals("eDoc.DocType", RefDocTypes.CustomsAuthority, eDoc.DocType);
		AssertEquals("eDoc.ImageData", "sample document content", eDoc.ImageData.ToUTF8());
	});

	[TestDate(2000, 1, 1, 0, 0, 0)]
	public void TestAlreadyReceived()
	{
		const string MRN = "MRN123";

		var (job, _) = CreateJobWithMRN(MRN);
		var (ediMessage, docTransaction, midTransaction) = CreateIncomingMessage(mrn: MRN, docStatus: StatusCodes.Closed);
		Factory.Save();
		var originalTime = ZDateTime.UtcNow;

		TestDateAttribute.AddMinutes(1);
		MessageProcessor.ProcessMessage(ediMessage);

		AssertNull("EM_LinkedObject (not linked)", ediMessage.EM_LinkedObject);
		AssertEquals("EM_Status", EDIMessage.Status.Discarded, ediMessage.EM_Status);
		AssertEquals("DOC CPT_SystemLastEditTimeUtc (not updated)", originalTime, docTransaction.CPT_SystemLastEditTimeUtc);
		AssertEquals("MID CPT_CPT_Status", StatusCodes.Closed, midTransaction.CPT_Status);

		var docManagerSupport = job as IDocManagerSupport;
		AssertEquals("No eDoc added", 0, docManagerSupport.DocManagerInfo.AllEDocs.Count);
	}

	[TestUtcOffset(3, 0, 0)]
	public void TestFromDeclareItNoReqMessage() => CombineAssertions(() =>
	{
		const string MRN = "MRN123";

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);

		CreateJobWithMRN(MRN);
		var (ediMessage, docTransaction, midTransaction) = CreateIncomingMessage(mrn: MRN, createDeliveryRequest: false);
		Factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);

		AssertNull("EM_LinkedObject", ediMessage.EM_LinkedObject);
		AssertEquals("EM_Status", EDIMessage.Status.Warning, ediMessage.EM_Status);
		AssertEquals("MID CPT_Status", StatusCodes.Closed, midTransaction.CPT_Status);
		AssertEquals("MID CPT_StatusTimeUtc", ZDateTime.UtcNow, midTransaction.CPT_StatusTimeUtc);
	});

	public override void TestUnableLinkMessageToParent()
	{
		const string MRN = "MRN123";

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);

		var (ediMessage, docTransaction, midTransaction) = CreateIncomingMessage(mrn: MRN);
		Factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);

		AssertNull("EM_LinkedObject", ediMessage.EM_LinkedObject);
		AssertEquals("EM_Status", EDIMessage.Status.Warning, ediMessage.EM_Status);
		AssertEquals("DOC CPT_Status", StatusCodes.Closed, docTransaction.CPT_Status);
		AssertEquals("DOC CPT_StatusTimeUtc", ZDateTime.UtcNow, docTransaction.CPT_StatusTimeUtc);
		AssertEquals("MID CPT_Status", StatusCodes.Closed, midTransaction.CPT_Status);
		AssertEquals("MID CPT_StatusTimeUtc", ZDateTime.UtcNow, midTransaction.CPT_StatusTimeUtc);
	}

	public virtual void TestEventDocumentAllocated() => CombineAssertions(() =>
	{
		const string MRN = "MRN123";

		BusinessObject job;
		using (DisposableEnvironment.ForBranch(Branch.PK.ToGuid()))
		{
			job = CreateJobWithMRN(MRN).job;
		}

		var (ediMessage, _, _) = CreateIncomingMessage(mrn: MRN);
		Factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);
		var logEvent = ((EnterpriseBusinessObject)job).Logs.MostRecentLogByEventTime(Events.DocumentAllocated);

		AssertNotNull("Event written", logEvent);
		AssertEquals("Event Reference", $"|EXT=" + DocumentType, logEvent?.SL_Reference);
	});

	(BusinessObject job, CusEntryNumber entryNum) CreateJobWithMRN(ZString mrn, GlbCompany company = null, string entryNumType = CusEntryNumberTypes.Standard.MovementReferenceNumber, string entryNumCountry = CountryCodes.Switzerland)
	{
		var job = CreateJobCore(company);
		var cusEntryNum = CusEntryNumber.LoadOrCreate(job, entryNumType, entryNumCountry);
		cusEntryNum.CE_EntryNum = mrn;
		return (job, cusEntryNum);
	}

	protected virtual BusinessObject CreateJobCore(GlbCompany company)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (company != null)
		{
			declaration.JE_GC = company.PK;
		}
		return declaration.CustomsEntryHeaders.AddNew();
	}

	(EDIMessage ediMessage, CusPollingTransaction docTransaction, CusPollingTransaction midTransaction) CreateIncomingMessage(string mrn = "MRN123", string docStatus = StatusCodes.New, bool createDeliveryRequest = true)
	{
		const string MessageId = "MID123";
		const string DocumentId = "DOC123";

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var processId = Guid.NewGuid().ToString();
		var messageText = GetResponseMessage(mrn: mrn, documentId: DocumentId, processId: processId);
		if (createDeliveryRequest)
		{
			MessageProcessorTestHelper.CreateEDIMessage(Factory, applicationCode: ApplicationCode, messageType: MessageTypeCodeList.Codes.REQ, messageSubType: MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest, direction: ReceiveTransmitList.Codes.Transmit, applicationReference: processId);
		}
		var (company, ediMessage, midTransaction) = testHelper.CreateGetMessageResponseObjects(ApplicationCode, messageText, messageSubType: MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryResult, cptType: TransactionTypes.MessageId, messageId: MessageId, companyCode: Branch.Company.GC_Code);
		AssertEquals("Pre-condition: incoming EM_ApplicationReference", ZString.Empty, ediMessage.EM_ApplicationReference);
		var docTransaction = testHelper.AddPollingTransaction(company, ApplicationCode, type: TransactionTypes.DocumentDelivery, transactionID: DocumentId, status: docStatus);
		return (ediMessage, docTransaction, midTransaction);
	}

	protected override string GetResponseMessage() => GetResponseMessage(mrn: "MRN123", documentId: "DOC123");

	protected virtual string DocumentType => "PAEDS";

	string GetResponseMessage(string mrn, string documentId, string processId = null)
	{
		var message = new CargoWise.Customs.CH.MessageDefinitions.Chartera.document_v2.Document()
		{
			ProcessId = processId ?? Guid.NewGuid().ToString(),
			SourceId = mrn,
			DocumentId = documentId,
			CorrelationId = Guid.NewGuid().ToString(),
			DocumentType = DocumentType,
			DocumentName = "sample.pdf",
			DocumentData = Encoding.UTF8.GetBytes("sample document content"),
		};
		return XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(message);
	}
}
