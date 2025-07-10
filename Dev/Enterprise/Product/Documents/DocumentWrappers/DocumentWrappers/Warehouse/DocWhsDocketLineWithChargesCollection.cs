using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsDocketLineWithChargesCollection : DocWhsDocketLineCollection
	{
		#region Contructors

		public DocWhsDocketLineWithChargesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsDocketLineWithChargesCollection(WhsDocketLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsDocketLineWithCharges this[int index]
		{
			get { return (DocWhsDocketLineWithCharges)base[index]; }
		}

		#endregion
	}
}
