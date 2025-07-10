using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceComponentIdentification))]
	internal class ClientDeviceComponentIdentificationTest : PersistentBusinessObjectTestCase
	{
		public void TestIdentificationTypeDescription()
		{
			var componentId = Factory.New<ClientDeviceComponentIdentification>();
			componentId.CDD_IdentificationType = componentId.Lookups.IdentificationTypeList[0].Code;
			AssertEquals(componentId.Lookups.IdentificationTypeList[0].Description, componentId.IdentificationTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var device = Factory.New<ClientDeviceHeader>();
			var component = device.Components.AddNew();
			return component.Identifiers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var device = factory.New<ClientDeviceHeader>();
			var component = device.Components.AddNew();
			return component.Identifiers.AddNew();
		}

		public void TestDoesNotLoadAbstractTypeForDmgDeviceComponentIdentificationTablePrefix()
		{
			var componentId = Factory.NewWithValidTestData<ClientDeviceComponentIdentification>();
			Factory.Save();
			AssertNoExceptionThrown("Exception thrown loading ClientDeviceComponentIdentification ensure that it is not trying to instantiate an abstract class and and that the type loaded is of the expected type", () => Factory.Load<ClientDeviceComponentIdentification>(DmgDeviceComponentIdentificationSchema.Constants.Prefix, componentId.PK));
		}
	}
}
