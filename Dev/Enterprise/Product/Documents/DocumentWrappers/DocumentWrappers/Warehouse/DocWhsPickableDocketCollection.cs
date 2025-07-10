using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickableDocketCollection : DocumentWrapperCollection
	{
		public DocWhsPickableDocketCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsPickableDocketCollection(WhsLegacyPickableDocketCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsPickableDocket this[int index]
		{
			get { return (DocWhsPickableDocket)base[index]; }
		}
	}
}
