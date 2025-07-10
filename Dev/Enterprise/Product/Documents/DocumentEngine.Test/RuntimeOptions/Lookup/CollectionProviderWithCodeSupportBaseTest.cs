using Enterprise.DocumentEngine.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestsSubclassesOf(typeof(CollectionProviderWithCodeSupport), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	public abstract class CollectionProviderWithCodeSupportBaseTest : CollectionProviderBaseTest
	{
		public void TestMaxLength()
		{
			AssertEquals(ExpectedMaxLength, ((CollectionProviderWithCodeSupport)Provider).MaxLength);
		}

		protected abstract int ExpectedMaxLength { get; }
	}
}
