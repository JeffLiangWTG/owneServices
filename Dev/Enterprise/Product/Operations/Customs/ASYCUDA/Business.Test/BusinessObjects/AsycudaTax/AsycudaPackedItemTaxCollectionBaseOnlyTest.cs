using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>))]
	sealed class AsycudaPackedItemTaxCollectionBaseOnlyTest : AsycudaPackedItemTaxCollectionAbstractTest<AsycudaTax, AsycudaPackedItem, AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>>
	{
		protected override AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem> GetNewCollection(AsycudaPackedItem packedItem) => new AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>(packedItem);
	}
}
