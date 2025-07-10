using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class NorwayAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IReportSAFTWriter>,
		IInstanceProvider<IReportModeAndCreditorSelectorDefault>,
		IInstanceProvider<IComplianceReportGUIActionProvider>
	{
		IComplianceReportGUIActionProvider IInstanceProvider<IComplianceReportGUIActionProvider>.Get() => new NorwayComplianceReportGUIActionProvider();

		IReportSAFTWriter IInstanceProvider<IReportSAFTWriter>.Get() => new NorwayReportSAFTWriterProvider();

		IReportModeAndCreditorSelectorDefault IInstanceProvider<IReportModeAndCreditorSelectorDefault>.Get() => new NorwayReportModeAndCreditorSelectorDefault();
	}
}
