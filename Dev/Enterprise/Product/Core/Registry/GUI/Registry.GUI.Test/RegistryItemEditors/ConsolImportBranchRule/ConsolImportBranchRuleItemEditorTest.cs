using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ConsolImportBranchRuleItemEditor))]
	sealed class ConsolImportBranchRuleItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ConsolImportBranchRuleItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ConsolImportBranchRuleRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ConsolImportBranchRuleRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ConsolImportBranchRuleRegistryItem("Consol Import Branch Rule", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ImportBranchRule());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ImportBranchRule() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeft; }
		}
	}
}
