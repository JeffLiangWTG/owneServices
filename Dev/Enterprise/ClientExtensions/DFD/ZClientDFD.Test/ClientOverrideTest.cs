using System;
using CargoWise.Definitions;
using Enterprise.Client.DFD.Module;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestModuleOverrides()
		{
			AssertNotNull(ClientOverride.Instance.ModuleOverrides);
			var info = ClientOverride.Instance.ModuleOverrides[ModuleIDs.ARTransaction, "AU"];
			Assert(info.IsClientOverride);
			Type moduleOverrideType = typeof(DFDARTransactionModule);
			Assert(info.TypePath.IndexOf(moduleOverrideType.Assembly.FullName) > -1);
			info = ClientOverride.Instance.ModuleOverrides[ModuleIDs.Organisation, "AU"];
			Assert(info.IsClientOverride);
			Assert(info.TypePath.IndexOf(typeof(DFDOrganisationModule).Assembly.FullName) > -1);
			info = ClientOverride.Instance.ModuleOverrides[ModuleIDs.SupplierPart, "US"];
			Assert(info.IsClientOverride);
			Assert(info.TypePath.IndexOf(typeof(DFDOrgSupplierPartModule).Assembly.FullName) > -1);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		public void TestClient()
		{
			AssertEquals(Clients.DFD, clientOverride.Client);
		}

		public void TestRegistry()
		{
			RegistryItemSet itemSet = (RegistryItemSet)clientOverride.AdditionalRegistryItemSet;
			IRegistryItem[] items = itemSet.GetAllItems();
			AssertEquals(2, items.Length);
		}

		#region Setup
		ClientOverride clientOverride;
		protected override void SetUp()
		{
			clientOverride = new ClientOverrideTestClass();
			base.SetUp();
		}

		public class ClientOverrideTestClass : ClientOverride
		{
		}
		#endregion
	}
}
