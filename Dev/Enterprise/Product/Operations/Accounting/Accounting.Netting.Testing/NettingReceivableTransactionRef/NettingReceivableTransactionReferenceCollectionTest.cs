using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingReceivableTransactionReferenceCollection))]
	public class NettingReceivableTransactionReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingReceivableTransactionReferenceCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var periodPK = ZGuid.NewZGuid();
			var transaction = Factory.New<NettingReceivableTransaction>();
			transaction.NRT_NSP_Period = periodPK;

			var collection = new NettingReceivableTransactionReferenceCollection(transaction);
			var transactionReference = collection.AddNew();
			AssertEquals("Line Period should be defaulted from Transaction Period", transaction.NRT_NSP_Period, transactionReference.NRR_NSP_Period);
		}

		protected override NettingReceivableTransactionReferenceCollection GetCollectionToTest()
		{
			return new NettingReceivableTransactionReferenceCollection(Factory.New<NettingReceivableTransaction>());
		}
	}
}
