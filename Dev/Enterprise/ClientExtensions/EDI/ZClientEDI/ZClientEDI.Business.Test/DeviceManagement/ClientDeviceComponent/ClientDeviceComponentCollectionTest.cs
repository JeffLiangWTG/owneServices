using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceComponentCollection))]
	internal class ClientDeviceComponentCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientDeviceComponentCollection>
	{
		protected override ClientDeviceComponentCollection GetCollectionToTest()
		{
			var device = Factory.New<ClientDeviceHeader>();
			return new ClientDeviceComponentCollection(device);
		}
	}
}
