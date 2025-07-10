using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class LocationRiskUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var form = new ZForm())
			{
				var locationRiskUserControl = new LocationRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
				form.Controls.Add(locationRiskUserControl);
				form.Show();

				var index = 0;
				var locationGrid = locationRiskUserControl.Controls.Find("LocationGrid", true).Single() as ZGrid;
				CombineAssertions(() =>
				{
					AssertEquals(4, locationGrid.ColumnStyles.Count);
					AssertEquals("RiskStatus", (locationGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("Location", (locationGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("LocationDescription", (locationGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("Description", (locationGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).ColumnName);
				});
			}
		}

		public void TestGridDisableImportDataMenuItem()
		{
			using var form = new ZForm();
			var locationRiskUserControl = new LocationRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
			form.Controls.Add(locationRiskUserControl);
			form.Show();

			var locationGrid = locationRiskUserControl.Controls.Find("LocationGrid", true).Single() as ZGrid;
			locationGrid.ContextMenu.ShowPopupMenu();

			CombineAssertions(() =>
			{
				AssertEquals(true, locationGrid.DisableImportDataMenuItem);
				AssertEquals(false, locationGrid.ContextMenu.MenuItems.FindByText("Import Data...").Enabled);
				AssertEquals(false, locationGrid.ContextMenu.MenuItems.FindByText("Import Data...").Visible);
			});
		}

		public void TestGridDisableImportDataMenuItem_ADAW()
		{
			using (GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			{
				var locationRiskUserControl = new LocationRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
				form.Controls.Add(locationRiskUserControl);
				form.Show();

				var locationGrid = locationRiskUserControl.Controls.Find("LocationGrid", true).Single() as ZGrid;
				locationGrid.ContextMenu.ShowPopupMenu();

				CombineAssertions(() =>
				{
					AssertEquals(true, locationGrid.DisableImportDataMenuItem);
					AssertEquals(false, locationGrid.ContextMenu.MenuItems.FindByText("Advanced Data Automation Wizard").Enabled);
					AssertEquals(false, locationGrid.ContextMenu.MenuItems.FindByText("Advanced Data Automation Wizard").Visible);
				});
			}
		}
	}
}
