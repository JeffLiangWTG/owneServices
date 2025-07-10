using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UsageData
	{
		public UsageData(string description, short order = 0, int indentLevel = 0)
		{
			Description = description;
			Order = order;
			IndentLevel = indentLevel;
			reportLinksCollection = new Collection<ReportLink>();
		}

		readonly Collection<ReportLink> reportLinksCollection;

		public ZString Description { get; set; }

		public ZShort Order { get; set; }

		public ZInt IndentLevel { get; set; }

		public IEnumerable<ReportLink> ReportLinks
		{
			get { return reportLinksCollection.OrderBy(x => x.ServerCode).ThenBy(x => x.CompanyCode); }
		}

		public void AddReportLink(ZString serverCode, ZString companyCode, ZString pdfLinkUrl, ZString csvLinkUrl)
		{
			var link = new ReportLink();
			link.ServerCode = serverCode.IsEmpty ? "[N/A]" : serverCode.ToString();
			link.CompanyCode = companyCode.IsEmpty ? "[ALL]" : companyCode.ToString();
			link.PdfLinkUrl = pdfLinkUrl;
			link.CsvLinkUrl = csvLinkUrl;
			reportLinksCollection.Add(link);
		}
	}
}
