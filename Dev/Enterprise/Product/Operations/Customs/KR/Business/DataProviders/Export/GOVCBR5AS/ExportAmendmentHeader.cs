using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ExportAmendmentHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportAmendmentHeader : IExportAmendmentHeader
	{
		public string ExportDeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Export5ASItem[] AmendmentItems { get; set; }
		public Organisation Exporter { get; set; }
		public string UnipassDeclarantID { get; set; }

		ZString IExportAmendmentHeader.ExportDeclarationNumber => ExportDeclarationNumber;
		ZString IExportAmendmentHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IExportAmendmentHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IEnumerable<IExport5ASItem> IExportAmendmentHeader.AmendmentItems => AmendmentItems;
		IOrganization IExportAmendmentHeader.Exporter => Exporter;
		ZString IExportAmendmentHeader.UnipassDeclarantID => UnipassDeclarantID;
	}
}
