using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection<AsycudaPack, AsycudaBill>))]
	sealed class AsycudaPackCollectionGenericTest : AsycudaPackCollectionAbstractTest<AsycudaPackCollection<AsycudaPack, AsycudaBill>>
	{
		protected override AsycudaPackCollection<AsycudaPack, AsycudaBill> GetNewCollection(AsycudaBill bill) => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(bill);
	}
}
