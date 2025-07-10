using System;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.WFN.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestClient()
		{
			AssertEquals("Client is set to WFN", Clients.WFN, clientOverride.Client);
		}

		public void TestRegistry()
		{
			RegistryItemSet itemSet = (RegistryItemSet)clientOverride.AdditionalRegistryItemSet;
			IRegistryItem[] items = itemSet.GetAllItems();
			AssertEquals(2, items.Length);
			var directoryItem = items.FirstOrDefault(i => i.GetType() == typeof(StringRegistryItem));
			AssertNotNull("directoryItem", directoryItem);
			var groupItem = items.FirstOrDefault(i => i.GetType() == typeof(GuidRegistryItem));
			AssertNotNull("groupItem", groupItem);
		}

		ClientOverride clientOverride;
		protected override void SetUp()
		{
			clientOverride = new ClientOverrideTestClass();
			base.SetUp();
		}

		public class ClientOverrideTestClass : ClientOverride
		{
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
