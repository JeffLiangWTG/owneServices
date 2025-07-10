using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CYDDeliveryWrapper : GenericWrapper
	{
		public CYDDeliveryWrapper(CYDDelivery deliveryBO, BusinessObjectFactory factory)
			: base(deliveryBO, factory)
		{
			this.deliveryBO = deliveryBO ?? factory.GetNull<CYDDelivery>();
		}
		readonly CYDDelivery deliveryBO;

		public ZString TransportReference => deliveryBO?.YDL_TransportReference ?? string.Empty;

		public ZString? UnloadUser => deliveryBO.UnloadUser?.GS_FullName ?? string.Empty;
	}
}
