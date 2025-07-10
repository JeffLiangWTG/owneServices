using CargoWise.Application;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TransactionBatch))]
	class TransactionBatchTest : DataObjectTestCase<TransactionBatch>
	{
		public void TestAccountingXSD()
		{
			var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
			string result = generator.GetXsdOutput(typeof(TransactionBatch));
			AssertNotEquals("", result);
		}
	}
}
