using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsWorkOrderLineCollection : DocWhsDocketLineCollection
	{
		#region Constructors

		public DocWhsWorkOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsWorkOrderLineCollection(WhsDocketLineCollection lines, BusinessObjectFactory factoryToWrap)
			: base(lines, factoryToWrap)
		{
		}

		public new DocWhsWorkOrderLine this[int index]
		{
			get { return (DocWhsWorkOrderLine)Elements[index]; }
		}

		#endregion
	}
}
