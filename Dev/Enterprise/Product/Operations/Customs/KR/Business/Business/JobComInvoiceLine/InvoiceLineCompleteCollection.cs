using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void SetDefaultForCommonInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			var jobcomInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var jobComInvoiceHeader = (JobComInvoiceHeader)invoiceLine.InvoiceHeader;
			if (jobComInvoiceHeader != null)
			{
				if (jobComInvoiceHeader.IsExport)
				{
					if (!jobComInvoiceHeader.CertificateOfOriginIssueStatus.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginIssueStatus = jobComInvoiceHeader.CertificateOfOriginIssueStatus;
					}
					if (!jobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin.IsEmpty)
					{
						jobcomInvoiceLine.CriteriaForDeterminingCountryOfOrigin = jobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin;
					}
				}
				else if (jobComInvoiceHeader.IsLocalExport)
				{
					if (!jobComInvoiceHeader.SupportingDocumentCode.IsEmpty)
					{
						jobcomInvoiceLine.SupportingDocumentCode = jobComInvoiceHeader.SupportingDocumentCode;
					}
					if (!jobComInvoiceHeader.SupportingDocumentReferenceNumber.IsEmpty)
					{
						jobcomInvoiceLine.SupportingDocumentReferenceNumber = jobComInvoiceHeader.SupportingDocumentReferenceNumber;
					}
					if (!jobComInvoiceHeader.JZ_InboundDate.IsEmpty)
					{
						jobcomInvoiceLine.JI_InboundDate = jobComInvoiceHeader.JZ_InboundDate;
					}
				}
				else if (jobComInvoiceHeader.IsImport)
				{
					if (!jobComInvoiceHeader.JZ_RN_NKDefaultOrigin.IsEmpty)
					{
						jobcomInvoiceLine.JI_CountryOfOrigin = jobComInvoiceHeader.JZ_RN_NKDefaultOrigin;
					}
					if (!jobComInvoiceHeader.JZ_COOLabelLocation.IsEmpty)
					{
						jobcomInvoiceLine.JI_COOLabelLocation = jobComInvoiceHeader.JZ_COOLabelLocation;
					}
					if (!jobComInvoiceHeader.JZ_COOLabelType.IsEmpty)
					{
						jobcomInvoiceLine.JI_COOLabelType = jobComInvoiceHeader.JZ_COOLabelType;
					}
					if (!jobComInvoiceHeader.JZ_COOExemptionReason.IsEmpty)
					{
						jobcomInvoiceLine.JI_COOExemptionReason = jobComInvoiceHeader.JZ_COOExemptionReason;
					}
					if (!jobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin.IsEmpty)
					{
						jobcomInvoiceLine.CriteriaForDeterminingCountryOfOrigin = jobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginIssuingCountry.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginIssuingCountry = jobComInvoiceHeader.CertificateOfOriginIssuingCountry;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginIssueDate.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginIssueDate = jobComInvoiceHeader.CertificateOfOriginIssueDate;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginNo.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginNo = jobComInvoiceHeader.CertificateOfOriginNo;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginCriteriaCode.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginCriteriaCode = jobComInvoiceHeader.CertificateOfOriginCriteriaCode;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginAgencyName.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginAgencyName = jobComInvoiceHeader.CertificateOfOriginAgencyName;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginAreaName.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginAreaName = jobComInvoiceHeader.CertificateOfOriginAreaName;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginPersonName.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginPersonName = jobComInvoiceHeader.CertificateOfOriginPersonName;
					}
					if (!jobComInvoiceHeader.CertificateOfOriginStatus.IsEmpty)
					{
						jobcomInvoiceLine.CertificateOfOriginStatus = jobComInvoiceHeader.CertificateOfOriginStatus;
					}
				}
			}
		}
	}
}
