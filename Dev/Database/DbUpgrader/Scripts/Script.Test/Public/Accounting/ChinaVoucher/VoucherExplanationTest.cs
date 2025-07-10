using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ChinaVoucher;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ChinaVoucher
{
	[TestedType(typeof(VoucherExplanation))]
	class VoucherExplanationTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var expectedResults = new[]
			{
				new { ledger = "AR", transactionType = "INV", expected = (object)"(INV) 营业收入" },
				new { ledger = "AR", transactionType = "CRD", expected = (object)"(CRD) 营业收入" },
				new { ledger = "AR", transactionType = "ADJ", expected = (object)"(ADJ) 营业收入" },
				new { ledger = "AR", transactionType = "REC", expected = (object)"(REC) 银行收入" },
				new { ledger = "AR", transactionType = "PAY", expected = (object)"(PAY) 银行支出" },
				new { ledger = "AR", transactionType = "JNL", expected = (object)"(JNL) 应收凭证" },
				new { ledger = "AR", transactionType = "OVP", expected = (object)"(OVP) 应收预付账款" },
				new { ledger = "AR", transactionType = "EXX", expected = (object)"(EXX) 应收利息差异" },
				new { ledger = "AR", transactionType = "DSC", expected = (object)"(DSC) 应收回扣" },
				new { ledger = "AR", transactionType = "CTR", expected = (object)"(CTR) 应收/应付抵账" },
				new { ledger = "AR", transactionType = "TRF", expected = (object)"(TRF) 应收转账" },

				new { ledger = "AP", transactionType = "INV", expected = (object)"(INV) 营业成本" },
				new { ledger = "AP", transactionType = "CRD", expected = (object)"(CRD) 营业成本" },
				new { ledger = "AP", transactionType = "ADJ", expected = (object)"(ADJ) 营业成本" },
				new { ledger = "AP", transactionType = "REC", expected = (object)"(REC) 银行收入" },
				new { ledger = "AP", transactionType = "PAY", expected = (object)"(PAY) 银行支出" },
				new { ledger = "AP", transactionType = "JNL", expected = (object)"(JNL) 应付凭证" },
				new { ledger = "AP", transactionType = "OVP", expected = (object)"(OVP) 应付预付账款" },
				new { ledger = "AP", transactionType = "EXX", expected = (object)"(EXX) 应付利息差异" },
				new { ledger = "AP", transactionType = "DSC", expected = (object)"(DSC) 应付回扣" },
				new { ledger = "AP", transactionType = "CTR", expected = (object)"(CTR) 应收/应付抵账" },
				new { ledger = "AP", transactionType = "TRF", expected = (object)"(TRF) 应付转账" },

				new { ledger = "CB", transactionType = "DPY", expected = (object)"(DPY) 直接付款单" },
				new { ledger = "CB", transactionType = "DRC", expected = (object)"(DRC) 直接收款单" },
				new { ledger = "CB", transactionType = "TRF", expected = (object)"(TRF) 银行转账" },
				new { ledger = "CB", transactionType = "EXX", expected = (object)"(EXX) 银行收入" },

				new { ledger = "CB", transactionType = "XXX",  expected = (object)DBNull.Value },
			};

			using (var cmd = TestConnection.Command("SELECT [Value] FROM [dbo].[VoucherExplanation](@Ledger, @TransactionType)"))
			{
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, 3, "");
				cmd.AddParameter("@TransactionType", SqlDbType.VarChar, 3, "");

				foreach (var r in expectedResults)
				{
					cmd.SetParameterValue("@Ledger", r.ledger);
					cmd.SetParameterValue("@TransactionType", r.transactionType);
					AssertEquals($"(ledger = {r.ledger}, transactionType = {r.transactionType})", r.expected, cmd.ExecuteScalar());
				}
			}
		}
	}
}

