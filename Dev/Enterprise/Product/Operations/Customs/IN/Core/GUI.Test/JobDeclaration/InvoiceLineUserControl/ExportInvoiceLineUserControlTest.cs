using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineUserControl))]
sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be export", nameof(DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestGridColumns()
	{
		using var userControl = new ExportInvoiceLineUserControl();
		userControl.InitializeGridLayout();
		var grid = userControl.CustomsInvoiceLinesBoundGrid;

		var columnInfo = (ZCalcEditColumnStyleInfo)GetColumnStyleAndCheck(AutoINJobComInvoiceLine.Schema.JI_UnitPrice);
		AssertEquals(5, columnInfo.Decimals);

		columnInfo = (ZCalcEditColumnStyleInfo)GetColumnStyleAndCheck(AutoINJobComInvoiceLine.Schema.JI_UnitQuantity);
		AssertEquals(99999999m, columnInfo.MaxValue);
		AssertEquals(0, columnInfo.Decimals);

		GetColumnStyleAndCheck(AutoINJobComInvoiceLine.Schema.JI_UnitUQ);

		var columnInfoForAccessoryStatus = (ZDropEditColumnStyleInfo)GetColumnStyleAndCheck(AutoINJobComInvoiceLine.Schema.JI_AccessoryStatus);
		AssertEquals(60, columnInfoForAccessoryStatus.Width);
		AssertEquals(false, columnInfoForAccessoryStatus.IsVisible);

		var columnInfoTransitCountry = (ZDropEditColumnStyleInfo)GetColumnStyleAndCheck(AutoINJobComInvoiceLine.Schema.JI_RN_NKCountryOfTransit);
		AssertEquals(120, columnInfoTransitCountry.Width);
		AssertEquals(false, columnInfoTransitCountry.IsVisible);

		ZGridColumnInfo GetColumnStyleAndCheck(string columnName)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			Assert(!columnStyle.IsUnavailable);
			return columnStyle;
		}
	}

	public void TestTabOrder()
	{
		using var form = new ZForm();
		using var control = new ExportInvoiceLineUserControl();
		form.Controls.Add(control);
		form.Show();
		string[] expectedTabOrder = {
					"LineDetailsTabPage",
					"NewLineDetailsTabPage",
					"ContainersTabPage",
					"SupportingDocumentTabPage",
					"SWConstituentTabPage",
					"InvoiceLineSWControlsTabPage",
					"JobWorkTabPage",
					"DutyFreeImportAuthorizationTabPage",
					"PackagesTabPage",
					"SWProductionTabPage",
					"PartiesTabPage",
					"CustomFieldsTabPage"
				};

		var actualTabOrder = control.LineDetailTabControl.TabPages.OfType<ZTabPage>().Select(tab => tab.Name).ToArray();
		AssertContainsExactElementsInExactOrder("Tab order should be as expected", expectedTabOrder, actualTabOrder);
	}

	public void TestDutyFreeImportAuthorizationPanelUserControlType()
	{
		using var form = new ZForm();
		using var control = new ExportInvoiceLineUserControl();
		form.Controls.Add(control);
		form.Show();
		var panel = control.FindSingle<ZDynamicControlCreationUserControl>("DutyFreeImportAuthorizationPanel");
		AssertNull("UserControlType before selecting tab", panel.UserControlType);
		control.LineDetailTabControl.SelectTab("DutyFreeImportAuthorizationTabPage");
		AssertEquals("UserControlType after selecting tab", typeof(DutyFreeImportAuthorizationUserControl), panel.UserControlType);
	}
}
