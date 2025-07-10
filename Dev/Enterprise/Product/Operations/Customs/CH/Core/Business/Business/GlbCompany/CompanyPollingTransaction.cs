using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.CusPollingTransaction;

namespace Enterprise.Customs.CH.Business;

public static class CompanyPollingTransaction
{
	public static class TransactionTypes
	{
		public const string MessageId = "MID";
		public const string LastMessageId = "LMI";
		public const string DocumentDelivery = "DOC";
		public const string DocumentSearchRequest = "DSR";
	}

	public static class StatusCodes
	{
		public const string New = "NEW";
		public const string Rejected = "REJ";
		public const string AwaitingResponse = "AWR";
		public const string Closed = "CLS";
		public const string Skip = "SKP";
		public const string Error = "ERR";
	}

	public static class ReferenceCodes
	{
		public const string Manual = "MAN";
	}

	#region LastMessageId

	public static CusPollingTransaction LoadLastMessageIdTransaction(this GlbCompany company, ZString applicationCode)
	{
		Argument.NotNull(company, nameof(company));

		return company.Factory.LoadTop1<CusPollingTransaction>(GetFilterForCompany(company.PK, applicationCode, TransactionTypes.LastMessageId));
	}

	public static CusPollingTransaction CreateLastMessageIdTransaction(this GlbCompany company, ZString applicationCode)
	{
		return CreateTransaction(company, applicationCode, TransactionTypes.LastMessageId);
	}

	#endregion

	#region MessageId Transaction

	public static CusPollingTransaction[] LoadMessageIdTransactionsToSend(this GlbCompany company, BusinessObjectFactory factory, ZString applicationCode, int maximumRows)
	{
		Argument.NotNull(company, nameof(company));

		var query = GetFilterForCompany(company.PK, applicationCode, TransactionTypes.MessageId);
		query.AddToFilter(CusPollingTransactionSchema.CPT_Status, new[] { StatusCodes.New, StatusCodes.Rejected });
		query.OrderBy = CusPollingTransactionSchema.Constants.CPT_SystemCreateTimeUtc + "," + CusPollingTransactionSchema.Constants.CPT_SequenceNumber;
		query.MaximumRows = maximumRows;

		return (factory ?? company.Factory).Load<CusPollingTransaction>(query);
	}

	public static CusPollingTransaction[] LoadDocumentDeliveryTransactionsToSend(this GlbCompany company, BusinessObjectFactory factory, int maximumRows)
	{
		Argument.NotNull(company, nameof(company));

		var query = GetFilterForCompany(company.PK, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentDelivery);
		query.AddToFilter(CusPollingTransactionSchema.CPT_Status, new[] { StatusCodes.New, StatusCodes.Error, StatusCodes.AwaitingResponse });
		var earliestTimeQuery = new ZQuery(CusPollingTransactionSchema.CPT_EarliestTimeOfNextAttemptUtc, ZDateTime.Empty);
		earliestTimeQuery.AddToFilter(JoinCondition.Or, CusPollingTransactionSchema.CPT_EarliestTimeOfNextAttemptUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
		query.AddToFilter(earliestTimeQuery);
		query.OrderBy = CusPollingTransactionSchema.Constants.CPT_SystemCreateTimeUtc;
		query.MaximumRows = maximumRows;

		return (factory ?? company.Factory).Load<CusPollingTransaction>(query);
	}

	public static CusPollingTransaction LoadFirstPendingMessageIdTransaction(this GlbCompany company, ZString applicationCode)
	{
		Argument.NotNull(company, nameof(company));

		var query = GetFilterForCompany(company.PK, applicationCode, TransactionTypes.MessageId);
		query.AddToFilter(CusPollingTransactionSchema.CPT_Status, PendingStatusCodes);
		query.OrderBy = CusPollingTransactionSchema.Constants.CPT_SystemCreateTimeUtc + "," + CusPollingTransactionSchema.Constants.CPT_SequenceNumber;

		return company.Factory.LoadTop1<CusPollingTransaction>(query);
	}

	public static CusPollingTransaction LoadDocumentSearchRequestForNextAttempt(this GlbCompany company, ZString applicationCode)
	{
		Argument.NotNull(company, nameof(company));

		var query = GetFilterForCompany(company.PK, applicationCode, TransactionTypes.DocumentSearchRequest);
		query.OrderBy = CusPollingTransactionSchema.Constants.CPT_EarliestTimeOfNextAttemptUtc + OrderByClause.Descending;

		return company.Factory.LoadTop1<CusPollingTransaction>(query);
	}

	public static CusPollingTransaction[] LoadDocumentSearchRequests(this GlbCompany company, ZDateTime earliestTimeOfNextAttemptsNotBefore, params string[] status)
	{
		Argument.NotNull(company, nameof(company));

		var query = GetFilterForCompany(company.PK, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest);
		query.AddToFilter(CusPollingTransactionSchema.CPT_Status, status);
		query.AddToFilter(new ZQuery(CusPollingTransactionSchema.CPT_EarliestTimeOfNextAttemptUtc, null)
			.AddToFilter(JoinCondition.Or, CusPollingTransactionSchema.CPT_EarliestTimeOfNextAttemptUtc, SQLComparisonOperator.GreaterThanOrEqualTo, earliestTimeOfNextAttemptsNotBefore));

		return company.Factory.Load<CusPollingTransaction>(query);
	}

	public static string[] PendingStatusCodes => new[] { StatusCodes.New, StatusCodes.AwaitingResponse, StatusCodes.Rejected };

	public static CusPollingTransaction LoadTransactionByMessageId(this GlbCompany company, ZString applicationCode, ZString messageId)
	{
		return LoadTransactionById(company, applicationCode, TransactionTypes.MessageId, messageId);
	}

	public static CusPollingTransaction LoadTransactionByDocumentId(this GlbCompany company, ZString documentId)
	{
		return LoadTransactionById(company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentDelivery, documentId);
	}

	public static CusPollingTransaction LoadTransactionBySearchProcessId(this GlbCompany company, ZString processId)
	{
		return LoadTransactionById(company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, processId);
	}

	static CusPollingTransaction LoadTransactionById(this GlbCompany company, ZString applicationCode, ZString type, ZString transactionId)
	{
		Argument.NotNull(company, nameof(company));

		var query = GetFilterForCompany(company.PK, applicationCode, type);
		query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, transactionId);
		return company.Factory.LoadTop1<CusPollingTransaction>(query);
	}

	public static CusPollingTransaction[] LoadTransactionsByDocumentId(this GlbCompany company, string[] documentIds)
	{
		Argument.NotNull(company, nameof(company));
		Argument.NotNull(documentIds, nameof(documentIds));

		var query = GetFilterForCompany(company.PK, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentDelivery);
		query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, documentIds);
		return company.Factory.Load<CusPollingTransaction>(query);
	}

	public static CusPollingTransaction CreateMessageIdTransaction(this GlbCompany company, ZString applicationCode, ZString messageId, ZShort sequenceNumber)
	{
		return CreateTransaction(company, applicationCode, TransactionTypes.MessageId, transactionId: messageId, sequenceNumber: sequenceNumber);
	}

	public static CusPollingTransaction CreateDocumentSearchTransaction(this GlbCompany company, ZString applicationCode, ZString processId, ZDateTime? timeOfNextAttempt = null, string status = StatusCodes.AwaitingResponse, byte numberOfAttempts = 1, string reference = null)
	{
		return CreateTransaction(company, applicationCode, TransactionTypes.DocumentSearchRequest, transactionId: processId, status: status, numberOfAttempts: numberOfAttempts, earliestTimeOfNextAttemptUtc: timeOfNextAttempt, reference: reference);
	}

	public static CusPollingTransaction CreateDocumentDeliveryTransaction(this GlbCompany company, ZString applicationCode, ZString documentId)
	{
		return CreateTransaction(company, applicationCode, TransactionTypes.DocumentDelivery, transactionId: documentId);
	}

	#endregion

	static ZQuery GetFilterForCompany(ZGuid companyPK, string applicationCode, string type)
	{
		var result = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, applicationCode);
		result.AddToFilter(CusPollingTransactionSchema.CPT_ParentTableCode, GlbCompanySchema.Constants.Prefix);
		result.AddToFilter(CusPollingTransactionSchema.CPT_ParentID, companyPK);
		result.AddToFilter(CusPollingTransactionSchema.CPT_Type, type);
		return result;
	}

	static CusPollingTransaction CreateTransaction(this GlbCompany company, ZString applicationCode, ZString type, string transactionId = null, short sequenceNumber = 0, string status = StatusCodes.New, byte numberOfAttempts = 0, ZDateTime? earliestTimeOfNextAttemptUtc = null, string reference = null, BusinessObjectFactory factory = null)
	{
		Argument.NotNull(company, nameof(company));

		var transaction = (factory ?? company.Factory).New<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = applicationCode;
		transaction.CPT_ParentTableCode = GlbCompanySchema.Constants.Prefix;
		transaction.CPT_ParentID = company.PK;
		transaction.CPT_Type = type;
		transaction.CPT_Status = status;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
		transaction.CPT_TransactionID = transactionId;
		transaction.CPT_NumberOfAttempts = numberOfAttempts;
		transaction.CPT_SequenceNumber = sequenceNumber;
		transaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttemptUtc ?? ZDateTime.Empty;
		transaction.CPT_Reference = reference ?? ZString.Empty;
		return transaction;
	}
}
