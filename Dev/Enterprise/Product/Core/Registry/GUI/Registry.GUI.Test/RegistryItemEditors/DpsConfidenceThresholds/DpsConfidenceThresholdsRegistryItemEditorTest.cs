using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DpsConfidenceThresholdsRegistryItemEditor))]
	sealed class DpsConfidenceThresholdsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new DpsConfidenceThresholdsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DpsConfidenceThresholdsUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DpsConfidenceThresholdsUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new DpsConfidenceThresholdsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new DpsConfidenceThresholdsBusinessObject());

		protected override object[] GetValidRegistryValues() => new[] { new DpsConfidenceThresholdsBusinessObject() };
	}
}
