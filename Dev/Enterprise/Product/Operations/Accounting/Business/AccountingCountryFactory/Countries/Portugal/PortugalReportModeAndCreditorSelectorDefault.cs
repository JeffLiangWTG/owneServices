using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class PortugalReportModeAndCreditorSelectorDefault : IReportModeAndCreditorSelectorDefault
	{
		ZBool IReportModeAndCreditorSelectorDefault.GenerateSalesInvoices => true;
		ZBool IReportModeAndCreditorSelectorDefault.GenerateCreditorInvoices => false;
		ZBool IReportModeAndCreditorSelectorDefault.GenerateAllInvoices => false;
		ZBool IReportModeAndCreditorSelectorDefault.ShouldPopup => true;
	}
}
