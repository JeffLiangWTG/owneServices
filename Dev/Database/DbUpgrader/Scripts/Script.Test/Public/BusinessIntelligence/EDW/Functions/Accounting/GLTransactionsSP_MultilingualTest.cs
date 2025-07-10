using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GLTransactionsSP_Multilingual))]
	class GLTransactionsSP_MultilingualTest : BiCreateScriptTest
	{
		public void TestMultilingualColumns()
		{
			var whiteList = new List<string>
			{
				"PK",
				"TransactionType",
				"InvoiceDate",
				"PostDate",
				"DueDate",
				"Branch",
				"Department",
				"Ledger",
				"TransactionNum",
				"SecondRef",
				"TransactionDesc",
				"GLAccount",
				"GLAccountDesc",
				"Units",
				"Job",
				"Account",
				"ChargeCode",
				"ChargeCodeDescription",
				"LocalLanguageChargeCodeDescription",
				"Period",
				"ReversePeriod",
				"Amount",
				"GSTAmount",
				"Debit",
				"Credit",
				"Balance",
				"IsLine",
				"TRPK",
				"OpeningPeriodDate",
				"ClosingPeriodDate",
				"OpeningBalance",
				"ClosingBalance",
				"IsControlTotal",
				"IsRevenueRecognition",
				"BatchNumber",
				"BatchType",
				"SubSelectID",
				"ParentTableCode",
				"PKFORBATCHING",
				"OsAmount",
				"ExRate",
				"Currency",
				"OsDebit",
				"OsCredit",
				"OsExTaxDebit",
				"OsExTaxCredit",
				"MultiSubAccountTypeCode",
				"OrganisationSubAccount",
				"SalesExpenseGroupsSubAccount",
				"StaffAndResourcesSubAccount",
				"StaffGroupSubAccount",
				"ComplianceSubType",
				"ComplianceNumber",
				"JournalEntriesNumber",
				"LocalGLAccount",
				"LocalGLAccountDesc"
			};
			var msgToHint = $@"If you see this error, you must be adding a new column into #Transactions table in store procedure 'GLTransactionsSP'.
Please follow below steps: 

1> Read through all parts in store procedure 'GLTransactionsSP' and understand their functional purpose.
2> Add the new column into whiteList.
";

			var result = Execute(CompanyPK, new List<string>());
			AssertEquals(msgToHint, 56, result.Columns.Count);
			foreach (DataColumn column in result.Columns)
			{
				Assert($@"Should contain column '{column.ColumnName}'", whiteList.Contains(column.ColumnName));
			}
		}

		public void TestMultilingualFilterLocalGLAccount()
		{
			var startLocalGLAccountPK = Guid.NewGuid();
			var endLocalGLAccountPK = Guid.NewGuid();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202304, 10, 0, -10, 1, '', 1, 1),
						(2, 202302, 10, 0, -10, 1, '', 1, 1),
						(3, 202303, 20, 0, -20, 1, '', 1, 1);

				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription])
					VALUES
						(1, newid(), '1000.00.01', 'EN', 'CN', 'COA', 'LADesc1'),
						(2, '{1}', '2000.00.03', 'GL', 'CN', 'BOA', 'LADesc2'),
						(3, newid(), '3000.00.01', 'EN', 'CN', 'COA', 'LADesc3'),
						(4, newid(), '4000.00.03', 'GL', 'C4', 'BOA', 'LADesc4'),
						(5, newid(), '5000.00.01', 'EN', 'C5', 'COA', 'LADesc5'),
						(6, '{2}', '6000.00.03', 'GL', 'C6', 'BOA', 'LADesc6'),
						(7, newid(), '7000.00.01', 'EN', 'C7', 'COA', 'LADesc7'),
						(8, newid(), '8000.00.03', 'GL', 'C8', 'BOA', 'LADesc8');

				INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(1, newid(), 1, 1),
						(2, newid(), 2, 2),
						(3, newid(), 3, 3),
						(4, newid(), 4, 4),
						(5, newid(), 5, 5),
						(6, newid(), 6, 6),
						(7, newid(), 7, 7),
						(8, newid(), 8, 8);

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'GL', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 1, 'AR', 'T002', '', 'Sub', 'HDesc', 'INV', 'CNum2', 'JobInvoiceNum2', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REC', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 2),
						(3, 3, 1, 2, 'GL', 'T003', '', 'Sub', 'HDesc', 'INV', 'CNum3', 'JobInvoiceNum3', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 3),
						(4, 4, 1, 1, 'AR', 'T004', '', 'Sub', 'HDesc', 'INV', 'CNum4', 'JobInvoiceNum4', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REC', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 4),
						(5, 5, 1, 2, 'GL', 'T005', '', 'Sub', 'HDesc', 'INV', 'CNum5', 'JobInvoiceNum5', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 5),
						(6, 6, 1, 1, 'AR', 'T006', '', 'Sub', 'HDesc', 'INV', 'CNum6', 'JobInvoiceNum6', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REC', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 6),
						(7, 7, 1, 2, 'GL', 'T007', '', 'Sub', 'HDesc', 'INV', 'CNum7', 'JobInvoiceNum7', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 7),
						(8, 8, 1, 1, 'AR', 'T008', '', 'Sub', 'HDesc', 'INV', 'CNum8', 'JobInvoiceNum8', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REC', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 8);
				",
				ScriptDbName, startLocalGLAccountPK, endLocalGLAccountPK
			);
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(CompanyPK, new List<string>() { "2023-08-03", "2023-08-19", startLocalGLAccountPK.ToString(), endLocalGLAccountPK.ToString() });
			AssertEquals("Should be 0 rows in report when localGLAccount has non-COA reportType", 0, result.Rows.Count);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
			INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription])
					VALUES
						(9, newid(), '2000.00.04', 'EN', 'CN', 'COA', 'LADesc9');
			INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(9, newid(), 2, 9);
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			result = Execute(CompanyPK, new List<string>() { "2023-08-03", "2023-08-19", startLocalGLAccountPK.ToString(), endLocalGLAccountPK.ToString() });
			AssertEquals("Should be 3 rows in report when localGLAccount has COA reportType", 3, result.Rows.Count);
			AssertEquals("LocalGLAccountDesc and LocalGLAccount", 2, result.Select("LocalGLAccount in ('2000.00.04','3000.00.01') and LocalGLAccountDesc in ('LADesc9','LADesc3') and GLAccount in ('2.45.000', '6.87.000')").Length);
			AssertEquals("LocalGLAccountDesc and LocalGLAccount for 6.97.000", 1, result.Select("LocalGLAccount is null and LocalGLAccountDesc is null and GLAccount = '6.97.000'").Length);
		}

		public void TestMultilingualFilterDates()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-10', 0.8, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-09-10', 0.8, 202309, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(4, 4, 1, 1, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-05-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-05-12', 0.8, 202305, 1, 2, null, 1, null, 2, 0, 2, 0, 'AUD', 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>() { "2023-06-03", "2023-08-19" });
			AssertEquals("Should be 1 rows in report when Date is right", 1, result.Rows.Count);
		}

		public void TestMultilingualFilterPeriod()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-10', 0.8, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-09-10', 0.8, 202309, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(4, 4, 1, 1, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-05-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-05-12', 0.8, 202305, 1, 2, null, 1, null, 2, 0, 2, 0, 'AUD', 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>(), 202306, 202308);
			AssertEquals("Should be 1 rows in report when Period is right", 1, result.Rows.Count);
		}

		public void TestGeneralLedgerTransactionDataFilterBranchPK()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-10', 0.8, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 1, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>() { null, null, null, null, BranchPK.ToString() });
			AssertEquals("Should be 1 rows in report when BranchPK is right", 1, result.Rows.Count);
		}

		public void TestGeneralLedgerTransactionDataFilterDepartmentPK()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-10', 0.8, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 1, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 1, 2, null, 1, null, 2, 0, 2, 0, 'AUD', 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>() { null, null, null, null, BranchPK.ToString(), DepartmentPK.ToString() });
			AssertEquals("Should be 1 rows in report when DepartmentPK is right", 1, result.Rows.Count);
		}

		public void TestGeneralLedgerTransactionDataWithDisplayDesc()
		{
			var headerID = Guid.NewGuid().ToString();

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'WIP', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1);
				",
				ScriptDbName, headerID
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>() { "2023-05-03", "2023-08-19" });
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals("Should be LineDesc", 2, result.Select("TransactionDesc='LDesc'").Length);

			result = Execute(CompanyPK, new List<string>() { "2023-05-03", "2023-08-19", null, null, null, null, "H" });
			AssertEquals("WIP should be LineDesc", 1, result.Select("TransactionDesc='LDesc'").Length);
			AssertEquals("Non-WIP should be HeaderDesc", 1, result.Select("TransactionDesc='HDesc'").Length);
		}

		public void TestGeneralLedgerTransactionDataFilterTransactionCategory()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'GL', 'T001', 'A', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 2, 'JC', 'T002', '', 'Sub', 'HDesc', 'WIP', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(3, 3, 1, 2, 'JC', 'T002', 'G', 'Sub', 'HDesc', 'WIP', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(4, 4, 1, 2, 'GL', 'T001', 'C', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'RJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(5, 5, 1, 2, 'GL', 'T001', 'D', 'Sub', 'HDesc', 'WIP', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'NJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(6, 6, 1, 2, 'GL', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'AJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(7, 7, 1, 2, 'GL', 'T007', 'F', 'Sub', 'HDesc', 'WIP', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>() { "2023-05-03", "2023-08-19", null, null, null, null, null, "A,B,C,D,E" });
			AssertEquals("Should be 6 rows in report", 6, result.Rows.Count);
			AssertEquals("TransactionCategory F is not here", 0, result.Select("TransactionNum='T007'").Length);
			AssertEquals("JC is here no matter what its transactionCategory is", 2, result.Select("Ledger='JC'").Length);
		}

		public void TestGeneralLedgerTransactionDataWithIncludeZeroBalance()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey],[GLAmountLocalBalance])
					VALUES
						(1, 202304, 10, 0, 1, '', 1, 1, -10),
						(2, 202304, 10, 0, 1, '', 1, 1, -10),
						(3, 202104, 10, 0, 1, 'A', 1, 1, -10);

				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription])
					VALUES
						(1, newid(), '2000.00.04', 'EN', 'CN', 'COA', 'LADesc1'),
						(2, newid(), '2000.00.05', 'EN', 'CN', 'COA', 'LADesc1'),
						(3, newid(), '2000.00.06', 'EN', 'CN', 'COA', 'LADesc2');
				INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(1, newid(), 1, 1),
						(2, newid(), 2, 2),
						(3, newid(), 3, 3);
					
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [TransactionHeaderKey], [TransactionLineKey], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountKey])
					VALUES
						(1, 1, 1, 2, 'GL', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 1),
						(2, 2, 1, 2, 'AR', 'T001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REC', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 2),
						(3, 3, 1, 2, 'GL', 'T001', 'A', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 3),
						(4, 4, 1, 2, 'AR', 'T001', 'A', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'REC', 'LDesc', 1, 1, '2023-08-10', 1, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', 9);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(CompanyPK, new List<string>() { "2023-08-03", "2023-08-19", null, null, null, null, null, null, "Y" });
			AssertEquals("Should be 5 rows in report", 5, result.Rows.Count);
			AssertEquals("Should be 3 rows with zero openingBalance", 3, result.Select("OpeningBalance=0").Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
		}

		void PrepareData()
		{
			CompanyPK = Guid.NewGuid();
			DepartmentPK = Guid.NewGuid();
			BranchPK = Guid.NewGuid();

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code])
					VALUES
						(1, newid(), 'OR1'),
						(2, newid(), 'OR2'),
						(3, newid(), 'OR3');

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
						(3, newid(), 'BSH', '6.87.000', 'G', 'ACC3', 'DEBIT' ,''),
						(4, newid(), 'BSH', '6.97.000', 'G', 'ACC4', 'DEBIT' ,''),
						(5, newid(), 'CLN', '1.23.600', 'G', 'ACC5', 'DEBIT' ,''),
						(6, newid(), 'TTL', '2.00'    , 'G', 'ACC6', 'CREDIT',''),
						(7, newid(), 'ALT', '2.00'    , 'G', 'ACC7', 'CREDIT',''),
						(8, newid(), 'HDR', '3'       , 'G', 'ACC8', 'DEBIT' ,''),
						(9, newid(), 'NTE', '6.99.456', 'G', 'ACC9', 'DEBIT' ,'KCN');

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, '{3}'  , 1, 'SYN', 1),
						(2, newid(), 2, 'DTW', 2);

				INSERT [{0}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
					VALUES
						(1, '{1}', 'AUD', 'DAU', 1, 1),
						(2, newid(), 'USD', 'DTW', 0, 0);

				INSERT [{0}].[Finance].[BAS__Currency]
					([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio])
					VALUES
						(1, newid(), 'AUD' , 100),
						(2, newid(), 'USD', 50);

				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, '{2}', 'AU1'),
						(2, newid(), 'TW1');

				INSERT [{0}].[Finance].[BAS__ChargeCode]
					([ChargeCodeKey], [ChargeCodeID], [ChargeType], [Code], [Desc], [LocalLanguageDescription])
					VALUES
						(901, newid(), 'CMT', 'CMT1', 'CMT desc', 'CMT local desc'),
						(902, newid(), 'ABC', 'MJA1', 'MJA desc', 'MJA local desc');

				INSERT [{0}].[Finance].[BAS__PeriodManagement]
					([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '2023-08-01', '2023-08-31',  202308, 2023),
						(2, newid(), 1, '2023-07-01', '2023-07-31',  202307, 2023),
						(3, newid(), 1, '2023-06-01', '2023-06-30',  202306, 2023),
						(4, newid(), 1, '2023-05-01', '2023-05-31',  202305, 2023),
						(5, newid(), 1, '2023-02-01', '2023-02-28',  202302, 2023),
						(6, newid(), 1, '2023-01-01', '2023-01-31',  202301, 2023);

				INSERT [{0}].[Customs].[BAS__Account]
					([AccountKey], [AccountID], [GLAccountKey], [AccountType])
					VALUES
						(1, newid(), 4, 'GL_PL_APPROPRIATION_ACCOUNT'),
						(2, newid(), 3, 'GL_BS_ACCOUNT_START');

				INSERT [{0}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
					VALUES
						(1, newid(), 'JournalEntriesLastProcessedDate', '{1}', '2023-01-01 00:00:00.000', 1);
				",
				ScriptDbName, CompanyPK, DepartmentPK, BranchPK
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable Execute(Guid companyPK, List<string> parameters, int startPeriod = 0, int endPeriod = 0)
		{
			if (parameters == null)
			{
				parameters = new List<string>(ParameterLength);
			}

			while (parameters.Count < ParameterLength)
			{
				parameters.Add(null);
			}

			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]");
			sqlBuilder.Append($"@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@StartPeriod = ").Append(startPeriod == 0 ? "NULL" : startPeriod.ToString());
			sqlBuilder.Append(
				$@",@EndPeriod = ").Append(endPeriod == 0 ? "NULL" : endPeriod.ToString());
			sqlBuilder.Append(
				$@",@StartDate = ").Append(parameters[0] == null ? "NULL" : $"'{parameters[0]}'");
			sqlBuilder.Append(
				$@",@EndDate = ").Append(parameters[1] == null ? "NULL" : $"'{parameters[1]}'");
			sqlBuilder.Append(
				$@",@StartGLAccountPK = ").Append(parameters[2] == null ? "NULL" : $"'{parameters[2]}'");
			sqlBuilder.Append(
				$@",@EndGLAccountPK = ").Append(parameters[3] == null ? "NULL" : $"'{parameters[3]}'");
			sqlBuilder.Append(
				$@",@BranchPK = ").Append(parameters[4] == null ? "NULL" : $"'{parameters[4]}'");
			sqlBuilder.Append(
				$@",@DepartmentPK = ").Append(parameters[5] == null ? "NULL" : $"'{parameters[5]}'");
			sqlBuilder.Append(
				$@",@DisplayDescription = ").Append(parameters[6] == null ? "'L'" : $"'{parameters[6]}'");
			sqlBuilder.Append(
				$@",@TransactionCategory = ").Append(parameters[7] == null ? "''" : $"'{parameters[7]}'");
			sqlBuilder.Append(
				$@",@BatchNumberToGet = 0");
			sqlBuilder.Append(
				$@",@BatchNumberToSet = 0");
			sqlBuilder.Append(
				$@",@IncludeZeroBalance = ").Append(parameters[8] == null ? "'N'" : $"'{parameters[8]}'");
			sqlBuilder.Append(
				$@",@Language = ").Append(parameters[9] == null ? "'EN'" : $"'{parameters[9]}'");
			sqlBuilder.Append(
				$@",@CountryCode = ").Append(parameters[10] == null ? "'CN'" : $"'{parameters[10]}'");
			sqlBuilder.Append(
				$@",@IsExportingBatch = 'N'");

			var sqlText = string.Format(CultureInfo.InvariantCulture, sqlBuilder.ToString());
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		static readonly int ParameterLength = 11;
		Guid CompanyPK, DepartmentPK, BranchPK;
	}
}
