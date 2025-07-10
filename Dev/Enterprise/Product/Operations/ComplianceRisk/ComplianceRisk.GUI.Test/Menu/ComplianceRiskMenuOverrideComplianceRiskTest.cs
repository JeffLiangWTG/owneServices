using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskMenuOverrideComplianceRiskTest : ComplianceRiskHelperTest
	{
		public void TestOverrideComplianceRisk_MenuPerformClick_WhenOverrideIsCancel()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceRiskPluginParentFormForTest(CreateNewShipment))
			{
				form.Show();

				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
				pluginBizO.ComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var menuItem = form.TopLevelMenu.MenuItems.FindByText("Override Compliance Risk", true);
				AssertNotNull("Override Compliance Risk menu should exists.", menuItem);
				menuItem.PerformClick();

				AssertEquals("Should not change overall risk status PSK", ComplianceRiskStatusCodeList.Codes.PotentialRisk, pluginBizO.ComplianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestOverrideComplianceRisk_MenuPerformClick_WhenOverrideIsOk()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceRiskPluginParentFormForTest(CreateNewShipment))
			{
				form.Show();

				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
				pluginBizO.ComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = form.TopLevelMenu.MenuItems.FindByText("Override Compliance Risk", true);
				AssertNotNull("Override Compliance Risk menu should exists.", menuItem);
				menuItem.PerformClick();

				AssertEquals("Should change overall risk status OVR", ComplianceRiskStatusCodeList.Codes.OverrideClear, pluginBizO.ComplianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestOverrideComplianceRisk_MenuPerformClick_WhenErrorMessages()
		{
			var securityCore = CreateNewSecurityCore;

			CombineAssertions("Shipment: Override Compliance Risk", () =>
			{
				securityCore.ShipmentsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;
				AssertOverrideComplianceRisk((IBusiness)CreateNewShipment);
			});

			CombineAssertions("Consolidations: Override Compliance Risk", () =>
			{
				securityCore.ConsolidationsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;
				AssertOverrideComplianceRisk((IBusiness)CreateNewConsolidation);
			});

			CombineAssertions("Quoted Booking: Override Compliance Risk", () =>
			{
				securityCore.BookingsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;
				AssertOverrideComplianceRisk((IBusiness)CreateNewBookingQuick);
			});

			void AssertOverrideComplianceRisk(IBusiness hostBusinessEntity)
			{
				using (Env.SetTemporarySecurityInstanceForTest(securityCore))
				using (var form = new ComplianceRiskPluginParentFormForTest(hostBusinessEntity))
				{
					form.Show();

					var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
					pluginBizO.ComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

					var menuItem = form.TopLevelMenu.MenuItems.FindByText("Override Compliance Risk", true);
					AssertNotNull("Override Compliance Risk menu should exists.", menuItem);

					menuItem.PerformClick();
					AssertEquals("Please save the form before overriding job compliance status.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					pluginBizO.ComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
					Factory.Save();

					menuItem.PerformClick();
					AssertEquals("There is no need to override job compliance status when it is Clear (CLR) or Override Clear (OVR).", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					pluginBizO.ComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
					Factory.Save();

					menuItem.PerformClick();
					AssertEquals("There is no need to override job compliance status when it is Clear (CLR) or Override Clear (OVR).", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOverrideComplianceRisk_MenuPerformClick__WhenClearedReason()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));

			Factory.Save();

			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title", IsMandatory = false }
			};
			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.ComplianceRiskOverrideDecisionReason.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			using (var plugin = new ComplianceRiskPlugInForTest(shipment))
			{
				plugin.OverrideComplianceRiskStatusExposed(new OverrideComplianceRiskConfirmationModel { Reason = "Test Reason" });

				var secEvent = Factory.LoadTop1<StmComplianceEvent>(new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK).AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear));

				var log = JsonConvert.DeserializeObject<ComplianceAuditSnapshot>(secEvent.SCE_Snapshot);
				AssertEquals("CCC", log.OverrideDecision.Code);
				AssertEquals("Title", log.OverrideDecision.Description);
				AssertEquals("Test Reason", log.OverrideDecision.Reason);
			}
		}
	}
}
