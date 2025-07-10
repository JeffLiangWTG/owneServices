using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NC084ResponseMessageProcessor))]
public class NC084ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NC084 - Document Notification Message Processor";

	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarDocumentNotification;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NC084ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNC084();

	[TestDate(2024, 11, 27)]
	public void TestProcessMessage()
	{
		var mrn = "24CH11EXXZYM40H3K1";
		var documentId = "DOC123";
		BusinessObject parent = null;
		var currentCompany = GlbCompany.CurrentCompany;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				parent = CreateParentCore(mrn);
				return parent;
			},
				(correlationIdentifier) => TestingData.GetNC084(documentId: documentId, mrn: mrn),
				(ediMessage) =>
				{
					AssertEquals("EM_LinkedObject", parent, ediMessage.EM_LinkedObject);
					AssertEquals("EM_ApplicationReference", documentId, ediMessage.EM_ApplicationReference);

					var docTransactions = currentCompany.Factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_Type, TransactionTypes.DocumentDelivery));
					AssertEquals("Number of expected transactions", 1, docTransactions.Length);

					var transaction = docTransactions[0];
					AssertEquals("CPT_ApplicationCode", ApplicationCodes.CHCustomsCharteraOutput, transaction.CPT_ApplicationCode);
					AssertEquals("CPT_Type", TransactionTypes.DocumentDelivery, transaction.CPT_Type);
					AssertEquals("CPT_ParentTableCode", GlbCompanySchema.Constants.Prefix, transaction.CPT_ParentTableCode);
					AssertEquals("CPT_ParentID", currentCompany.PK, transaction.CPT_ParentID);
					AssertEquals("CPT_Status", StatusCodes.New, transaction.CPT_Status);
					AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
				},
				expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected virtual BusinessObject CreateParentCore(string mrn)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(mrn + ".1");
		return entryHeader;
	}
}
