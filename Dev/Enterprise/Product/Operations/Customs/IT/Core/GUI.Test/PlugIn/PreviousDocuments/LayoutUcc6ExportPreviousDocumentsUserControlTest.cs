using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class LayoutUcc6ExportPreviousDocumentsUserControlTest : EU.GUI.PlugIn.Testing.PreviousDocumentUserControlAbstractTest<LayoutUcc6ExportPreviousDocumentsUserControl, JobDeclaration>
{
	public void TestPreviousDocumentsFields_InvoiceLines() => TestPreviousDocumentsFields(nameof(JobDeclaration.FilteredInvoiceLines));

	void TestPreviousDocumentsFields(string gridBindingMember)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(Declaration, configurationValue: true))
		using (var form = new ZForm(Declaration))
		using (var control = CreateControl())
		{
			form.Controls.Add(control);
			form.Show();
			form.SetDataBinding(null, null);
			new ControlRebinder().Rebind(control, nameof(JobDeclaration.FilteredInvoiceLines), gridBindingMember);
			form.SetDataBinding(Declaration, "");

			var hostControl = control.FindSingle<PreviousDocumentsDetailsLayoutControl>(nameof(LayoutPreviousDocumentsUserControl.DetailsLayoutControl))?.DetailsPanel;

			var expectedFields = GetFieldWithTypes().ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Controls amount", expectedFields.Length, hostControl.Controls.Count);
				foreach (var (fieldName, fieldType) in expectedFields)
				{
					var fieldControl = hostControl.FindSingleOrDefault<Control>(fieldName);
					AssertNotNull($"Field: {fieldName}", fieldControl);
					AssertEquals($"Field Type: {fieldType}", fieldType, fieldControl?.GetType());
					AssertEquals($"Field Visible: {fieldType}", true, fieldControl?.Visible);
				}
			});
		}
	}

	IEnumerable<(string, Type)> GetFieldWithTypes()
	{
		yield return ("CodeDropEdit", typeof(ZDropEdit));
		yield return ("ReferenceTextBox", typeof(ZTextBox));
		yield return ("PackageQuantityCalcDropEdit", typeof(ZCalcDropEdit));
		yield return ("QuantityCalcDropEdit", typeof(ZCalcDropEdit));
		yield return ("ItemNumberCalcEdit", typeof(ZCalcEdit));
	}

	protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
	{
		yield return (PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle));
		yield return (PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle));
		yield return (PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyle));
		yield return (PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyle));
		yield return (PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle));
		yield return (PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle));
		yield return (PreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle));
	}
}
