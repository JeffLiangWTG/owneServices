using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class InvoiceLineViewCollection : InvoiceLineViewCollection<JobComInvoiceLine>
	{
		public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void CopyLastLineDetailsToNewLinesIfEnabled(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			if (previousLine?.HasChanges ?? false)
			{
				((IAddInfoManager)previousLine).AddInfo.UpdateRelatedPropertyInfo();
			}

			base.CopyLastLineDetailsToNewLinesIfEnabled(newLine, previousLine);

			if (CopyLastLineDetailsToNewLines)
			{
				JobComInvoiceLineDeepCloneStrategy.CloneCountrySpecificData(newLine, previousLine);
			}
		}
	}
}
