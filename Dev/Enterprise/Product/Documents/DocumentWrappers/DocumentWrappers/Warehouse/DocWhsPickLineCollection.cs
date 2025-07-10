using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickLineCollection : DocumentWrapperCollection, IPickingSlipLineWrapperCollection
	{
		public DocWhsPickLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsPickLineCollection(WhsPickLineCollectionND collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsPickLine this[int index]
		{
			get { return (DocWhsPickLine)base[index]; }
		}

		#region IPickingSlipWrapperCollection methods

		void IPickingSlipLineWrapperCollection.AddPickLine(IPickingSlipLineWrapper line)
		{
			Add((DocWhsPickLine)line);
		}

		#endregion
	}
}
