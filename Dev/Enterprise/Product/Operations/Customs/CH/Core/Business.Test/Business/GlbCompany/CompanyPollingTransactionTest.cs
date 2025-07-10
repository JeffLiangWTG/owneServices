using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

[TestDate(2000, 1, 1, 12, 0, 0)]
[TestUtcOffset(2, 0, 0)]
sealed class CompanyPollingTransactionTest : TestCaseWithFactory
{
	public void TestLoadMessageIdTransactionsToSend() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New);
		var transaction2 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.Rejected);

		var transaction3 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.AwaitingResponse);
		var transaction4 = CreateCusPollingTransaction(company2, type: TransactionTypes.MessageId, status: StatusCodes.New);
		var transaction5 = CreateCusPollingTransaction(company1, type: TransactionTypes.LastMessageId, status: StatusCodes.New);
		var transaction8 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New, applicationCode: OtherApplicationCode);

		TestDateAttribute.AddMinutes(1);
		var transaction6 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New, sequenceNumber: 2);
		var transaction7 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New, sequenceNumber: 1);

		var transactions = company1.LoadMessageIdTransactionsToSend(Factory, DefaultApplicationCode, 20);

		AssertEquals("Number of transactions", 4, transactions.Length);
		AssertEquals("Status=NEW", true, transactions.Contains(transaction1));
		AssertEquals("Status=REJ", true, transactions.Contains(transaction2));
		AssertEquals("Status neither NEW nor REJ", false, transactions.Contains(transaction3));
		AssertEquals("Other company", false, transactions.Contains(transaction4));
		AssertEquals("Type != MID", false, transactions.Contains(transaction5));
		AssertEquals("Status=New", true, transactions.Contains(transaction7));

		AssertLessThan("Ordered by SystemCreateTimeUTC", transactions.IndexOf(t => t.PK == transaction1.PK), transactions.IndexOf(t => t.PK == transaction7.PK));
		AssertLessThan("Ordered by SequenceNumber", transactions.IndexOf(t => t.PK == transaction7.PK), transactions.IndexOf(t => t.PK == transaction6.PK));

		AssertEquals("maximumRows", 2, company1.LoadMessageIdTransactionsToSend(Factory, DefaultApplicationCode, 2).Length);
	});

	public void TestLoadDocumentDeliveryTransactionsToSend() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New);
		var transaction2 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.Error);
		var transaction3 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.AwaitingResponse);

		var transaction4 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.MessageId, status: StatusCodes.Skip);
		var transaction5 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.MessageId, status: StatusCodes.New);
		var transaction6 = CreateCusPollingTransaction(company2, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New);
		var transaction7 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsPassar, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New);

		TestDateAttribute.AddMinutes(1);
		var transaction8 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New);

		var transaction9 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New, earliestTimeOfNextAttemptUtc: ZDateTime.UtcNow.AddMinutes(1));
		var transaction10 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsCharteraOutput, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New, earliestTimeOfNextAttemptUtc: ZDateTime.UtcNow.AddMinutes(-1));
		var transaction11 = CreateCusPollingTransaction(company1, applicationCode: ApplicationCodes.CHCustomsPassar, type: TransactionTypes.DocumentDelivery, status: StatusCodes.New, earliestTimeOfNextAttemptUtc: ZDateTime.UtcNow.AddMinutes(-1));

		var transactions = company1.LoadDocumentDeliveryTransactionsToSend(Factory, 100);

		AssertEquals("Status=NEW", true, transactions.Contains(transaction1));
		AssertEquals("Status=ERR", true, transactions.Contains(transaction2));
		AssertEquals("Status=AWR", true, transactions.Contains(transaction3));
		AssertEquals("Status=SKP", false, transactions.Contains(transaction4));
		AssertEquals("Type!=DOC", false, transactions.Contains(transaction5));
		AssertEquals("ApplicationCode<>CHO", false, transactions.Contains(transaction7));
		AssertEquals("Other company", false, transactions.Contains(transaction6));
		AssertEquals("now < earlistTime", false, transactions.Contains(transaction9));
		AssertEquals("now > earlistTime", true, transactions.Contains(transaction10));
		AssertEquals("ApplicationCode<>CHO", false, transactions.Contains(transaction11));

		AssertLessThan("Ordered by SystemCreateTimeUTC", transactions.IndexOf(t => t.PK == transaction1.PK), transactions.IndexOf(t => t.PK == transaction8.PK));

		AssertEquals("maximumRows", 3, company1.LoadDocumentDeliveryTransactionsToSend(Factory, 3).Length);
	});

	public void TestLoadLastMessageIdTransaction()
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		company1.GC_Code = "CH1";
		company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		var company2 = Factory.NewWithValidTestData<GlbCompany>();
		company2.GC_Code = "CH2";
		company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, transactionID: "1");
		CreateCusPollingTransaction(company1, type: TransactionTypes.LastMessageId, transactionID: "2");
		CreateCusPollingTransaction(company2, type: TransactionTypes.MessageId, transactionID: "3");
		CreateCusPollingTransaction(company2, type: TransactionTypes.LastMessageId, transactionID: "4");

		CombineAssertions(() =>
		{
			AssertEquals("GetLastMessageId for Company 1", "2", company1.LoadLastMessageIdTransaction(DefaultApplicationCode).CPT_TransactionID);
			AssertEquals("GetLastMessageId for Company 2", "4", company2.LoadLastMessageIdTransaction(DefaultApplicationCode).CPT_TransactionID);
			AssertNull("GetLastMessageId for Current Company", GlbCompany.CurrentCompany.LoadLastMessageIdTransaction(DefaultApplicationCode));
			AssertNull("GetLastMessageId with other application code", GlbCompany.CurrentCompany.LoadLastMessageIdTransaction(OtherApplicationCode));
		});
	}

	[TestDate(2023, 4, 24)]
	public void TestCreateLastMessageIdTransaction() => CombineAssertions(() =>
	{
		var transaction = GlbCompany.CurrentCompany.CreateLastMessageIdTransaction(DefaultApplicationCode);
		AssertEquals("CPT_ApplicationCode", DefaultApplicationCode, transaction.CPT_ApplicationCode);
		AssertEquals("CPT_ParentTableCode", GlbCompanySchema.Constants.Prefix, transaction.CPT_ParentTableCode);
		AssertEquals("CPT_ParentID", GlbCompany.CurrentCompany.PK, transaction.CPT_ParentID);
		AssertEquals("CPT_Type", TransactionTypes.LastMessageId, transaction.CPT_Type);
		AssertEquals("CPT_Status", StatusCodes.New, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_TransactionID", ZString.Empty, transaction.CPT_TransactionID);
		AssertEquals("CPT_NumberOfAttempts", new ZByte(0), transaction.CPT_NumberOfAttempts);
	});

	[TestDate(2023, 4, 24)]
	public void TestLoadFirstPendingMessageIdTransaction() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var sequenceNumber = (short)2;
		var transaction1 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New, sequenceNumber: ++sequenceNumber);
		var transaction2 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.AwaitingResponse, sequenceNumber: ++sequenceNumber);
		var transaction3 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.Rejected, sequenceNumber: ++sequenceNumber);
		var transaction4 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.Skip, sequenceNumber: ++sequenceNumber);
		var transaction5 = CreateCusPollingTransaction(company2, type: TransactionTypes.MessageId, status: StatusCodes.New, sequenceNumber: 1);
		CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New, sequenceNumber: ++sequenceNumber);
		CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.AwaitingResponse, sequenceNumber: ++sequenceNumber);
		CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.Rejected, sequenceNumber: ++sequenceNumber);
		TestDateAttribute.AddMinutes(1);
		CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId, status: StatusCodes.New, sequenceNumber: 1);

		AssertEquals(GetAssertionMessage(transaction1), transaction1.PK, company1.LoadFirstPendingMessageIdTransaction(DefaultApplicationCode).PK);
		transaction1.Delete();

		AssertEquals(GetAssertionMessage(transaction2), transaction2.PK, company1.LoadFirstPendingMessageIdTransaction(DefaultApplicationCode).PK);
		transaction2.Delete();

		AssertEquals(GetAssertionMessage(transaction3), transaction3.PK, company1.LoadFirstPendingMessageIdTransaction(DefaultApplicationCode).PK);
		transaction3.Delete();

		AssertNotEquals(GetAssertionMessage(transaction4), transaction4.PK, company1.LoadFirstPendingMessageIdTransaction(DefaultApplicationCode).PK);
		AssertNotEquals("other company", transaction5.PK, company1.LoadFirstPendingMessageIdTransaction(DefaultApplicationCode).PK);

		string GetAssertionMessage(CusPollingTransaction transaction, string message = "") => $"CPT_Status={transaction.CPT_Status} CPT_SystemCreateTimeUtc={transaction.CPT_SystemCreateTimeUtc} CPT_SequenceNumber={transaction.CPT_SequenceNumber} {message}";
	});

	public void TestLoadTransactionByMessageId() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateCusPollingTransaction(company1, type: TransactionTypes.MessageId);
		var transaction2 = CreateCusPollingTransaction(company1, type: TransactionTypes.LastMessageId);

		AssertNotNull("Existing transaction", CompanyPollingTransaction.LoadTransactionByMessageId(company1, DefaultApplicationCode, transaction1.CPT_TransactionID));
		AssertNull("Other application", CompanyPollingTransaction.LoadTransactionByMessageId(company1, OtherApplicationCode, transaction1.CPT_TransactionID));
		AssertNull("Other company", CompanyPollingTransaction.LoadTransactionByMessageId(company2, DefaultApplicationCode, transaction1.CPT_TransactionID));
		AssertNull("Other Type", CompanyPollingTransaction.LoadTransactionByMessageId(company1, DefaultApplicationCode, transaction2.CPT_TransactionID));
		AssertNull("Unknown transactionID", CompanyPollingTransaction.LoadTransactionByMessageId(company1, DefaultApplicationCode, Guid.NewGuid().ToString()));

		AssertExceptionThrown<ArgumentNullException>(() => CompanyPollingTransaction.LoadTransactionByMessageId(null, DefaultApplicationCode, Guid.NewGuid().ToString()));
	});

	public void TestLoadTransactionByDocumentId() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateTransaction(company1);
		var transaction2 = CreateTransaction(company1, type: TransactionTypes.LastMessageId);
		var transaction3 = CreateTransaction(company1, applicationCode: ApplicationCodes.CHCustomsPassar);

		AssertNotNull("Existing transaction", CompanyPollingTransaction.LoadTransactionByDocumentId(company1, transaction1.CPT_TransactionID));
		AssertNull("Other application", CompanyPollingTransaction.LoadTransactionByDocumentId(company1, transaction3.CPT_TransactionID));
		AssertNull("Other company", CompanyPollingTransaction.LoadTransactionByDocumentId(company2, transaction1.CPT_TransactionID));
		AssertNull("Other Type", CompanyPollingTransaction.LoadTransactionByDocumentId(company1, transaction2.CPT_TransactionID));
		AssertNull("Unknown transactionID", CompanyPollingTransaction.LoadTransactionByDocumentId(company1, Guid.NewGuid().ToString()));

		AssertExceptionThrown<ArgumentNullException>(() => CompanyPollingTransaction.LoadTransactionByDocumentId(null, Guid.NewGuid().ToString()));

		CusPollingTransaction CreateTransaction(GlbCompany company, string applicationCode = ApplicationCodes.CHCustomsCharteraOutput, string type = TransactionTypes.DocumentDelivery) => CreateCusPollingTransaction(company, applicationCode: applicationCode, type: type);
	});

	public void TestLoadTransactionBySearchProcessId() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateTransaction(company1);
		var transaction2 = CreateTransaction(company1, type: TransactionTypes.LastMessageId);
		var transaction3 = CreateTransaction(company1, applicationCode: ApplicationCodes.CHCustomsPassar);

		AssertNotNull("Existing transaction", CompanyPollingTransaction.LoadTransactionBySearchProcessId(company1, transaction1.CPT_TransactionID));
		AssertNull("Other application", CompanyPollingTransaction.LoadTransactionBySearchProcessId(company1, transaction3.CPT_TransactionID));
		AssertNull("Other company", CompanyPollingTransaction.LoadTransactionBySearchProcessId(company2, transaction1.CPT_TransactionID));
		AssertNull("Other Type", CompanyPollingTransaction.LoadTransactionBySearchProcessId(company1, transaction2.CPT_TransactionID));
		AssertNull("Unknown transactionID", CompanyPollingTransaction.LoadTransactionBySearchProcessId(company1, Guid.NewGuid().ToString()));

		AssertExceptionThrown<ArgumentNullException>(() => CompanyPollingTransaction.LoadTransactionBySearchProcessId(null, Guid.NewGuid().ToString()));

		CusPollingTransaction CreateTransaction(GlbCompany company, string applicationCode = ApplicationCodes.CHCustomsCharteraOutput, string type = TransactionTypes.DocumentSearchRequest) => CreateCusPollingTransaction(company, applicationCode: applicationCode, type: type);
	});

	public void TestLoadTransactionsByDocumentId() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateTransaction(company1);
		var transaction2 = CreateTransaction(company1, type: TransactionTypes.LastMessageId);
		var transaction3 = CreateTransaction(company1, applicationCode: ApplicationCodes.CHCustomsPassar);
		var transaction4 = CreateTransaction(company1);
		var transaction5 = CreateTransaction(company2);

		var documentIds = new string[] { transaction1.CPT_TransactionID, transaction2.CPT_TransactionID, transaction3.CPT_TransactionID, transaction4.CPT_TransactionID, transaction5.CPT_TransactionID, Guid.NewGuid().ToString() };

		AssertContainsExactElementsInAnyOrder("Company 1", new[] { transaction1, transaction4 }, CompanyPollingTransaction.LoadTransactionsByDocumentId(company1, documentIds));
		AssertContainsExactElementsInAnyOrder("Company 2", new[] { transaction5 }, CompanyPollingTransaction.LoadTransactionsByDocumentId(company2, documentIds));

		AssertExceptionThrown<ArgumentNullException>("company null", () => CompanyPollingTransaction.LoadTransactionsByDocumentId(null, documentIds));
		AssertExceptionThrown<ArgumentNullException>("documentIds null", () => CompanyPollingTransaction.LoadTransactionsByDocumentId(company1, null));

		CusPollingTransaction CreateTransaction(GlbCompany company, string applicationCode = ApplicationCodes.CHCustomsCharteraOutput, string type = TransactionTypes.DocumentDelivery) => CreateCusPollingTransaction(company, applicationCode: applicationCode, type: type);
	});

	[TestDate(2023, 4, 24)]
	public void TestCreateMessageIdTransaction() => CombineAssertions(() =>
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var messageId = Guid.NewGuid().ToString();

		var transaction = company.CreateMessageIdTransaction(DefaultApplicationCode, messageId, 5);

		AssertEquals("CPT_ApplicationCode", DefaultApplicationCode, transaction.CPT_ApplicationCode);
		AssertEquals("CPT_ParentTableCode", company.TablePrefix, transaction.CPT_ParentTableCode);
		AssertEquals("CPT_ParentID", company.PK, transaction.CPT_ParentID);
		AssertEquals("CPT_Type", TransactionTypes.MessageId, transaction.CPT_Type);
		AssertEquals("CPT_TransactionID", messageId, transaction.CPT_TransactionID);
		AssertEquals("CPT_Status", StatusCodes.New, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_NumberOfAttempts", new ZByte(0), transaction.CPT_NumberOfAttempts);
		AssertEquals("CPT_SequenceNumber", new ZByte(5), transaction.CPT_SequenceNumber);
	});

	public void TestLoadDocumentSearchRequestForNextAttempt() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();
		var company3 = Factory.NewWithValidTestData<GlbCompany>();

		CreateTransaction(company1, new ZDateTime(2020, 8, 15));
		CreateTransaction(company1, new ZDateTime(2020, 8, 16));
		CreateTransaction(company1, new ZDateTime(2020, 12, 31), type: TransactionTypes.DocumentDelivery);
		CreateTransaction(company2, new ZDateTime(2020, 12, 31));
		Factory.Save();

		AssertMaxTime(company1, new ZDateTime(2020, 8, 16));
		AssertNull(LoadTransactionWithMostRecentTimeOfNextAttempt(company3));
		AssertExceptionThrown<ArgumentNullException>(() => LoadTransactionWithMostRecentTimeOfNextAttempt(null));

		CusPollingTransaction CreateTransaction(GlbCompany company, ZDateTime time, string type = TransactionTypes.DocumentSearchRequest) => CreateCusPollingTransaction(company, type, earliestTimeOfNextAttemptUtc: time, applicationCode: CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput);

		CusPollingTransaction LoadTransactionWithMostRecentTimeOfNextAttempt(GlbCompany company) => company.LoadDocumentSearchRequestForNextAttempt(ApplicationCodes.CHCustomsCharteraOutput);

		void AssertMaxTime(GlbCompany company, ZDateTime expected) => AssertEquals(expected, LoadTransactionWithMostRecentTimeOfNextAttempt(company)?.CPT_EarliestTimeOfNextAttemptUtc);
	});

	public void TestLoadDocumentSearchRequests() => CombineAssertions(() =>
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		var company2 = Factory.NewWithValidTestData<GlbCompany>();

		var transaction1 = CreateTransaction(company1);
		var transaction2 = CreateTransaction(company1, status: StatusCodes.AwaitingResponse);
		var transaction3 = CreateTransaction(company2);
		var transaction4 = CreateTransaction(company1, type: TransactionTypes.DocumentDelivery);
		var transaction5 = CreateTransaction(company1, status: StatusCodes.Skip);

		var transactions = company1.LoadDocumentSearchRequests(ZDateTime.MinSmallDateTimeValue, StatusCodes.Error, StatusCodes.AwaitingResponse);
		AssertEquals("Transaction in error", true, transactions.Contains(transaction1));
		AssertEquals("Transaction in awaiting response", true, transactions.Contains(transaction2));
		AssertEquals("Other company", false, transactions.Contains(transaction3));
		AssertEquals("Other type", false, transactions.Contains(transaction4));
		AssertEquals("Other status", false, transactions.Contains(transaction5));

		AssertExceptionThrown<ArgumentNullException>(() => CompanyPollingTransaction.LoadDocumentSearchRequests(null, ZDateTime.MinSmallDateTimeValue, StatusCodes.Error));

		CusPollingTransaction CreateTransaction(GlbCompany company, string type = TransactionTypes.DocumentSearchRequest, string status = StatusCodes.Error) => CreateCusPollingTransaction(company, type, status, applicationCode: CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput);
	});

	static CusPollingTransaction CreateCusPollingTransaction(BusinessObject parent, string type, string status = null, string transactionID = null, short sequenceNumber = 0, string applicationCode = null, ZDateTime? earliestTimeOfNextAttemptUtc = null)
	{
		var transaction = parent.Factory.NewWithValidTestData<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = applicationCode ?? DefaultApplicationCode;
		transaction.CPT_Status = status ?? Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
		transaction.CPT_TransactionID = transactionID ?? Guid.NewGuid().ToString();
		transaction.CPT_Type = type;
		transaction.CPT_NumberOfAttempts = 0;
		transaction.CPT_SequenceNumber = sequenceNumber;
		transaction.CPT_SystemCreateTimeUtc = ZDateTime.UtcNow;
		transaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttemptUtc ?? ZDateTime.Empty;
		if (parent != null)
		{
			transaction.CPT_ParentID = parent.PK;
			transaction.CPT_ParentTableCode = parent.TablePrefix;
		}
		transaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttemptUtc ?? ZDateTime.Empty;
		return transaction;
	}

	const string DefaultApplicationCode = ApplicationCodes.CHCustomsPassar;
	const string OtherApplicationCode = ApplicationCodes.CHCustomsCharteraOutput;
}
