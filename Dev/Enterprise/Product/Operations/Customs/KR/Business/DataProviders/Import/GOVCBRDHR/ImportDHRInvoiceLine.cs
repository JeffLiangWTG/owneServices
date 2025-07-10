using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportDHRInvoiceLine : IImportDHRInvoiceLine
	{
		public int EntryLineNo { get; set; }
		public int InvoiceLineNo { get; set; }
		public string CertificateOfOriginNo { get; set; }
		public int CertificateOfOriginSeqNo { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal CertificateOfOriginUsedQuantity { get; set; }
		public string CertificateOfOriginUsedUQ { get; set; }

		ZInt IImportDHRInvoiceLine.EntryLineNo => EntryLineNo;
		ZInt IImportDHRInvoiceLine.InvoiceLineNo => InvoiceLineNo;
		ZString IImportDHRInvoiceLine.CertificateOfOriginNo => CertificateOfOriginNo;
		ZInt IImportDHRInvoiceLine.CertificateOfOriginSeqNo => CertificateOfOriginSeqNo;
		ZDecimal IImportDHRInvoiceLine.CertificateOfOriginUsedQuantity => CertificateOfOriginUsedQuantity;
		ZString IImportDHRInvoiceLine.CertificateOfOriginUsedUQ => CertificateOfOriginUsedUQ;
	}
}
