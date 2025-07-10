using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsInventoryCollection : DocumentWrapperCollection
	{
		public DocWhsInventoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsInventoryCollection(WhsDocketLine[] collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsInventory this[int index]
		{
			get { return (DocWhsInventory)base[index]; }
		}
	}
}
