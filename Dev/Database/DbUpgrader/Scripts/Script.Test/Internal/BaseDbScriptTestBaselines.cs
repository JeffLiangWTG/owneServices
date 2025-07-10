using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ARAP;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Budget;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.GLConsolidations;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLoss;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLossByBranch;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLossWithDepartment;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using CargoWise.DbUpgrader.Scripts.Definitions.Documents;
using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterData.ComplianceRisk;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.Sales;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;

namespace Enterprise.Build.Database.Script.Testing.Internal;

static class BaseDbScriptTestBaselines
{
	internal static HashSet<string> TestFunctionsCalledByFunctionsAreInlineable => new ()
	{
		nameof(AccGLAggregateWithBranchDept),
		nameof(AccMultilingualTrialBalanceExcludingAlternates),
		nameof(AccPLBSExcludingAlternates),
		nameof(AccRawPLBSExcludingAlternates),
		nameof(AgencyBookedActualContainerMap),
		nameof(BalanceSheet),
		nameof(BalanceSheet2),
		nameof(BalanceSheet3),
		nameof(BalanceSheetAndProfitAndLossBase),
		nameof(BalanceSheetwithTotalReference),
		nameof(BaseTrialBalance),
		nameof(BaseTrialBalanceAcc),
		nameof(BaseTrialBalanceAccBudget),
		nameof(BaseTrialBalanceBudget),
		nameof(CalculateDatesForAgeing),
		nameof(csfn_DisbursementChargeCodes),
		nameof(csfn_ListStaffCodeForSelectRole),
		nameof(csfn_ListStaffNameForSelectRole),
		nameof(csfn_ListStaffPKForSelectRole),
		nameof(ctfn_GetCustomsEntryNumbers),
		nameof(ctfn_SingaporeJobDeclarationAddInfoValues),
		nameof(DecContainerReportInfos),
		nameof(GetAccreditationLevelJobSkillGroupScore),
		nameof(GetAddressPksFilteredForOrg),
		nameof(GetAddressPksForOrg),
		nameof(GetCategoryDetailsFromRegistry),
		nameof(GetComplianceSequenceBookWithFallBack),
		nameof(GetHighestLevelAccreditationAttemptForGroup),
		nameof(GetOpportunityStatuses),
		nameof(GetOrganisationsLinkedToConsolidationGroup),
		nameof(GetSalesRelationActivityWithLastEditDataByRelatedActivityV2),
		nameof(GetScreeningStatusOfShipment),
		nameof(GetShipmentStatusForConsol),
		nameof(GetWorstPartyScreeningStatusOfConsol),
		nameof(GetWorstPartyScreeningStatusOfDeclaration),
		nameof(GetWorstPartyScreeningStatusOfShipment),
		nameof(GetWorstPartyScreeningStatusOfShipmentWithoutChildren),
		nameof(GetWorstPartyScreeningStatusOfShipmentWithoutChildrenNew),
		nameof(GroupsForContact),
		nameof(JobProfitPeriodBase),
		nameof(JobProfitPeriodBase2),
		nameof(JobsAssociatedWithVessel),
		nameof(MissingKeyContactTypes),
		nameof(OrgSupplierPartQuantityConverter),
		nameof(OrgSupplierPartUnitsPerPallet),
		nameof(OtherAtendeesList),
		nameof(PKsByList),
		nameof(ProfitAndLoss_Multilingual),
		nameof(ProfitAndLossBase_CN),
		nameof(ProfitAndLossWithDepartment),
		nameof(ProfitAndLossWithDepartment2),
		nameof(ProfitAndLossWithDepartment2Budget),
		nameof(ProfitAndLossWithDepartment3),
		nameof(ProfitAndLossWithDepartment3Budget),
		nameof(ProfitAndLossWithDepartmentBudget),
		nameof(ProfitAndLossWithDepartmentwithTotalReference),
		nameof(ProfitAndLossWithDepartmentwithTotalReferenceBudget),
		nameof(RankingAddresses),
		nameof(RawPLBSHALT),
		nameof(RawPLBSHALTAcc),
		nameof(RawPLBSHALTAccBranchCurrentOrYearToPeriod),
		nameof(RawPLBSHALTAccBranchRetainedEarnings),
		nameof(RawPLBSHALTAccBudget),
		nameof(RawPLBSHALTAccDepCurrent),
		nameof(RawPLBSHALTAccDepRetainedEarnings),
		nameof(RawPLBSHALTAccDepYeartoPeriod),
		nameof(RawPLBSHALTBudget),
		nameof(RawProfitAndLossOrBalanceSheet),
		nameof(RawProfitAndLossOrBalanceSheetAcc),
		nameof(RawProfitAndLossOrBalanceSheetAccBudget),
		nameof(RawProfitAndLossOrBalanceSheetBudget),
		nameof(Report_CashFlowStatement),
		nameof(ShipmentsAssociatedWithOrgOrDocAddress),
		nameof(TemplatesForMenuItem),
		nameof(WhsPerfGetNumberOfDocketLinesPerHour),
		nameof(WhsPerfGetNumberOfDocketsPerHour),
		nameof(WhsPerfGetNumberOfWorkingHoursBetweenDates),
		nameof(WhsPerfGetNumberOfWorkingHoursInDayFromDateTime),
		nameof(WhsPerfIsInWorkingHours)
	};

	internal static HashSet<Type> TestTempTableElementsAreInlined => new()
	{
		typeof(GLTransactionsSP),
		typeof(GLTransactionsSP_Multilingual),
		typeof(ARAPTransactionsSP),
		typeof(GLSummarySP_Multilingual),
		typeof(ProfitAndLossReportPeriodAnalysis),
		typeof(Report_Shipment_ComplianceRiskSummaryReport),
		typeof(CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitAndLossReportPeriodAnalysis),
		typeof(CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks.ProfitAndLossReportPeriodAnalysisReportingBook)
	};
}
