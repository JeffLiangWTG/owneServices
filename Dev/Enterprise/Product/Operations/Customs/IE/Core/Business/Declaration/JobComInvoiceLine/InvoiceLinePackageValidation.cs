using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine) : base(package, invoiceLine)
		{
		}

		protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new InvoiceLineCusLinkPackage Parent => (InvoiceLineCusLinkPackage)base.Parent;

		protected override void CheckIsLinked()
		{
			base.CheckIsLinked();

			var parent = Parent;
			if (parent.IsLinked)
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine.OverallPackageType == PackageType.Mixed)
				{
					var packageType = parent.Package.PackageType;
					if (packageType != PackageType.Bulk)
					{
						var pivots = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>();
						if (pivots.Any(x => x.Package is Package package && package.PackageType == PackageType.Bulk))
						{
							parent.IsLinkedInfo.AddMessageError(Res.GetString("F77A907B-8998-42D6-9484-960753C41BE3", "Where an Invoice Line is linked to a bulk package type, it is invalid to link the Invoice Line to other non-bulk package types."));
						}
						else if (packageType != PackageType.BreakBulk && pivots.Any(x => x.Package is Package package && package.PackageType == PackageType.BreakBulk))
						{
							parent.IsLinkedInfo.AddMessageError(Res.GetString("F77A907B-8998-42D6-9484-960753C41BE4", "Where an Invoice Line is linked to a break bulk package type, it is invalid to link the Invoice Line to other non-break bulk package types."));
						}
					}
				}
			}
		}
	}
}
