using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceComponent))]
	internal class ClientDeviceComponentTest : PersistentBusinessObjectTestCase
	{
		public void TestReadOnlyProperties()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_IsTemplate = true;
			var templateComponent = device.Components.AddNew();
			AssertEquals(true, templateComponent.CDC_LastKnownDeviceInfoDataInfo.ReadOnly);
		}

		public void TestComponentTypeDescription()
		{
			var component = Factory.New<ClientDeviceComponent>();
			component.CDC_ComponentType = component.Lookups.ComponentTypeList[0].Code;
			AssertEquals(component.Lookups.ComponentTypeList[0].Description, component.ComponentTypeDescription);
		}

		public void TestDelete()
		{
			var component = Factory.New<ClientDeviceComponent>();
			var id1 = component.Identifiers.AddNew();
			component.Delete();
			AssertEquals(true, id1.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var device = Factory.New<ClientDeviceHeader>();
			return device.Components.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var device = factory.New<ClientDeviceHeader>();
			return device.Components.AddNew();
		}

		public void TestDoesNotLoadAbstractTypeForDmgDeviceComponentTablePrefix()
		{
			var component = Factory.NewWithValidTestData<ClientDeviceComponent>();
			Factory.Save();
			AssertNoExceptionThrown("Exception thrown loading ClientDeviceComponent ensure that it is not trying to instantiate an abstract class and that the type loaded is of the expected type", () => Factory.Load<ClientDeviceComponent>(DmgDeviceComponentSchema.Constants.Prefix, component.PK));
		}
	}
}
