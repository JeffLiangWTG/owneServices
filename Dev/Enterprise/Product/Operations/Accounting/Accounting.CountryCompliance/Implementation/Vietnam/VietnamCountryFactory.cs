using Enterprise.Accounting.CountryCompliance.Implementation.Vietnam;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class VietnamCountryFactory :
		IInstanceProvider<IComplianceNumberProvider>,
		IInstanceProvider<IComplianceInvoiceBookRegime>,
		IInstanceProvider<ISupportNegativeAmountOnARTransactions>
	{
		IComplianceNumberProvider IInstanceProvider<IComplianceNumberProvider>.Get() => new VietnamComplianceNumberProvider();

		IComplianceInvoiceBookRegime IInstanceProvider<IComplianceInvoiceBookRegime>.Get() => new VietnamComplianceInvoiceBookRegime();

		ISupportNegativeAmountOnARTransactions IInstanceProvider<ISupportNegativeAmountOnARTransactions>.Get() => new VietnamSupportNegativeAmountProvider();
	}
}
