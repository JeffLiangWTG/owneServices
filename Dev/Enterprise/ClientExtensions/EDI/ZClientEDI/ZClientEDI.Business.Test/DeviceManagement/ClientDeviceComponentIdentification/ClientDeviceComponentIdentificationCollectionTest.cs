using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceComponentIdentificationCollection))]
	internal class ClientDeviceComponentIdentificationCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientDeviceComponentIdentificationCollection>
	{
		protected override ClientDeviceComponentIdentificationCollection GetCollectionToTest()
		{
			var device = Factory.New<ClientDeviceHeader>();
			var component = device.Components.AddNew();
			return new ClientDeviceComponentIdentificationCollection(component);
		}
	}
}
