using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackProductWrapperCollection))]
	sealed class PackProductWrapperCollectionTest : GenericWrapperCollectionTest<PackProductWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PackProductWrapper(null, Factory);
		}

		protected override PackProductWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PackProductWrapperCollection(Factory);
		}

		public void TestProducts()
		{
			ForwardingPackLine packLine = Factory.New<ForwardingPackLine>();

			PackProduct product1 = packLine.Products.AddNew();
			product1.D2_ProductCode = "AAA";

			PackProduct product2 = packLine.Products.AddNew();
			product2.D2_ProductCode = "BBB";

			PackProductWrapperCollection productCollectionWrapper = new PackProductWrapperCollection(packLine, Factory);
			AssertEquals(2, productCollectionWrapper.Count);
			AssertEquals("AAA", productCollectionWrapper[0].ProductCode);
			AssertEquals("BBB", productCollectionWrapper[1].ProductCode);

			PackProductWrapper product1Wrapper = new PackProductWrapper(product1, Factory);
			PackProductWrapper product2Wrapper = new PackProductWrapper(product2, Factory);

			productCollectionWrapper = new PackProductWrapperCollection(Factory);
			productCollectionWrapper.Add(product1Wrapper);
			productCollectionWrapper.Add(product2Wrapper);
			AssertContainsExactElementsInAnyOrder(new PackProductWrapper[] { product1Wrapper, product2Wrapper }, productCollectionWrapper);

			AssertNoExceptionThrown(() => productCollectionWrapper = new PackProductWrapperCollection(null, Factory));
		}

		public void TestEmpty()
		{
			PackProductWrapperCollection emptyCollection = PackProductWrapperCollection.Empty;
			AssertNotNull(emptyCollection);
			AssertEquals(0, emptyCollection.Count);
			AssertEquals(emptyCollection, PackProductWrapperCollection.Empty);
		}
	}
}
