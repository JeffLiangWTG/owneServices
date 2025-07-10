using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackedItemWrapperCollection : GenericWrapperCollection<PackedItemWrapper>
	{
		public PackedItemWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PackedItemWrapperCollection(PkgPackageItemDivotsWrapper[] packedItems, BusinessObjectFactory factory)
			: base(factory)
		{
			var groupedPackedItems = packedItems.GroupBy(d => new { Key = d.Key, PackableItemParent = d.PackableItemParent });
			foreach (var packedItemGroup in groupedPackedItems)
			{
				Add(PackedItemWrapper.New(packedItemGroup.Key.PackableItemParent, factory, packedItemGroup.ToArray()));
			}
		}
	}
}
