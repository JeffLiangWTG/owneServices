using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(DraftTransactionStatusReasonCodeRegistryItemEditor))]
	class DraftTransactionStatusReasonCodeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DraftTransactionStatusReasonCodeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DraftTransactionStatusReasonCodeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DraftTransactionStatusReasonCodeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DraftTransactionStatusReasonCodeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new DraftTransactionStatusReasonCodeCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
