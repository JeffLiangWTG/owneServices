using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocWhsDocketLineCollection : DocumentWrapperCollection
	{
		#region Contructors

		public DocWhsDocketLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsDocketLineCollection(WhsDocketLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsDocketLine this[int index]
		{
			get { return (DocWhsDocketLine)base[index]; }
		}

		#endregion
	}
}
