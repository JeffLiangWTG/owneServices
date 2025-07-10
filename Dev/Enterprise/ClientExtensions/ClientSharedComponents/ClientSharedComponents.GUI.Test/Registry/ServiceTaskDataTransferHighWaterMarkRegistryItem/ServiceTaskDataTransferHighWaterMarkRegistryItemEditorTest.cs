using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferHighWaterMarkRegistryItemEditor))]
	public class ServiceTaskDataTransferHighWaterMarkRegistryItemEditorTest : DataTransferRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ServiceTaskDataTransferHighWaterMarkRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServiceTaskDataTransferHighWaterMarkRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServiceTaskDataTransferHighWaterMarkRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServiceTaskDataTransferHighWaterMarkRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
