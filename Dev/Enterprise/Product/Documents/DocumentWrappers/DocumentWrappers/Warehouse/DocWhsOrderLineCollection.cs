using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsOrderLineCollection : DocWhsDocketLineCollection
	{
		#region Constructors

		public DocWhsOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsOrderLineCollection(WhsDocketLineCollection lines, BusinessObjectFactory factoryToWrap)
			: base(lines, factoryToWrap)
		{
		}

		public new DocWhsPickableDocketLine this[int index]
		{
			get { return (DocWhsPickableDocketLine)Elements[index]; }
		}

		#endregion
	}
}
