using System;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestRegistry()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("AdditionalRegistryItemSet", TGEDataRegistry.Instance, overrideForClient.AdditionalRegistryItemSet);
		}

		public void TestClient()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client", Clients.TGE, overrideForClient.Client);
		}

		public void TestClientDisplayName()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client Display Name", "Toll Transport", overrideForClient.ClientDisplayName);
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
