using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ModuleIDsTest : BaseModuleIDsTest
	{
		protected override IEnumerable<ModuleIdentifier> GetModuleIDs()
		{
			return ModuleIDs.AllExcludingClientModules;
		}

		public override void TestNestedModules()
		{
			foreach (ModuleIdentifier iD in GetModuleIDs())
			{
				if (iD == ModuleIDs.Customs.NestedDummy)
				{
					Assert(true);
					return;
				}
			}
			Fail("HouseAirCargo was not found");
		}

		public void TestDummyModuleName()
		{
			AssertEquals("Dummy", DummyModuleIDs.Dummy.ToString());
		}

		public void TestModuleIDLoader()
		{
			Array result = ModuleIDLoader.GetModuleIDs(typeof(ModuleIDs), typeof(ModuleIdentifier));
			AssertEquals(typeof(ModuleIdentifier[]), result.GetType());
		}

		public void TestExtendedDescription()
		{
			var modulesWithExtendedDescription = GetModuleIDsWithExtendedDescription();

			foreach (var iD in GetModuleIDs())
			{
				if (modulesWithExtendedDescription.Contains(iD))
				{
					AssertNotEquals(string.Format("Extended Description should differ from Description ({0})", iD.Name), iD.Description, iD.ExtendedDescription);
				}
				else
				{
					AssertEquals(string.Format("Extended Description should be the same as Description ({0})", iD.Name), iD.Description, iD.ExtendedDescription);
				}
			}
		}

		public void TestAllIncludingClientModules()
		{
			var moduleId = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID1, (NoResString)"ClientModuleID1");
			var info = new ModuleInfo(moduleId, "Enterprise.ZArchitecture.GUI", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule");
			var clientModuleInfo = new NewClientModuleInfo("CategoryName1", "SectionName1", info);
			var clientModuleInfos = new[] { clientModuleInfo };
			TestClientHook.Instance.NewClientModulesForTest = clientModuleInfos;

			using (ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance))
			{
				var all = ModuleIDs.AllExcludingClientModules;
				var allIncludingClientModules = ModuleIDs.AllIncludingClientModules;
				AssertCollectionNotContains("All property should not contain new client modules, and yet...", moduleId, all);
				AssertCollectionContains("AllAllIncludingClientModules property should contain new client modules, and yet...", moduleId, allIncludingClientModules);
				AssertEquals(true, all.Any());
				AssertEquals(all.Count() + 1, allIncludingClientModules.Count());
			}
		}

		ModuleIdentifier[] GetModuleIDsWithExtendedDescription()
		{
			return new ModuleIdentifier[]
			{
				ModuleIDs.AccountReports,
				ModuleIDs.AgencyReports,
				ModuleIDs.BookingsReports,
				ModuleIDs.BudgetReports,
				ModuleIDs.NettingReports,
				ModuleIDs.CashBookReports,
				ModuleIDs.CFSCTOReports,
				ModuleIDs.CustFilesReports,
				ModuleIDs.CustomsGlobalReport,
				ModuleIDs.CustomsReport,
				ModuleIDs.DocManagerReports,
				ModuleIDs.DtbBookingReports,
				ModuleIDs.DtbReports,
				ModuleIDs.EISReports,
				ModuleIDs.ForwardingReport,
				ModuleIDs.GLReports,
				ModuleIDs.GLReportingBooksReport,
				ModuleIDs.HRReports,
				ModuleIDs.LocationsReports,
				ModuleIDs.JobCostingReport,
				ModuleIDs.OrdersReport,
				ModuleIDs.PayablesReports,
				ModuleIDs.ProcessMgrReports,
				ModuleIDs.BMReports,
				ModuleIDs.ReceivReports,
				ModuleIDs.RefFilesReports,
				ModuleIDs.ArchiveReports,
				ModuleIDs.MasterDataReports,
				ModuleIDs.SalesMgrReports,
				ModuleIDs.TariffRateReports,
				ModuleIDs.UserAdminReports,
				ModuleIDs.SystemReports,
				ModuleIDs.TransportReports,
				ModuleIDs.WhsReport,
				ModuleIDs.WhsTransitReport,
				ModuleIDs.CYDYardReport,
				DummyModuleIDs.DummyWithExtendedDescription,
				ModuleIDs.Orders,
				ModuleIDs.APPaymentProcessing,
				ModuleIDs.ARPaymentProcessing,
				ModuleIDs.JobShipment,
				ModuleIDs.AccChargeCodeForRegistry,
				ModuleIDs.RefContainer,
				ModuleIDs.GlbBranchNotCurrentCompanyRelated,
				ModuleIDs.GlbCompanyCampaignWithoutFilter,
				ModuleIDs.ImportAccountingData,
				ModuleIDs.OrderLine,
				ModuleIDs.AccTaxRateForRegistry,
				ModuleIDs.ViewLocation,
				ModuleIDs.ShipmentReceival,
				ModuleIDs.JobSailing,
				ModuleIDs.JobSeaVoyage,
				ModuleIDs.ARAccQueryClaim,
				ModuleIDs.APAccQueryClaim,
				ModuleIDs.CartageWorkSheet,
				ModuleIDs.ZARMatching,
				ModuleIDs.ZAPMatching,
				ModuleIDs.DtbBooking,
				ModuleIDs.DtbConsignmentRunSheet,
				ModuleIDs.WhsConfigProduct,
				ModuleIDs.WhsHandlingUnit,
				ModuleIDs.WhsOrder,
				ModuleIDs.WhsOrderLine,
				ModuleIDs.WhsEntryLine,
				ModuleIDs.AgencyBooking,
				ModuleIDs.BMBoard,
				ModuleIDs.SupplierPart,
				ModuleIDs.EntryLine,
				ModuleIDs.Customs.ImportCustomsFilesData,
				ModuleIDs.Customs.AU.CusSCADepotHouse,
				ModuleIDs.Customs.CA.CAQueryMessages,
				ModuleIDs.Customs.CA.CAExportClassification,
				ModuleIDs.Customs.US.Country,
				ModuleIDs.Customs.US.USTariffBulkChange,
				ModuleIDs.Customs.US.QueryMessage,
				ModuleIDs.Customs.US.InBondNumber,
				ModuleIDs.AssetManagementReports,
				ModuleIDs.WhsTransfer,
				ModuleIDs.WhsItemTransferHeader,
				ModuleIDs.WhsProductionRulesPortal,
			};
		}
	}
}
