using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterfaceConnectorTemporarilyEnabledUntilRegistryItem))]
	sealed class InterfaceConnectorTemporarilyEnabledUntilRegistryItemTest : StronglyTypedRegistryItemTestCase<InterfaceConnectorTemporarilyEnabledUntil>
	{
		protected override StronglyTypedRegistryItem<InterfaceConnectorTemporarilyEnabledUntil, InterfaceConnectorTemporarilyEnabledUntil> GetNewRegistryItem()
		{
			return new InterfaceConnectorTemporarilyEnabledUntilRegistryItem("InterfaceConnectorTemporarilyEnabledUntil",
									Enterprise.Registry.Business.eHubMessagingRegistry.Categories.eServices,
									(NoResString)"Interface Connector temporarily enabled until",
									(NoResString)"Enter a date to temporarily enable InterfaceConnector until:-",
									RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
		}
	}
}
