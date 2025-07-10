using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsDocketContainerCollection : DocumentWrapperCollection
	{
		#region Constructors

		public DocWhsDocketContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsDocketContainerCollection(WhsDocketContainerCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		#endregion

		public new DocWhsDocketContainer this[int index]
		{
			get { return (DocWhsDocketContainer)base[index]; }
		}

		#region Collections

		public IDocSimpleContainerCollection ToIDocSimpleContainerCollection()
		{
			IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
			foreach (DocWhsDocketContainer container in this)
			{
				result.Add(container);
			}
			return result;
		}

		#endregion
	}
}
