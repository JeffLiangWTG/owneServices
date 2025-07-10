using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceHeaderCollection))]
	internal class ClientDeviceHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientDeviceHeaderCollection>
	{
		public void TestDefaultValues()
		{
			var simpleCollection = new ClientDeviceHeaderCollection(Factory);
			var newItem = simpleCollection.AddNew();
			AssertEquals(ZString.Empty, newItem.CDH_EnterpriseCode);
			AssertEquals(ZString.Empty, newItem.CDH_ServerCode);

			var licenceDatabase = Factory.New<LicenceDatabase>();
			var licenceDependentCollection = new ClientDeviceHeaderCollection(licenceDatabase);
			var newItem2 = licenceDependentCollection.AddNew();
			AssertEquals(licenceDatabase.EnterpriseCode, newItem2.CDH_EnterpriseCode);
			AssertEquals(licenceDatabase.LD_ServerCode, newItem2.CDH_ServerCode);
		}
	}
}
