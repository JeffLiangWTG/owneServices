using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ImportSupplierHeaderUserControl))]
sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
{
	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "IncoTermExplainButton" });

	public void TestColumnLayoutContext()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestGridId()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			AssertEquals("GridLayoutT+oYGAheR1633dT3oeP+lw==", control.InvoiceHeadersBoundGrid.GridId);
		}
	}

	public void TestTabOrder()
	{
		using var form = new ZForm();
		using var control = new ImportSupplierHeaderUserControl();
		form.Controls.Add(control);
		form.Show();
		string[] expectedTabOrder = {
			"ComInvoiceDetailsTabPage",
			"SupportingDocumentTabPage",
			"CustomFieldsTabPage",
		};

		var actualTabOrder = control.InvoiceTabControl.TabPages.OfType<ZTabPage>().Select(tab => tab.Name).ToArray();
		AssertContainsExactElementsInExactOrder("Tab order should be as expected", expectedTabOrder, actualTabOrder);
	}
}
