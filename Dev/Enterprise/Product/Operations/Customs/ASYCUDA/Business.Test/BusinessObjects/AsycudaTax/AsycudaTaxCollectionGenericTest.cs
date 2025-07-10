using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaTaxCollection<AsycudaTax, AsycudaBill>))]
	sealed class AsycudaTaxCollectionGenericTest : AsycudaTaxCollectionAbstractTest<AsycudaTax, AsycudaBill, AsycudaTaxCollection<AsycudaTax, AsycudaBill>>
	{
		protected override AsycudaTaxCollection<AsycudaTax, AsycudaBill> GetNewCollection(AsycudaBill bill) => new AsycudaTaxCollection<AsycudaTax, AsycudaBill>(bill);
	}
}
