using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskMenuResynchronizeComplianceRiskStatusTest : ComplianceRiskHelperTest
	{
		#region Menu: Resynchronize ComplianceRisk Status

		public void TestResynchronizeComplianceRiskStatus_MenuPerformClick_WhenErrorMessages()
		{
			var securityCore = CreateNewSecurityCore;

			CombineAssertions("Shipment: Resynchronize Compliance Risk Status", () =>
			{
				securityCore.ShipmentsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;
				AssertResynchronizeComplianceRiskStatus((IBusiness)CreateNewShipment);
			});

			CombineAssertions("Consolidations: Resynchronize Compliance Risk Status", () =>
			{
				securityCore.ConsolidationsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;
				AssertResynchronizeComplianceRiskStatus((IBusiness)CreateNewConsolidation);
			});

			CombineAssertions("Quoted Booking: Resynchronize Compliance Risk Status", () =>
			{
				securityCore.BookingsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;
				AssertResynchronizeComplianceRiskStatus((IBusiness)CreateNewBookingQuick);
			});

			void AssertResynchronizeComplianceRiskStatus(IBusiness hostBusinessEntity)
			{
				using (new DisposableAction(() => Env.SetTemporarySecurityInstanceForTest(securityCore)))
				using (var form = new ComplianceRiskPluginParentFormForTest(hostBusinessEntity))
				{
					form.Show();

					var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
					pluginBizO.ComplianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

					var menuItem = form.TopLevelMenu.MenuItems.FindByText("Resynchronize Compliance Risk Status", true);
					AssertNotNull("Resynchronize Compliance Risk Status menu should exists.", menuItem);

					menuItem.PerformClick();
					AssertEquals("Please save form before resynchronizing Compliance Risk statuses", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, pluginBizO.ComplianceRiskStatus.IsInDatabase);

					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessages();

					menuItem.PerformClick();
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, pluginBizO.ComplianceRiskStatus.COR_PartyRisk);
					AssertEquals("Resynchronizing completed.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					menuItem.PerformClick();
					AssertEquals($"{(form.PlugIns.Instances[0] as ComplianceRiskPlugIn).GetJobUniqueRef()} is synchronized, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, pluginBizO.ComplianceRiskStatus.IsInDatabase);
				}
			}
		}

		#endregion

		#region Module Menu: Resynchronize Compliance Risk Status

		public void TestResynchronizeComplianceRiskStatus_ModuleActionMenuClick_UnifiedSaving()
		{
			var shipment1 = CreateNewShipment;
			shipment1.JS_HouseBill = "HouseBill1";
			var complianceRisk1 = CreateNewComplianceRiskStatus((BusinessObject)shipment1);

			var shipment2 = CreateNewShipment;
			shipment2.JS_HouseBill = "HouseBill2";
			var complianceRisk2 = CreateNewComplianceRiskStatus((BusinessObject)shipment2);

			Factory.Save();

			var dummyBizOs = new List<BusinessObject>
			{
				(BusinessObject)shipment1, (BusinessObject)shipment2
			};

			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var moduleFilterGrid = new FilterGridModuleForTest(dummyBizOs.ToArray()))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var menuItems = new List<MenuItem>();
				new DeniedPartyScreeningActionsProvider(moduleFilterGrid, menuItems).AddJobsMenuItem();

				var savingCount = 0;
				Factory.Saving += factory => savingCount++;

				menuItems.FindByText("Resynchronize Compliance Risk Status").PerformClick();

				var statuses = Factory.Load<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, dummyBizOs.Select(x => x.PK)));

				AssertEquals(2, statuses.Length);
				Assert(statuses.All(x => x.IsInDatabase));
				AssertEquals(1, savingCount);
			}
		}

		public void TestResynchronizeComplianceRiskStatus_ModuleActionMenuClick_MultipleMessages()
		{
			var shipment1 = CreateNewShipment;
			shipment1.JS_HouseBill = "HouseBill1";
			var complianceRisk1 = CreateNewComplianceRiskStatus((BusinessObject)shipment1);
			complianceRisk1.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var shipment2 = CreateNewShipment;
			shipment2.JS_HouseBill = "HouseBill2";
			var complianceRisk2 = CreateNewComplianceRiskStatus((BusinessObject)shipment2);

			var shipment3 = CreateNewShipment;
			shipment3.JS_HouseBill = "HouseBill3";
			var complianceRisk3 = CreateNewComplianceRiskStatus((BusinessObject)shipment3);
			complianceRisk3.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			Factory.Save();

			var dummyBizOs = new List<BusinessObject>()
			{
				(BusinessObject)shipment1, (BusinessObject)shipment2, (BusinessObject)shipment3
			};

			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;

			using (var moduleFilterGrid1 = new FilterGridModuleForTest(new[] { (BusinessObject)shipment2 }))
			using (var moduleFilterGrid2 = new FilterGridModuleForTest(dummyBizOs.ToArray()))
			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var menuItems1 = new List<MenuItem>();
				var menuItems2 = new List<MenuItem>();

				new DeniedPartyScreeningActionsProvider(moduleFilterGrid1, menuItems1).AddJobsMenuItem();
				new DeniedPartyScreeningActionsProvider(moduleFilterGrid2, menuItems2).AddJobsMenuItem();

				menuItems1.FindByText("Resynchronize Compliance Risk Status").PerformClick();
				UnitTestUserNotification.Instance.ClearMessages();

				menuItems2.FindByText("Resynchronize Compliance Risk Status").PerformClick();

				AssertEquals($@"{shipment1.JS_UniqueConsignRef} resynchronizing completed.
{shipment2.JS_UniqueConsignRef} is synchronized, no need for resynchronization.
{shipment3.JS_UniqueConsignRef} resynchronizing completed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResynchronizeComplianceRiskStatus_ModuleActionMenuClick()
		{
			using (var moduleFilterGrid1 = new FilterGridModuleForTest(Array.Empty<BusinessObject>()))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var menuItems = new List<MenuItem>();
				new DeniedPartyScreeningActionsProvider(moduleFilterGrid1, menuItems).AddJobsMenuItem();
				menuItems.FindByText("Resynchronize Compliance Risk Status").PerformClick();

				AssertEquals("Please select at least 1 row to Process.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResynchronizeComplianceRiskStatus_ModuleActionMenuExists()
		{
			AssertNotNull("Resynchronize Compliance Risk Status menu should exists", ResynchronizeComplianceRiskStatusModuleActionMenuExists(true));
			AssertNull("Resynchronize Compliance Risk Status menu should not exists", ResynchronizeComplianceRiskStatusModuleActionMenuExists(false));

			MenuItem ResynchronizeComplianceRiskStatusModuleActionMenuExists(bool enableComplianceRisk)
			{
				using (var moduleFilterGrid = new FilterGridModuleForTest(Array.Empty<BusinessObject>()))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				{
					var menuItems = new List<MenuItem>();
					new DeniedPartyScreeningActionsProvider(moduleFilterGrid, menuItems).AddJobsMenuItem();
					return menuItems.FindByText("Resynchronize Compliance Risk Status");
				}
			}
		}

		#endregion
	}
}
