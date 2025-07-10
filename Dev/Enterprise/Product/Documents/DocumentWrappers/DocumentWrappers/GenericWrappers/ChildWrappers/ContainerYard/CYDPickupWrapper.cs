using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CYDPickupWrapper : GenericWrapper
	{
		public CYDPickupWrapper(CYDPickup pickupBO, BusinessObjectFactory factory)
			: base(pickupBO, factory)
		{
			this.pickupBO = pickupBO ?? factory.GetNull<CYDPickup>();
		}
		readonly CYDPickup pickupBO;

		public ZString TransportReference => pickupBO?.YPL_TransportReference ?? string.Empty;

		public ZString? LoadUser => pickupBO.LoadUser?.GS_FullName ?? string.Empty;
	}
}
