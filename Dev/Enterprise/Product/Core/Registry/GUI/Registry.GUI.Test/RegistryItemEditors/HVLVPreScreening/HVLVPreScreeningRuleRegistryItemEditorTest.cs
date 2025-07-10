using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HVLVPreScreeningRuleRegistryItemEditor))]
	sealed class HVLVPreScreeningRuleRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new HVLVPreScreeningRuleRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((HVLVPreScreeningRuleControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HVLVPreScreeningRuleControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new HVLVPreScreeningRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new HVLVDetailsPreScreeningConfiguration() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
