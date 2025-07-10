using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAmountByChargeCodeCollectionExcept))]
	sealed class DocAmountByChargeCodeCollectionExceptTests : GenericWrapperCollectionTest<DocAmountByChargeCodeCollectionExcept>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			AmountByChargeCode amountSplittedByChargeCode = new AmountByChargeCode("FRT", 100, 90);
			return DocAmountByChargeCode.New(amountSplittedByChargeCode, Factory);
		}

		protected override DocAmountByChargeCodeCollectionExcept GetNewDocumentWrapperCollection()
		{
			var collection = new DocAmountByChargeCodeCollectionExcept(Factory);
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 200m, 180m), Factory));
			return collection;
		}

		public override void TestTypedStringIndexer()
		{
			DocAmountByChargeCodeCollectionExcept collection = new DocAmountByChargeCodeCollectionExcept(Factory);
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 200m, 180m), Factory));
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 300m, 270m), Factory));
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("CAF", 15m, 13.5m), Factory));
			collection.Add(DocAmountByChargeCode.New(new AmountByChargeCode("CAF", 20m, 18m), Factory));

			AssertEquals(35m, collection["FRT"].TotalAmount);
			AssertEquals(500m, collection["CAF"].TotalAmount);
			AssertEquals(535m, collection["BAF"].TotalAmount);
			AssertEquals(500m, collection["CAF,BAF"].TotalAmount);
			AssertEquals(0m, collection["CAF,FRT"].TotalAmount);

			AssertEquals(31.5m, collection["FRT"].TotalAmountExTax);
			AssertEquals(450m, collection["CAF"].TotalAmountExTax);
			AssertEquals(481.5m, collection["BAF"].TotalAmountExTax);
			AssertEquals(0m, collection["FRT,CAF"].TotalAmountExTax);
		}
	}
}
