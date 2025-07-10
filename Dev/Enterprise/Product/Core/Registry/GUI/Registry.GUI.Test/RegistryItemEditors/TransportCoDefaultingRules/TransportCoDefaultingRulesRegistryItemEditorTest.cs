using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TransportCoDefaultingRulesRegistryItemEditor))]
	sealed class TransportCoDefaultingRulesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new TransportCoDefaultingRulesRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TransportCoDefaultingRulesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TransportCoDefaultingRulesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TransportCoDefaultingRulesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new TransportCoDefaultingRules() };
		}
	}
}
