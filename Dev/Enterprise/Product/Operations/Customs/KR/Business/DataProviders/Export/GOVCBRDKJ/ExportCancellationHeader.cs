using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportCancellationHeader : IExportCancellationHeader
	{
		public string ExportDeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Organisation Exporter { get; set; }
		public string UnipassDeclarantID { get; set; }

		ZString IExportCancellationHeader.ExportDeclarationNumber => ExportDeclarationNumber;
		ZString IExportCancellationHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IExportCancellationHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IOrganization IExportCancellationHeader.Exporter => Exporter;
		ZString IExportCancellationHeader.UnipassDeclarantID => UnipassDeclarantID;
	}
}
