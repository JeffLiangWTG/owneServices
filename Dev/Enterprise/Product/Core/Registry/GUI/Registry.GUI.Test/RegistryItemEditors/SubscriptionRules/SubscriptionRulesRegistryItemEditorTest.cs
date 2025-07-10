using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SubscriptionRulesRegistryItemEditor))]
	sealed class SubscriptionRulesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SubscriptionRulesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new SubscriptionRulesRegistryItemEditor(null, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SubscriptionRulesRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var rules = new SubscriptionRuleCollection();
			var rule = rules.AddNewRule("CD1", ResString.GetMultilingualString("d874b79f-5a99-47d4-8b3e-8d6327bf4aae", "Default Pubished List AAA"), false, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			rule.IsDefault = true;
			return new[] { rules };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SubscriptionRulesRegistryControl)editorPane).ReadOnly;
		}
	}
}
