using System;
using CargoWise.Definitions;
using Enterprise.Client.STI;
using NUnit.Framework;

namespace Enterprise.Client.ZClientSTI.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestClient()
		{
			AssertEquals("Client should be STI", Clients.STI, ClientOverride.Instance.Client);
		}

		public void TestClientDisplayName()
		{
			AssertEquals("Client Name should be 'Strang International'", "Strang International", ClientOverride.Instance.ClientDisplayName);
		}

		public void TestRegistry()
		{
			AssertEquals("AdditionalRegistryItemSet", STIDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
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
