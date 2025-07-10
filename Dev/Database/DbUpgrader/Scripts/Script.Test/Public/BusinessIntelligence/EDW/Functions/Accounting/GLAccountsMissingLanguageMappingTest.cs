using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GLAccountsMissingLanguageMapping))]
	class GLAccountsMissingLanguageMappingTest : BiCreateScriptTest
	{
		public void TestGLAccountsMissingLanguageMapping()
		{
			Helper.InsertPeriodForInputYear(2023);
			Helper.InsertStmData(new DateTime(2023, 01, 01));

			var companyPK = Guid.NewGuid();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey]
					,[GLAccountID]
					,[AccountTypeCode]
					,[AccountNo]
					,[AccountGroup]
					,[Description]
					,[DebitCredit]
					,[Units]
					)
					VALUES
						(1, newid(), 'NTE', '1.23.456', 'G', 'ACC1', 'DEBIT' ,'KG'),
						(2, newid(), 'P&L', '2.45.000', 'G', 'ACC2', 'CREDIT',''),
						(3, newid(), 'BSH', '6.87.000', 'G', 'ACC3', 'DEBIT' ,'');

				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202304, 10, 0, -10, 1, '', 1, 1),
						(2, 202302, 10, 0, -10, 1, '', 1, 1),
						(3, 202303, 20, 0, -20, 1, '', 1, 1);

				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription])
					VALUES
						(1, newid(), '1000.00.01', 'EN', 'CB', 'COA', 'LADesc1'),
						(2, newid(), '2000.00.03', 'GL', 'CN', 'BOA', 'LADesc2'),
						(3, newid(), '3000.00.01', 'EN', 'CN', 'COA', 'LADesc3');

				INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(1, newid(), 1, 1),
						(2, newid(), 2, 2),
						(3, newid(), 3, 3);
				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, newid(), 1, 'SYN', 1);
				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1');
				INSERT [{0}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
					VALUES
						(1, '{1}', 'AUD', 'DAU', 1, 1)
				", ScriptDbName, companyPK
			);
			TestConnection.ExecuteNonQuery(sqlText);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] ('{companyPK}', 'EN', 'CN')");
			AssertNotNull(result);
			AssertEquals(2, result.Rows.Count);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
			INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription])
					VALUES
						(8, newid(), '1000.00.01', 'EN', 'CN', 'COA', 'LADesc1'),
						(9, newid(), '2000.00.03', 'EN', 'CN', 'COA', 'LADesc2');
			INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(8, newid(), 1, 8),
						(9, newid(), 2, 9);
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] ('{companyPK}', 'EN', 'CN')");
			AssertNotNull(result);
			AssertEquals(0, result.Rows.Count);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
