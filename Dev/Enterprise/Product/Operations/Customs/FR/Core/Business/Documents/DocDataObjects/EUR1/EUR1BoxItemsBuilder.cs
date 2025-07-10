using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class EUR1BoxItemsBuilder : EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder
	{
		public EUR1BoxItemsBuilder(IEnumerable<JobComInvoiceLine> invoiceLines) : base(invoiceLines)
		{
		}

		protected override EU.Business.Documents.DocDataObjects.PackageAndMarksAndNumbersInfo GetPackageAndMarksAndNumberInfo(JobComInvoiceLine line) => new PackageAndMarksAndNumbersInfo(line);

		protected override bool ShouldShowDescription => InvoiceLines.Select(x => x.JI_Tariff).Distinct().Count() == 1;

		protected override bool ShouldShowInvoiceNumber => false;
	}
}
