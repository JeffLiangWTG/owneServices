using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5BBLine : IImport5BBLine
	{
		public string AmendDataItemID { get; set; }
		public string BeforeDescription { get; set; }
		public string AfterDescription { get; set; }
		public int EntryLineNo { get; set; }
		public string HSCode { get; set; }
		public string HSDescription { get; set; }
		public string InvoiceDescription { get; set; }

		ZString IImport5BBLine.AmendDataItemID => AmendDataItemID;
		ZString IImport5BBLine.BeforeDescription => BeforeDescription;
		ZString IImport5BBLine.AfterDescription => AfterDescription;

		ZInt IImport5BALine.EntryLineNo => EntryLineNo;
		ZString IImport5BALine.HSCode => HSCode;
		ZString IImport5BALine.HSDescription => HSDescription;
		ZString IImport5BALine.InvoiceDescription => InvoiceDescription;
	}
}
