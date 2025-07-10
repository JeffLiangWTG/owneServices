using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsLocation : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsLocation(WhsLocation location, BusinessObjectFactory factoryToWrap)
			: base(location, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsLocation New(WhsLocation location, BusinessObjectFactory factoryToWrap)
		{
			return location != null ? new DocWhsLocation(location, factoryToWrap) : null;
		}

		#endregion

		#region Properties

		#region ZString

		public ZString WarehouseName => Location?.Row?.Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty;

		public ZString LocationString => Location.WLV_LocationString_UserFriendly;

		public ZString LocationBarcode => new TextBarcode(Location.WLV_LocationString).TextAs128sFontString;

		public ZString LocationCheckDigitString => Location.FormattedCheckDigit;

		#endregion

		#endregion

		#region Implementation

		WhsLocation Location => (WhsLocation)base.WrappedObject;

		#endregion
	}
}
