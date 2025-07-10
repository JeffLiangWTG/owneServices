using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(DocBuilderUsageCollection))]
	sealed class DocBuilderUsageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocBuilderUsageCollection>
	{
		protected override DocBuilderUsageCollection GetCollectionToTest()
		{
			return new DocBuilderUsageCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocBuilderUsage("foo", "bar");
		}
	}
}
