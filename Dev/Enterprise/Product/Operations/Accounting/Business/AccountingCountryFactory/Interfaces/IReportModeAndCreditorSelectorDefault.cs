using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IReportModeAndCreditorSelectorDefault
	{
		ZBool GenerateSalesInvoices { get; }
		ZBool GenerateCreditorInvoices { get; }
		ZBool GenerateAllInvoices { get; }
		ZBool ShouldPopup { get; }
	}
}
