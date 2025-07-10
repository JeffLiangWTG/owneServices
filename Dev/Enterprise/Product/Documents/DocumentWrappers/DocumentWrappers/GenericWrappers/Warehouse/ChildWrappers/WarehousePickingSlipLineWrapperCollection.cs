using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickingSlipLineWrapperCollection : GenericWrapperCollection<WarehousePickingSlipLineWrapper>, IPickingSlipLineWrapperCollection
	{
		public WarehousePickingSlipLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IPickingSlipWrapperCollection methods

		void IPickingSlipLineWrapperCollection.AddPickLine(IPickingSlipLineWrapper line)
		{
			Add((WarehousePickingSlipLineWrapper)line);
		}

		#endregion

	}
}
