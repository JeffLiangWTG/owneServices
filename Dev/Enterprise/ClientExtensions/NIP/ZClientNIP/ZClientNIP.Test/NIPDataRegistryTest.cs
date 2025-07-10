using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.NIP.Testing
{
	[TestedType(typeof(NIPDataRegistry))]
	public class NIPDataRegistryTest : RegistryItemSetTestCase<NIPDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("User visible registry items count", 2, AllItems.Count);
			AssertVisible(ItemSet.ConsolAndShipmentImportDirectory);
			AssertVisible(ItemSet.ConsolAndShipmentImportNotificationGroup);
		}

		public void TestConsolAndShipmentImportNotificationGroup()
		{
			AssertEquals("ConsolAndShipmentImportNotificationGroup not set", ZGuid.Empty, ItemSet.ConsolAndShipmentImportNotificationGroup.Value);
			ZGuid group = ZGuid.NewZGuid();
			ItemSet.ConsolAndShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.ToGuid());
			AssertEquals("ConsolAndShipmentImportNotificationGroup is now set", group.ToGuid(), ItemSet.ConsolAndShipmentImportNotificationGroup.Value);
		}

		public void TestConsolAndShipmentImportDirectory()
		{
			AssertEquals("ConsolAndShipmentImportDirectory not set", ZString.Empty, ItemSet.ConsolAndShipmentImportDirectory.Value);
			ItemSet.ConsolAndShipmentImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("ConsolAndShipmentImportDirectory is now set", Env.TempPath, ItemSet.ConsolAndShipmentImportDirectory.Value);
		}
	}
}
