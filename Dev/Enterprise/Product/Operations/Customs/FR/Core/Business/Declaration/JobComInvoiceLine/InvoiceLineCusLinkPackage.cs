using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceLineCusLinkPackage : BaseCusLinkPackage
	{
		public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		protected override bool IsLinked_ReadOnly
		{
			get
			{
				var declaration = invoiceLine.InvoiceHeader.JobDeclaration;
				if (declaration != null && declaration.IsUCC6)
				{
					return false;
				}
				if (invoiceLine.PackagesPivot.Count == 0)
				{
					return false;
				}
				return invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().All(x => x.Package?.CW_PackType != Package?.CW_PackType);
			}
		}

		readonly JobComInvoiceLine invoiceLine;
	}
}
