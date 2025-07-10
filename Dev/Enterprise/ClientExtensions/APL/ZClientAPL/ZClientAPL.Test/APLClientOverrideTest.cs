using System;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ZClientAPL.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class APLClientOverrideTest : ClientOverrideTest
	{
		public void TestBizOverrides()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals("Client", Clients.APL, @override.Client);
			AssertEquals("Client Display Name", "All Ports International Logistics P/L", @override.ClientDisplayName);
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
