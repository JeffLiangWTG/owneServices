using System;
using System.Text;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	class CreateEDWDataHelper
	{
		readonly DbConnection TestConnection;
		readonly string ScriptDbName;

		public CreateEDWDataHelper(DbConnection dbConnection, string scriptDbName)
		{
			TestConnection = dbConnection;
			ScriptDbName = scriptDbName;
		}

		public void PrepareTestData()
		{
			var companyKey = 1;
			var gLAccountKey1 = 1;
			var gLAccountKey2 = 2;

			CreateBASCompany(companyKey, CurrentCompany);
			CreateBASBranch(1, Guid.NewGuid(), companyKey);
			CreateBASDepartment(1, Guid.NewGuid());
			CreateBASGLAccount(gLAccountKey1, "1000.10.10", "P&L");
			CreateBASGLAccount(gLAccountKey2, "2000.10.10", "BSH");
			CreateGRPGeneralLedgerAggregateData(gLAccountKey1, 202311, 1m, 0);
			CreateGRPGeneralLedgerAggregateData(gLAccountKey2, 202311, 2m, 0);

			InsertPeriodForInputYear(2023);
			CreateBASStmDataDate("JournalEntriesLastProcessedDate", "2023-01-01 00:00:00.000", CurrentCompany);
			CreateBASCurrency();
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

		string GetInsertAccPeriodScript(int year, int month)
		{
			var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
			var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");
			return $@"
					INSERT {ScriptDbName}.Finance.BAS__PeriodManagement
						([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '{startDate}', '{endDate}', {year * 100 + month}, {year});";
		}

		public void CreateBASStmDataDate(string name, string value, Guid companyId, int companyKey = 1)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
					VALUES
						(1, newid(), '{name}', '{companyId}', '{value}', {companyKey});";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASGLAccount(int gLAccoutKey, string accountNo, string accountTypeCode = "P&L", string debitCreditCode = "CR", string units = "")
		{
			var sqlText = $@"
					INSERT [{ScriptDbName}].[Finance].[BAS__GLAccount]
						(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType)
					VALUES
						({gLAccoutKey}, newid(), '{accountTypeCode}', '{accountNo}', 'G', 'ACC{gLAccoutKey}', '{debitCreditCode}', '{units}', 'AP');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASAccount(string accountType, int gLAccountKey)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Customs].[BAS__Account]
					(AccountID, AccountKey, AccountType, GLAccountKey)
				VALUES
					(newid(), 1, '{accountType}', {gLAccountKey})";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateGRPGeneralLedgerAggregateData(int gLAccountKey, int period, decimal localCredit, decimal localDebit, int companyKey = 1, int departmentKey = 1, int branchKey = 1, string transactionCategory = "")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						('{gLAccountKey}', {period}, {localCredit}, {localDebit}, {localDebit}-{localCredit}, {companyKey}, '{transactionCategory}', {departmentKey}, {branchKey});";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASCompany(int companyKey, Guid companyID, string companyCode = "AUD")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
				VALUES
					({companyKey}, '{companyID}', '{companyCode}', 'DAU', 1, 1);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASBranch(int branchKey, Guid branchPK, int companyKey, string code = "SYN")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						({branchKey}, '{branchPK}' , {companyKey}, '{code}', 1);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASDepartment(int departmentKey, Guid pk, string code = "AU1")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						({departmentKey}, '{pk}', '{code}');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASCurrency()
		{
			var sqlText = $@"
				INSERT {ScriptDbName}.Finance.BAS__Currency
				([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio])
					VALUES
						(1, newid(), 'AUD' , 100)";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertAccountDescriptor(int glAccountDescriptorKey = 1, string debitCredit = "DR", int glAccountKey = 1, string language = "", string localAccountNumber = "1234", string reportCategory = "HDR", string reportType = "COA", int totalLevel = 0
			, int alternativeDescriptorKey = 0, int carriedForwardDescriptorKey = 0, int consolidationDescriptorKey = 0, string countryOfCompliance = "")
		{
			TestConnection.ExecuteNonQuery($@"INSERT {ScriptDbName}.Finance.BAS__GLAccountDescriptor([DebitCredit], [GLAccountDescriptorID], [GLAccountDescriptorKey], [GLAccountKey]
				, [Language], [LocalAccountNumber], [ReportCategory], [ReportType], [TotalLevel], [AlternativeDescriptorKey], [CarriedForwardDescriptorKey]
				, [ConsolidationDescriptorKey], [CountryOfCompliance])
			VALUES
				('{debitCredit}', newid(), {glAccountDescriptorKey}, {glAccountKey}, '{language}', '{localAccountNumber}', '{reportCategory}', '{reportType}', '{totalLevel}', {alternativeDescriptorKey},
				{carriedForwardDescriptorKey}, {consolidationDescriptorKey}, '{countryOfCompliance}')");
		}

		public void InsertAccountDescriptorPivot(int glAccountDescriptorKey = 1, int glAccountKey = 1)
		{
			TestConnection.ExecuteNonQuery($@"
			INSERT {ScriptDbName}.Finance.BAS__GLDescriptorPivot(GLAccountDescriptorKey, GLAccountKey, GLDescriptorPivotID, GLDescriptorPivotKey)
			VALUES
				({glAccountDescriptorKey}, {glAccountKey}, newid(), 1)");
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

		public Guid CurrentCompany
		{
			get
			{
				if (currentCompany == Guid.Empty)
				{
					currentCompany = Guid.NewGuid();
				}
				return currentCompany;
			}
		}
		Guid currentCompany;
	}
}
