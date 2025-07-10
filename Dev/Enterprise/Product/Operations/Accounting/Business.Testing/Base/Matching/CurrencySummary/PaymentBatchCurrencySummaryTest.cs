using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(PaymentBatchCurrencySummary))]
	public class PaymentBatchCurrencySummaryTest : CurrencySummary_InnerTest
	{
		protected override void AssertInitCurrencySummaryRowCollection(PaymentApprovalBase paymentApproval1, PaymentApprovalBase paymentApproval2, PaymentApprovalBase paymentApproval3, PaymentApprovalBase paymentApproval4, CurrencySummaryRow summaryRowUSD, CurrencySummaryRow summaryRowCNY)
		{
			AssertEquals("paymentApproval1's Exchange Rate not changed", 1m, paymentApproval1.AV_PayExRate);
			AssertEquals("paymentApproval2's Exchange Rate not changed", 2m, paymentApproval2.AV_PayExRate);
			AssertEquals("paymentApproval3's Exchange Rate not changed", 3m, paymentApproval3.AV_PayExRate);
			AssertEquals("paymentApproval4's Exchange Rate not changed", 3m, paymentApproval4.AV_PayExRate);

			AssertEquals(1000m, paymentApproval1.AV_Calc_LocalAmount);

			AssertEquals(1000m, summaryRowUSD.LocalAmount);
			AssertEquals(1m, summaryRowUSD.AverageExRate);

			AssertEquals(1000m, paymentApproval2.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApproval3.AV_Calc_LocalAmount);
			AssertEquals(1333.33m, paymentApproval4.AV_Calc_LocalAmount);

			AssertEquals(1000m + 1000m + 1333.33m, summaryRowCNY.LocalAmount);
			AssertEquals(2.700003m, summaryRowCNY.AverageExRate);
		}

		protected override CurrencySummary GetNewBusinessObject(IMatchingCollection collection)
		{
			return new PaymentBatchCurrencySummary(collection);
		}
	}
}
