using System;
using System.Text;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	class PrepareDataHelper
	{
		readonly DbConnection TestConnection;

		readonly string ScriptDbName;

		public PrepareDataHelper(DbConnection dbConnection, string scriptDbName)
		{
			TestConnection = dbConnection;
			ScriptDbName = scriptDbName;
		}

		public void InsertGLAccount(int accoutKey, string number, string accountTypeCode = "P&L", string debitCreditCode = "DR", string sectionType = "AP", int alternateAccountKey = 0, int totalLevel = 0, int consolidationAccountKey = 0, string cashFlowType = "")
		{
			var sqlText = $@"
					INSERT {ScriptDbName}.Finance.BAS__GLAccount
						(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType, AlternateAccountKey, TotalLevel, ConsolidationAccountKey, CashFlowType)
					VALUES
						({accoutKey}, newid(), '{accountTypeCode}', '{number}', 'G', 'ACC{accoutKey}', '{debitCreditCode}', '', '{sectionType}', '{alternateAccountKey}', {totalLevel}, {consolidationAccountKey}, '{cashFlowType}');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertGLAggregate(decimal amount, int period, int glAccountKey, string presentationCategory = "", decimal gLAmountLocalBalance = 0m)
		{
			if (gLAmountLocalBalance == 0)
			{
				gLAmountLocalBalance = amount;
			}

			var sqlText = $@"
					INSERT {ScriptDbName}.Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						({glAccountKey}, {period}, 0, {amount}, 1, '{presentationCategory}', 1, 1, {gLAmountLocalBalance})";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertCompanyBranchAndDepartment()
		{
			TestConnection.ExecuteNonQuery($@"INSERT [{ScriptDbName}].[Organization].[BAS__Company]([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )VALUES(1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'AUD', 'DAU', 1, 1);
				INSERT[{ScriptDbName}].[Organization].[BAS__Branch]([BranchKey], [BranchID], [CompanyKey], [BranchCode])VALUES(1, newid(), 1, 'CBH')
				INSERT[{ScriptDbName}].[Organization].[BAS__Department]([DepartmentKey], [DepartmentID], [Code])VALUES(1, newid(), 'AU1')");
		}

		public void InsertBASAccount(long gLAccountKey, string accountType = "GL_PL_APPROPRIATION_ACCOUNT")
		{
			TestConnection.ExecuteNonQuery($@"INSERT {ScriptDbName}.Customs.BAS__Account(AccountID, AccountKey, AccountType, GLAccountKey)VALUES(newid(), 1, '{accountType}', {gLAccountKey})");
		}

		public Guid InsertAccountDescriptor(int glAccountDescriptorKey = 1, string debitCredit = "DR", int gLAccountKey = 1, string language = "", string localAccountNumber = "1234", string reportCategory = "HDR", string reportType = "COA", int totalLevel = 0
		, int alternativeDescriptorKey = 0, int carriedForwardDescriptorKey = 0, int consolidationDescriptorKey = 0, string countryOfCompliance = "")
		{
			var gLAccountDescriptorID = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"INSERT {ScriptDbName}.Finance.BAS__GLAccountDescriptor([DebitCredit], [GLAccountDescriptorID], [GLAccountDescriptorKey], [GLAccountKey]
				, [Language], [LocalAccountNumber], [ReportCategory], [ReportType], [TotalLevel], [AlternativeDescriptorKey], [CarriedForwardDescriptorKey]
				, [ConsolidationDescriptorKey], [CountryOfCompliance])
			VALUES
				('{debitCredit}', '{gLAccountDescriptorID}', {glAccountDescriptorKey}, {gLAccountKey}, '{language}', '{localAccountNumber}', '{reportCategory}', '{reportType}', '{totalLevel}', {alternativeDescriptorKey}
				, {carriedForwardDescriptorKey}, {consolidationDescriptorKey}, '{countryOfCompliance}')");

			return gLAccountDescriptorID;
		}

		public void InsertAccountDescriptorPivot(int gLAccountDescriptorKey = 1, int gLAccountKey = 1, int gLDescriptorPivotKey = 1)
		{
			TestConnection.ExecuteNonQuery($@"
			INSERT {ScriptDbName}.Finance.BAS__GLDescriptorPivot(GLAccountDescriptorKey, GLAccountKey, GLDescriptorPivotID, GLDescriptorPivotKey)
			VALUES
				({gLAccountDescriptorKey}, {gLAccountKey}, newid(), {gLDescriptorPivotKey})");
		}

		public void InsertPeriodManagement(int period, DateTime startDate)
		{
			TestConnection.ExecuteNonQuery($@"
			INSERT {ScriptDbName}.Finance.[BAS__PeriodManagement](CompanyKey, StartDate, PeriodManagementID, PeriodManagementKey, Period)
			VALUES
				(1, '{startDate}', newid(), 1, {period})");
		}

		public void InsertStmData(DateTime dateTime, int companyKey = 1)
		{
			var startDate = dateTime.ToString("yyyy-MM-dd");
			TestConnection.ExecuteNonQuery($@"
			DELETE FROM {ScriptDbName}.[Finance].[BAS__StmDataDate] WHERE CompanyKey = {companyKey};
			INSERT {ScriptDbName}.Finance.[BAS__StmDataDate](CompanyID, StmDataDateID, StmDataDateKey, Name, Value, CompanyKey)
			VALUES
				('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', newid(), 1, 'JournalEntriesLastProcessedDate', '{startDate}', {companyKey})");
		}

		public void InsertLocalNumberFormatData(string countryCode = "CN", string language = "ZH-CN", string formatStr = "4-2-2")
		{
			var stmXmlInfo = $@"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfGLLocalNumberFormat>
<GLLocalNumberFormat><Language>{language}</Language><CountryCode>{countryCode}</CountryCode><NumberFormat>{formatStr}</NumberFormat><IsFixedLength>N</IsFixedLength></GLLocalNumberFormat>
</ArrayOfGLLocalNumberFormat>";

			TestConnection.ExecuteNonQuery($@"
			INSERT [{ScriptDbName}].[Finance].[BAS__LocalNumberFormatData]
					( [LocalNumberFormatDataID], [LocalNumberFormatDataKey], [LocalNumberFormatDataValue])
					VALUES
						(newid(), 1, '{stmXmlInfo}')");
		}

		public void InsertBASAggregate(int glAccountKey = 1, int period = 202301, decimal amount = 30M, string transactionCategory = "")
		{
			TestConnection.ExecuteNonQuery($@"
			INSERT [{ScriptDbName}].[Finance].[BAS__GLAggregate]
			([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
			VALUES
				(1, newid(), {glAccountKey}, {period}, {amount}, 1, '{transactionCategory}', 1, 1)");
		}

		public void InsertPeriodForInputYear(int year)
		{
			var sqlText = new StringBuilder();
			for (var i = 1; i <= 12; i++)
			{
				sqlText.AppendLine(GetInsertAccPeriodScript(year, i));
			}

			TestConnection.ExecuteNonQuery(sqlText.ToString());
		}

		string GetInsertAccPeriodScript(int year, int month, Guid? companyId = null)
		{
			var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
			var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");
			companyId = companyId ?? Guid.NewGuid();
			var sqlText = $@"INSERT {ScriptDbName}.Finance.BAS__PeriodManagement ([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate], [Period], [Year], [CompanyID]) VALUES (1, newid(), 1, '{startDate}', '{endDate}', {year * 100 + month}, {year}, '{companyId}');";

			return sqlText;
		}

		public void InsertBASGLAggregate(decimal amount, int period, int glAccountKey, int companyKey = 1, int departmentKey = 1, int branchKey = 1, string presentationCategory = "")
		{
			var sqlText = $@"
					INSERT [{ScriptDbName}].[Finance].[BAS__GLAggregate]
						([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						((SELECT ISNull(MAX(GLAggregateKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__GLAggregate]),
						newid(),
						{glAccountKey},
						{period},
						{amount},
						{companyKey},
						'{presentationCategory}',
						{departmentKey},
						{branchKey});";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertPeriodManagement(int periodManagementKey, int year, int month)
		{
			var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
			var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");

			var sqlText = $@"
					INSERT {ScriptDbName}.Finance.BAS__PeriodManagement
						([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						({periodManagementKey}, newid(), 1, '{startDate}', '{endDate}',  {year * 100 + month}, {year})";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertCUSGeneralLedgerTransactionData(string dueDate, string postDate, int amountLocalCredit, int postPeriod, int transactionDataKey = 1, int generalLedgerDataKey = 1, int transactionLineKey = 1, int glAccountKey = 1, int transactionHeaderKey = 1, string ledgerCode = "AR")
		{
			var sqlText = $@"
					INSERT {ScriptDbName}.Finance.CUS__GeneralLedgerTransactionData
						([GeneralLedgerDataKey], [GeneralLedgerTransactionDataKey], [TransactionLineKey], [BranchKey], [CompanyKey], [DepartmentKey], [GLAccountKey], [TransactionHeaderKey], [DueDate], [PostDate], [LedgerCode], [GLAmountLocalCredit], [PostPeriod])
					VALUES
						('{generalLedgerDataKey}', '{transactionDataKey}', {transactionLineKey}, '1', '1', '1', {glAccountKey}, {transactionHeaderKey}, '{dueDate}', '{postDate}', '{ledgerCode}', {amountLocalCredit}, {postPeriod});";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		public Guid InsertCompany(int companyKey, string companyCode, string name, string currency, string countryCode, bool isReciprocal, bool isGSTRegistered)
		{
			var companyPK = Guid.NewGuid();
			name = $"[{companyCode.Trim()}] {name}";
			var sqlText = $@"INSERT INTO {ScriptDbName}.Organization.BAS__Company ([CompanyKey], [CompanyID], [CompanyCode], [Name], [LocalCurrency], [CountryCode], [IsReciprocal], [IsGSTRegistered]) VALUES ({companyKey}, '{companyPK.ToString()}', '{companyCode.Trim()}', '{name}', '{currency.Trim()}', '{countryCode.Trim()}', {(isReciprocal ? 1 : 0)}, {(isGSTRegistered ? 1 : 0)});";
			TestConnection.ExecuteNonQuery(sqlText);
			return companyPK;
		}

		#region Branch

		public Guid InsertBranch(int branchKey, string code, int companyKey, string branchName = null)
		{
			branchName = branchName ?? string.Empty;
			var branchPK = Guid.NewGuid();
			var sqlText = $@"INSERT INTO {ScriptDbName}.Organization.BAS__Branch ([BranchKey], [BranchID], [BranchCode], [CompanyKey], [BranchShortName]) VALUES ({branchKey}, '{branchPK}', '{code}', {companyKey}, '{branchName}');";
			TestConnection.ExecuteNonQuery(sqlText);

			return branchPK;
		}

		#endregion

		public void CreateOrganisation(int organizationKey, string code, string name, string closestPort = "", Guid? orgHeaderPK = null)
		{
			var organisationPK = orgHeaderPK ?? Guid.NewGuid();
			var sqlText = $@"INSERT INTO {ScriptDbName}.Organization.BAS__Organization ([OrganizationKey], [OrganizationID], [Code], [FullName], [ClosestPort], [SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditUser]) VALUES ({organizationKey}, '{organisationPK.ToString()}', '{code.TrimEnd()}', '{name}', '{closestPort.Trim()}', '{DateTime.UtcNow.ToString("yyyy-MM-dd")}', '~BP', '~BP')";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertAccPeriodScriptWithCompanyId(int year, int month, Guid companyId)
		{
			TestConnection.ExecuteNonQuery(GetInsertAccPeriodScript(year, month, companyId));
		}

		public void InsertAccTransactionHeader(int glTransactionHeaderKey, string glTransactionHeaderID, string ledgerCode, string transactionTypeCode, string transactionNo, DateTime invoiceDate, DateTime dueDate, char postToGL, decimal localAmount, decimal overseasAmount, string currency, decimal exchangeRate, DateTime postDate, int organizationHeaderKey, int companyKey, int branchKey, int departmentKey, string transactionCategory)
		{
			var sqlText = $@"INSERT INTO {ScriptDbName}.Finance.BAS__GLTransactionHeader(GLTransactionHeaderKey, GLTransactionHeaderID, LedgerCode, TransactionTypeCode, TransactionNo, InvoiceDateTime, DueDateTime, PostToGL, LocalAmount, OverseasAmount, Currency, ExchangeRate, PostDateTime, OrganizationHeaderKey, CompanyKey, BranchKey, DepartmentKey, TransactionCategory) VALUES ({glTransactionHeaderKey}, '{glTransactionHeaderID}', '{ledgerCode}', '{transactionTypeCode.Trim()}', '{transactionNo}', '{invoiceDate.ToString("yyyy-MM-dd")}', '{dueDate.ToString("yyyy-MM-dd")}',  '{postToGL}', {localAmount}, {overseasAmount}, '{currency.Trim()}', {exchangeRate}, '{postDate.ToString("yyyy-MM-dd")}', {organizationHeaderKey}, {companyKey}, {branchKey}, {departmentKey}, '{transactionCategory}')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
