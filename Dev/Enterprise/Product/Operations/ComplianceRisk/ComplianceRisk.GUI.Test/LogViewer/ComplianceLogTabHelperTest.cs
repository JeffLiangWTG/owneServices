using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	class ComplianceLogTabHelperTest : ComplianceRiskHelperTest
	{
		const string ComplianceLogsTabPage = "Compliance Logs";

		public void TestFreightModule_AddLogTabIfNeeded()
		{
			AssertAddLogTabIfNeeded((BusinessObject)CreateNewShipment);
			AssertAddLogTabIfNeeded((BusinessObject)CreateNewBookingWithQuote);
			AssertAddLogTabIfNeeded((BusinessObject)CreateNewConsolidation);

			void AssertAddLogTabIfNeeded(BusinessObject parentJob)
			{
				using (var form = new ZForm(parentJob))
				using (var tabControl = new ZTemplateTabControl())
				using (var logsTabPage = new ZLogsTabPage())
				{
					tabControl.TabPages.Add(logsTabPage);
					form.Controls.Add(tabControl);

					var tabControlOfLogs = logsTabPage.Controls[0].Controls[0] as ZTabControl;
					var dpsLogsUserControl = new ZUserControl();
					var complianceLogUserControl = new ComplianceLogUserControl(dpsLogsUserControl);

					using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						ComplianceWiseRegistryHelper.SetValue(false)))
					using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						ComplianceLogTabHelper.AddLogTabIfNeeded(parentJob, logsTabPage, complianceLogUserControl);

						AssertNotNull(tabControlOfLogs);
						AssertEquals(false, tabControlOfLogs.TabPages.OfType<ZTabPage>().Any(u => u.Text == ComplianceLogsTabPage));
					}

					using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						ComplianceWiseRegistryHelper.SetValue(true)))
					{
						ComplianceLogTabHelper.AddLogTabIfNeeded(parentJob, logsTabPage, complianceLogUserControl);
						var logViewer = tabControlOfLogs.TabPages.OfType<ZTabPage>().SingleOrDefault(u => u.Text == ComplianceLogsTabPage)?.Controls[0] as ComplianceLogUserControl;
						AssertNotNull(logViewer);
						AssertEquals(".", logViewer.GetBindingMember());
						AssertEquals(3, tabControlOfLogs.TabPages.Count);

						// Should hide the Removed Commodities tab page for consolidation
						var innerTabControl = logViewer.Controls[0] as ZTemplateTabControl;
						AssertNotNull(innerTabControl);
						AssertEquals(parentJob is IForwardingConsol ? 2 : 3, innerTabControl.TabPages.Count);
					}
				}
			}
		}

		public void TestComplianceLogTabVisible()
		{
			AssertComplianceLogTabVisible((BusinessObject)CreateNewShipment, true, false, true);
			AssertComplianceLogTabVisible((BusinessObject)CreateNewShipment, false, false, false);
			AssertComplianceLogTabVisible((BusinessObject)CreateNewShipment, false, true, true);

			AssertComplianceLogTabVisible((BusinessObject)CreateNewConsolidation, true, true, false);

			void AssertComplianceLogTabVisible(BusinessObject parentJob, bool enableCommodityScreening, bool isAssessmentUsed, bool expected)
			{
				using (var form = new ZForm(parentJob))
				using (var tabControl = new ZTemplateTabControl())
				using (var logsTabPage = new ZLogsTabPage())
				{
					tabControl.TabPages.Add(logsTabPage);
					form.Controls.Add(tabControl);

					var tabControlOfLogs = logsTabPage.Controls[0].Controls[0] as ZTabControl;
					var dpsLogsUserControl = new ZUserControl();
					var complianceLogUserControl = new ComplianceLogUserControl(dpsLogsUserControl);

					if (isAssessmentUsed)
					{
						var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
						complianceRiskStatus.COR_ParentID = parentJob.PK;
						complianceRiskStatus.COR_ParentTableCode = parentJob.TablePrefix;

						StmComplianceEventHelperTest.CreateAssessmentEvent(parentJob, ComplianceEventList.Codes.AssessmentInitialized);
					}

					using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
					using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
					using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
					{
						ComplianceLogTabHelper.AddLogTabIfNeeded(parentJob, logsTabPage, complianceLogUserControl);

						var logViewer = tabControlOfLogs.TabPages.OfType<ZTabPage>().SingleOrDefault(u => u.Text == ComplianceLogsTabPage)?.Controls[0] as ComplianceLogUserControl;
						AssertNotNull(logViewer);

						var innerTabControl = logViewer.Controls[0] as ZTemplateTabControl;
						AssertNotNull(innerTabControl);

						var removedCommoditiesTab = innerTabControl.TabPages.OfType<ZTabPage>().SingleOrDefault(u => u.Text == "Removed Commodities");
						AssertEquals(expected, removedCommoditiesTab != null);
					}
				}
			}
		}

		public void TestFreightModule_ComplianceLogsTabPage_ExcludeFromBindingOnSaveTrue()
		{
			using (var form = new ZForm((BusinessObject)CreateNewShipment))
			using (var tabControl = new ZTemplateTabControl())
			using (var logsTabPage = new ZLogsTabPage())
			{
				tabControl.TabPages.Add(logsTabPage);
				form.Controls.Add(tabControl);

				var dpsLogsUserControl = new ZUserControl();
				var complianceLogUserControl = new ComplianceLogUserControl(dpsLogsUserControl);

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				{
					ComplianceLogTabHelper.AddLogTabIfNeeded((BusinessObject)form.BusinessEntity, logsTabPage, complianceLogUserControl);
					var tabControlOfLogs = logsTabPage.Controls[0].Controls[0] as ZTabControl;
					AssertEquals(true, tabControlOfLogs.TabPages.OfType<ZTabPage>().SingleOrDefault(u => u.Text == ComplianceLogsTabPage).ExcludeFromBindingOnSave);
				}
			}
		}
	}
}
