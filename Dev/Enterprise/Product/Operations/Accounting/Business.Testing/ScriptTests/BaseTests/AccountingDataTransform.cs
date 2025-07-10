using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Product.DataLoad.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests
{
	static class AccountingDataTransform
	{
		public static void TransformDataToEdw(Action insertAction, AdminConnection connection, List<string> insertTables)
		{
			var testHelper = new EdwTsqlScriptTestHelper();
			testHelper.ETLRunTransformationForTest(insertTables, insertAction, connection);
			var sqlText = $@"insert {Db.EdwDatabaseName}.Finance.GRP__GeneralLedgerAggregateData(GLAccountKey, PostPeriod, GLAmountLocalCredit, GLAmountLocalDebit, CompanyKey, TransactionCategory, DepartmentKey, BranchKey, GLAmountLocalBalance, GLAmountOsBalance)
						select GLAccountKey, AA_Period,
								CASE
								WHEN AA_Amount < 0 THEN AA_Amount
								ELSE  0
								END AS CreditAmount,
								CASE
								WHEN AA_Amount > 0 THEN AA_Amount
								ELSE  0
								END AS DebitAmount,
								company.CompanyKey, AA_TransactionCategory, DepartmentKey,BranchKey, AA_Amount, AA_Amount
						from {Db.DatabaseName}.dbo.AccGLAggregate
						left join {Db.EdwDatabaseName}.Finance.BAS__GLAccount on AA_AG = GLAccountID
						left join {Db.EdwDatabaseName}.Organization.BAS__Company company on AA_GC = company.CompanyID
						left join {Db.EdwDatabaseName}.Organization.BAS__Department on AA_GE = DepartmentID
						left join {Db.EdwDatabaseName}.Organization.BAS__Branch on AA_GB = BranchID;";
			connection.ExecuteNonQuery(sqlText);
		}

		public static IDisposable CreateSnapShotForEdwAndTransformData(Action insertAction, List<string> insertTableList)
		{
			var disposableList = new DisposableList(2);
			var connection = Db.NewAdminConnection();
			disposableList.Add(connection);

			try
			{
				var snapshot = SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS");
				disposableList.Add(snapshot);
				TransformDataToEdw(insertAction, connection, insertTableList);
			}
			catch (Exception)
			{
				disposableList.Dispose();
			}

			return disposableList;
		}

		#region create data for EDW reports

		public static void CreateAccGLHeader(AdminConnection connection, string dbName, AccGLHeader gLHeader, long gLAccoutKey)
		{
			var sql = $@"
INSERT [{dbName}].[Finance].[BAS__GLAccount]
	(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType, TotalLevel)
VALUES
	(@gLAccoutKey, @pk, @accountTypeCode, @accountNo, 'G', @description, @debitCreditCode, @units, @column, @totalLevel);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, gLHeader.PK);
				command.AddParameter("@gLAccoutKey", SqlDbType.BigInt, gLAccoutKey);
				command.AddParameter("@accountNo", SqlDbType.VarChar, gLHeader.AG_AccountNum);
				command.AddParameter("@description", SqlDbType.VarChar, gLHeader.AG_Description);
				command.AddParameter("@accountTypeCode", SqlDbType.VarChar, gLHeader.AG_AccountType);
				command.AddParameter("@units", SqlDbType.VarChar, gLHeader.AG_StatisticalUnits);
				command.AddParameter("@debitCreditCode", SqlDbType.VarChar, gLHeader.AG_DebitCredit);
				command.AddParameter("@totalLevel", SqlDbType.Int, gLHeader.AG_TotalLevel);
				command.AddParameter("@column", SqlDbType.VarChar, gLHeader.AG_Column);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateAccountGLAggregate(AdminConnection connection, string dbName, decimal amount, int period, long accountEDWKey, long branchEDWKey, long companyEDWKey, long departmentEDWKey, string transactionCategory = "")
		{
			var sql = $@"
INSERT [{dbName}].[Finance].[GRP__GeneralLedgerAggregateData]
	([GLAccountKey], [PostPeriod], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
VALUES
	(@accountKey, @period, @amount, @companyKey, @transactionCategory, @departmentKey, @branchKey);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@amount", SqlDbType.Money, amount);
				command.AddParameter("@period", SqlDbType.Int, period);
				command.AddParameter("@accountKey", SqlDbType.BigInt, accountEDWKey);
				command.AddParameter("@branchKey", SqlDbType.BigInt, branchEDWKey);
				command.AddParameter("@companyKey", SqlDbType.BigInt, companyEDWKey);
				command.AddParameter("@departmentKey", SqlDbType.BigInt, departmentEDWKey);
				command.AddParameter("@transactionCategory", SqlDbType.VarChar, transactionCategory);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateCompany(AdminConnection connection, string dbName, GlbCompany company, long eDWKey)
		{
			CreateCurrency(connection, dbName, company.GC_RX_NKLocalCurrency);

			var sql = $@"
				INSERT [{dbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [CountryCode], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
				VALUES
					(@currentCompanyKey, @companyPK, @countryCode, @currencyCode, @companyCode, 1, 1);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@currentCompanyKey", SqlDbType.BigInt, eDWKey);
				command.AddParameter("@companyCode", SqlDbType.VarChar, company.GC_Code.ToString());
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, company.PK.ToGuid());
				command.AddParameter("@countryCode", SqlDbType.VarChar, company.Country.Code.ToString());
				command.AddParameter("@currencyCode", SqlDbType.VarChar, company.GC_RX_NKLocalCurrency.ToString());
				command.ExecuteNonQuery();
			}
		}

		public static void CreateCurrency(AdminConnection connection, string dbName, string currencyCode)
		{
			var sql = $@"
				INSERT {dbName}.Finance.BAS__Currency
				([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio])
					VALUES
						(1, newid(), @currencyCode , 100)";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@currencyCode", SqlDbType.VarChar, currencyCode);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateBranch(AdminConnection connection, string dbName, GlbBranch branch, long branchEDWKey, long companyEDWKey, long homePortEDWKey)
		{
			var sql = $@"
				INSERT [{dbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [HomePortKey], [BranchCode], [OrganizationKey])
					VALUES
						(@branchKey, @branchPK, @companyKey, @homePortKey, @branchCode, 1);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branch.PK);
				command.AddParameter("@companyKey", SqlDbType.BigInt, companyEDWKey);
				command.AddParameter("@branchKey", SqlDbType.BigInt, branchEDWKey);
				command.AddParameter("@branchCode", SqlDbType.VarChar, branch.GB_Code);
				command.AddParameter("@homePortKey", SqlDbType.BigInt, homePortEDWKey);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateDepartment(AdminConnection connection, string dbName, GlbDepartment department, long departmentEDWKey)
		{
			var sql = $@"
				INSERT [{dbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(@departmentKey, @departmentPK, @departmentCode);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@departmentCode", SqlDbType.VarChar, department.GE_Code);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, department.PK);
				command.AddParameter("@departmentKey", SqlDbType.BigInt, departmentEDWKey);
				command.ExecuteNonQuery();
			}
		}

		public static void CreatePeriodManagement(AdminConnection connection, string dbName, AccPeriodManagement periodManager, long periodManagerEDWKey, long companyKey)
		{
			var sql = $@"
				INSERT [{dbName}].[Finance].[BAS__PeriodManagement]
					([CompanyKey], [EndDate], [EndDateFormat], [Period], [PeriodManagementID], [PeriodManagementKey], [StartDate], [Year])
					VALUES
						(@companyKey, @endDate, @endDateFormat, @period, @periodManagementID, @periodManagementKey, @startDate, @year);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@companyKey", SqlDbType.BigInt, companyKey);
				command.AddParameter("@endDate", SqlDbType.SmallDateTime, periodManager.AM_EndDate);
				command.AddParameter("@endDateFormat", SqlDbType.DateTime, periodManager.AM_EndDate);
				command.AddParameter("@period", SqlDbType.Int, periodManager.AM_Period);
				command.AddParameter("@periodManagementID", SqlDbType.UniqueIdentifier, periodManager.PK);
				command.AddParameter("@periodManagementKey", SqlDbType.BigInt, periodManagerEDWKey);
				command.AddParameter("@startDate", SqlDbType.SmallDateTime, periodManager.AM_StartDate);
				command.AddParameter("@year", SqlDbType.SmallInt, periodManager.AM_Year);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateGLTransactionHeader(AdminConnection connection, string dbName, AccTransactionHeader transactionHeader,long gLTransactionHeaderKey, long bankAccountKey, long branchKey, long companyKey, long departmentKey, long gLAccountKey, long invoiceAddressOverrideKey, long jobHeaderKey, long organizationHeaderKey, long complianceSequenceKey)
		{
			var sql = $@"
				INSERT [{dbName}].[Finance].[BAS__GLTransactionHeader]
					([BankAccountID], [BankAccountKey], [BranchKey], [ChequeOrReference], [CompanyKey], [ComplianceDocumentDate], [ComplianceSubType], [CreateDateTimeUtc], [CreateDateUtc], [Currency], [DepartmentID], [DepartmentKey], [Description], [DueDate], [DueDateTime], [ExchangeRate], [FullyPaidDate], [GLAccountKey], [GLTransactionHeaderID], [GLTransactionHeaderKey], [InternalReference], [InvoiceAddressOverrideKey], [InvoiceDate], [JobHeaderKey], [JobInvoiceNumber], [Ledger], [LedgerCode], [LocalAmount], [OrganizationHeaderKey], [OutstandingAmount], [OverseasAmount], [PaymentOrReceiptType], [PostDate], [PostDateTime], [TaxAmount], [TransactionBelongsToGroup], [TransactionCategory], [TransactionNo], [TransactionReference], [TransactionType], [TransactionTypeCode], [InvoiceDateTime], [IsOSOutstandingAmountApplicable], [LocalTotal], [PostToGL], [OrganizationHeaderID], [ComplianceSequenceKey], [IsCancelled])
					VALUES
					(@bankAccountID, @bankAccountKey, @branchKey, @chequeOrReference, @companyKey, @complianceDocumentDate, @complianceSubType, @createDateTimeUtc, @createDateUtc, @currency, @departmentID, @departmentKey, @description, @dueDate, @dueDateTime, @exchangeRate, @fullyPaidDate, @gLAccountKey, @gLTransactionHeaderID, @gLTransactionHeaderKey, @internalReference, @invoiceAddressOverrideKey, @invoiceDate, @jobHeaderKey, @jobInvoiceNumber, @ledger, @ledgerCode, @localAmount, @organizationHeaderKey, @outstandingAmount, @overseasAmount, @paymentOrReceiptType, @postDate, @postDateTime, @taxAmount, @transactionBelongsToGroup, @transactionCategory, @transactionNo, @transactionReference, @tansactionType, @transactionTypeCode, @invoiceDateTime, @isOSOutstandingAmountApplicable, @localTotal, @postToGL, @organizationHeaderID, @complianceSequenceKey, @isCancelled);
			";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@bankAccountID", SqlDbType.UniqueIdentifier, transactionHeader.AH_DrawerBank);
				command.AddParameter("@bankAccountKey", SqlDbType.BigInt, bankAccountKey);
				command.AddParameter("@branchKey", SqlDbType.BigInt, branchKey);
				command.AddParameter("@chequeOrReference", SqlDbType.VarChar, transactionHeader.AH_ChequeOrReference);
				command.AddParameter("@companyKey", SqlDbType.BigInt, companyKey);
				command.AddParameter("@complianceDocumentDate", SqlDbType.Date, transactionHeader.AH_ComplianceDocumentDate);
				command.AddParameter("@complianceSubType", SqlDbType.VarChar, transactionHeader.AH_ComplianceSubType);
				command.AddParameter("@createDateTimeUtc", SqlDbType.SmallDateTime, transactionHeader.AH_SystemCreateTimeUtc);
				command.AddParameter("@createDateUtc", SqlDbType.Date, transactionHeader.AH_SystemCreateTimeUtc.Date);
				command.AddParameter("@currency", SqlDbType.VarChar, transactionHeader.AH_RX_NKTransactionCurrency);
				command.AddParameter("@departmentID", SqlDbType.UniqueIdentifier, transactionHeader.AH_GE);
				command.AddParameter("@departmentKey", SqlDbType.BigInt, departmentKey);
				command.AddParameter("@description", SqlDbType.NVarChar, transactionHeader.AH_Desc);
				command.AddParameter("@dueDate", SqlDbType.Date, transactionHeader.AH_DueDate);
				command.AddParameter("@dueDateTime", SqlDbType.SmallDateTime, transactionHeader.AH_DueDate);
				command.AddParameter("@exchangeRate", SqlDbType.Decimal, transactionHeader.AH_ExchangeRate);
				command.AddParameter("@fullyPaidDate", SqlDbType.Date, transactionHeader.AH_FullyPaidDate);
				command.AddParameter("@gLAccountKey", SqlDbType.BigInt, gLAccountKey);
				command.AddParameter("@gLTransactionHeaderID", SqlDbType.UniqueIdentifier, transactionHeader.PK);
				command.AddParameter("@gLTransactionHeaderKey", SqlDbType.BigInt, gLTransactionHeaderKey);
				command.AddParameter("@internalReference", SqlDbType.VarChar, transactionHeader.AH_PostedInternal);
				command.AddParameter("@invoiceAddressOverrideKey", SqlDbType.BigInt, invoiceAddressOverrideKey);
				command.AddParameter("@invoiceDate", SqlDbType.Date, transactionHeader.AH_InvoiceDate);
				command.AddParameter("@jobHeaderKey", SqlDbType.BigInt, jobHeaderKey);
				command.AddParameter("@jobInvoiceNumber", SqlDbType.VarChar, transactionHeader.AH_JobNumber);
				command.AddParameter("@ledger", SqlDbType.NVarChar, transactionHeader.AH_Ledger);
				command.AddParameter("@ledgerCode", SqlDbType.Char, transactionHeader.AH_Ledger);
				command.AddParameter("@localAmount", SqlDbType.Decimal, transactionHeader.AH_InvoiceAmount);
				command.AddParameter("@organizationHeaderKey", SqlDbType.BigInt, organizationHeaderKey);
				command.AddParameter("@outstandingAmount", SqlDbType.Money, transactionHeader.AH_OSOutstandingAmount);
				command.AddParameter("@overseasAmount", SqlDbType.Money, transactionHeader.AH_OSTotal);
				command.AddParameter("@paymentOrReceiptType", SqlDbType.Char, transactionHeader.AH_ReceiptType);
				command.AddParameter("@postDate", SqlDbType.Date, transactionHeader.AH_PostDate);
				command.AddParameter("@postDateTime", SqlDbType.DateTime, transactionHeader.AH_PostDate);
				command.AddParameter("@taxAmount", SqlDbType.Money, transactionHeader.AH_WithholdingTax);
				command.AddParameter("@transactionBelongsToGroup", SqlDbType.UniqueIdentifier, transactionHeader.AH_TransactionBelongsToGroup);
				command.AddParameter("@transactionCategory", SqlDbType.VarChar, transactionHeader.AH_TransactionBelongsToGroup);
				command.AddParameter("@transactionNo", SqlDbType.VarChar, transactionHeader.AH_TransactionNum);
				command.AddParameter("@transactionReference", SqlDbType.VarChar, transactionHeader.AH_TransactionReference);
				command.AddParameter("@tansactionType", SqlDbType.NChar, transactionHeader.AH_TransactionType);
				command.AddParameter("@transactionTypeCode", SqlDbType.Char, transactionHeader.AH_TransactionType);
				command.AddParameter("@invoiceDateTime", SqlDbType.SmallDateTime, transactionHeader.AH_InvoiceDate);
				command.AddParameter("@isOSOutstandingAmountApplicable", SqlDbType.Bit, transactionHeader.AH_IsOSOutstandingAmountApplicable);
				command.AddParameter("@localTotal", SqlDbType.Decimal, transactionHeader.AH_LocalTotal);
				command.AddParameter("@postToGL", SqlDbType.VarChar, transactionHeader.AH_PostToGL);
				command.AddParameter("@organizationHeaderID", SqlDbType.UniqueIdentifier, transactionHeader.AH_OH);
				command.AddParameter("@complianceSequenceKey", SqlDbType.BigInt, complianceSequenceKey);
				command.AddParameter("@isCancelled", SqlDbType.Bit, transactionHeader.IsCancelled);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateTransactionMatchLink(AdminConnection connection, string dbName, AccTransactionMatchLink gLTransactionMatchLink, long gLTransactionMatchLinkEDWKey, long gLTransactionHeaderEDWKey)
		{
			var sql = $@"
				INSERT [{dbName}].[Finance].[BAS__GLTransactionMatchLink]
					([Amount], [GLTransactionHeaderKey], [GLTransactionMatchLinkID], [GLTransactionMatchLinkKey], [MatchDate], [MatchGroupNum], [MatchPeriod], [OSAmount])
					VALUES
						(@amount, @gLTransactionHeaderKey, @gLTransactionMatchLinkID, @gLTransactionMatchLinkKey, @matchDate, @matchGroupNum, @matchPeriod, @oSAmount);";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@amount", SqlDbType.Money, gLTransactionMatchLink.AP_Amount);
				command.AddParameter("@gLTransactionHeaderKey", SqlDbType.BigInt, gLTransactionHeaderEDWKey);
				command.AddParameter("@gLTransactionMatchLinkID", SqlDbType.UniqueIdentifier, gLTransactionMatchLink.PK);
				command.AddParameter("@gLTransactionMatchLinkKey", SqlDbType.BigInt, gLTransactionMatchLinkEDWKey);
				command.AddParameter("@matchDate", SqlDbType.SmallDateTime, gLTransactionMatchLink.AP_MatchDate);
				command.AddParameter("@matchGroupNum", SqlDbType.VarChar, gLTransactionMatchLink.AP_MatchGroupNum);
				command.AddParameter("@matchPeriod", SqlDbType.Int, gLTransactionMatchLink.AP_MatchPeriod);
				command.AddParameter("@oSAmount", SqlDbType.Money, gLTransactionMatchLink.AP_OSAmount);
				command.ExecuteNonQuery();
			}
		}

		public static void ExecuteTableLoad(AdminConnection connection, string dbName, string configTableName, string schemaName, string tableName, string sqlName = "IncrementalLoadQuery", string selfReferencedQuery = "")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[{2}] WHERE  [ModelSchemaName] = '{3}' AND [ModelTableName] = '{4}'",
				sqlName, dbName, configTableName, schemaName, tableName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(connection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + dbName + " " + incLoadSQLText;
			sqlTextDL = string.Format(CultureInfo.InvariantCulture,
@"
DECLARE @MaxID BIGINT = 0
CREATE TABLE #Keys 
(
		PK_GUID UNIQUEIDENTIFIER NOT NULL, 
		PK_INT BIGINT NOT NULL, 
		TransformID INT NOT NULL,
		PRIMARY KEY CLUSTERED (PK_GUID ASC, TransformID ASC)
);
{0} {1} {2}",
			selfReferencedQuery, sqlTextDL, @"DROP TABLE #Keys;
IF OBJECT_ID('tempdb..#ForeignKeys') IS NOT NULL
DROP TABLE #ForeignKeys;");
			connection.ExecuteNonQuery(sqlTextDL);
		}

		public static long GetEDWKey(AdminConnection connection, string dbName, string schemaName, string tableName, string keyName, string idName, Guid id)
		{
			return  (long)connection.ExecuteScalar($"select {keyName} from {dbName}.{schemaName}.{tableName} where {idName} = '{id}' ");
		}

		public static long GetEDWKey(AdminConnection connection, string dbName, string tableName, Guid id)
		{
			var tableInfo = QueryTableList[tableName];
			return (long)connection.ExecuteScalar($"select {tableInfo.keyName} from {dbName}.{tableInfo.schemaName}.{tableName} where {tableInfo.idName} = '{id}' ");
		}

		static readonly Dictionary<string, (string schemaName, string keyName, string idName)> QueryTableList = new Dictionary<string, (string schemaName, string keyName, string idName)>()
		{
			{ "BAS__Company", ("Organization", "CompanyKey", "CompanyID") },
			{ "BAS__Currency", ("Finance", "CurrencyKey", "CurrencyID") },
			{ "BAS__Department", ("Organization", "DepartmentKey", "DepartmentID") },
			{ "BAS__GLAccount", ("Finance", "GLAccountKey", "GLAccountID") },
			{ "BAS__Organization", ("Organization", "OrganizationKey", "OrganizationID") },
			{ "BAS__AccountStmData", ("Finance", "AccountStmDataKey", "AccountStmDataID") },
			{ "BAS__GLAccountDescriptor", ("Finance", "GLAccountDescriptorKey", "GLAccountDescriptorID") },
			{ "BAS__Account", ("Customs", "AccountKey", "AccountID") },
			{ "BAS__Branch", ("Organization", "BranchKey", "BranchID") },
			{ "BAS__GLDescriptorPivot", ("Finance", "GLDescriptorPivotKey", "GLDescriptorPivotID") },
			{ "BAS__PeriodManagement", ("Finance", "PeriodManagementKey", "PeriodManagementID") },
			{ "BAS__StmDataDate", ("Finance", "StmDataDateKey", "StmDataDateID") },
			{ "BAS__BankAccount", ("Finance", "BankAccountKey", "BankAccountID") },
			{ "BAS__GLAggregate", ("Finance", "GLAggregateKey", "GLAggregateID") },
			{ "BAS__GLTransactionHeader", ("Finance", "GLTransactionHeaderKey", "GLTransactionHeaderID") },
			{ "BAS__GLTransactionMatchLink", ("Finance", "GLTransactionMatchLinkKey", "GLTransactionMatchLinkID") },
			{ "BAS__GeneralLedgerData", ("Finance", "GeneralLedgerDataKey", "GeneralLedgerDataID") },
		};

		public class EDWTestDataCreator : IDisposable
		{
			public EDWTestDataCreator(List<(string schemaName, string tablenName)> insertTableList, AdminConnection connection, string dbName, BusinessObjectFactory factory)
			{
				InsertConfigTableList = insertTableList;
				Connection = connection;
				DbName = dbName;
				AddDataForInitialLoadQuery();
				InsertConfigTableList.Where(x => !InitialLoadList.Select(x => x.tablenName).Contains(x.tablenName)).ForEach(x => ExecuteTableLoad(Connection, DbName, "CustomTableConfiguration", x.schemaName, x.tablenName, "InitialLoadQuery"));
			}

			readonly Dictionary<string, long> EDWKeyDict = new Dictionary<string, long>();
			readonly List<(string schemaName, string tablenName)> InsertConfigTableList;
			readonly AdminConnection Connection;
			readonly string DbName;

			//find isSelfReferenced by [Odyssey_EDW].[biAdmin].[TransformTableConfiguration], must order by DependencyOrder
			readonly List<(string schemaName, string tablenName, string configTableName, bool isSelfReferenced, string pkColumnm, string stagingTableName)> InitialLoadList = new List<(string schemaName, string tablenName, string configTableName, bool isSelfReferenced, string pkColumn, string stagingTableName)>()
			{
				("Organization","BAS__Capability", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__Currency", "TransformTableConfiguration", false, "", ""),
				("Organization","BAS__DefectReportGroup", "TransformTableConfiguration", false, "", ""),
				("Organization","BAS__Department", "TransformTableConfiguration", true, "GE_PK", "GlbDepartment"),
				("Finance","BAS__GLAccount", "TransformTableConfiguration", true, "AG_PK", "AccGLHeader"),
				("Organization","BAS__Organization", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__AccountStmData", "TransformTableConfiguration", false, "", ""),
				("Organization","BAS__Company", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__GLAccountDescriptor", "TransformTableConfiguration", true, "AJ_PK", "AccGLAccountDescriptor"),
				("Customs","BAS__Account", "TransformTableConfiguration", false, "", ""),
				("Organization","BAS__Branch", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__GLDescriptorPivot", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__PeriodManagement", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__StmDataDate", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__BankAccount", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__GLAggregate", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__GLTransactionHeader", "TransformTableConfiguration", false, "", ""),
				("Finance","BAS__GLTransactionMatchLink", "TransformTableConfiguration", false, "", ""),
			};

			public long CreateAccGLHeader(AccGLHeader accGLHeader)
			{
				var eDWKey = GetEDWKey("BAS__GLAccount");
				AccountingDataTransform.CreateAccGLHeader(Connection, DbName, accGLHeader, eDWKey);

				return eDWKey;
			}

			public long CreateAccountGLAggregate(decimal amount, int period, long accountEDWKey, long branchEDWKey, long companyEDWKey, long departmentEDWKey, string transactionCategory = "")
			{
				var eDWKey = GetEDWKey("GRP__GeneralLedgerAggregateData");
				AccountingDataTransform.CreateAccountGLAggregate(Connection, DbName, amount, period, accountEDWKey, branchEDWKey, companyEDWKey, departmentEDWKey, transactionCategory);

				return eDWKey;
			}

			public long CreateCompany(GlbCompany company)
			{
				var eDWKey = GetEDWKey("BAS__Company");
				AccountingDataTransform.CreateCompany(Connection, DbName, company, eDWKey);

				return eDWKey;
			}

			public long CreateBranch(GlbBranch branch, long companyEDWKey, long homePortEDWKey)
			{
				var eDWKey = GetEDWKey("BAS__Branch");
				AccountingDataTransform.CreateBranch(Connection, DbName, branch, eDWKey, companyEDWKey, homePortEDWKey);

				return eDWKey;
			}

			public long CreateDepartment(GlbDepartment department)
			{
				var eDWKey = GetEDWKey("BAS__Department");
				AccountingDataTransform.CreateDepartment(Connection, DbName, department, eDWKey);

				return eDWKey;
			}

			public long CreatePeriodManagement(AccPeriodManagement periodManagement, long companyEDWKey)
			{
				var eDWKey = GetEDWKey("BAS__PeriodManagement");
				AccountingDataTransform.CreatePeriodManagement(Connection, DbName, periodManagement, eDWKey, companyEDWKey);

				return eDWKey;
			}

			public long CreateTransactionMatchLink(AccTransactionMatchLink gLTransactionMatchLink, long gLTransactionHeaderEDWKey)
			{
				var eDWKey = GetEDWKey("BAS__GLTransactionMatchLink");
				AccountingDataTransform.CreateTransactionMatchLink(Connection, DbName, gLTransactionMatchLink, eDWKey, gLTransactionHeaderEDWKey);

				return eDWKey;
			}

			long GetEDWKey(string sourceName)
			{
				if (!EDWKeyDict.TryGetValue(sourceName, out long value))
				{
					var tableInfo = QueryTableList[sourceName];
					value = (long)Connection.ExecuteScalar($"select ISNULL(MAX({tableInfo.keyName}), 0) from {DbName}.{tableInfo.schemaName}.{sourceName}");
				}

				EDWKeyDict[sourceName] += 1;

				return value;
			}

			public void Dispose()
			{
				InsertConfigTableList.ForEach(x => ExecuteTableLoad(Connection, DbName, "CustomTableConfiguration", x.schemaName, x.tablenName));
			}

			void AddDataForInitialLoadQuery()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{0}].[Staging].[GlbCompany]
	([GC_PK], [GC_RN_NKCountryCode], [GC_RX_NKLocalCurrency], [GC_Code], [GC_IsGSTCashBasis], [GC_IsGSTRegistered])
SELECT
	[GC_PK], [GC_RN_NKCountryCode], [GC_RX_NKLocalCurrency], [GC_Code], [GC_IsGSTCashBasis], [GC_IsGSTRegistered]
FROM [Dbo].[GlbCompany];

INSERT INTO [{0}].[Staging].[AccPeriodManagement]
	([AM_EndDate], [AM_GC_Company], [AM_Period], [AM_PK], [AM_StartDate], [AM_Year])
SELECT
	[AM_EndDate], [AM_GC_Company], [AM_Period], [AM_PK], [AM_StartDate], [AM_Year]
FROM [Dbo].[AccPeriodManagement];

INSERT INTO [{0}].[Staging].[StmData]
	(
	[SD_BinaryValue]
    ,[SD_DepartmentGuid]
    ,[SD_GuidValue]
    ,[SD_Name]
    ,[SD_Owner]
    ,[SD_PK]
	)
SELECT
	[SD_BinaryValue]
    ,[SD_DepartmentGuid]
    ,[SD_GuidValue]
    ,[SD_Name]
    ,[SD_Owner]
    ,[SD_PK]
FROM [Dbo].[StmData];

INSERT INTO [{0}].[Staging].[AccTransactionMatchLink]
	([AP_AH], [AP_Amount], [AP_MatchDate], [AP_MatchGroupNum], [AP_MatchPeriod], [AP_OSAmount], [AP_PK])
SELECT 
	[AP_AH], [AP_Amount], [AP_MatchDate], [AP_MatchGroupNum], [AP_MatchPeriod], [AP_OSAmount], [AP_PK]
From [Dbo].[AccTransactionMatchLink];

INSERT INTO [{0}].[Staging].[AccTransactionHeader]
    ([AH_AB]
    ,[AH_AG]
    ,[AH_ChequeOrReference]
    ,[AH_ComplianceDocumentDate]
    ,[AH_ComplianceSubType]
    ,[AH_ConsolidatedInvoiceRef]
    ,[AH_Desc]
    ,[AH_DueDate]
    ,[AH_ExchangeRate]
    ,[AH_FullyPaidDate]
    ,[AH_GB]
    ,[AH_GC]
    ,[AH_GE]
    ,[AH_GSTAmount]
    ,[AH_InvoiceAmount]
    ,[AH_InvoiceDate]
    ,[AH_JH]
    ,[AH_Ledger]
    ,[AH_OA_InvoiceAddressOverride]
    ,[AH_OH]
    ,[AH_OSTotal]
    ,[AH_OutstandingAmount]
    ,[AH_PK]
    ,[AH_PostDate]
    ,[AH_ReceiptType]
    ,[AH_RX_NKTransactionCurrency]
    ,[AH_SystemCreateTimeUtc]
    ,[AH_TransactionBelongsToGroup]
    ,[AH_TransactionCategory]
    ,[AH_TransactionNum]
    ,[AH_TransactionReference]
    ,[AH_TransactionType]
    ,[AH_IsOSOutstandingAmountApplicable]
    ,[AH_LocalTaxAmountOtherTaxes]
    ,[AH_PostToGL]
    ,[AH_XD_ComplianceBook]
    ,[AH_IsCancelled])
SELECT
	[AH_AB]
    ,[AH_AG]
    ,[AH_ChequeOrReference]
    ,[AH_ComplianceDocumentDate]
    ,[AH_ComplianceSubType]
    ,[AH_ConsolidatedInvoiceRef]
    ,[AH_Desc]
    ,[AH_DueDate]
    ,[AH_ExchangeRate]
    ,[AH_FullyPaidDate]
    ,[AH_GB]
    ,[AH_GC]
    ,[AH_GE]
    ,[AH_GSTAmount]
    ,[AH_InvoiceAmount]
    ,[AH_InvoiceDate]
    ,[AH_JH]
    ,[AH_Ledger]
    ,[AH_OA_InvoiceAddressOverride]
    ,[AH_OH]
    ,[AH_OSTotal]
    ,[AH_OutstandingAmount]
    ,[AH_PK]
    ,[AH_PostDate]
    ,[AH_ReceiptType]
    ,[AH_RX_NKTransactionCurrency]
    ,[AH_SystemCreateTimeUtc]
    ,[AH_TransactionBelongsToGroup]
    ,[AH_TransactionCategory]
    ,[AH_TransactionNum]
    ,[AH_TransactionReference]
    ,[AH_TransactionType]
    ,[AH_IsOSOutstandingAmountApplicable]
    ,[AH_LocalTaxAmountOtherTaxes]
    ,[AH_PostToGL]
    ,[AH_XD_ComplianceBook]
    ,[AH_IsCancelled]
FROM [Dbo].[AccTransactionHeader];

INSERT INTO [{0}].[Staging].[AccGLAggregate]
    ([AA_AG]
    ,[AA_Amount]
    ,[AA_GB]
    ,[AA_GC]
    ,[AA_GE]
    ,[AA_Period]
    ,[AA_PK]
    ,[AA_TransactionCategory])
SELECT 
	 [AA_AG]
	,[AA_Amount]
	,[AA_GB]
	,[AA_GC]
	,[AA_GE]
	,[AA_Period]
	,[AA_PK]
	,[AA_TransactionCategory]
FROM [Dbo].[AccGLAggregate];

INSERT INTO [{0}].[Staging].[AccGLHeader]
    ([OP_TYPE]
	,[AG_AccountGroup]
    ,[AG_AccountNum]
    ,[AG_AccountType]
    ,[AG_AG_AlternateNum]
    ,[AG_AG_ConsolidationNum]
    ,[AG_AG_HeaderDependsOnTotal]
    ,[AG_AG_PercentNum]
    ,[AG_CashFlowType]
    ,[AG_Column]
    ,[AG_DebitCredit]
    ,[AG_Description]
    ,[AG_PK]
    ,[AG_PrintSequence]
    ,[AG_StatisticalUnits]
    ,[AG_TotalLevel])
SELECT
    0
	,[AG_AccountGroup]
    ,[AG_AccountNum]
    ,[AG_AccountType]
    ,[AG_AG_AlternateNum]
    ,[AG_AG_ConsolidationNum]
    ,[AG_AG_HeaderDependsOnTotal]
    ,[AG_AG_PercentNum]
    ,[AG_CashFlowType]
    ,[AG_Column]
    ,[AG_DebitCredit]
    ,[AG_Description]
    ,[AG_PK]
    ,[AG_PrintSequence]
    ,[AG_StatisticalUnits]
    ,[AG_TotalLevel]
FROM [Dbo].[AccGLHeader];

INSERT INTO [{0}].[Staging].[GlbDepartment]
    ([OP_TYPE]
	,[GE_Air]
    ,[GE_Code]
    ,[GE_CustomsBrokerage]
    ,[GE_DepotCFS]
    ,[GE_Desc]
    ,[GE_Domestic]
    ,[GE_Export]
    ,[GE_GE]
    ,[GE_Import]
    ,[GE_InternationalFreight]
    ,[GE_IsActive]
    ,[GE_IsValid]
    ,[GE_LineHaul]
    ,[GE_LocalTransport]
    ,[GE_Misc]
    ,[GE_NonDirectional]
    ,[GE_NonTransport]
    ,[GE_PK]
    ,[GE_Post]
    ,[GE_Rail]
    ,[GE_Road]
    ,[GE_Sea]
    ,[GE_SystemCode]
    ,[GE_Warehouse])
SELECT
	0
	,[GE_Air]
    ,[GE_Code]
    ,[GE_CustomsBrokerage]
    ,[GE_DepotCFS]
    ,[GE_Desc]
    ,[GE_Domestic]
    ,[GE_Export]
    ,[GE_GE]
    ,[GE_Import]
    ,[GE_InternationalFreight]
    ,[GE_IsActive]
    ,[GE_IsValid]
    ,[GE_LineHaul]
    ,[GE_LocalTransport]
    ,[GE_Misc]
    ,[GE_NonDirectional]
    ,[GE_NonTransport]
    ,[GE_PK]
    ,[GE_Post]
    ,[GE_Rail]
    ,[GE_Road]
    ,[GE_Sea]
    ,[GE_SystemCode]
    ,[GE_Warehouse]
FROM [Dbo].[GlbDepartment];

INSERT INTO [{0}].[Staging].[GlbBranch]
    ([GB_AccountingGroupCode]
    ,[GB_Address1]
    ,[GB_Address2]
    ,[GB_BranchName]
    ,[GB_City]
    ,[GB_Code]
    ,[GB_Email]
    ,[GB_Fax]
    ,[GB_GC]
    ,[GB_InternalExtension]
    ,[GB_IsActive]
    ,[GB_IsValid]
    ,[GB_LocalDocLanguage]
    ,[GB_OA_AddressProxy]
    ,[GB_OH_OrgProxy]
    ,[GB_Phone]
    ,[GB_PK]
    ,[GB_PostCode]
    ,[GB_RL_NKHomePort]
    ,[GB_RN_NKCountryCode]
    ,[GB_State]
    ,[GB_WebAddress])
SELECT 
	 [GB_AccountingGroupCode]
    ,[GB_Address1]
    ,[GB_Address2]
    ,[GB_BranchName]
    ,[GB_City]
    ,[GB_Code]
    ,[GB_Email]
    ,[GB_Fax]
    ,[GB_GC]
    ,[GB_InternalExtension]
    ,[GB_IsActive]
    ,[GB_IsValid]
    ,[GB_LocalDocLanguage]
    ,[GB_OA_AddressProxy]
    ,[GB_OH_OrgProxy]
    ,[GB_Phone]
    ,[GB_PK]
    ,[GB_PostCode]
    ,[GB_RL_NKHomePort]
    ,[GB_RN_NKCountryCode]
    ,[GB_State]
    ,[GB_WebAddress]
FROM [Dbo].[GlbBranch];

INSERT INTO [{0}].[Staging].[RefCurrency]
    ([RX_Code]
    ,[RX_Desc]
    ,[RX_IsActive]
    ,[RX_IsSystem]
    ,[RX_PK]
    ,[RX_SubUnitName]
    ,[RX_SubUnitRatio]
    ,[RX_Symbol]
    ,[RX_UnitName])
SELECT 
	 [RX_Code]
    ,[RX_Desc]
    ,[RX_IsActive]
    ,[RX_IsSystem]
    ,[RX_PK]
    ,[RX_SubUnitName]
    ,[RX_SubUnitRatio]
    ,[RX_Symbol]
    ,[RX_UnitName]
FROM [Dbo].[RefCurrency];

INSERT INTO [{0}].[Staging].[AccGLAccountDescriptor]
    ([OP_TYPE]
	,[AJ_AccountDescription]
    ,[AJ_AG]
    ,[AJ_DebitCredit]
    ,[AJ_Language]
    ,[AJ_LocalAccountNumber]
    ,[AJ_PK]
    ,[AJ_ReportCategory]
    ,[AJ_ReportType]
    ,[AJ_RN_NKCountryOfCompliance]
    ,[AJ_TotalLevel]
    ,[AJ_AJ_AlternativeNum]
    ,[AJ_AJ_CarriedForwardAccount]
    ,[AJ_AJ_ConsolidationNum]
    ,[AJ_AJ_HeaderDependsOnTotal]
    ,[AJ_AJ_PercentNum]
    ,[AJ_PrintSequence])
SELECT
	0
	,[AJ_AccountDescription]
    ,[AJ_AG]
    ,[AJ_DebitCredit]
    ,[AJ_Language]
    ,[AJ_LocalAccountNumber]
    ,[AJ_PK]
    ,[AJ_ReportCategory]
    ,[AJ_ReportType]
    ,[AJ_RN_NKCountryOfCompliance]
    ,[AJ_TotalLevel]
    ,[AJ_AJ_AlternativeNum]
    ,[AJ_AJ_CarriedForwardAccount]
    ,[AJ_AJ_ConsolidationNum]
    ,[AJ_AJ_HeaderDependsOnTotal]
    ,[AJ_AJ_PercentNum]
    ,[AJ_PrintSequence]
FROM [Dbo].[AccGLAccountDescriptor];

INSERT INTO [{0}].[Staging].[AccGLDescriptorPivot]
    ([YJ_AG]
    ,[YJ_AJ]
    ,[YJ_PK])
SELECT
	[YJ_AG]
    ,[YJ_AJ]
    ,[YJ_PK]
FROM [Dbo].[AccGLDescriptorPivot];

INSERT INTO [{0}].[Staging].[AccBankAccount]
    ([AB_AG]
    ,[AB_Code]
    ,[AB_GB]
    ,[AB_GC]
    ,[AB_PK]
    ,[AB_RX_NKAccountCurrency])
SELECT
	[AB_AG]
    ,[AB_Code]
    ,[AB_GB]
    ,[AB_GC]
    ,[AB_PK]
    ,[AB_RX_NKAccountCurrency]
FROM [Dbo].[AccBankAccount];

INSERT INTO [{0}].[Staging].[GlbCapability]
    ([G4_Code]
    ,[G4_Description]
    ,[G4_IsActive]
    ,[G4_PK])
SELECT
	[G4_Code]
    ,[G4_Description]
    ,[G4_IsActive]
    ,[G4_PK]
FROM [Dbo].[GlbCapability];


INSERT INTO  [{0}].[Staging].[OrgHeader]
    ([OH_Category]
    ,[OH_Code]
    ,[OH_FullName]
    ,[OH_IsActive]
    ,[OH_IsAirCTO]
    ,[OH_IsAirLine]
    ,[OH_IsAirWholesaler]
    ,[OH_IsBroker]
    ,[OH_IsCompetitor]
    ,[OH_IsConsignee]
    ,[OH_IsConsignor]
    ,[OH_IsContainerYard]
    ,[OH_IsControllingAgent]
    ,[OH_IsControllingCustomer]
    ,[OH_IsDistributionCentre]
    ,[OH_IsForwarder]
    ,[OH_IsFumigationContractor]
    ,[OH_IsGlobalAccount]
    ,[OH_IsLineHaulProvider]
    ,[OH_IsLocalTransport]
    ,[OH_IsMiscFreightServices]
    ,[OH_IsNationalAccount]
    ,[OH_IsPackDepot]
    ,[OH_IsPersonalEffectsAccount]
    ,[OH_IsRailHead]
    ,[OH_IsRailProvider]
    ,[OH_IsRoadFreightDepot]
    ,[OH_IsSalesLead]
    ,[OH_IsSeaCTO]
    ,[OH_IsSeaWholesaler]
    ,[OH_IsShippingConsortium]
    ,[OH_IsShippingLine]
    ,[OH_IsShippingProvider]
    ,[OH_IsTempAccount]
    ,[OH_IsTransportClient]
    ,[OH_IsUnpackDepot]
    ,[OH_IsUserFlag1]
    ,[OH_IsUserFlag10]
    ,[OH_IsUserFlag11]
    ,[OH_IsUserFlag12]
    ,[OH_IsUserFlag13]
    ,[OH_IsUserFlag14]
    ,[OH_IsUserFlag15]
    ,[OH_IsUserFlag16]
    ,[OH_IsUserFlag17]
    ,[OH_IsUserFlag18]
    ,[OH_IsUserFlag19]
    ,[OH_IsUserFlag2]
    ,[OH_IsUserFlag20]
    ,[OH_IsUserFlag21]
    ,[OH_IsUserFlag22]
    ,[OH_IsUserFlag23]
    ,[OH_IsUserFlag24]
    ,[OH_IsUserFlag3]
    ,[OH_IsUserFlag4]
    ,[OH_IsUserFlag5]
    ,[OH_IsUserFlag6]
    ,[OH_IsUserFlag7]
    ,[OH_IsUserFlag8]
    ,[OH_IsUserFlag9]
    ,[OH_IsValid]
    ,[OH_IsWarehouseClient]
    ,[OH_Language]
    ,[OH_PK]
    ,[OH_RL_NKClosestPort]
    ,[OH_ScreeningStatus]
    ,[OH_SystemCreateTimeUtc]
    ,[OH_SystemCreateUser]
    ,[OH_SystemLastEditUser])
SELECT
	[OH_Category]
    ,[OH_Code]
    ,[OH_FullName]
    ,[OH_IsActive]
    ,[OH_IsAirCTO]
    ,[OH_IsAirLine]
    ,[OH_IsAirWholesaler]
    ,[OH_IsBroker]
    ,[OH_IsCompetitor]
    ,[OH_IsConsignee]
    ,[OH_IsConsignor]
    ,[OH_IsContainerYard]
    ,[OH_IsControllingAgent]
    ,[OH_IsControllingCustomer]
    ,[OH_IsDistributionCentre]
    ,[OH_IsForwarder]
    ,[OH_IsFumigationContractor]
    ,[OH_IsGlobalAccount]
    ,[OH_IsLineHaulProvider]
    ,[OH_IsLocalTransport]
    ,[OH_IsMiscFreightServices]
    ,[OH_IsNationalAccount]
    ,[OH_IsPackDepot]
    ,[OH_IsPersonalEffectsAccount]
    ,[OH_IsRailHead]
    ,[OH_IsRailProvider]
    ,[OH_IsRoadFreightDepot]
    ,[OH_IsSalesLead]
    ,[OH_IsSeaCTO]
    ,[OH_IsSeaWholesaler]
    ,[OH_IsShippingConsortium]
    ,[OH_IsShippingLine]
    ,[OH_IsShippingProvider]
    ,[OH_IsTempAccount]
    ,[OH_IsTransportClient]
    ,[OH_IsUnpackDepot]
    ,[OH_IsUserFlag1]
    ,[OH_IsUserFlag10]
    ,[OH_IsUserFlag11]
    ,[OH_IsUserFlag12]
    ,[OH_IsUserFlag13]
    ,[OH_IsUserFlag14]
    ,[OH_IsUserFlag15]
    ,[OH_IsUserFlag16]
    ,[OH_IsUserFlag17]
    ,[OH_IsUserFlag18]
    ,[OH_IsUserFlag19]
    ,[OH_IsUserFlag2]
    ,[OH_IsUserFlag20]
    ,[OH_IsUserFlag21]
    ,[OH_IsUserFlag22]
    ,[OH_IsUserFlag23]
    ,[OH_IsUserFlag24]
    ,[OH_IsUserFlag3]
    ,[OH_IsUserFlag4]
    ,[OH_IsUserFlag5]
    ,[OH_IsUserFlag6]
    ,[OH_IsUserFlag7]
    ,[OH_IsUserFlag8]
    ,[OH_IsUserFlag9]
    ,[OH_IsValid]
    ,[OH_IsWarehouseClient]
    ,[OH_Language]
    ,[OH_PK]
    ,[OH_RL_NKClosestPort]
    ,[OH_ScreeningStatus]
    ,[OH_SystemCreateTimeUtc]
    ,[OH_SystemCreateUser]
    ,[OH_SystemLastEditUser]
FROM [Dbo].[OrgHeader];
",
					DbName);
				Connection.ExecuteNonQuery(sqlText);

				InitialLoadList.ForEach(x => ExecuteTableLoad(Connection, DbName, x.configTableName, x.schemaName, x.tablenName, "InitialLoadQuery", selfReferencedQuery: CreateSelfReferencedQuery(x.stagingTableName, x.isSelfReferenced, x.pkColumnm)));
			}

			string CreateSelfReferencedQuery(string stagingTableName, bool isSelfReferenced, string pkColumn)
			{
				if (isSelfReferenced)
				{
					return $@"
INSERT INTO #Keys  WITH (TABLOCK) (PK_GUID, PK_INT, TransformID)
SELECT
{pkColumn},
ROW_NUMBER() OVER (ORDER BY {pkColumn}) AS PK_INT,
1
FROM [Staging].{stagingTableName};
";
				}

				return string.Empty;
			}
		}

		#endregion
	}
}
