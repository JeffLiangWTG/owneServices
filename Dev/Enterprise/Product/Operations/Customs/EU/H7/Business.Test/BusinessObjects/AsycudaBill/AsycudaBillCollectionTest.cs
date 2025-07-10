using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
			return new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(asycudaManifestHeader);
		}
	}
}
