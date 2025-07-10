using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingPayableLineReferenceCollection))]
	public class NettingPayableLineReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingPayableLineReferenceCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var periodPK = ZGuid.NewZGuid();
			var line = Factory.New<NettingPayableTransactionLine>();
			line.NPL_NSP_Period = periodPK;

			var collection = new NettingPayableLineReferenceCollection(line);
			var lineReference = collection.AddNew();
			AssertEquals("Line Reference Period should be defaulted from line Period", line.NPL_NSP_Period, lineReference.NP1_NSP_Period);
		}

		protected override NettingPayableLineReferenceCollection GetCollectionToTest()
		{
			return new NettingPayableLineReferenceCollection(Factory.New<NettingPayableTransactionLine>());
		}
	}
}
