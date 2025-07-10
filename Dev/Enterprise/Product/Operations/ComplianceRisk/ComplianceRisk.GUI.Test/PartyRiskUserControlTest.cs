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
	public class PartyRiskUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var form = new ZForm())
			{
				var partyRiskUserControl = new PartyRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
				form.Controls.Add(partyRiskUserControl);
				form.Show();

				var partyGrid = partyRiskUserControl.Controls.Find("PartyGrid", true).Single() as ZGrid;

				var index = 0;
				CombineAssertions(() =>
				{
					AssertEquals(4, partyGrid.ColumnStyles.Count);
					AssertEquals("ScreeningStatusDescription", (partyGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("OrgCode", (partyGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("Code", (partyGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("ParentsDescription", (partyGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).ColumnName);
				});
			}
		}

		public void TestGridColumnCaptions()
		{
			using (var form = new ZForm())
			{
				var partyRiskUserControl = new PartyRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
				form.Controls.Add(partyRiskUserControl);
				form.Show();

				var partyGrid = partyRiskUserControl.Controls.Find("PartyGrid", true).Single() as ZGrid;

				var index = 0;
				CombineAssertions(() =>
				{
					AssertEquals("Risk Status", (partyGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Party Code", (partyGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Party Name", (partyGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Description", (partyGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
				});
			}
		}

		public void TestGridDisableImportDataMenuItem()
		{
			using var form = new ZForm();
			var partyRiskUserControl = new PartyRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
			form.Controls.Add(partyRiskUserControl);
			form.Show();

			var partyGrid = partyRiskUserControl.Controls.Find("PartyGrid", true).Single() as ZGrid;
			partyGrid.ContextMenu.ShowPopupMenu();

			CombineAssertions(() =>
			{
				AssertEquals(true, partyGrid.DisableImportDataMenuItem);
				AssertEquals(false, partyGrid.ContextMenu.MenuItems.FindByText("Import Data...").Enabled);
				AssertEquals(false, partyGrid.ContextMenu.MenuItems.FindByText("Import Data...").Visible);
			});
		}

		public void TestGridDisableImportDataMenuItem_ADAW()
		{
			using (GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			{
				var partyRiskUserControl = new PartyRiskUserControl(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory).ComplianceRiskPlugInBusinessObjectForTest);
				form.Controls.Add(partyRiskUserControl);
				form.Show();

				var partyGrid = partyRiskUserControl.Controls.Find("PartyGrid", true).Single() as ZGrid;
				partyGrid.ContextMenu.ShowPopupMenu();

				CombineAssertions(() =>
				{
					AssertEquals(true, partyGrid.DisableImportDataMenuItem);
					AssertEquals(false, partyGrid.ContextMenu.MenuItems.FindByText("Advanced Data Automation Wizard").Enabled);
					AssertEquals(false, partyGrid.ContextMenu.MenuItems.FindByText("Advanced Data Automation Wizard").Visible);
				});
			}
		}
	}
}
