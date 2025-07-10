using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsReceiveLineCollection : DocWhsDocketLineCollection
	{
		#region Constructors

		public DocWhsReceiveLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsReceiveLineCollection(WhsDocketLineCollection lines, BusinessObjectFactory factoryToWrap)
			: base(lines, factoryToWrap)
		{
		}

		public new DocWhsReceiveLine this[int index]
		{
			get { return (DocWhsReceiveLine)Elements[index]; }
		}

		#endregion
	}
}
