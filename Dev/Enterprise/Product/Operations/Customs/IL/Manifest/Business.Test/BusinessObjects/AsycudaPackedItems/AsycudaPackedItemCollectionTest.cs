using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemCollection<,>))]
	public class AsycudaPackedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<AsycudaBill>();
			return new AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaBill>(bill);
		}

		public void TestAsycudaPackedItemCollectionMaster()
		{
			var bill = Factory.New<AsycudaBill>();
			var billCollection = new AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaBill>(bill);
			AssertType<AsycudaBill>(billCollection.Master);
		}
	}
}
