using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
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
				using (newLine.SuspendDefaultingByPrimaryPreference())
				{
					var previousCoo = previousLine?.CertificateOfOriginDocument;
					if (previousCoo != null && previousLine != null && previousLine.IsCertificateOfOriginRequired)
					{
						var newDoc = newLine.CreateNewCertificateOfOrigin();
						newDoc.CSI_ReferenceNumber = previousCoo.CSI_ReferenceNumber;
						newDoc.CSI_RN_NKCountryCode = previousCoo.CSI_RN_NKCountryCode;
						newDoc.CSI_SubType = previousCoo.CSI_SubType;
						newDoc.CSI_LineNo = previousCoo.CSI_LineNo;
					}
				}
			}
		}
	}
}
