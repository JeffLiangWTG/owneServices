using System;
using CargoWise.Definitions;
using Enterprise.Client.NZP;
using Enterprise.Client.NZP.CMS;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(ClientOverrideTestClass))]
	public class NZPClientOverrideTest : ClientOverrideTest
	{
		public void TestClientEnumeration()
		{
			AssertEquals("Client is set to NZP", Clients.NZP, ClientOverride.Instance.Client);
		}

		public void TestClientDisplayName()
		{
			AssertEquals("Client Display Name should be 'New Zealand Post'", "New Zealand Post", ClientOverride.Instance.ClientDisplayName);
		}

		public void TestRegistry()
		{
			AssertEquals(NZPDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
		}

		public void TestInitialiseAndUnitialise()
		{
			ClientOverride clientOverride = new ClientOverrideTestClass();
			clientOverride.Initialise();
			AssertEquals("CMSBatchNumberFountain was not initialised to the correct type", typeof(CMSBatchNumberFountain), CMSBatchNumberFountain.New().GetType());
			clientOverride.Uninitialise();
			AssertEquals("CMSBatchNumberFoutain was not unitiailised", typeof(FileNameNumberFountain), CMSBatchNumberFountain.New().GetType());
		}

		public void TestModuleOverride()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("ModuleOverrides should not be null", moduleOverrides);
			AssertNotNull("ModulesOverries should contain ARTransaction", moduleOverrides[ModuleIDs.ARTransaction, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		class ClientOverrideTestClass : ClientOverride
		{
		}
	}
}
