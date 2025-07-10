using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5BALine : IImport5BALine
	{
		public int EntryLineNo { get; set; }
		public string HSCode { get; set; }
		public string HSDescription { get; set; }
		public string InvoiceDescription { get; set; }

		ZInt IImport5BALine.EntryLineNo => EntryLineNo;
		ZString IImport5BALine.HSCode => HSCode;
		ZString IImport5BALine.HSDescription => HSDescription;
		ZString IImport5BALine.InvoiceDescription => InvoiceDescription;
	}
}
