using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingPayableLineCollection))]
	public class NettingPayableLineCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingPayableLineCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var periodPK = ZGuid.NewZGuid();
			var transaction = Factory.New<NettingPayableTransaction>();
			transaction.NPT_NSP_Period = periodPK;

			var collection = new NettingPayableLineCollection(transaction);
			var line = collection.AddNew();
			AssertEquals("Line Period should be defaulted from Transaction Period", transaction.NPT_NSP_Period, line.NPL_NSP_Period);
		}

		protected override NettingPayableLineCollection GetCollectionToTest()
		{
			return new NettingPayableLineCollection(Factory.New<NettingPayableTransaction>());
		}
	}
}
