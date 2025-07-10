using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class NorwayReportModeAndCreditorSelectorDefault : IReportModeAndCreditorSelectorDefault
	{
		ZBool IReportModeAndCreditorSelectorDefault.GenerateSalesInvoices => false;
		ZBool IReportModeAndCreditorSelectorDefault.GenerateCreditorInvoices => false;
		ZBool IReportModeAndCreditorSelectorDefault.GenerateAllInvoices => true;
		ZBool IReportModeAndCreditorSelectorDefault.ShouldPopup => false;
	}
}
