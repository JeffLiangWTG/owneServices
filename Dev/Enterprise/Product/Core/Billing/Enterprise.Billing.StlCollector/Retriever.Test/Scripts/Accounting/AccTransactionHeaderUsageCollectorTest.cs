using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Accounting;
using CargoWise.Types;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccTransactionHeaderUsageCollector))]
	sealed class AccTransactionHeaderUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 2);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 77, transactions.Count());

			assertARAPRows("AR");
			assertARAPRows("AP");

			AssertCBRows();
			AssertGLRows();

			AssertRows(ledger: "PA", "IPA", "CPA");
			AssertRows(ledger: "UA", "UAI", "UAC");
			AssertRows(ledger: "JC", "JNL", "JRJ");
			AssertRows(ledger: "IN", "INA", "INC", "INI");

			void assertARAPRows(string ledger)
			{
				var rows = transactions.Where(x => x.Reference1 == ledger).ToArray();
				AssertEquals($"There are 24 rows with {ledger} Ledger", 24, rows.Length);

				Assert($"All rows with {ledger} Ledger have EDI CompanyCode", rows.All(x => x.GetCompanyCode() == "EDI"));
				Assert($"All rows with {ledger} Ledger have SYD BranchCode", rows.All(x => x.GetBranchCode() == "SYD"));
				Assert($"All rows with {ledger} Ledger have AU Country Code", rows.All(x => x.Reference3 == "AU"));

				var feb15 = new DateTime(2022, 2, 15);
				AssertEquals($"12 rows with {ledger} have count of 2 and dated 15 Feb", 12, rows.Count(x => x.BillableCount == 2 && x.ServiceOccuredUTC == feb15));
				var feb17 = new DateTime(2022, 2, 17);
				AssertEquals($"12 rows with {ledger} have count of 1 and dated 17 Feb", 12, rows.Count(x => x.BillableCount == 1 && x.ServiceOccuredUTC == feb17));
			}

			void AssertCBRows()
			{
				var ledger = "CB";
				var rows = transactions.Where(x => x.Reference1 == ledger).ToArray();
				AssertEquals($"There are 14 rows with {ledger} Ledger", 14, rows.Length);

				Assert($"All rows with {ledger} Ledger have EDI CompanyCode", rows.All(x => x.GetCompanyCode() == "EDI"));
				AssertEquals($"7 rows with {ledger} Ledger have SYD BranchCode", 7, rows.Count(x => x.GetBranchCode() == "SYD"));
				AssertEquals($"7 rows with {ledger} Ledger have BNE BranchCode", 7, rows.Count(x => x.GetBranchCode() == "BNE"));
				Assert($"All rows with {ledger} Ledger have AU Country Code", rows.All(x => x.Reference3 == "AU"));

				var feb17 = new DateTime(2022, 2, 17);
				Assert($"All rows with {ledger} have count of 1 and dated 17 Feb", rows.All(x => x.BillableCount == 1 && x.ServiceOccuredUTC == feb17));
			}

			void AssertGLRows()
			{
				var ledger = "GL";
				var rows = transactions.Where(x => x.Reference1 == ledger).ToArray();
				AssertEquals($"There are 6 rows with {ledger} Ledger", 6, rows.Length);

				AssertEquals($"3 rows with {ledger} Ledger have EDI CompanyCode, SYD BranchCode, AU Country Code", 3,
					rows.Count(x => x.GetCompanyCode() == "EDI" && x.GetBranchCode() == "SYD" && x.Reference3 == "AU"));
				AssertEquals($"3 rows with {ledger} Ledger have SIN CompanyCode, SIN BranchCode, SG Country Code", 3,
					rows.Count(x => x.GetCompanyCode() == "SIN" && x.GetBranchCode() == "SIN" && x.Reference3 == "SG"));

				var feb18 = new DateTime(2022, 2, 18);
				Assert($"All rows with {ledger} have count of 1 and dated 18 Feb", rows.All(x => x.BillableCount == 1 && x.ServiceOccuredUTC == feb18));
			}

			void AssertRows(string ledger, params string[] transactionTypes)
			{
				var rows = transactions.Where(x => x.Reference1 == ledger).ToArray();
				AssertEquals($"There are {transactionTypes.Length} rows with {ledger} Ledger", transactionTypes.Length, rows.Length);

				Assert($"All rows with {ledger} Ledger have EDI CompanyCode", rows.All(x => x.GetCompanyCode() == "EDI"));
				Assert($"All rows with {ledger} Ledger have SYD BranchCode", rows.All(x => x.GetBranchCode() == "SYD"));
				Assert($"All rows with {ledger} Ledger have AU Country Code", rows.All(x => x.Reference3 == "AU"));

				var feb22 = new DateTime(2022, 2, 22);
				Assert($"All rows with {ledger} have count of 1 and dated 22 Feb", rows.All(x => x.BillableCount == 1 && x.ServiceOccuredUTC == feb22));
			}
		}

		protected override void PrepareTestData()
		{
			var sqlBuilder = new ZStringBuilder(@"
DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'
DECLARE @BNEBranchPk UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'

DECLARE @SINCompanyPk UNIQUEIDENTIFIER = '22C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061'
DECLARE @SINBranchPk UNIQUEIDENTIFIER = 'EF8CBDB5-9F53-4360-921E-C2929BF77A85'

DECLARE @DepartmentPk UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc) VALUES");

			foreach (var ledger in new[] { "AR", "AP" })
			{
				foreach (var transactionType in new[] { "ADJ", "CRD", "CTR", "DSC", "EXX", "INB", "INV", "JNL", "OVP", "PAY", "REC", "TRF" })
				{
					sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', '{0}', '{1}', '000', '2022-1-31 23:59'),", ledger, transactionType);
					sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', '{0}', '{1}', '001', '2022-2-15 00:00'),", ledger, transactionType);
					sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', '{0}', '{1}', '002', '2022-2-15 23:59'),", ledger, transactionType);
					sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', '{0}', '{1}', '003', '2022-2-17 08:45'),", ledger, transactionType);
				}
			}

			foreach (var transactionType in new[] { "DDB", "DPY", "DRC", "EXX", "ORC", "RCB", "TRF" })
			{
				sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', 'CB', '{0}', '001', '2022-2-17 09:00'),", transactionType);
				sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @BNEBranchPk, @DepartmentPk, '2022-2-22', 'CB', '{0}', '002', '2022-2-17 09:00'),", transactionType);
				sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @BNEBranchPk, @DepartmentPk, '2022-2-22', 'CB', '{0}', '003', '2022-3-1 00:00'),", transactionType);
			}

			foreach (var transactionType in new[] { "AJL", "GJL", "RJL" })
			{
				sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', 'GL', '{0}', '001', '2022-2-18 09:00'),", transactionType);
				sqlBuilder.AppendFormat("(NEWID(), @SINCompanyPk, @SINBranchPk, @DepartmentPk, '2022-2-22', 'GL', '{0}', '002', '2022-2-18 09:00'),", transactionType);
			}

			var otherTypes = new[]
			{
				(Ledger: "PA", TransactionTypes: new[] { "IPA", "CPA" }),
				(Ledger: "UA", TransactionTypes: new[] { "UAI", "UAC" }),
				(Ledger: "JC", TransactionTypes: new[] { "JNL", "JRJ" }),
				(Ledger: "IN", TransactionTypes: new[] { "INA", "INC", "INI" }),
			};

			foreach (var ledgerAndTypes in otherTypes)
			{
				foreach (var transactionType in ledgerAndTypes.TransactionTypes)
				{
					sqlBuilder.AppendFormat("(NEWID(), @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-2-22', '{0}', '{1}', '001', '2022-2-22 10:30'),", ledgerAndTypes.Ledger, transactionType);
				}
			}

			TestConnection.ExecuteNonQuery(sqlBuilder.ToStringWithNewLineBetweenAppends().TrimEnd(','));
		}
	}
}
