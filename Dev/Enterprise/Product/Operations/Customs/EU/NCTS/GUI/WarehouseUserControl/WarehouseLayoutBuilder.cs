using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class WarehouseLayoutBuilder<T> : ColumnLayoutBuilder<T, WarehouseControlBag> where T : NctsDepartureCargoDesc
	{
		public override WarehouseControlBag CommonBag => WarehouseControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
