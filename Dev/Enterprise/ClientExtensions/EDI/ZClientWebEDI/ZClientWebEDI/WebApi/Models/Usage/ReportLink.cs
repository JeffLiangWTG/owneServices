using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class ReportLink
	{
		public ZString ServerCode { get; set; }
		public ZString CompanyCode { get; set; }
		public ZString PdfLinkUrl { get; set; }
		public ZString CsvLinkUrl { get; set; }
	}
}