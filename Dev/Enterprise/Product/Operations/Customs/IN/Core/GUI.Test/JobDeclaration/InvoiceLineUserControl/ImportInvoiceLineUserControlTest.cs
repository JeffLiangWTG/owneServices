using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineUserControl))]
sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestTabOrder()
	{
		using var form = new ZForm();
		using var control = new ImportInvoiceLineUserControl();
		form.Controls.Add(control);
		form.Show();
		string[] expectedTabOrder = {
			"LineDetailsTabPage",
			"NewLineDetailsTabPage",
			"ContainersTabPage",
			"LineChargesTabPage",
			"SupportingDocumentTabPage",
			"CustomFieldsTabPage"
		};

		var actualTabOrder = control.LineDetailTabControl.TabPages.OfType<ZTabPage>().Select(tab => tab.Name).ToArray();
		AssertContainsExactElementsInExactOrder("Tab order should be as expected", expectedTabOrder, actualTabOrder);
	}
}
