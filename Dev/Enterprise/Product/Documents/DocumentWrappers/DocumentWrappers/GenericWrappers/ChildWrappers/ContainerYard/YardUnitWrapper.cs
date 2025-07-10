using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class YardUnitWrapper : GenericWrapper
	{
		public YardUnitWrapper(CYDYardUnitState yardUnitBO, BusinessObjectFactory factory)
			: base(yardUnitBO, factory)
		{
			this.yardUnitStateBO = yardUnitBO ?? factory.GetNull<CYDYardUnitState>();
		}
		readonly CYDYardUnitState yardUnitStateBO;

		public CYDReceiveAdviceLineWrapper ReceiveLine => new(yardUnitStateBO?.ReceiveAdviceLine, Factory);

		public CYDReleaseAdviceLineWrapper ReleaseLine => new(yardUnitStateBO?.ReleaseAdviceLine, Factory);

		public CYDDeliveryWrapper Delivery => new(yardUnitStateBO?.Delivery, Factory);

		public CYDPickupWrapper Pickup => new(yardUnitStateBO?.Pickup, Factory);

		public CYDTransportationUnitWrapper DispatchTransportationUnit => new(yardUnitStateBO?.DispatchTransportationUnit, Factory);

		public CYDTransportationUnitWrapper ReceiveTransportationUnit => new(yardUnitStateBO?.ReceiveTransportationUnit, Factory);

		public ZDateTimeOffset? LoadTime => yardUnitStateBO?.YUS_LoadTime;

		public ZString? LoadUser => yardUnitStateBO?.LoadUser?.GS_FullName;

		public ZDateTimeOffset? UnloadTime => yardUnitStateBO?.YUS_UnloadTime;

		public ZString? UnloadUser => yardUnitStateBO?.UnloadUser?.GS_FullName;

		public ZString UnitNumber => yardUnitStateBO.YUS_UnitID;

		public WarehouseBOWrapper Yard => new(YardTitle, yardUnitStateBO.CurrentYard, Factory);

		public ZString? CurrentYardLocation => yardUnitStateBO?.CurrentYardLocation?.WLV_LocationString;

		ZString YardTitle => Res.GetString("YardUnitWrapper|WarehouseTitle", "Yard");
	}
}
