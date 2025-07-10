using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	public class AccountingFunctionTestingHelper
	{
		public AccountingFunctionTestingHelper(DbConnection testConnection, string scriptDbName)
		{
			ScriptDbName = scriptDbName;
			TestConnection = testConnection;
		}

		readonly string ScriptDbName;
		readonly DbConnection TestConnection;

		public void InsertStmDataDate(string dateType, DateTime dateTime, long companyKey)
		{
			var sqlText = $@"
				DELETE [{ScriptDbName}].[Finance].[BAS__StmDataDate] WHERE CompanyKey = {companyKey} AND [Name] = '{dateType}'
				INSERT [{ScriptDbName}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
				VALUES
					(
						(SELECT ISNull(MAX(StmDataDateKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__StmDataDate]),
						newid(),
						'{dateType}',
						(SELECT TOP 1 CompanyID FROM [{ScriptDbName}].[Organization].[BAS__Company] WHERE CompanyKey = {companyKey}),
						'{dateTime.ToString("yyyy-MM-dd")}',
						{companyKey}
					);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void DeleteStmDataDate(string dateType, long companyKey)
		{
			var sqlText = $@"
				DELETE FROM [{ScriptDbName}].[Finance].[BAS__StmDataDate]
				WHERE
					[Name] = '{dateType}' AND [CompanyKey] = '{companyKey}';";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertCashFlowCategoryBasedOnCreditorGroup(string cashFlowCategory, Guid creditorGroupPK)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__CashFlowCategoryBasedOnCreditorGroup]
					([CashFlowCategory], [CreditorGroupPK])
					VALUES
						('{cashFlowCategory}', '{creditorGroupPK}');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertGLAggregate(decimal amount, int period, int glAccountKey, int companyKey = 1, int departmentKey = 1, int branchKey = 1, string presentationCategory = "")
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

		public void InsertGLAggregateForGLD(decimal localDebitAmount, int period, int glAccountKey, int companyKey = 1, int departmentKey = 1, int branchKey = 1, string presentationCategory = "", decimal localCreditAmount = 0m)
		{
			var sqlText = $@"
					INSERT {ScriptDbName}.Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						({glAccountKey}, {period}, {localCreditAmount}, {localDebitAmount}, {localDebitAmount - localCreditAmount}, {companyKey}, '{presentationCategory}', {departmentKey}, {branchKey})";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertPeriodForInputYear(int year, int companyKey = 1)
		{
			var sqlText = new StringBuilder();
			for (var i = 1; i <= 12; i++)
			{
				sqlText.AppendLine(GetInsertAccPeriodScript(year, i, companyKey));
			}

			TestConnection.ExecuteNonQuery(sqlText.ToString());
		}

		string GetInsertAccPeriodScript(int year, int month, int companyKey = 1)
		{
			var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
			var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");
			return $@"
					INSERT {ScriptDbName}.Finance.BAS__PeriodManagement
						([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate], [Period], [Year], [CompanyID])
					VALUES
						((SELECT ISNull(MAX(PeriodManagementKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__PeriodManagement), newid(), {companyKey}, '{startDate}', '{endDate}', {year * 100 + month}, {year}, (select TOP 1 companyID from {ScriptDbName}.[Organization].[BAS__Company] WHERE CompanyKey = {companyKey}));";
		}

		public void InsertPeriodManagement(int year, int month, long companyKey)
		{
			var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
			var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__PeriodManagement]
					([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [CompanyID], [StartDate], [EndDate], [Period], [Year])
					VALUES
						((SELECT ISNull(MAX(PeriodManagementKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__PeriodManagement), newid(), {companyKey},
						(Select CompanyID FROM [{ScriptDbName}].[Organization].[BAS__Company] WHERE CompanyKey = {companyKey}),
						'{startDate}', '{endDate}', {year * 100 + month}, {year});";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void CreateBASBranch(int branchKey, Guid branchPK, int companyKey, string code = "SYN")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey], [CompanyID])
					VALUES
						({branchKey}, '{branchPK}' , {companyKey}, '{code}', 1, (Select CompanyID from [{ScriptDbName}].[Organization].[BAS__Company] where CompanyKey = {companyKey}));";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public long InsertBranchAndGetBranchKey(Guid branchPK, long companyKey, string code = "SYN")
		{
			var sqlText = $@"
				DECLARE @BranchKey BIGINT;
				SET @BranchKey = (SELECT ISNULL(MAX(BranchKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__Branch]);

				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey], [CompanyID])
					VALUES
						(@BranchKey, '{branchPK}' , {companyKey}, '{code}', 1, (Select CompanyID from [{ScriptDbName}].[Organization].[BAS__Company] where CompanyKey = {companyKey}));

				 SELECT @BranchKey;";

			return TestConnection.ExecuteScalar<long>(sqlText);
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

		public long CreateBASDepartment(Guid departmentPk, string code = "AU1")
		{
			var sqlText = $@"
				DECLARE @DepartmentKey BIGINT;
				SET @DepartmentKey = (SELECT ISNULL(MAX(DepartmentKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__Department]);

				INSERT [{ScriptDbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
				VALUES
					(@DepartmentKey, '{departmentPk}', '{code}');

				SELECT @DepartmentKey;";

			return TestConnection.ExecuteScalar<long>(sqlText);
		}

		internal void InsertALternateChart(string alternateChartCode, string balanceSheetStyle = "ELA", string description = "", int isFixedLength = 1, int isGlobal = 0, string reportOrder = "PTB", Guid? secondAccountID = null, Guid? companyID = null)
		{
			secondAccountID = secondAccountID == null ? Guid.Empty : secondAccountID;
			companyID = companyID == null ? Guid.Empty : companyID;

			var sqlText = $@"
				INSERT [{ScriptDbName}].Finance.BAS__AlternateChart
					([AlternateChartKey], [AlternateChartID], [AlternateChartCode], [BalanceSheetStyle], [Description], [IsFixedLength], [IsGlobal], [ReportOrder], [SecondReportStartAccountID], CompanyKey, CompanyID)
					VALUES
						((SELECT ISNull(MAX(AlternateChartKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__AlternateChart),
						newid(),
						'{alternateChartCode}',
						'{balanceSheetStyle}',
						'{description}',
						{isFixedLength},
						{isGlobal},
						'{reportOrder}',
						'{secondAccountID}',
						(select companyKey from [{ScriptDbName}].Organization.BAS__Company where companyID = '{companyID}'),
						'{companyID}'
						);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal void InsertAlternateChartCurrencyTranslation(int alternateChartKey, int alternateGlAccountKey, string accountType = "P&L", string currencyTranslationLevel = "JNL", string rateType = "BUY")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].Finance.BAS__AlternateChartCurrencyTranslation
					([AlternateChartCurrencyTranslationID], [AlternateChartCurrencyTranslationKey], [AccountType], [AlternateChartKey], [CurrencyTranslationLevel], [ExRateType], [AlternateGLAccountKey])
					VALUES
						(newid(),
						(SELECT ISNull(MAX(AlternateChartCurrencyTranslationKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__AlternateChartCurrencyTranslation),
						'{accountType}',
						{alternateChartKey},
						'{currencyTranslationLevel}',
						'{rateType}',
						(select AlternateGLAccountKey from [{ScriptDbName}].Finance.BAS__AlternateGLAccount where AlternateGLAccountKey = {alternateGlAccountKey})
						);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal void InsertExchangeRate(int companyKey, int? organizationKey, string startDate, string endDate, decimal sellRate = 1.0m, string rateType = "BUY", string currency = "CNY", string asPublished = "1")
		{
			var orgKey = organizationKey == null ? "null" : organizationKey.ToString();
			var sqlText = $@"
				INSERT [{ScriptDbName}].Finance.BAS__ExchangeRate
					([ExchangeRateID], [ExchangeRateKey], [CompanyKey], [StartDate], [ExpiryDate], [ExRateType], [OrganizationKey], [RX_NKExCurrency], [SellRate], [AsPublished])
					VALUES
						(newid(),
						(SELECT ISNull(MAX(ExchangeRateKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__ExchangeRate),
						{companyKey},
						'{startDate}',
						'{endDate}',
						'{rateType}',
						{orgKey},
						'{currency}',
						{sellRate},
						'{asPublished}'
						);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal void InsertALternateChartFormat(string format, int alternaterChartKey = 1, string separator = ".", int tier = 1)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].Finance.BAS__AlternateChartFormat
					([AlternateChartFormatKey], [AlternateChartFormatID], [AlternateChartKey], [Format], [Separator], [Tier])
					VALUES
						((SELECT ISNull(MAX(AlternateChartFormatKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__AlternateChartFormat),
						newid(),
						'{alternaterChartKey}',
						'{format}',
						'{separator}',
						'{tier}'
						);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal void InsertGLAccount(string accountNo, string accountType = "BSH", string accountGroup = "1", string description = "desc", string debitCredit = "DR", string units = "KG", string cashFlowType = "YYY")
		{
			var sqlText = $@"INSERT[{ScriptDbName}].[Finance].[BAS__GLAccount]
			([GLAccountKey],[GLAccountID],[AccountTypeCode],[AccountNo],[AccountGroup],[Description],[DebitCredit],[Units], [CashFlowType])
			VALUES
				((SELECT ISNull(MAX(GLAccountKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__GLAccount), newid(), '{accountType}', '{accountNo}', '{accountGroup}', '{description}', '{debitCredit}', '{units}', '{cashFlowType}')
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal long InsertGLAccountAndGetGLAccountKey(string accountNo, Guid glAccountID, string accountType = "BSH", string accountGroup = "1", string description = "desc", string debitCredit = "DR", string units = "KG", string cashFlowType = "")
		{
			var sqlText = $@"
			DECLARE @GLAccountKey BIGINT;
			SET @GLAccountKey = (SELECT ISNull(MAX(GLAccountKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__GLAccount);

			INSERT[{ScriptDbName}].[Finance].[BAS__GLAccount]
			([GLAccountKey], [GLAccountID], [AccountTypeCode], [AccountNo], [AccountGroup], [Description], [DebitCredit], [Units], [CashFlowType])
			VALUES
				(@GLAccountKey, '{glAccountID}', '{accountType}', '{accountNo}', '{accountGroup}', '{description}', '{debitCredit}', '{units}', '{cashFlowType}');

			SELECT @GLAccountKey;
			";

			return TestConnection.ExecuteScalar<long>(sqlText);
		}

		internal long InsertOrgCreditorGroupAndGetOrgCreditorGroupKey(Guid orgCreditorGroupID)
		{
			var sql = $@"
			DECLARE @OrgCreditorGroupKey BIGINT;
			SET @OrgCreditorGroupKey = (SELECT ISNULL(MAX(OrgCreditorGroupKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__OrgCreditorGroup]);

			INSERT[{ScriptDbName}].[Organization].[BAS__OrgCreditorGroup]
			([OrgCreditorGroupID],[OrgCreditorGroupKey])
			VALUES
				('{orgCreditorGroupID}', @OrgCreditorGroupKey);

			SELECT @OrgCreditorGroupKey;
			";

			return TestConnection.ExecuteScalar<long>(sql);
		}

		internal Guid InsertAlternateGLAccount(string accountNum, string accountType, int alternateChartKey = 1, int alternateNumKey = 1, int consolidateKey = 1, string debitCredit = "DR", string description = "xxx", int headerTotalKey = 1, int percentKey = 1, int printSequence = 1, string reportSection = "AL", int totalLevel = 1, int alternateGLAccountKey = -1)
		{
			var pk = Guid.NewGuid();
			var sqlText = $@"INSERT[{ScriptDbName}].[Finance].[BAS__AlternateGLAccount]
			(AlternateGLAccountKey, AlternateGLAccountID, AccountNum, AccountType, AlternateChartKey, AlternateNumKey, ConsolidateNumKey, DebitCredit, Description, HeaderDependsonTotalKey, PercentnumKey, PrintSequence, ReportSection, TotalLevel)
					VALUES
						((SELECT ISNull(MAX(AlternateGLAccountKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__AlternateGLAccount), '{pk}',
						'{accountNum}', '{accountType}',
						{alternateChartKey}, {alternateNumKey}, {consolidateKey}, '{debitCredit}', '{description}', {headerTotalKey}, {percentKey}, {printSequence}, '{reportSection}', {totalLevel})
			";
			TestConnection.ExecuteNonQuery(sqlText);

			if (alternateGLAccountKey > 0)
			{
				sqlText = $@"UPDATE [{ScriptDbName}].[Finance].[BAS__AlternateGLAccount] set AlternateGLAccountKey = {alternateGLAccountKey} where AlternateGLAccountID = '{pk}'";
				TestConnection.ExecuteNonQuery(sqlText);
			}
			return pk;
		}

		internal void InsertCUSAlternateAccountAttributeInfo(int alternateChartKey, int glAccountKey, int alternateGLAccountKey, int sequence, string attributeORG = "", string attributeOCG = "", string attributeLFO = "", string attributeLFE = "", string attributeTIC = "", string attributeSPR = "", string cashFlowType = "XXX", string units = "KG", string parentGLAccountNo = "223344", string parentGLAccountName = "Control Account A", int organizationKey = 1)
		{
			var sqlText = $@"INSERT[{ScriptDbName}].[Finance].[CUS__AlternateGLAccountAttributeInfo]
			(AlternateGLAccountAttributeInfoKey, AlternateChartKey, GLAccountKey, Sequence, AlternateGlAccountKey, Attribute_ORG, Attribute_SPR, Attribute_OCG, Attribute_TIC, Attribute_LFE, Attribute_LFO, CashFlowType, Units, ParentGLAccountNo, ParentGLAccountName, OrganizationKey)
			VALUES
			((SELECT ISNull(MAX(AlternateGLAccountAttributeInfoKey), 0) + 1 FROM [{ScriptDbName}].Finance.CUS__AlternateGLAccountAttributeInfo),
			{alternateChartKey}, {glAccountKey}, {sequence}, {alternateGLAccountKey}, '{attributeORG}', '{attributeSPR}', '{attributeOCG}', '{attributeTIC}', '{attributeLFE}', '{attributeLFO}','{cashFlowType}', '{units}', '{parentGLAccountNo}', '{parentGLAccountName}', {organizationKey})
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal void InsertAlternateAccountAttribute(int alternateChartKey, int alternateGLAccountKey, string attributeName, string attributeValue, int glHeaderKey, int sequence, Guid attributeID)
		{
			var pk = Guid.NewGuid();
			var sqlText = $@"INSERT[{ScriptDbName}].[Finance].[BAS__AlternateGLAccountAttribute]
			(AlternateGLAccountAttributeKey, AlternateGLAccountAttributeID, AlternateChartKey, AlternateGLAccountKey, AttributeName, AttributeValue, GLHeaderKey, Sequence, AttributeValueID)
					VALUES
						((SELECT ISNull(MAX(AlternateGLAccountAttributeKey), 0) + 1 FROM [{ScriptDbName}].Finance.BAS__AlternateGLAccountAttribute), '{pk}', {alternateChartKey}, {alternateGLAccountKey},
						'{attributeName}', '{attributeValue}', {glHeaderKey}, {sequence}, '{attributeID}')
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal long InsertAddress(string code, int organizationKey = 1, string countryCode = "CN", string address1 = "address1", string address2 = "address2", string city = "NJ", string state = "CN", string postCode = "112211", string createTime = "2023-09-08", int isActive = 1, string portCode = "", string language = "")
		{
			var sql = $@"
					DECLARE @OrganizationAddressKey BIGINT;
					SET @OrganizationAddressKey = (SELECT ISNULL(MAX(OrganizationAddressKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__OrganizationAddress]);

					INSERT INTO [{ScriptDbName}].[Organization].[BAS__OrganizationAddress] (OrganizationKey, Code, CountryCode, Address1, Address2, City, State, PostCode, OrganizationAddressID, OrganizationAddressKey, SystemCreateTimeUtc, isActive, RelatedPortCode, Language)
							VALUES ({organizationKey}, '{code}', '{countryCode}', '{address1}', '{address2}', '{city}', '{state}', '{postCode}', newID(), @OrganizationAddressKey, '{createTime}', {isActive}, '{portCode}', '{language}');

					SELECT @OrganizationAddressKey;";
			return TestConnection.ExecuteScalar<long>(sql);
		}

		internal void InsertOrganizationAddressCapability(long organizationAddressKey, string addressType = "OFC", int isMainAddress = 1)
		{
			var sql = $@"INSERT INTO [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability] (OrganizationAddressKey, AddressType, IsMainAddress, OrganizationAddressCapabilityID, OrganizationAddressCapabilityKey) VALUES
				({organizationAddressKey}, '{addressType}', {isMainAddress}, newID(), (SELECT ISNull(MAX(OrganizationAddressCapabilityKey), 0) + 1 FROM [{ScriptDbName}].[Organization].BAS__OrganizationAddressCapability))";

			TestConnection.ExecuteNonQuery(sql);
		}

		internal void InsertCountry(string code = "CN", string country = "China", string localCurrency = "CNY", string economicGrouping = "EUN")
		{
			var sql = $@"INSERT INTO [{ScriptDbName}].[Geography].[BAS__Country]([Code], [Country], [CountryID], [CountryKey], [LocalCurrency], [EconomicGrouping])
			VALUES('{code}', '{country}', newID(), (SELECT ISNull(MAX(CountryKey), 0) + 1 FROM [{ScriptDbName}].[Geography].BAS__Country), '{localCurrency}', '{economicGrouping}')";
			TestConnection.ExecuteNonQuery(sql);
		}

		internal Guid InsertCompany(string currency, string companyCode, int isGstCashBasis = 1, int isGSTRegistered = 1, string countryCode = "CN", int isReciprocal = 1)
		{
			var pk = Guid.NewGuid();
			var sql = $@"INSERT [{ScriptDbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered], [CountryCode], [isReciprocal])
					VALUES
						((SELECT ISNull(MAX(CompanyKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__Company]), '{pk}', '{currency}', '{companyCode}', {isGstCashBasis}, {isGSTRegistered}, '{countryCode}', {isReciprocal})";
			TestConnection.ExecuteNonQuery(sql);
			return pk;
		}

		internal long InsertCompanyAndGetCompanyKey(Guid companyPk, string currency, string companyCode, int isGstCashBasis = 1, int isGSTRegistered = 1, int isReciprocal = 1, string countryCode = "CN")
		{
			var sql = $@"
					DECLARE @CompanyKey BIGINT;
					SET @CompanyKey = (SELECT ISNULL(MAX(CompanyKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__Company]);

					INSERT INTO [{ScriptDbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered], [IsReciprocal], [CountryCode] )
					VALUES
						(@CompanyKey, '{companyPk}', '{currency}', '{companyCode}', {isGstCashBasis}, {isGSTRegistered}, {isReciprocal}, '{countryCode}');

					SELECT @CompanyKey;";
			return TestConnection.ExecuteScalar<long>(sql);
		}

		internal int InsertOrganization(string code, Guid orgId)
		{
			var sql = $@"
					DECLARE @OrganizationKey INT;
					SET @OrganizationKey = (SELECT ISNULL(MAX(OrganizationKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__Organization]);

					INSERT INTO [{ScriptDbName}].[Organization].[BAS__Organization]
						([OrganizationKey], [OrganizationID], [Code])
					VALUES
						(@OrganizationKey, '{orgId}', '{code}');

					SELECT @OrganizationKey;";

			return Convert.ToInt32(TestConnection.ExecuteScalar(sql));
		}

		internal long CreateGLTransaction(string ledgerCode, long orgHeaderKey, string transactionCategory, string transactionTypeCode, long isCancelled = 0, long jobKey = 0)
		{
			var sql = $@"
				DECLARE @GLTransactionHeaderKey BIGINT;
				SET @GLTransactionHeaderKey = (SELECT ISNULL(MAX(GLTransactionHeaderKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]);

				INSERT [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [LedgerCode], [OrganizationHeaderKey], [TransactionCategory], [TransactionTypeCode], [IsCancelled], [JobHeaderKey])
					VALUES
						(@GLTransactionHeaderKey, newid(), '{ledgerCode}', {orgHeaderKey}, '{transactionCategory}', '{transactionTypeCode}',
						{isCancelled}, {jobKey});

				SELECT @GLTransactionHeaderKey;";

			return TestConnection.ExecuteScalar<long>(sql);
		}

		internal int InsertOrganizationAndGetOrganizationKey(string code, Guid orgId)
		{
			var sql = $@"
			DECLARE @OrganizationKey INT;
			SET @OrganizationKey = (SELECT ISNULL(MAX(OrganizationKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__Organization]);

			INSERT INTO [{ScriptDbName}].[Organization].[BAS__Organization]
				([OrganizationKey], [OrganizationID], [Code])
			VALUES
				(@OrganizationKey, '{orgId}', '{code}');

			SELECT @OrganizationKey;";

			return Convert.ToInt32(TestConnection.ExecuteScalar(sql));
		}

		internal void InsertGLHeaderSubAccount(int glAccountKey, string subClass)
		{
			var sql = $@"
			INSERT INTO [{ScriptDbName}].[Finance].[BAS__GLHeaderSubAccount]
				([SubClass], [GLHeaderSubAccountKey], [GLHeaderSubAccountID], [GLAccountKey])
			VALUES
				('{subClass}', (SELECT ISNULL(MAX(GLHeaderSubAccountKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__GLHeaderSubAccount]),
				newid(), {glAccountKey});
			";

			TestConnection.ExecuteNonQuery(sql);
		}

		internal void InsertTransactionLineSubAccount(int lineKey, string subClassCode, Guid subParentID)
		{
			var sql = $@"
			INSERT INTO [{ScriptDbName}].[Finance].[BAS__TransactionLineSubAccount]
				([TransactionLineSubAccountKey], [TransactionLineSubAccountID], [GLTransactionLineKey], [subclassParentTableCode], [SubClassParentID])
			VALUES
				((SELECT ISNULL(MAX(TransactionLineSubAccountKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__TransactionLineSubAccount]),
				newid(), {lineKey}, '{subClassCode}', '{subParentID}');
			";
			TestConnection.ExecuteNonQuery(sql);
		}

		internal void InsertTransactionHeaderSubAccount(int headerKey, string subClassCode, Guid subParentID)
		{
			var sql = $@"
			INSERT INTO [{ScriptDbName}].[Finance].[BAS__TransactionHeaderSubAccount]
				([TransactionHeaderSubAccountKey], [TransactionHeaderSubAccountID], [GLTransactionHeaderKey], [subclassParentTableCode], [SubClassParentID])
			VALUES
				((SELECT ISNULL(MAX(TransactionHeaderSubAccountKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__TransactionHeaderSubAccount]),
				newid(), {headerKey}, '{subClassCode}', '{subParentID}');
			";
			TestConnection.ExecuteNonQuery(sql);
		}

		public Guid InsertReportingBook(int alternateChartKey, int companyOfPeriodKey, string code = "RRR", string description = "desc", string presentationJournals = "", int isActive = 1, string currency = "CNY", int includeChildPresentation = 0)
		{
			var pk = Guid.NewGuid();
			var sql = $@"INSERT [{ScriptDbName}].[Finance].[BAS__AccReportingBook]
					([AccReportingBookKey], [AccReportingBookID], [AlternateChartKey], [CompanyOfPeriodKey], [Description], [IncludePresentationJournals], [IsActive], [ReportingBookCode], [RX_NKCurrency], [CompanyOfPeriodID], [IncludeChildPresentation])
					VALUES
						((SELECT ISNull(MAX(AccReportingBookKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__AccReportingBook]), '{pk}', {alternateChartKey}, {companyOfPeriodKey}, '{description}',
						'{presentationJournals}', {isActive}, '{code}', '{currency}', (select Top 1 companyID from [{ScriptDbName}].[Organization].[BAS__Company] where companykey = {companyOfPeriodKey}), {includeChildPresentation})";
			TestConnection.ExecuteNonQuery(sql);

			if (companyOfPeriodKey == -1)
			{
				sql = $@"UPDATE [{ScriptDbName}].[Finance].[BAS__AccReportingBook] set companyOfPeriodKey = null where AccReportingBookID = '{pk}'";
				TestConnection.ExecuteNonQuery(sql);
			}

			return pk;
		}

		public void InsertBASAccount(long gLAccountKey, string accountType = "GL_PL_APPROPRIATION_ACCOUNT")
		{
			TestConnection.ExecuteNonQuery($@"INSERT {ScriptDbName}.Customs.BAS__Account(AccountID, AccountKey, AccountType, GLAccountKey) VALUES (newid(), (SELECT ISNull(MAX(AccountKey), 0) + 1 FROM [{ScriptDbName}].Customs.BAS__Account), '{accountType}', {gLAccountKey})");
		}

		public void InsertOrgCompanyData(long companyKey, long organizationKey, string category, long? aPCreditorGroupKey = null, long? aRDebtorGroupKey = null)
		{
			var sql =
				$@"INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationCompanyData] ([OrganizationCompanyDataKey], [OrganizationCompanyDataID], [CompanyKey], [OrganizationHeaderKey], [ConsolidatedAccountingCategory], [APCreditorGroupKey], [ARDebtorGroupKey])
						VALUES ((SELECT ISNull(MAX(OrganizationCompanyDataKey), 0) + 1 FROM [{ScriptDbName}].[Organization].[BAS__OrganizationCompanyData]), newid(), {companyKey}, {organizationKey}, '{category}', {(aPCreditorGroupKey.HasValue ? aPCreditorGroupKey.ToString() : "NULL")}, {(aRDebtorGroupKey.HasValue ? aRDebtorGroupKey.ToString() : "NULL")})";
			TestConnection.ExecuteNonQuery(sql);
		}

		public void InsertConsolidatedAccountingCategory(string code, string classInfo = "TPY", string description = "desc")
		{
			var sql =
				$@"INSERT [{ScriptDbName}].[Finance].[GRP__ConsolidatedAccountingCategory] ([code], [class], [description])
						VALUES ('{code}', '{classInfo}', '{description}')";
			TestConnection.ExecuteNonQuery(sql);
		}

		public void InsertAlternateGLAccountDissection(int glAccountKey, int alternateChartKey, string attribute, int separateNumber)
		{
			var sql =
				$@"INSERT [{ScriptDbName}].[Finance].[BAS__AccAlternateGLAccountDissection] ([AccAlternateGLAccountDissectionKey], [AccAlternateGLAccountDissectionID], [AlternateChartKey], [Attribute], [GLAccountKey], [SeparateNumbering])
						VALUES ((SELECT ISNull(MAX(AccAlternateGLAccountDissectionKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__AccAlternateGLAccountDissection]), newid(), {alternateChartKey}, '{attribute}', {glAccountKey}, {separateNumber})";
			TestConnection.ExecuteNonQuery(sql);
		}

		public void InsertTransactionLineDissectionAttribute(int lineKey, string attribute, string attributeValue, Guid? attributeID)
		{
			var sql =
				string.Format(@"INSERT [{0}].[Finance].[BAS__TransactionLineDissectionAttribute] ([TransactionLineDissectionAttributeKey], [TransactionLineDissectionAttributeID], [AccGLTransactionLineKey], [Attribute], [AttributeValue], [AttributeValueID])
						VALUES ((SELECT ISNull(MAX(TransactionLineDissectionAttributeKey), 0) + 1 FROM [{0}].[Finance].[BAS__TransactionLineDissectionAttribute]), newid(), {1}, '{2}', {3}, {4})",
					ScriptDbName,
					lineKey,
					attribute,
					attributeValue == null ? "null" : "'" + attributeValue + "'",
					attributeID == null ? "null" : "'" + attributeID + "'");
			TestConnection.ExecuteNonQuery(sql);
		}

		internal void InsertCurrency(string currencyCode, int subUnitRatio = 100, string currency = "currency desc", int isSystem = 1)
		{
			var sql =
				string.Format(@"INSERT [{0}].[Finance].[BAS__Currency] ([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio], [Currency], [IsSystemUse])
						VALUES ((SELECT ISNull(MAX(CurrencyKey), 0) + 1 FROM [{0}].[Finance].[BAS__Currency]), newid(), '{1}', {2}, '{3}', {4})",
					ScriptDbName,
					currencyCode,
					subUnitRatio,
					currency,
					isSystem);
			TestConnection.ExecuteNonQuery(sql);
		}

		internal long CreateJobHeader(string jobNo, Guid companyID, int isActive = 1, int shipmentKey = 1)
		{
			var sql = $@"
					DECLARE @JobHeaderKey BIGINT;
					SET @JobHeaderKey = (SELECT ISNULL(MAX(JobHeaderKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__JobHeader]);

					INSERT [{ScriptDbName}].Finance.BAS__JobHeader
					([JobHeaderKey], [JobHeaderID], [JobNo], [CompanyID], IsActive, ParentID)
						VALUES (@JobHeaderKey, newid(), '{jobNo}','{companyID}', {isActive}, (select ShipmentID from [{ScriptDbName}].InternationalLogistics.BAS__Shipment where shipmentKey = {shipmentKey}));

					SELECT @JobHeaderKey;";

			return TestConnection.ExecuteScalar<long>(sql);
		}

		internal void InsertGLTransactionLineExtended(string lineType, Guid companyID, string postDate, string receiveDate, decimal wipAmount = 1m, decimal acrAmount = 0m, decimal revAmount = 0m, decimal cstAmount = 1m, decimal lineAmount = 1m, int jobHeaderKey = 1)
		{
			var sql =
				string.Format(@"INSERT [{0}].[Finance].[CUS__GLTransactionLineExtended] ([GLTransactionLineExtendedKey], [GLTransactionLineID], [WIPAmount], [CSTAmount], [ACRAmount], [REVAmount], [CompanyID], [LineAmount], [LineType], [PostDateTime], [ReverseDateTime], [GLTransactionLineJobHeaderKey])
						VALUES ((SELECT ISNull(MAX(GLTransactionLineExtendedKey), 0) + 1 FROM [{0}].[Finance].[CUS__GLTransactionLineExtended]), newID(), {1}, {2}, {3}, {4}, '{5}', {6}, '{7}', '{8}', {9}, {10})",
					ScriptDbName,
					wipAmount,
					cstAmount,
					acrAmount,
					revAmount,
					companyID,
					lineAmount,
					lineType,
					postDate,
					string.IsNullOrEmpty(receiveDate) ? "NULL" : "'" + receiveDate + "'",
					jobHeaderKey);
			TestConnection.ExecuteNonQuery(sql);
		}

		internal long InsertGLTransactionHeader(AccTransactionHeader header, DateTime dueDateTime, DateTime postDate, string postToGL, long organizationHeaderKey, long companyKey, long branchKey, long departmentKey, string transactionCategory, string currency = "CNY", decimal exchangeRate = 1.0m)
		{
			var sql = string.Format(@"
		DECLARE @TransactionHeaderKey BIGINT;
		SET @TransactionHeaderKey = (SELECT ISNULL(MAX(GLTransactionHeaderKey), 0) + 1 FROM [{0}].[Finance].[BAS__GLTransactionHeader]);

		INSERT INTO [{0}].[Finance].[BAS__GLTransactionHeader] 
			([GLTransactionHeaderKey], [GLTransactionHeaderID], [LedgerCode], [TransactionTypeCode], [TransactionNo], 
			[InvoiceDateTime], [DueDateTime], [PostToGL], [OrganizationHeaderID], [OrganizationHeaderKey], [CompanyKey], 
			[BranchKey], [DepartmentKey], [TransactionCategory], [LocalAmount], [OverseasAmount], 
			[Currency], [PostDateTime], [ExchangeRate])
		VALUES 
			(@TransactionHeaderKey, NEWID(),
			'{1}', '{2}', '{3}', '{4:yyyy-MM-dd HH:mm:ss}', '{5:yyyy-MM-dd HH:mm:ss}', '{6}',
			(SELECT OrganizationID FROM [{0}].[Organization].[BAS__Organization] WHERE OrganizationKey = {7}),'{7}',
			{8}, {9}, {10}, '{11}', {12}, {13}, '{14}', '{15:yyyy-MM-dd HH:mm:ss}', {16});

		SELECT @TransactionHeaderKey;",
				ScriptDbName,
				header.AH_Ledger,
				header.AH_TransactionType,
				header.AH_TransactionNum,
				header.AH_InvoiceDate,
				dueDateTime,
				postToGL,
				organizationHeaderKey,
				companyKey,
				branchKey,
				departmentKey,
				transactionCategory,
				header.AH_InvoiceAmount,
				header.AH_OSTotal,
				currency,
				postDate,
				exchangeRate);

			return TestConnection.ExecuteScalar<long>(sql);
		}

		public void InsertBASCashBasisVAT(long companyKey, long transactionLineKey, string postDate, decimal taxAmount)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__CashBasisVAT]
					([CashBasisVATKey],
					[CashBasisVATID], [CompanyKey], [GLTransactionLineKey], [PostDate], [TaxAmount])
				VALUES
					((SELECT ISNull(MAX(CashBasisVATKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__CashBasisVAT]),
					newid(), {companyKey}, {transactionLineKey}, '{postDate}', {taxAmount})";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void InsertTransactionHeader(AccTransactionHeader header, string chequeOrReference, string currency, string dueDate, string postDate, Guid organizationHeaderID,
			string description = "", decimal exchangeRate = 1, int invoiceAddressOverrideKey = 1, string postToGL = "Y", string transactionCategory = "FIN", long companyKey = 1L, long branchKey = 1L, long departmentKey = 1L)
		{
			var localTotal = header.AH_InvoiceAmount + header.AH_GSTAmount + header.AH_LocalTaxAmountOtherTaxes;
			var invoiceDateStr = header.AH_InvoiceDate.ToString("yyyy-MM-dd HH:mm:ss");
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey],
					[BranchKey], [ChequeOrReference], [CompanyKey], [Currency], [DepartmentID], [DepartmentKey], [Description],
					[DueDate], [ExchangeRate], [GLTransactionHeaderID], [InvoiceAddressOverrideKey], [InvoiceDate], [LedgerCode],
					[LocalAmount], [LocalTotal], [OrganizationHeaderID], [OutstandingAmount], [OverseasAmount], [PostDate], [PostToGL], [TaxAmount],
					[TransactionCategory], [TransactionNo], [TransactionTypeCode])
				VALUES
					((SELECT ISNull(MAX(GLTransactionHeaderKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]),
					{branchKey}, '{chequeOrReference}', {companyKey}, '{currency}', '{header.AH_GE}', {departmentKey}, '{description}',
					'{dueDate}', {exchangeRate}, newid(), {invoiceAddressOverrideKey}, '{invoiceDateStr}', '{header.AH_Ledger}', 
					{header.AH_InvoiceAmount}, {localTotal}, '{organizationHeaderID}', {header.AH_OutstandingAmount}, {header.AH_OSTotal}, '{postDate}', '{postToGL}', {header.AH_GSTAmount},
					'{transactionCategory}', '{header.AH_TransactionNum}', '{header.AH_TransactionType}')";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		internal long InsertAccGLTransactionLine(string lineType, Guid companyID, string postDate, string reverseDate, decimal lineAmount = 1m, int jobHeaderKey = 1, int chargecodeKey = 1)
		{
			var sql =
				string.Format(@"INSERT [{0}].[Finance].[BAS__AccGLTransactionLine] ([AccGLTransactionLineKey], [AccGLTransactionLineID], [CompanyID], [LineAmount], [LineType], [PostDateTime], [ReverseDateTime], [JobHeaderID], ChargeCodeID)
					VALUES ((SELECT ISNull(MAX(AccGLTransactionLineKey), 0) + 1 FROM [{0}].[Finance].[BAS__AccGLTransactionLine]), newID(), '{1}', {2}, '{3}', '{4}', {5},
							(select jobheaderid from [{0}].Finance.BAS__JobHeader where jobheaderkey = {6}),
							(select chargecodeID from [{0}].Finance.BAS__ChargeCode where chargecodeKey = {7}))",
					ScriptDbName,
					companyID,
					lineAmount,
					lineType,
					postDate,
					string.IsNullOrEmpty(reverseDate) ? "NULL" : "'" + reverseDate + "'",
					jobHeaderKey,
					chargecodeKey);
			TestConnection.ExecuteNonQuery(sql);

			return TestConnection.ExecuteScalar<long>(
				$"SELECT ISNull(MAX(AccGLTransactionLineKey), 0) + 1 FROM [{ScriptDbName}].[Finance].[BAS__AccGLTransactionLine]");
		}

		internal void InsertAccGLTransactionLine(long companyKey, long branchKey, long departmentKey, long glAccountKey, long transactionHeaderKey, string lineType, DateTime postDate, decimal lineAmount = 1m, decimal taxAmount = 0m)
		{
			var sql =
				string.Format(@"INSERT [{0}].[Finance].[BAS__AccGLTransactionLine]
							([AccGLTransactionLineKey], [AccGLTransactionLineID], [CompanyID], [CompanyKey], [BranchID], [BranchKey], [DepartmentID], [DepartmentKey], [GLAccountID], [GLAccountKey], [GLTransactionHeaderID], [GLTransactionHeaderKey],
							[LineType], [PostDateTime], [LineAmount], [TaxAmount])
					VALUES ((SELECT ISNull(MAX(AccGLTransactionLineKey), 0) + 1 FROM [{0}].[Finance].[BAS__AccGLTransactionLine]), newID(),
							(SELECT CompanyID FROM [{0}].[Organization].[BAS__Company] WHERE CompanyKey = {1}), {1},
							(SELECT BranchID FROM [{0}].[Organization].[BAS__Branch] WHERE BranchKey = {2}), {2},
							(SELECT DepartmentID FROM [{0}].[Organization].[BAS__Department] WHERE DepartmentKey = {3}), {3},
							(SELECT GLAccountID FROM [{0}].[Finance].[BAS__GLAccount] WHERE GLAccountKey = {4}), {4},
							(SELECT GLTransactionHeaderID FROM [{0}].[Finance].[BAS__GLTransactionHeader] WHERE GLTransactionHeaderKey = {5}), {5},
							'{6}', '{7:yyyy-MM-dd}', '{8}', '{9}')",
					ScriptDbName,
					companyKey,
					branchKey,
					departmentKey,
					glAccountKey,
					transactionHeaderKey,
					lineType,
					postDate,
					lineAmount,
					taxAmount);
			TestConnection.ExecuteNonQuery(sql);
		}

		internal void CreateShipment(int isForwardRegistered = 1, string transportMode = "AIR", string weight = "KG")
		{
			var sql =
				string.Format(@"INSERT [{0}].InternationalLogistics.BAS__Shipment ([ShipmentKey], [ShipmentID], [IsForwardRegistered], [TransportMode], [UnitOfWeight])
						VALUES ((SELECT ISNull(MAX(ShipmentKey), 0) + 1 FROM [{0}].[InternationalLogistics].[BAS__Shipment]), newid(), {1}, '{2}', '{3}')",
					ScriptDbName,
					isForwardRegistered,
					transportMode,
					weight);
			TestConnection.ExecuteNonQuery(sql);
		}

		internal void CreateConsolShipmentPivot(string jobNum, int shipmentKey = 1)
		{
			var sql =
				string.Format(@"INSERT [{0}].InternationalLogistics.AGG__ConsolShipmentPivot ([ConsolShipmentPivotKey], [ShipmentID], [JobNumber])
						VALUES ((SELECT ISNull(MAX(ConsolShipmentPivotKey), 0) + 1 FROM [{0}].[InternationalLogistics].[AGG__ConsolShipmentPivot]),
						(select ShipmentID from [{0}].InternationalLogistics.BAS__Shipment where shipmentKey = {1}), '{2}')",
					ScriptDbName,
					shipmentKey,
					jobNum);
			TestConnection.ExecuteNonQuery(sql);
		}

		internal void CreateChargeCode(string chargeType = "DSB")
		{
			var sql =
				string.Format(@"INSERT [{0}].Finance.BAS__ChargeCode ([ChargeCodeKey], [ChargeCodeID], [ChargeType])
						VALUES ((SELECT ISNull(MAX(ChargeCodeKey), 0) + 1 FROM [{0}].Finance.BAS__ChargeCode), newID(),
						'{1}')",
					ScriptDbName,
					chargeType);
			TestConnection.ExecuteNonQuery(sql);
		}
	}
}
