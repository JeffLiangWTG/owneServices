using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingPayableTransactionReferenceCollection))]
	public class NettingPayableTransactionReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingPayableTransactionReferenceCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var periodPK = ZGuid.NewZGuid();
			var transaction = Factory.New<NettingPayableTransaction>();
			transaction.NPT_NSP_Period = periodPK;

			var collection = new NettingPayableTransactionReferenceCollection(transaction);
			var transactionReference = collection.AddNew();
			AssertEquals("Line Period should be defaulted from Transaction Period", transaction.NPT_NSP_Period, transactionReference.NPR_NSP_Period);
		}

		protected override NettingPayableTransactionReferenceCollection GetCollectionToTest()
		{
			return new NettingPayableTransactionReferenceCollection(Factory.New<NettingPayableTransaction>());
		}
	}
}
