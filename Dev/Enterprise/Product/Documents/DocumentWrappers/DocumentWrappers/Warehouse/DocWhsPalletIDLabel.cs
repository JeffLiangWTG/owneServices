using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPalletIDLabel : DocWhsLabel
	{
		#region Constructors

		protected DocWhsPalletIDLabel(WhsLabel label, BusinessObjectFactory factoryToWrap)
			: base(label, factoryToWrap)
		{
		}

		#endregion

		#region New

		public static new DocWhsPalletIDLabel New(WhsLabel label, BusinessObjectFactory factoryToWrap)
		{
			return (label != null ? new DocWhsPalletIDLabel(label, factoryToWrap) : null);
		}

		#endregion

		#region Properties

		public ZString PalletIDBarcode
		{
			get
			{
				TextBarcode barcode = new TextBarcode(PalletID);
				return barcode.TextAs128sFontString;
			}
		}

		public ZString PalletID
		{
			get { return this.palletID; }
			set { this.palletID = value; }
		}

		public ZDateTime PrintDate
		{
			get { return this.printDate; }
			set { this.printDate = value; }
		}

		#endregion

		#region Implementation

		ZString palletID;
		ZDateTime printDate;

		#endregion
	}
}
