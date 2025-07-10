using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Main.Navigation;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Data;

class SnapshotsModulesProvider
{
	readonly HashSet<ModuleIdentifier> ReportModules =
	[
		ModuleIDs.BookingsReports,
		ModuleIDs.ForwardingReport,
		ModuleIDs.CustomsReport,
		ModuleIDs.CustomsGlobalReport,
		ModuleIDs.OrdersReport,
		ModuleIDs.DtbBookingReports,
		ModuleIDs.DtbReports,
		ModuleIDs.TransportReports,
		ModuleIDs.CFSCTOReports,
		ModuleIDs.WhsReport,
		ModuleIDs.WhsTransitReport,
		ModuleIDs.AgencyReports,
		ModuleIDs.SalesMgrReports,
		ModuleIDs.TariffRateReports,
		ModuleIDs.ProcessMgrReports,
		ModuleIDs.DocManagerReports,
		ModuleIDs.ReceivReports,
		ModuleIDs.PayablesReports,
		ModuleIDs.CashBookReports,
		ModuleIDs.JobCostingReport,
		ModuleIDs.GLReports,
		ModuleIDs.BudgetReports,
		ModuleIDs.MasterDataReports,
		ModuleIDs.RefFilesReports,
		ModuleIDs.LocationsReports,
		ModuleIDs.AccountReports,
		ModuleIDs.CustFilesReports,
		ModuleIDs.HRReports,
		ModuleIDs.ArchiveReports,
		ModuleIDs.UserAdminReports,
		ModuleIDs.SystemReports,
		ModuleIDs.ScheduledReports,
		ModuleIDs.ErrorReporting,
	];

	readonly HashSet<ModuleIdentifier> ExcludedModules =
	[
		ModuleIDs.SailingDataVendorImporting,
		ModuleIDs.OnlineSailingSchedules,
		ModuleIDs.RoutingLookups,
		ModuleIDs.ConsolPlanningBoard,
		ModuleIDs.CartageRunSheetDashboard,
		ModuleIDs.CartageLegPlanner,
		ModuleIDs.WhsProductWarehousePortal,
		ModuleIDs.TransitWarehousePortal,
		ModuleIDs.WiseRatesCargoguide,
		ModuleIDs.WiseRatesCargoSphere,
		ModuleIDs.WiseRates,
		ModuleIDs.BiManager,
		ModuleIDs.DocumentAllocation,
		ModuleIDs.DocumentDbMerger,
		ModuleIDs.DocumentDbManager,
		ModuleIDs.ArchiveEDocs,
		ModuleIDs.InvoicePrinting,
		ModuleIDs.Statement,
		ModuleIDs.CASSCostFileImport,
		ModuleIDs.PeriodManagement,
		ModuleIDs.CsvTransactionsImport,
		ModuleIDs.AdministrationPanel,
		ModuleIDs.ProductionRulesPortal,
		ModuleIDs.CsvAccountsImport,
		ModuleIDs.ImportAccountingData,
		ModuleIDs.JobBillingExRateSysConfig,
		ModuleIDs.SendTestCustomsMessage,
		ModuleIDs.Customs.ImportCustomsFilesData,
		ModuleIDs.WhsProductionRulesPortal,
		ModuleIDs.GlowHRMS,
		ModuleIDs.ActiveUsers,
		ModuleIDs.ServiceRequest,
		ModuleIDs.StmUpgrade,
		ModuleIDs.StmFeatureTest,
		ModuleIDs.UpdateNotesPortal,
		ModuleIDs.TranslationFeedback,
		ModuleIDs.ResourceStrings,
	];

	public IEnumerable<SnapshotModule> FindModules() => ModuleTree.Tree.Categories.Values
		.Where(IsNotJumpCategory)
		.SelectMany(c => c.Sections.Values)
		.SelectMany(s => s.Modules.Values)
		.Where(IsNotReportModule)
		.Where(IsNotExcludedModule)
		.Select(module => new SnapshotModule
		{
			ModuleId = module.ModuleID,
			ModuleName = module.ExtendedDescription,
		});

	bool IsNotJumpCategory(ModuleCategory category) =>
		!category.Name.Equals(ModuleTreeLoaderConstant.Category.Jump.Name, StringComparison.OrdinalIgnoreCase);

	bool IsNotReportModule(IMainFormModule module) => !ReportModules.Contains(module.ModuleID);
	bool IsNotExcludedModule(IMainFormModule module) => !ExcludedModules.Contains(module.ModuleID);
}
