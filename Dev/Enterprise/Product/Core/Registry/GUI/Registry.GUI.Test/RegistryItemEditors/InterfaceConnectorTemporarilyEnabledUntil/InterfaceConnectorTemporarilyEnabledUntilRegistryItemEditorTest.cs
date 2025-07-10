using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InterfaceConnectorTemporarilyEnabledUntilRegistryItemEditor))]
	sealed class InterfaceConnectorTemporarilyEnabledUntilRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new InterfaceConnectorTemporarilyEnabledUntilRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InterfaceConnectorTemporarilyEnabledUntilControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InterfaceConnectorTemporarilyEnabledUntilControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InterfaceConnectorTemporarilyEnabledUntilRegistryItem("InterfaceConnectorTemporarilyEnabledUntil",
			Enterprise.Registry.Business.eHubMessagingRegistry.Categories.eServices,
			(NoResString)"Interface Connector temporarily enabled until",
			(NoResString)"Enter a date to temporarily enable InterfaceConnector until:-",
			RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new InterfaceConnectorTemporarilyEnabledUntil() };
		}
	}
}
