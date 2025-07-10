using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class CommodityAssessmentPanelUserControlTest : ComplianceRiskHelperTest
	{
		public void TestSecurityRightsEditComplianceAssessmentUserControlDisabled()
		{
			var securityInstance = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityInstance.ShipmentsComplianceEditComplianceAssessment.IsAllowed = false;
			securityInstance.BookingsComplianceEditComplianceAssessment.IsAllowed = false;
			securityInstance.AgencyBookingsComplianceEditComplianceAssessment.IsAllowed = false;
			securityInstance.BillsOfLadingComplianceEditComplianceAssessment.IsAllowed = false;

			AssertPanelUserControlEnabled((BusinessObject)CreateNewShipment);
			AssertPanelUserControlEnabled((BusinessObject)CreateNewBookingQuick);
			AssertPanelUserControlEnabled((BusinessObject)CreateNewBookingWithQuote);
			AssertPanelUserControlEnabled((BusinessObject)CreateNewBookingSpotQuote);
			AssertPanelUserControlEnabled((BusinessObject)CreateNewAgencyBooking);
			AssertPanelUserControlEnabled((BusinessObject)CreateNewBillOfLading);

			void AssertPanelUserControlEnabled(BusinessObject hostBusinessEntity)
			{
				using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
				{
					var pluginBizO = new ComplianceRiskPlugInBusinessObject(hostBusinessEntity);
					using var panel = new CommodityAssessmentPanelUserControl(pluginBizO);
					var commodityAssessmentRiskUserControl = (ZUserControl)panel.Controls.Find("CommodityAssessmentRiskUserControl", searchAllChildren: true).SingleOrDefault();

					AssertNotNull(commodityAssessmentRiskUserControl);
					AssertEquals("Security Rights - Edit Compliance Assessment : " + hostBusinessEntity.TableName, true, commodityAssessmentRiskUserControl.Enabled);

					var assessmentNotesGroupBox = (ZGroupBox)commodityAssessmentRiskUserControl.Controls.Find("AssessmentNotesGroupBox", searchAllChildren: true).SingleOrDefault();
					var commodityRiskStatusDropEdit = (ZDropEdit)commodityAssessmentRiskUserControl.Controls.Find("CommodityRiskStatusDropEdit", searchAllChildren: true).SingleOrDefault();
					var commodityRiskStatusLabel = (ZLabel)commodityAssessmentRiskUserControl.Controls.Find("CommodityRiskStatusLabel", searchAllChildren: true).SingleOrDefault();
					var commodityBorderWiseLinkLabel = (ZLinkLabel)commodityAssessmentRiskUserControl.Controls.Find("CommodityBorderWiseLinkLabel", searchAllChildren: true).SingleOrDefault();

					AssertEquals("Security Rights - Edit Compliance Assessment : " + assessmentNotesGroupBox.Name, false, assessmentNotesGroupBox.Enabled);
					AssertEquals("Security Rights - Edit Compliance Assessment : " + commodityRiskStatusDropEdit.Name, false, commodityRiskStatusDropEdit.Enabled);
					AssertEquals("Security Rights - Edit Compliance Assessment : " + commodityRiskStatusLabel.Name, false, commodityRiskStatusLabel.Enabled);
					AssertEquals("Security Rights - Edit Compliance Assessment : " + commodityBorderWiseLinkLabel.Name, true, commodityBorderWiseLinkLabel.Enabled);
				}
			}
		}

		public void TestHideShowToolStripItemWithActiveFilterCountInfoLabel()
		{
			var pluginBizO = new ComplianceRiskPlugInBusinessObject((BusinessObject)CreateNewShipment);
			using var form = new ZForm(pluginBizO);
			using var panel = new CommodityAssessmentPanelUserControl(pluginBizO);
			form.Controls.Add(panel);
			form.Show();

			var commodityFilterStrip = panel.CommodityFilterStrip;
			var toolstrip = panel.HideShowToolStripItem;
			CombineAssertions("Precondition", () =>
			{
				AssertNotNull("CommodityFilterStrip: should not be null", panel.CommodityFilterStrip);
				AssertEquals("Hide/Show filters", toolstrip.Text);
			});

			var filterStrip = commodityFilterStrip.AddNewFilterStrip();
			filterStrip.CurrentDataItem.FilterDescription = "Risk Status";
			panel.FilterStripControl_PerformSearch(this, EventArgs.Empty);
			AssertEquals("Hide/Show filters (1)", toolstrip.Text);
		}

		public void TestRemoveFiltersButton()
		{
			var pluginBizO = new ComplianceRiskPlugInBusinessObject((BusinessObject)CreateNewShipment);
			using var form = new ZForm(pluginBizO);
			using var panel = new CommodityAssessmentPanelUserControl(pluginBizO);
			form.Controls.Add(panel);
			form.Show();

			CombineAssertions("Precondition", () =>
			{
				var removeFiltersToolStrip = (ZToolStrip)panel.Controls.Find("RemoveFiltersToolStrip", searchAllChildren: true).SingleOrDefault();
				var removeFiltersButton = (ZToolStripButton)removeFiltersToolStrip.Items.Find("RemoveFiltersButton", searchAllChildren: true).SingleOrDefault();
				AssertNotNull(removeFiltersButton);
				AssertNotNull("CommodityFilterStrip: should not be null", panel.CommodityFilterStrip);
				AssertEquals("ActiveModuleFilters: count should be 0", 0, panel.CommodityFilterStrip.FilterBusinessObject.ActiveModuleFilters.Count);
			});

			var filterStrip = panel.CommodityFilterStrip.AddNewFilterStrip();
			filterStrip.CurrentDataItem.FilterDescription = "Risk Status";

			AssertEquals("ActiveFilterCountInfoLabel: count should be 1", 1, panel.CommodityFilterStrip.FilterBusinessObject.ActiveModuleFilters.Count);
			AssertEquals("ActiveModuleFilters: count should be 1", 1, panel.CommodityFilterStrip.FilterBusinessObject.ActiveModuleFilters.Count);

			panel.RemoveFiltersButton_Click(this, EventArgs.Empty);
			AssertEquals("ActiveModuleFilters: count should be 0", 0, panel.CommodityFilterStrip.FilterBusinessObject.ActiveModuleFilters.Count);
		}

		public void TestFilterCountInfoLabel()
		{
			var pluginBizO = new ComplianceRiskPlugInBusinessObject((BusinessObject)CreateNewShipment);
			using var form = new ZForm(pluginBizO);
			using var panel = new CommodityAssessmentPanelUserControl(pluginBizO);
			form.Controls.Add(panel);
			form.Show();

			var filterCountInfoLabel = (ZLabel)panel.Controls.Find("FilterCountInfoLabel", searchAllChildren: true).SingleOrDefault();

			CombineAssertions("Precondition:", () =>
			{
				AssertNotNull("FilterCountInfoLabel: should not be null", filterCountInfoLabel);
				AssertEquals("FilterCountInfoLabel: should be (0 of 0 commodities)", "0 of 0 commodities", filterCountInfoLabel.Text);
			});

			panel.SetFilterCountInfoLabelText(1, 1);
			AssertEquals("FilterCountInfoLabel: should be (1 of 1 commodities)", "1 of 1 commodities", filterCountInfoLabel.Text);
		}
	}
}
