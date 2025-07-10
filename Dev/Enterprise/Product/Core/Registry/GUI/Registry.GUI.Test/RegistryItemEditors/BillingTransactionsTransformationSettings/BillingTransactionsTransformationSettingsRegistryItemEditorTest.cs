using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.eHub.Testing
{
	[TestedType(typeof(BillingTransactionsTransformationSettingsRegistryItemEditor))]
	sealed class BillingTransactionsTransformationSettingsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new BillingTransactionsTransformationSettingsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BillingTransactionsTransformationSettingsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BillingTransactionsTransformationSettingsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillingTransactionsTransformationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new BillingTransactionsTransformationSettings() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
