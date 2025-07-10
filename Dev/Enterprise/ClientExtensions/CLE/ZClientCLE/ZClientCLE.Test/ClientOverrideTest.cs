using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.CLE.Modules;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.CLE.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestAdditionalUserVisibleRegistryItems()
		{
			AssertEquals("AdditionalRegistryItemSet", CLEDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
		}

		public void TestClient()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client", Clients.CLE, overrideForClient.Client);
		}

		public void TestClientDisplayName()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client Display Name", "CLIENT", overrideForClient.ClientDisplayName);
		}

		public void TestModuleOverrides()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("ModuleOverrides should not be null", moduleOverrides);
			AssertNotNull("ModulesOverries should contain Containers", moduleOverrides[ModuleIDs.Containers, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertEquals("ModulesOverrie should be CLEContainerModule Override", typeof(CLEContainerModule).FullName, moduleOverrides[ModuleIDs.Containers, GlbCompany.CurrentCompany.GC_RN_NKCountryCode].TypePath.Split(',')[0]);
			AssertNotNull("ModulesOverries should contain orders", moduleOverrides[ModuleIDs.Orders, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertEquals("ModulesOverrie should be CLEOrdersModule Override", typeof(CLEOrdersModule).FullName, moduleOverrides[ModuleIDs.Orders, GlbCompany.CurrentCompany.GC_RN_NKCountryCode].TypePath.Split(',')[0]);
		}

		public void TestClientTypeDeciders()
		{
			var factory = new BusinessObjectFactory();
			ITypeDeciderDictionary clientTypeDeciders = ClientOverride.Instance.ClientTypeDeciders;
			AssertNotNull("ClientTypeDeciders", clientTypeDeciders);
			AssertEquals("ClientTypeDeciders.Count", 1, new List<KeyValuePair<Type, ITypeDecider>>(clientTypeDeciders).Count);
			AssertEquals("Order should map to CLEOrder", typeof(CLEOrder), factory.New<Order>().GetType());
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
