using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportSupplierHeaderUserControl))]
sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestSupportingDocumentsUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new ImportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
			var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(InvoiceLayoutSupportingDocumentsUserControl), supportingDocument.UserControlType);
		}
	}

	public void TestPreviousDocumentsUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new ImportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			control.Controls.Find("PreviousDocumentsTabPage", true).First().Show();
			var previousDocument = control.Controls.Find("previousDocumentsUserControl1", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(LayoutPreviousDocumentsUserControl), previousDocument.UserControlType);
		}
	}

	public void TestDisableImportDataMenuItem()
	{
		using (var form = new ZForm())
		using (var control = new ImportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var invoiceHeaderGrid = control.FindSingle<ZGrid>("Grid");
			AssertEquals("Invoice Header Grid, DisableImportDataMenuItem", false, invoiceHeaderGrid.DisableImportDataMenuItem);
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" });
}
