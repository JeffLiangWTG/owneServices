using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TagRuleThrottlingRegistryEditor))]
	public class TagRuleThrottlingRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TagRuleThrottlingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TagRuleThrottlingRegistryEditor(new TagRuleThrottlingRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TagRuleThrottlingRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var header = new TagRuleThrottlingHeader();
			return new object[] { header };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TagRuleThrottlingRegistryControl)editorPane).ReadOnly;
		}

		#endregion Implementation
	}
}
