using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Accounting;
using CargoWise.Types;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccPayableTransactionBillingCollector))]
	sealed class AccPayableTransactionBillingCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(new DateTime(2022, 4, 20, 12, 0, 0), new DateTime(2022, 4, 20, 13, 0, 0));

		protected override bool IsMandatoryForMilestones => true;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 15, transactions.Count());
			assertARAPRows("AP", "INV", "CRD", "ADJ");

			void assertARAPRows(string ledger, params string[] transactionTypes)
			{
				var rows = transactions.Where(t => transactionTypes.Contains(t.Reference4));
				var expectedCount = transactionTypes.Length * 5;
				AssertEquals($"There are {expectedCount} rows with {ledger} Ledger", expectedCount, rows.Count());

				Assert($"All rows with {ledger} Ledger have EDI CompanyCode", rows.All(t => t.GetCompanyCode() == "EDI"));
				Assert($"All rows with {ledger} Ledger have SYD BranchCode", rows.All(t => t.GetBranchCode() == "SYD"));
				Assert($"All rows with {ledger} Ledger have AU Country Code", rows.All(t => t.Reference3 == "AU"));
				Assert($"All rows with {ledger} Ledger have TST UserCode", rows.All(t => t.ClientStaffCode == "TST"));
				Assert($"All rows with {ledger} Ledger have expected AdditionalRefs", rows.All(t => t.AdditionalRefs
					== "{\"AH_RX_NKTransactionCurrency\":\"USD\",\"GC_RX_NKLocalCurrency\":\"AUD\",\"AH_OSTotal\":-75.0000,\"AH_LocalTotal\":-100.0000,\"AH_InvoiceAmount\":-90.0000,\"AH_TransactionCategory\":\"FIN\"}"));

				var apr20_1200 = new DateTime(2022, 4, 20, 12, 0, 0);
				var rows1200 = rows.Where(t => t.BillableCount == 1 && t.ServiceOccuredUTC == apr20_1200);
				AssertEquals($"{transactionTypes.Length} rows with {ledger} have count of 1 and created 20 Apr 12:00", transactionTypes.Length, rows1200.Count());
				Assert("Transactions created at 12:00 have '001' Transaction Number", rows1200.All(t => t.Reference1 == "001"));
				Assert("Transactions created at 12:00 have '1000/01' Consolidated Invoice Ref", rows1200.All(t => t.Reference2 == "1000/01"));

				var apr20_1201 = new DateTime(2022, 4, 20, 12, 1, 0);
				var rows1201 = rows.Where(t => t.BillableCount == 1 && t.ServiceOccuredUTC == apr20_1201);
				AssertEquals($"{transactionTypes.Length} rows with {ledger} have count of 1 and created 20 Apr 12:01", transactionTypes.Length, rows1201.Count());
				Assert("Transactions created at 12:01 have '002' Transaction Number", rows1201.All(t => t.Reference1 == "002"));
				Assert("Transactions created at 12:01 have '1000/02' Consolidated Invoice Ref", rows1201.All(t => t.Reference2 == "1000/02"));

				var apr20_1230 = new DateTime(2022, 4, 20, 12, 30, 0);
				var rows1230 = rows.Where(t => t.BillableCount == 1 && t.ServiceOccuredUTC == apr20_1230);
				AssertEquals($"{transactionTypes.Length} rows with {ledger} have count of 1 and created 20 Apr 12:30", transactionTypes.Length, rows1230.Count());
				Assert("Transactions created at 12:30 have '003' Transaction Number", rows1230.All(t => t.Reference1 == "003"));
				Assert("Transactions created at 12:30 have '1000/03' Consolidated Invoice Ref", rows1230.All(t => t.Reference2 == "1000/03"));

				var apr20_1258 = new DateTime(2022, 4, 20, 12, 58, 0);
				var rows1258 = rows.Where(t => t.BillableCount == 1 && t.ServiceOccuredUTC == apr20_1258);
				AssertEquals($"{transactionTypes.Length} rows with {ledger} have count of 1 and created 20 Apr 12:58", transactionTypes.Length, rows1258.Count());
				Assert("Transactions created at 12:58 have '004' Transaction Number", rows1258.All(t => t.Reference1 == "004"));
				Assert("Transactions created at 12:58 have '1000/04' Consolidated Invoice Ref", rows1258.All(t => t.Reference2 == "1000/04"));

				var apr20_1259 = new DateTime(2022, 4, 20, 12, 59, 0);
				var rows1259 = rows.Where(t => t.BillableCount == 1 && t.ServiceOccuredUTC == apr20_1259);
				AssertEquals($"{transactionTypes.Length} rows with {ledger} have count of 1 and created 20 Apr 12:59", transactionTypes.Length, rows1259.Count());
				Assert("Transactions created at 12:59 have '005' Transaction Number", rows1259.All(t => t.Reference1 == "005"));
				Assert("Transactions created at 12:59 have '1000/05' Consolidated Invoice Ref", rows1259.All(t => t.Reference2 == "1000/05"));
			}
		}

		protected override void PrepareTestData()
		{
			var transactionBuilder = new ZStringBuilder(@"
DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'
DECLARE @DepartmentPk UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_ConsolidatedInvoiceRef, AH_SystemLastEditTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_InvoiceAmount, AH_GSTAmount, AH_TransactionCategory) VALUES");

			var stmALogBuilder = new ZStringBuilder(@"INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference) VALUES");

			foreach (var ledger in new[] { "AR", "AP" })
			{
				var minus = ledger == "AP" ? "-" : "";
				foreach (var transactionType in new[] { "ADJ", "CRD", "CTR", "DSC", "EXX", "INB", "INV", "JNL", "OVP", "PAY", "REC", "TRF" })
				{
					var transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '000', '1000/00', '2022-4-20 11:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 11:59', '{0}', 'EDT', 'Compliance Sub Type was defaulted to TXI.'),", transactionPK.ToString());

					transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '001', '1000/01', '2022-4-20 11:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 12:00', '{0}', 'ADD', 'AP|{1}|Posted'),", transactionPK.ToString(), transactionType);

					transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '002', '1000/02', '2022-4-20 11:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 12:01', '{0}', 'EDT', 'Compliance Sub Type was defaulted to TXI.'),", transactionPK.ToString());

					transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '003', '1000/03', '2022-4-20 11:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 12:30', '{0}', 'ADD', 'AP|{1}|Reversed'),", transactionPK.ToString(), transactionType);

					transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '004', '1000/04', '2022-4-20 11:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 12:58', '{0}', 'ADD', 'AP|{1}|Posted'),", transactionPK.ToString(), transactionType);

					transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '005', '1000/05', '2022-4-20 11:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 12:59', '{0}', 'EDT', 'Compliance Sub Type was defaulted to ABC.'),", transactionPK.ToString());

					transactionPK = ZGuid.NewZGuid();
					transactionBuilder.AppendFormat("('{0}', @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-4-20', '{1}', '{2}', '006', '1000/06', '2022-4-20 12:59', 'TST', 'USD', {3}75, {3}90, {3}10, 'FIN'),", transactionPK.ToString(), ledger, transactionType, minus);
					stmALogBuilder.AppendFormat("(NEWID(), 'AccTransactionHeader', '2022-4-20', '2022-4-20 13:00', '{0}', 'EDT', 'Compliance Sub Type was defaulted to TXI.'),", transactionPK.ToString());
				}
			}

			TestConnection.ExecuteNonQuery(transactionBuilder.ToStringWithNewLineBetweenAppends().TrimEnd(',') + System.Environment.NewLine + stmALogBuilder.ToStringWithNewLineBetweenAppends().TrimEnd(','));
		}
	}
}
