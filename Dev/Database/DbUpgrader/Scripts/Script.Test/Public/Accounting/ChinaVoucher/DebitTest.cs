using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ChinaVoucher;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ChinaVoucher
{
	[TestedType(typeof(Debit))]
	class DebitTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var expectedResults = new[]
			{
				new { ledger = "AR", transactionType = "PAY", reportType = "SubLedger", amount = 10, expected = (object)10m },
				new { ledger = "AR", transactionType = "PAY", reportType = "!SubLedger", amount = 10, expected = (object)DBNull.Value },
				new { ledger = "AR", transactionType = "PAY", reportType = "!SubLedger", amount = -10, expected = (object)10m },
				new { ledger = "XX", transactionType = "PAY", reportType = "SubLedger", amount = 10, expected = (object)DBNull.Value },
			};

			using (var cmd = TestConnection.Command("SELECT [Value] FROM [dbo].[Debit](@Ledger, @TransactionType, @ReportType, @Amount)"))
			{
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, 3, "");
				cmd.AddParameter("@TransactionType", SqlDbType.VarChar, 3, "");
				cmd.AddParameter("@ReportType", SqlDbType.VarChar, 10, "");
				cmd.AddParameter("@Amount", SqlDbType.Money, 0);

				foreach (var r in expectedResults)
				{
					cmd.SetParameterValue("@Ledger", r.ledger);
					cmd.SetParameterValue("@TransactionType", r.transactionType);
					cmd.SetParameterValue("@ReportType", r.reportType);
					cmd.SetParameterValue("@Amount", r.amount);
					AssertEquals($"(ledger = {r.ledger}, transactionType = {r.transactionType}, reportType = {r.reportType}, amount = {r.amount})", r.expected, cmd.ExecuteScalar());
				}
			}
		}
	}
}

