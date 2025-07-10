using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.Security.ActiveDirectory.GUI.Registry;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(ADRegistryControlItemEditor))]
	class ADRegistryControlItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ADRegistryControlItemEditor(new ADConfigRegistryDataType(ADConfig.DefaultValue));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ADRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ADRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ADConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, ADConfig.DefaultValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { ADConfig.DefaultValue };
		}
	}
}
