using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseAreaWrapper : FreightWrapper
	{
		#region Constructors

		public WarehouseAreaWrapper(WhsArea area, BusinessObjectFactory factoryToWrap)
			: base(area, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static WarehouseAreaWrapper New(WhsArea area, BusinessObjectFactory factoryToWrap)
		{
			return (area == null) ? null : new WarehouseAreaWrapper(area, factoryToWrap);
		}

		#endregion

		#region Properties

		protected override ZString GetWarehouseName()
		{
			return (Area == null ? ZString.Empty : (Area.Warehouse == null ? ZString.Empty : Area.Warehouse.WW_WarehouseNameMultilingual));
		}

		protected override ZString GetAreaName()
		{
			return Area.WA_NameMultilingual;
		}

		protected override ZString GetAreaBarcode()
		{
			return new TextBarcode(AreaName).TextAs128sFontString;
		}

		#endregion

		#region Implementation

		WhsArea Area
		{
			get { return (WhsArea)WrappedObject; }
		}

		#endregion
	}
}
