using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsTransferLineCollection : DocWhsDocketLineCollection
	{
		#region Constructors

		public DocWhsTransferLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsTransferLineCollection(WhsDocketLineCollection lines, BusinessObjectFactory factoryToWrap)
			: base(lines, factoryToWrap)
		{
		}

		public new DocWhsTransferLine this[int index]
		{
			get { return (DocWhsTransferLine)Elements[index]; }
		}

		#endregion
	}
}
