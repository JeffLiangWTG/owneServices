using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickableDocketLineCollection : DocWhsDocketLineCollection
	{
		#region Constructors

		public DocWhsPickableDocketLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsPickableDocketLineCollection(WhsDocketLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsPickableDocketLine this[int index]
		{
			get { return (DocWhsPickableDocketLine)Elements[index]; }
		}

		#endregion
	}
}
