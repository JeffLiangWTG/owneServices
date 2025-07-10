using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceLineImportLayoutPreviousDocumentsUserControlTest : EU.GUI.PlugIn.Testing.PreviousDocumentUserControlAbstractTest<InvoiceLineImportLayoutPreviousDocumentsUserControl, JobDeclaration>
{
	public void TestPreviousDocumentsFields_InvoiceLines() => TestPreviousDocumentsFields(nameof(JobDeclaration.FilteredInvoiceLines));

	public void TestISupportMultipleResourceStringDataSupporter()
	{
		using (var control = CreateControl())
		{
			var supportMultipleResourceStringDataSupporter = control as ISupportMultipleResourceStringDataSupporter;
			AssertNotNull("Control as ISupportMultipleResourceStringDataSupporter", supportMultipleResourceStringDataSupporter);
			AssertSequencesEqual("MultipleKeysToUse", new string[] { PreviousDocument.InvoiceLineImportResourceStringKey }, supportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData.MultipleKeysToUse);
		}
	}

	void TestPreviousDocumentsFields(string gridBindingMember)
	{
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
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
				AssertEquals("Controls amount", expectedFields.Length, hostControl.Controls.Count - 1);
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
		return new[]
		{
			("ProcedureDropEdit", typeof(ZDropEdit)),
			("CodeDropEdit", typeof(ZDropEdit)),
			("ReferenceTextBox", typeof(ZTextBox)),
			("Reference2TextBox", typeof(ZTextBox)),
			("IssueDateEdit", typeof(ZDateEdit)),
			("LineNoCalcEdit", typeof(ZCalcEdit)),
			("CustomsOfficeCodeFindBox", typeof(ZCodeFindBox)),
			("QuantityCalcDropEdit", typeof(ZCalcDropEdit)),
			("PackageQuantityCalcDropEdit", typeof(ZCalcDropEdit)),
		};
	}

	protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
	{
		return new[]
		{
			(PreviousDocument.Schema.CSI_Procedure, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle)),
			(PreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle)),
			(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle)),
			(PreviousDocument.Schema.CSI_CustomsOffice, typeof(ZCodeFindBoxColumnStyle)),
			(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
			(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyle)),
			(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyle)),
		};
	}
}
