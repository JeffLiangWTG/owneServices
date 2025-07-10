using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(KafkaSecurityRegistryItemEditor))]
	sealed class KafkaSecurityRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new KafkaSecurityRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((KafkaSecurityControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(KafkaSecurityControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new KafkaSecurityRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new KafkaSecurity());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new KafkaSecurity() };
		}
	}
}
