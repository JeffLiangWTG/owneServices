using System.Linq;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	public class AdditionalRefsUTF16EncodingTests : TransactionedTestCase
	{
		public void TestUTF16AdditionalRefsBillingTransactionFactory()
		{
			var script = new TestUtf16AdditionalRefsStlItem();
			script.UseNVARCHAR = true;
			var range = AusydMonthRange.New(2024, 12);

			var createdTransactions = script.Run(range);
			var transaction = createdTransactions.FirstOrDefault();
			AssertEquals("AdditionalRefs should have decoded from UTF16 encoding successfully", """{"additionalRefsTest":"This is good"}""", transaction.AdditionalRefs);
		}

		public void TestUTF8AdditionalRefsBillingTransactionFactory()
		{
			var script = new TestUtf16AdditionalRefsStlItem();
			script.UseNVARCHAR = false;
			var range = AusydMonthRange.New(2024, 12);

			var createdTransactions = script.Run(range);
			var transaction = createdTransactions.FirstOrDefault();
			AssertEquals("AdditionalRefs should have decoded from UTF8 encoding successfully", """{"additionalRefsTest":"This is good"}""", transaction.AdditionalRefs);
		}

		public void TestUTF16AdditionalRefsUsageTransactionFactory()
		{
			var script = new TestUtf16AdditionalRefsStlItem();
			script.UseNVARCHAR = true;
			script.UseUsageTransactionFactory = true;
			var range = AusydMonthRange.New(2024, 12);

			var createdTransactions = script.Run(range);
			var transaction = createdTransactions.FirstOrDefault();
			AssertEquals("AdditionalRefs should have decoded from UTF16 encoding successfully", """{"additionalRefsTest":"This is good"}""", transaction.AdditionalRefs);
		}

		public void TestUTF8AdditionalRefsUsageTransactionFactory()
		{
			var script = new TestUtf16AdditionalRefsStlItem();
			script.UseNVARCHAR = false;
			script.UseUsageTransactionFactory = true;
			var range = AusydMonthRange.New(2024, 12);

			var createdTransactions = script.Run(range);
			var transaction = createdTransactions.FirstOrDefault();
			AssertEquals("AdditionalRefs should have decoded from UTF8 encoding successfully", """{"additionalRefsTest":"This is good"}""", transaction.AdditionalRefs);
		}
	}
}
