using System.Collections.Generic;
using System.Linq;

namespace Enterprise.ZArchitecture.Modules
{
	public static class ReportModules
	{
		public static IEnumerable<ModuleIdentifier> GetReportModules()
		{
			yield return ModuleIDs.AccountReports;
			yield return ModuleIDs.AgencyReports;
			yield return ModuleIDs.AnalyticsReports;
			yield return ModuleIDs.ArchiveReports;
			yield return ModuleIDs.AssetManagementReports;
			yield return ModuleIDs.BookingsReports;
			yield return ModuleIDs.BudgetReports;
			yield return ModuleIDs.BMReports;
			yield return ModuleIDs.CashBookReports;
			yield return ModuleIDs.CFSCTOReports;
			yield return ModuleIDs.CustFilesReports;
			yield return ModuleIDs.CustomsGlobalReport;
			yield return ModuleIDs.CustomsReport;
			yield return ModuleIDs.CYDYardReport;
			yield return ModuleIDs.DocManagerReports;
			yield return ModuleIDs.DtbBookingReports;
			yield return ModuleIDs.DtbReports;
			yield return ModuleIDs.EISReports;
			yield return ModuleIDs.ForwardingReport;
			yield return ModuleIDs.GLReportingBooksReport;
			yield return ModuleIDs.GLReports;
			yield return ModuleIDs.NettingReports;
			yield return ModuleIDs.HRReports;
			yield return ModuleIDs.JobCostingReport;
			yield return ModuleIDs.LocationsReports;
			yield return ModuleIDs.MasterDataReports;
			yield return ModuleIDs.RefFilesReports;
			yield return ModuleIDs.ProcessMgrReports;
			yield return ModuleIDs.SystemReports;
			yield return ModuleIDs.UserAdminReports;
			yield return ModuleIDs.OceanCarrierBookingAnalysisReport;
			yield return ModuleIDs.OrdersReport;
			yield return ModuleIDs.PayablesReports;
			yield return ModuleIDs.ReceivReports;
			yield return ModuleIDs.SalesMgrReports;
			yield return ModuleIDs.TariffRateReports;
			yield return ModuleIDs.TransportReports;
			yield return ModuleIDs.WhsReport;
			yield return ModuleIDs.WhsTransitReport;

			yield return ModuleIDs.Customs.NZ.InwardCargoReport;
			yield return ModuleIDs.Customs.NZ.OutwardReport;
			yield return ModuleIDs.Customs.EU.NctsReportsModule;
			yield return ModuleIDs.Customs.EU.GB.GbCcsukReports;

			foreach (var module in ClientHookLoader.Instance.ClientHook?.NewReportModules ?? Enumerable.Empty<ModuleIdentifier>())
			{
				yield return module;
			}
		}
	}
}
