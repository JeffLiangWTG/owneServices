using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EntityPrecedenceRuleEditor))]
	sealed class EntityPrecedenceRuleEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EntityPrecedenceRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, new EntityPrecedenceRuleItemCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EntityPrecedenceRuleEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EntityPrecedenceRuleControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EntityPrecedenceRule() };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EntityPrecedenceRuleControl)editorPane).ReadOnly;
		}
	}
}
