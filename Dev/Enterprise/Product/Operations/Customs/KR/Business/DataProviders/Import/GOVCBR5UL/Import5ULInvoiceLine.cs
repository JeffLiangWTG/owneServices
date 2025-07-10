using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5ULInvoiceLine : IImport5ULInvoiceLine
	{
		public int InvoiceLineNo { get; set; }
		public string HSDescription { get; set; }
		public string ItemDescription { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		public decimal RefundQuantity { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		public decimal InvoiceQuantity { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.UnitPrice)]
		public decimal UnitPrice { get; set; }

		ZInt IImport5ULInvoiceLine.InvoiceLineNo => InvoiceLineNo;
		ZString IImport5ULInvoiceLine.HSDescription => HSDescription;
		ZString IImport5ULInvoiceLine.ItemDescription => ItemDescription;
		ZDecimal IImport5ULInvoiceLine.RefundQuantity => RefundQuantity;
		ZDecimal IImport5ULInvoiceLine.InvoiceQuantity => InvoiceQuantity;
		ZDecimal IImport5ULInvoiceLine.UnitPrice => UnitPrice;
	}
}
