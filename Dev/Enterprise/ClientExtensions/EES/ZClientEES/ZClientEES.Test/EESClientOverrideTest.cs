using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EES.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EES.Testing
{
	[TestedType(typeof(ClientOverride))]
	sealed class EESClientOverrideTest : ClientOverrideTest
	{
		public void TestClientTypeDeciders()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertNotNull("ClientTypeDeciders", clientOverride.ClientTypeDeciders);
			AssertEquals("ClientTypeDeciders Count", 1, new List<KeyValuePair<Type, ITypeDecider>>(clientOverride.ClientTypeDeciders).Count);
			AssertEquals("Type Of ForwardingShipment", typeof(EESForwardingShipment), ((TypeDeciderImpl)clientOverride.ClientTypeDeciders[typeof(ForwardingShipment)]).ClientType);
		}

		public void TestClients()
		{
			AssertEquals("Should Return EES client", Clients.EES, ClientOverride.Instance.Client);
		}

		public void TestDisplayName()
		{
			AssertEquals("Display Name", "EES Shipping Pty Ltd", ClientOverride.Instance.ClientDisplayName);
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
