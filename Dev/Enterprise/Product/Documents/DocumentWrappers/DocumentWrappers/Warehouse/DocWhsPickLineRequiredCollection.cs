using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickOrderedInventoryCollection : DocumentWrapperCollection
	{
		public DocWhsPickOrderedInventoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsPickOrderedInventoryCollection(WhsPickOrderedInventoryCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsPickOrderedInventory this[int index]
		{
			get { return (DocWhsPickOrderedInventory)base[index]; }
		}
	}
}
