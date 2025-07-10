using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IComplianceInfoToImport
	{
		ZString ComplianceDocumentNumber { get; set; }

		ZString ComplianceSubType { get; set; }

		ZGuid ComplianceDocumentOrganization { get; set; }

		ZString ComplianceDocumentVATRegistrationNum { get; set; }

		ZDateTime ComplianceDocumentDate { get; set; }

		ZInt ComplianceDocumentReportingPeriod { get; set; }

		ZString ComplianceDocumentSupportingReason { get; set; }

		ZString ComplianceSupportingDocumentType { get; set; }

		ZString ComplianceSupportingDocumentNumber { get; set; }

		ZBool CreateComplianceDocumentRecordOnPosting { get; set; }
	}
}
