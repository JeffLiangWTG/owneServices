using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingReceivableLineCollection))]
	public class NettingReceivableLineCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingReceivableLineCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var periodPK = ZGuid.NewZGuid();
			var transaction = Factory.New<NettingReceivableTransaction>();
			transaction.NRT_NSP_Period = periodPK;

			var collection = new NettingReceivableLineCollection(transaction);
			var line = collection.AddNew();
			AssertEquals("Line Period should be defaulted from Transaction Period", transaction.NRT_NSP_Period, line.NRL_NSP_Period);
		}

		protected override NettingReceivableLineCollection GetCollectionToTest()
		{
			return new NettingReceivableLineCollection(Factory.New<NettingReceivableTransaction>());
		}
	}
}
