using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(ReportFieldUsageCollection))]
	sealed class ReportFieldUsageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportFieldUsageCollection>
	{
		protected override ReportFieldUsageCollection GetCollectionToTest()
		{
			return new ReportFieldUsageCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReportFieldUsage("foo", "bar");
		}
	}
}
