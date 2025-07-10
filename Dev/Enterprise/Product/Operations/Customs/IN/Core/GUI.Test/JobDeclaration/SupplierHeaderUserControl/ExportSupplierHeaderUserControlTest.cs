using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportSupplierHeaderUserControl))]
sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
{
	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "IncoTermExplainButton" });

	public void TestColumnLayoutContext()
	{
		using (var control = new ExportSupplierHeaderUserControl())
		{
			AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestInvoicePartiesTabPage()
	{
		using var control = new ExportSupplierHeaderUserControl();
		AssertType<ZTabPage>("InvoiceParties tab page check", control.Find(c => c.Name == "InvoicePartiesTabPage").First());
	}

	public void TestBuyerDocAddress()
	{
		using var control = new ExportSupplierHeaderUserControl();
		AssertType<ZDocAddressControl>("BuyerDocAddress check", control.Controls.Find("BuyerDocAddress", true).First());
	}

	public void TestColumnsExistanceAndVisibility()
	{
		using var control = new ExportSupplierHeaderUserControl();
		TestColumn(JobComInvoiceHeader.Schema.JZ_ExporterContractNumber, true, false);
		TestColumn(JobComInvoiceHeader.Schema.JZ_PaymentDays, true, false);
		TestColumn(JobComInvoiceHeader.Schema.JZ_PaymentMethod, true, false);
		TestColumn(JobComInvoiceHeader.Schema.BuyingPartyOrgPK, true, true);
		TestColumn(JobComInvoiceHeader.Schema.BuyingPartyAddressPK, true, true);
		TestColumn(JobComInvoiceHeader.Schema.AuthorizedEconomicOperatorOrgPK, true, false);
		TestColumn(JobComInvoiceHeader.Schema.AuthorizedEconomicOperatorCountry, true, false);
		TestColumn(JobComInvoiceHeader.Schema.AuthorizedEconomicOperatorCode, true, false);
		TestColumn(JobComInvoiceHeader.Schema.JZ_AuthorizedEconomicOperatorRole, true, false);

		void TestColumn(string columnName, bool shouldExist, bool shouldVisible)
		{
			var unitFactorColumn = control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(columnName);
			CombineAssertions("Column existance and visibilty check", () =>
			{
				if (shouldExist)
				{
					AssertNotNull($"Column {columnName}", unitFactorColumn);
					AssertEquals($"Column IsVisible {columnName}", shouldVisible, unitFactorColumn.IsVisible);
				}
				else
				{
					AssertNull($"Column {columnName}", unitFactorColumn);
				}
			});
		}
	}

	public void TestTabOrder()
	{
		using var form = new ZForm();
		using var control = new ExportSupplierHeaderUserControl();
		form.Controls.Add(control);
		form.Show();
		string[] expectedTabOrder = {
			"ComInvoiceDetailsTabPage",
			"SupportingDocumentTabPage",
			"InvoicePartiesTabPage",
			"InvoiceHeaderSWControlsTabPage",
			"CustomFieldsTabPage"
		};

		var actualTabOrder = control.InvoiceTabControl.TabPages.OfType<ZTabPage>().Select(tab => tab.Name).ToArray();
		AssertContainsExactElementsInExactOrder("Tab order should be as expected", expectedTabOrder, actualTabOrder);
	}
}
