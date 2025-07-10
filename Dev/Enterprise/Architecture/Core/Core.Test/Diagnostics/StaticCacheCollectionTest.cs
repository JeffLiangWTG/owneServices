using CargoWise.Common.MemoryManagement;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	[TestedType(typeof(StaticCacheCollection))]
	sealed class StaticCacheCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StaticCacheCollection>
	{
		protected override StaticCacheCollection GetCollectionToTest()
		{
			return new StaticCacheCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var reclaimable = new Mock<IReclaimable>();
			return new StaticCacheDetails(reclaimable.Object);
		}
	}
}
