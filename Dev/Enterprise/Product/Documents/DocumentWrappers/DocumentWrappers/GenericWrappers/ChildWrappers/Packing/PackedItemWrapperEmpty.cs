using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackedItemWrapperEmpty : PackedItemWrapper
	{
		public PackedItemWrapperEmpty(PkgPackageItemDivotsWrapper[] packedItems, BusinessObjectFactory factory)
			: base(null, packedItems, factory)
		{
		}
	}
}
