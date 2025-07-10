using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IComplianceDocumentHeaderDetail
	{
		ZString DocumentSubType { get; }

		ZString DocumentNumber { get; }

		ZDateTime DocumentDate { get; }

		ZInt DocumentReportingPeriod { get; }

		ZString DocumentLedger { get; }

		ZGuid DocumentPK { get; }

		ZGuid DocumentCompany { get; }

		ZString DocumentTransactionType { get; }
	}
}
