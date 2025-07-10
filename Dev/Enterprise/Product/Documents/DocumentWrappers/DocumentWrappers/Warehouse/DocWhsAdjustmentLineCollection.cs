using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsAdjustmentLineCollection : DocWhsDocketLineCollection
	{
		#region Constructors

		public DocWhsAdjustmentLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsAdjustmentLineCollection(WhsDocketLineCollection lines, BusinessObjectFactory factoryToWrap)
			: base(lines, factoryToWrap)
		{
		}

		public new DocWhsAdjustmentLine this[int index]
		{
			get { return (DocWhsAdjustmentLine)Elements[index]; }
		}

		#endregion
	}
}
