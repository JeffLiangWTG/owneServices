using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ChinaVoucher;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ChinaVoucher
{
	[TestedType(typeof(AmountMultiplier))]
	class AmountMultiplierTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var expectedResults = new[]
			{
				new { ledger = "AR", transactionType = "INV",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "CRD",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "ADJ",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "REC",  reportType = "SubLedger", expected = (object)1 },
				new { ledger = "AR", transactionType = "REC",  reportType = "!SubLedger", expected = (object)-1 },
				new { ledger = "AR", transactionType = "PAY",  reportType = "SubLedger", expected = (object)1 },
				new { ledger = "AR", transactionType = "PAY",  reportType = "!SubLedger", expected = (object)-1 },
				new { ledger = "AR", transactionType = "JNL",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "OVP",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "EXX",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "DSC",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "CTR",  reportType = "any", expected = (object)1 },
				new { ledger = "AR", transactionType = "TRF",  reportType = "any", expected = (object)1 },

				new { ledger = "AP", transactionType = "INV",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "CRD",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "ADJ",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "REC",  reportType = "SubLedger", expected = (object)1 },
				new { ledger = "AP", transactionType = "REC",  reportType = "!SubLedger", expected = (object)-1 },
				new { ledger = "AP", transactionType = "PAY",  reportType = "SubLedger", expected = (object)1 },
				new { ledger = "AP", transactionType = "PAY",  reportType = "!SubLedger", expected = (object)-1 },
				new { ledger = "AP", transactionType = "JNL",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "OVP",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "EXX",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "DSC",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "CTR",  reportType = "any", expected = (object)1 },
				new { ledger = "AP", transactionType = "TRF",  reportType = "any", expected = (object)1 },

				new { ledger = "CB", transactionType = "DPY",  reportType = "any", expected = (object)1 },
				new { ledger = "CB", transactionType = "DRC",  reportType = "any", expected = (object)1 },
				new { ledger = "CB", transactionType = "TRF",  reportType = "any", expected = (object)1 },
				new { ledger = "CB", transactionType = "EXX",  reportType = "any", expected = (object)1 },

				new { ledger = "CB", transactionType = "XXX",  reportType = "any", expected = (object)DBNull.Value },
			};

			using (var cmd = TestConnection.Command("SELECT [Value] FROM [dbo].[AmountMultiplier](@Ledger, @TransactionType, @ReportType)"))
			{
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, 3, "");
				cmd.AddParameter("@TransactionType", SqlDbType.VarChar, 3, "");
				cmd.AddParameter("@ReportType", SqlDbType.VarChar, 10, "");

				foreach (var r in expectedResults)
				{
					cmd.SetParameterValue("@Ledger", r.ledger);
					cmd.SetParameterValue("@TransactionType", r.transactionType);
					cmd.SetParameterValue("@ReportType", r.reportType);
					AssertEquals($"(ledger = {r.ledger}, transactionType = {r.transactionType}, reportType = {r.reportType})", r.expected, cmd.ExecuteScalar());
				}
			}
		}
	}
}

