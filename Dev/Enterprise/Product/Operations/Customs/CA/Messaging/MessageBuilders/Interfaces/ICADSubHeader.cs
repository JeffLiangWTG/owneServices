using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ICADSubHeader
	{
		ZString VendorDetails_Box36 { get; }
		ZString PurchaserDetails_Box37 { get; }
		ZString InvoiceNo_Box38 { get; }
		ZDecimal InvoiceValue_Box39 { get; }
		ZString InvoiceCurrencyCode_Box40 { get; }
		ZString PurchaseOrderNo_Box41 { get; }
		ZDecimal FreightCharges_Box42 { get; }
		ZString USPortOfExit_Box43 { get; }
		IEnumerable<ICADLine> CADLines { get; }
	}
}
