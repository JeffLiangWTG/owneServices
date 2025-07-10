using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IComplianceNumberResetStatus
	{
		bool CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(IComplianceNumberResetStatusInputData inputData);
	}

	public interface IComplianceNumberResetStatusInputData
	{
		ZGuid CompanyPK { get; }

		ZString CountryCode { get; }

		ZDate InvoiceDate { get; }

		ZString EInvoicingStatus { get; }
	}
}
