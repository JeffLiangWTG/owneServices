using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class LayoutUcc6ExportEntryInstructionPreviousDocumentsUserControlTest : EU.GUI.PlugIn.Testing.PreviousDocumentUserControlAbstractTest<LayoutUcc6ExportEntryInstructionPreviousDocumentsUserControl, JobDeclaration>
{
	public void TestISupportMultipleResourceStringDataSupporter()
	{
		using (var control = CreateControl())
		{
			var supportMultipleResourceStringDataSupporter = control as ISupportMultipleResourceStringDataSupporter;
			AssertNotNull("Control as ISupportMultipleResourceStringDataSupporter", supportMultipleResourceStringDataSupporter);
			AssertSequencesEqual("MultipleKeysToUse", new string[] { PreviousDocument.Ucc6ExportEntryInstructionResourceStringKey }, supportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData.MultipleKeysToUse);
		}
	}

	public void TestPreviousDocumentsFields()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(Declaration, configurationValue: true))
		using (var form = new ZForm(Declaration))
		using (var control = CreateControl())
		{
			form.Controls.Add(control);
			form.Show();
			form.SetDataBinding(null, null);
			new ControlRebinder().Rebind(control, nameof(JobDeclaration.FilteredInvoiceLines), nameof(JobDeclaration.CustomsEntryInstructions));
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
		return new[]
		{
			("CodeDropEdit", typeof(ZDropEdit)),
			("ReferenceTextBox", typeof(ZTextBox)),
		};
	}

	protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
	{
		return new[]
		{
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle)),
		};
	}
}
