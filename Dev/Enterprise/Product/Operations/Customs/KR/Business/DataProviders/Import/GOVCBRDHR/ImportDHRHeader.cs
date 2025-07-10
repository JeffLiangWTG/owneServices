using System.Collections.Generic;
using System.Xml.Serialization;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ImportDHRHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportDHRHeader : ImportFTAHeaderCore, IImportDHRHeader
	{
		public ImportDHRInvoiceLine[] DHRInvoiceLines { get; set; }

		IEnumerable<IImportDHRInvoiceLine> IImportDHRHeader.DHRInvoiceLines => DHRInvoiceLines;
	}
}
