using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientTelRimRegistrationCollection))]
	class ClientTelRimRegistrationCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientTelRimRegistrationCollection>
	{
		protected override ClientTelRimRegistrationCollection GetCollectionToTest()
		{
			var device = Factory.New<ClientDeviceHeader>();
			return new ClientTelRimRegistrationCollection(device);
		}
	}
}
