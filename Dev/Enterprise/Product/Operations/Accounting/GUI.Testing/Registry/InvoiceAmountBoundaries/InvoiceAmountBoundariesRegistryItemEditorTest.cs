using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.Registry.InvoiceAmountBoundaries;

[TestedType(typeof(InvoiceAmountBoundariesRegistryItemEditor))]
sealed class InvoiceAmountBoundariesRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
	{
		return new InvoiceAmountBoundariesRegistryItem("", null, null, null, RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport, new InvoiceAmountBoundaryCollection());
	}

	protected override RegistryItemEditor GetEditor()
	{
		return new InvoiceAmountBoundariesRegistryItemEditor(RegistryItem.DataType, null, null);
	}

	protected override Type GetExpectedEditorPaneType()
	{
		return typeof(InvoiceAmountBoundariesControl);
	}

	protected override object[] GetValidRegistryValues()
	{
		return new object[] { new InvoiceAmountBoundaryCollection() };
	}

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		return !((InvoiceAmountBoundariesControl)editorPane).ReadOnly;
	}

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
	{
		get { return RegistryItemEditor.EditorPaneAnchor.All; }
	}
}
