using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingReceivableLineReferenceCollection))]
	public class NettingReceivableLineReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingReceivableLineReferenceCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var periodPK = ZGuid.NewZGuid();
			var line = Factory.New<NettingReceivableTransactionLine>();
			line.NRL_NSP_Period = periodPK;

			var collection = new NettingReceivableLineReferenceCollection(line);
			var lineReference = collection.AddNew();
			AssertEquals("Line Reference Period should be defaulted from line Period", line.NRL_NSP_Period, lineReference.NR1_NSP_Period);
		}

		protected override NettingReceivableLineReferenceCollection GetCollectionToTest()
		{
			return new NettingReceivableLineReferenceCollection(Factory.New<NettingReceivableTransactionLine>());
		}
	}
}
