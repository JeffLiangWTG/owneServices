using System;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Client.GCG.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		public void TestClient()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client", Clients.GCG, overrideForClient.Client);
		}

		public void TestClientDisplayName()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client Display Name", "G C F Griffin Pty Ltd", overrideForClient.ClientDisplayName);
		}
	}
}
