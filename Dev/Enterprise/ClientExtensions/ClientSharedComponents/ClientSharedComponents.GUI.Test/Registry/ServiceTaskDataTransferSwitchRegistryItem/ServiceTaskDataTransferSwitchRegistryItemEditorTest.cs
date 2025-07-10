using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferSwitchRegistryItemEditor))]
	public class ServiceTaskDataTransferSwitchRegistryItemEditorTest : DataTransferSwitchRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ServiceTaskDataTransferSwitchRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServiceTaskDataTransferSwitchRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServiceTaskDataTransferSwitchRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServiceTaskDataTransferSwitchRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
