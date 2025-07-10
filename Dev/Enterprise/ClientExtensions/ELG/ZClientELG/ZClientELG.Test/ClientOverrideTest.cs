using System;
using CargoWise.Definitions;
using Enterprise.Client.ELG;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.ZClientELG.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestClients()
		{
			AssertEquals("Should Return ELG client", Clients.ELG, ClientOverride.Client);
		}

		public void TestDisplayName()
		{
			AssertEquals("Client Display Name", "Elite Group Logistics", ClientOverride.ClientDisplayName);
		}

		public void TestModuleOverrides()
		{
			ModuleOverrides moduleOverrides = ClientOverride.ModuleOverrides;
			AssertNotNull("ModuleOverrides should not be null", moduleOverrides);
			AssertNotNull("ModulesOverries should contain ARTransaction", moduleOverrides[ModuleIDs.ARTransaction, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertEquals("ModulesOverrie should be ELGExportModule", typeof(ELGExportModuleStrip).FullName, moduleOverrides[ModuleIDs.ARTransaction, GlbCompany.CurrentCompany.GC_RN_NKCountryCode].TypePath.Split(',')[0]);
		}

		public void TestRegistry()
		{
			AssertEquals("AdditionalRegistryItemSet", ELGDataRegistry.Instance, ClientOverride.AdditionalRegistryItemSet);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		readonly ClientOverride ClientOverride = ClientOverride.Instance;
	}
}
