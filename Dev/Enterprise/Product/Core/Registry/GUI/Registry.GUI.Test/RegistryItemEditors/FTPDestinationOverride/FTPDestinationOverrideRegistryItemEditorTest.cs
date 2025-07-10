using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FTPDestinationOverrideRegistryItemEditor))]
	sealed class FTPDestinationOverrideRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new FTPDestinationOverrideRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((FTPDestinationOverrideUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FTPDestinationOverrideUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FTPDestinationOverrideRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new FTPDestinationOverrideInfo());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new FTPDestinationOverrideInfo() };
		}
	}
}
