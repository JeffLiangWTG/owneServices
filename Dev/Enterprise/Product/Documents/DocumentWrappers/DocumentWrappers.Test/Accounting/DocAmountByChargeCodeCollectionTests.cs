using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAmountByChargeCodeCollection))]
	sealed class DocAmountByChargeCodeCollectionTests : GenericWrapperCollectionTest<DocAmountByChargeCodeCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			AmountByChargeCode amountSplittedByChargeCode = new AmountByChargeCode("FRT", 100, 90);
			return DocAmountByChargeCode.New(amountSplittedByChargeCode, Factory);
		}

		protected override DocAmountByChargeCodeCollection GetNewDocumentWrapperCollection()
		{
			DocAmountByChargeCodeCollection collection = new DocAmountByChargeCodeCollection(Factory);
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 200m, 180m), Factory));
			return collection;
		}

		public override void TestTypedStringIndexer()
		{
			DocAmountByChargeCodeCollection collection = new DocAmountByChargeCodeCollection(Factory);
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 200m, 180m), Factory));
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 300m, 270m), Factory));
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("CAF", 15m, 13.5m), Factory));
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("CAF", 20m, 18m), Factory));

			AssertEquals(500m, collection["FRT"].TotalAmount);
			AssertEquals(35m, collection["CAF"].TotalAmount);
			AssertEquals(0m, collection["BAF"].TotalAmount);
			AssertEquals(535m, collection["FRT,CAF"].TotalAmount);

			AssertEquals(450m, collection["FRT"].TotalAmountExTax);
			AssertEquals(31.5m, collection["CAF"].TotalAmountExTax);
			AssertEquals(0m, collection["BAF"].TotalAmountExTax);
			AssertEquals(481.5m, collection["FRT,CAF"].TotalAmountExTax);
		}
	}
}
