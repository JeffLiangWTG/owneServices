using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ComplianceReport;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetComplianceReportLines_Base))]
	class GetComplianceReportLines_BaseTest : BiCreateScriptTest
	{
		[TestDate(2023, 01, 1)]
		public void TestGetComplianceReportLine_GLD_NoGrouping()
		{
			GenerateAccComplianceReportTransactionPivot("");
			var pivotTableRows = RunScript(ScriptSql, SetParameters);
			AssertEquals(24, pivotTableRows.Count());
			Assert(pivotTableRows.Take(2).All(row => row.Field<Guid>("AH_PK") == gLGJLPK));
			Assert(pivotTableRows.Skip(2).Take(12).All(row => row.Field<Guid>("AH_PK") == aRInvoicePK));
			Assert(pivotTableRows.Skip(14).Take(2).All(row => row.Field<Guid>("AH_PK") == aRInvoicePKWithCashBasis));
			Assert(pivotTableRows.Skip(16).Take(2).All(row => row.Field<Guid>("AH_PK") == apJournalPK));
			Assert(pivotTableRows.Skip(18).Take(6).All(row => row.Field<Object>("AH_PK") == null));
		}

		[TestDate(2023, 01, 1)]
		public void TestGetComplianceReportLine_GLD_GroupByDAB()
		{
			GenerateAccComplianceReportTransactionPivot("DAB");
			var pivotTableRows = RunScript(ScriptSql, SetParameters);
			AssertEquals(12, pivotTableRows.Count());
			Assert(pivotTableRows.Take(3).All(row => row.Field<Object>("AH_PK") == null && row.Field<int>("ACL_ReportSequence") == 1));
			Assert(pivotTableRows.Skip(3).Take(2).All(row => row.Field<Guid>("AH_PK") == apJournalPK && row.Field<int>("ACL_ReportSequence") == 2));
			Assert(pivotTableRows.Skip(5).Take(4).All(row => row.Field<Guid>("AH_PK") == aRInvoicePK && row.Field<int>("ACL_ReportSequence") == 3));
			Assert(pivotTableRows.Skip(9).Take(1).All(row => row.Field<Guid>("AH_PK") == aRInvoicePKWithCashBasis && row.Field<int>("ACL_ReportSequence") == 4));
			Assert(pivotTableRows.Skip(10).Take(2).All(row => row.Field<Guid>("AH_PK") == gLGJLPK && row.Field<int>("ACL_ReportSequence") == 5));
		}

		[TestDate(2023, 01, 1)]
		public void TestGetComplianceReportLine_GLD_GroupByDBW()
		{
			GenerateAccComplianceReportTransactionPivot("DBW");
			var pivotTableRows = RunScript(ScriptSql, SetParameters);
			AssertEquals(12, pivotTableRows.Count());
			Assert(pivotTableRows.Take(3).All(row => row.Field<Object>("AH_PK") == null && row.Field<int>("ACL_ReportSequence") == 1));
			Assert(pivotTableRows.Skip(3).Take(2).All(row => row.Field<Guid>("AH_PK") == apJournalPK && row.Field<int>("ACL_ReportSequence") == 2));
			Assert(pivotTableRows.Skip(5).Take(2).All(row => row.Field<Guid>("AH_PK") == aRInvoicePK && row.Field<int>("ACL_ReportSequence") == 3));
			Assert(pivotTableRows.Skip(7).Take(2).All(row => row.Field<Guid>("AH_PK") == aRInvoicePK && row.Field<int>("ACL_ReportSequence") == 4));

			Assert(pivotTableRows.Skip(9).Take(1).All(row => row.Field<Guid>("AH_PK") == aRInvoicePKWithCashBasis && row.Field<int>("ACL_ReportSequence") == 5));
			Assert(pivotTableRows.Skip(10).Take(1).All(row => row.Field<Guid>("AH_PK") == gLGJLPK && row.Field<int>("ACL_ReportSequence") == 6));
			Assert(pivotTableRows.Skip(11).Take(1).All(row => row.Field<Guid>("AH_PK") == gLGJLPK && row.Field<int>("ACL_ReportSequence") == 7));
		}

		public virtual EnumerableRowCollection<DataRow> RunScript(string scriptSql, Action<DbCommand> setParameter)
		{
			using (var command = Db.Connection.Command(scriptSql))
			{
				setParameter(command);
				return DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			}
		}

		void GenerateAccComplianceReportTransactionPivot(string groupBy)
		{
			var companyPK = Guid.NewGuid();
			reportPK = Guid.NewGuid();
			apJournalPK = Guid.NewGuid();
			aRInvoicePK = Guid.NewGuid();
			aRInvoicePKWithCashBasis = Guid.NewGuid();
			var cashBasisVATPK = Guid.NewGuid();
			gLGJLPK = Guid.NewGuid();
			var parentID = Guid.NewGuid();
			var testDate = new DateTime(2023, 01, 1);
			GeneralLedgerDataID1 = Guid.NewGuid();
			GeneralLedgerDataID2 = Guid.NewGuid();
			GeneralLedgerDataID3 = Guid.NewGuid();
			GeneralLedgerDataID4 = Guid.NewGuid();
			GeneralLedgerDataID5 = Guid.NewGuid();
			GeneralLedgerDataID6 = Guid.NewGuid();
			GeneralLedgerDataID7 = Guid.NewGuid();
			GeneralLedgerDataID8 = Guid.NewGuid();
			GeneralLedgerDataID9 = Guid.NewGuid();
			GeneralLedgerDataID10 = Guid.NewGuid();
			GeneralLedgerDataID11 = Guid.NewGuid();
			GeneralLedgerDataID12 = Guid.NewGuid();
			GeneralLedgerDataID13 = Guid.NewGuid();
			GeneralLedgerDataID14 = Guid.NewGuid();
			GeneralLedgerDataID15 = Guid.NewGuid();
			GeneralLedgerDataID16 = Guid.NewGuid();
			GeneralLedgerDataID17 = Guid.NewGuid();
			GeneralLedgerDataID18 = Guid.NewGuid();
			GeneralLedgerDataID19 = Guid.NewGuid();
			GeneralLedgerDataID20 = Guid.NewGuid();
			GeneralLedgerDataID21 = Guid.NewGuid();
			GeneralLedgerDataID22 = Guid.NewGuid();
			GeneralLedgerDataID23 = Guid.NewGuid();
			GeneralLedgerDataID24 = Guid.NewGuid();
			var glAccountID = Guid.NewGuid();
			var glAccountID2 = Guid.NewGuid();

			var sqlText = $@"

				INSERT {ScriptDbName}.Organization.BAS__Company(CompanyKey, CompanyID, CountryCode) VALUES (1, newid(), 'AU')

				INSERT {ScriptDbName}.Finance.BAS__PeriodManagement ([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
				VALUES
						(1, newid(), 1, '2023-01-01', '2023-01-31',  202301, 2023)

				INSERT [{ScriptDbName}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code])
				VALUES
						(1, newid(), 'ABIGAS')

				INSERT [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [LedgerCode], [OrganizationHeaderKey], [TransactionTypeCode])
				VALUES
						(1, '{aRInvoicePK}', 'AR', 1, 'INV'),
						(2, '{apJournalPK}', 'AP', 1, 'JNL'),
						(3, '{aRInvoicePKWithCashBasis}', 'AR', 1, 'INV'),
						(4, '{gLGJLPK}', 'GL', 1, 'GJL')

				INSERT [{ScriptDbName}].[Finance].[BAS__AccGLTransactionLine]
					([AccGLTransactionLineKey], [AccGLTransactionLineID], [GLTransactionHeaderKey], [LineType], [OrganizationKey], [TaxRateKey], [TaxExtraRateNumerator])
				VALUES
						(1, newid(), 1, 'REV', 1, null, 2),
						(2, newid(), 1, 'REV', 1, null, 2),
						(3, newid(), 3, 'REV', 1, 1, 0),
						(4, newid(), 4, 'GJL', 1, null, 2),
						(5, newid(), 4, 'GJL', 1, null, 2),
						(6, newid(), null, 'WIP', 1, null, 0),
						(7, newid(), null, 'ACR', 1, null, 0)

				INSERT [{ScriptDbName}].[Finance].[BAS__CashBasisVAT]
					([CashBasisVATKey], [CashBasisVATID], [CompanyKey], [GLTransactionLineKey], [PostDate], [TaxAmount])
				VALUES
					(1, '{cashBasisVATPK}', 1, 3, '{testDate}', 100)

				INSERT [{ScriptDbName}].[Finance].[CUS__GeneralLedgerTransactionData]
					([GeneralLedgerTransactionDataKey], [GeneralLedgerDataKey], [GeneralLedgerDataID], [TransactionHeaderKey], [TransactionHeaderID], [TransactionLineKey], [GLType], [LedgerCode], [TransactionNum], [TransactionCategory], [ComplianceSubType], [HeaderDescription], [TransactionType], [ComplianceNumber], [JobInvoiceNumber], [InternalReference], 
					[TransactionDate], [OrganizationKey], [ChequeOrReference], [TransactionLineType], [LineDescription], [LineJobKey], [ChargeCodeKey], [PostDate], [ExchangeRate], 
					[PostPeriod], [BranchKey], [DepartmentKey], [TaxGLMovementKey], [CompanyKey], [CashBasisVATKey], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountOSCredit], [GLAmountOSDebit], [Currency], [GLAccountID])
				VALUES
					(1, 1, '{GeneralLedgerDataID1}', 4, '{gLGJLPK}', 4, 'PST', 'GL', '00001000', '', 'Sub', 'HDesc', 'GJL', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-10', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-10', 0.8, 202308, 1, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(2, 2, '{GeneralLedgerDataID2}', 4, '{gLGJLPK}', 5, 'PST', 'GL', '00001000', '', 'Sub', 'HDesc', 'GJL', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'GJL', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID2}'),
					(3, 3, '{GeneralLedgerDataID3}', 1, '{aRInvoicePK}', 1, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(4, 4, '{GeneralLedgerDataID4}', 1, '{aRInvoicePK}', 1, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(5, 5, '{GeneralLedgerDataID5}', 1, '{aRInvoicePK}', 1, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(6, 6, '{GeneralLedgerDataID6}', 1, '{aRInvoicePK}', 1, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),						(7, 7, '{GeneralLedgerDataID7}', 1, '{aRInvoicePK}', 1, 'REC', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(8, 8, '{GeneralLedgerDataID8}', 1, '{aRInvoicePK}', 1, 'REC', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(9, 9, '{GeneralLedgerDataID9}', 1, '{aRInvoicePK}', 2, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(10, 10, '{GeneralLedgerDataID10}', 1, '{aRInvoicePK}', 2, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(11, 11, '{GeneralLedgerDataID11}', 1, '{aRInvoicePK}', 2, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(12, 12, '{GeneralLedgerDataID12}', 1, '{aRInvoicePK}', 2, 'PST', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(13, 13, '{GeneralLedgerDataID13}', 1, '{aRInvoicePK}', 2, 'REC', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(14, 14, '{GeneralLedgerDataID14}', 1, '{aRInvoicePK}', 2, 'REC', 'AR', '00001000', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(15, 15, '{GeneralLedgerDataID15}', 3, '{aRInvoicePKWithCashBasis}', 3, 'CBV', 'AR', '00001001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, 1, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(16, 16, '{GeneralLedgerDataID16}', 3, '{aRInvoicePKWithCashBasis}', 3, 'CBV', 'AR', '00001001', '', 'Sub', 'HDesc', 'INV', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, 1, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(17, 17, '{GeneralLedgerDataID17}', 1, '{apJournalPK}', null, 'PST', 'AP', '00001000', '', 'Sub', 'HDesc', 'JNL', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(18, 18, '{GeneralLedgerDataID18}', 1, '{apJournalPK}', null, 'PST', 'AP', '00001000', '', 'Sub', 'HDesc', 'JNL', 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'REV', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID2}'),
					(19, 19, '{GeneralLedgerDataID19}', null, null, 6, 'PST', 'JC', 'T001', '', 'Sub', 'HDesc', null, 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(20, 20, '{GeneralLedgerDataID20}', null, null, 6, 'PST', 'JC', 'T001', '', 'Sub', 'HDesc', null, 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(21, 21, '{GeneralLedgerDataID21}', null, null, 6, 'REC', 'JC', 'T001', '', 'Sub', 'HDesc', null, 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(22, 22, '{GeneralLedgerDataID22}', null, null, 6, 'REC', 'JC', 'T001', '', 'Sub', 'HDesc', null, 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'WIP', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(23, 23, '{GeneralLedgerDataID23}', null, null, 7, 'PST', 'JC', 'T001', '', 'Sub', 'HDesc', null, 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'ACR', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}'),
					(24, 24, '{GeneralLedgerDataID24}', null, null, 7, 'PST', 'JC', 'T001', '', 'Sub', 'HDesc', null, 'CNum1', 'JobInvoiceNum1', 'InternalRef1', '2023-08-12', 1, 'ChequeRef1', 'ACR', 'LDesc', 1, 1, '2023-08-12', 0.8, 202305, 2, 1, null, 1, null, 2, 0, 2, 0, 'AUD', '{glAccountID}')
				";
			string insertComplianceReportTransactionPivot = null;
			if (groupBy.IsNullOrEmpty())
			{
				insertComplianceReportTransactionPivot = PopulateComplianceReportTransactionPivotSql(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24);
			}
			else if (groupBy == "DAB")
			{
				insertComplianceReportTransactionPivot = PopulateComplianceReportTransactionPivotSql(5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 2, 2, 1, 1, 1, 1, 1, 1);
			}
			else if (groupBy == "DBW")
			{
				insertComplianceReportTransactionPivot = PopulateComplianceReportTransactionPivotSql(6, 7, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 2, 2, 1, 1, 1, 1, 1, 1);
			}

			sqlText = sqlText + insertComplianceReportTransactionPivot;
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		string PopulateComplianceReportTransactionPivotSql(int v1, int v2, int v3, int v4, int v5, int v6, int v7, int v8, int v9, int v10, int v11, int v12, int v13, int v14, int v15, int v16, int v17, int v18, int v19, int v20, int v21, int v22, int v23, int v24)
		{
			return $@"INSERT [{ScriptDbName}].[Finance].[BAS__ComplianceReportTransactionPivot]
					([ComplianceReportTransactionPivotKey], [ComplianceReportTransactionPivotID], [CompanyKey], [ParentID], [ParentTableCode],[ReportID],[ReportSequence],[ReportSubCode])
				VALUES
					(1, NEWID(), 1, '{GeneralLedgerDataID1}', 'GLD', '{reportPK}', {v1}, ''),
					(2, NEWID(), 1, '{GeneralLedgerDataID2}', 'GLD', '{reportPK}', {v2}, ''),
					(3, NEWID(), 1, '{GeneralLedgerDataID3}', 'GLD', '{reportPK}', {v3}, ''),
					(4, NEWID(), 1, '{GeneralLedgerDataID4}', 'GLD', '{reportPK}', {v4}, ''),
					(5, NEWID(), 1, '{GeneralLedgerDataID5}', 'GLD', '{reportPK}', {v5}, ''),
					(6, NEWID(), 1, '{GeneralLedgerDataID6}', 'GLD', '{reportPK}', {v6}, ''),
					(7, NEWID(), 1, '{GeneralLedgerDataID7}', 'GLD', '{reportPK}', {v7}, ''),
					(8, NEWID(), 1, '{GeneralLedgerDataID8}', 'GLD', '{reportPK}', {v8}, ''),
					(9, NEWID(), 1, '{GeneralLedgerDataID9}', 'GLD', '{reportPK}', {v9}, ''),
					(10, NEWID(), 1, '{GeneralLedgerDataID10}', 'GLD', '{reportPK}', {v10}, ''),
					(11, NEWID(), 1, '{GeneralLedgerDataID11}', 'GLD', '{reportPK}', {v11}, ''),
					(12, NEWID(), 1, '{GeneralLedgerDataID12}', 'GLD', '{reportPK}', {v12}, ''),
					(13, NEWID(), 1, '{GeneralLedgerDataID13}', 'GLD', '{reportPK}', {v13}, ''),
					(14, NEWID(), 1, '{GeneralLedgerDataID14}', 'GLD', '{reportPK}', {v14}, ''),
					(15, NEWID(), 1, '{GeneralLedgerDataID15}', 'GLD', '{reportPK}', {v15}, ''),
					(16, NEWID(), 1, '{GeneralLedgerDataID16}', 'GLD', '{reportPK}', {v16}, ''),
					(17, NEWID(), 1, '{GeneralLedgerDataID17}', 'GLD', '{reportPK}', {v17}, ''),
					(18, NEWID(), 1, '{GeneralLedgerDataID18}', 'GLD', '{reportPK}', {v18}, ''),
					(19, NEWID(), 1, '{GeneralLedgerDataID19}', 'GLD', '{reportPK}', {v19}, ''),
					(20, NEWID(), 1, '{GeneralLedgerDataID20}', 'GLD', '{reportPK}', {v20}, ''),
					(21, NEWID(), 1, '{GeneralLedgerDataID21}', 'GLD', '{reportPK}', {v21}, ''),
					(22, NEWID(), 1, '{GeneralLedgerDataID22}', 'GLD', '{reportPK}', {v22}, ''),
					(23, NEWID(), 1, '{GeneralLedgerDataID23}', 'GLD', '{reportPK}', {v23}, ''),
					(24, NEWID(), 1, '{GeneralLedgerDataID24}', 'GLD', '{reportPK}', {v24}, '')";
		}

		protected Guid apJournalPK, aRInvoicePK, aRInvoicePKWithCashBasis, gLGJLPK, reportPK;

		protected Guid GeneralLedgerDataID1, GeneralLedgerDataID2, GeneralLedgerDataID3, GeneralLedgerDataID4, GeneralLedgerDataID5, GeneralLedgerDataID6, GeneralLedgerDataID7,
			GeneralLedgerDataID8, GeneralLedgerDataID9, GeneralLedgerDataID10, GeneralLedgerDataID11, GeneralLedgerDataID12, GeneralLedgerDataID13, GeneralLedgerDataID14,
			GeneralLedgerDataID15, GeneralLedgerDataID16, GeneralLedgerDataID17, GeneralLedgerDataID18, GeneralLedgerDataID19, GeneralLedgerDataID20, GeneralLedgerDataID21,
			GeneralLedgerDataID22, GeneralLedgerDataID23, GeneralLedgerDataID24;

		public virtual string ScriptSql => $"SELECT * FROM {ScriptDbName}.dbo.GetComplianceReportLines_Base(@PK, @ReportTablePrefix, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @GS, @ReverseSign) ORDER by ACL_ReportSequence";

		public virtual Action<DbCommand> SetParameters => command =>
		{
			command.AddParameterBasedOnDbColumn("@PK", reportPK, AccComplianceReportSchema.PK);
			command.AddParameterBasedOnDbColumn("@ReportTablePrefix", "GLD",
				CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@RegType", "AEO",
				CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@ReportCountry", "DAU",
				CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@RepCountryRegistrationCodeType", "",
				CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@GS", "", CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@ReverseSign", true, CargoWise.Schema.Schema.GenericBitSchemaColumn);
		};

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
