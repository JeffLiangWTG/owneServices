using System;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.Client.ZClientCCP.Module;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.CCP.Testing
{
	[TestedType(typeof(ClientOverrideForTest))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInstance()
		{
			AssertEquals("Incorrect instance type", typeof(ClientOverride), ClientOverride.Instance.GetType());
		}

		public void TestClient()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client type is supposed to be CCP", typeof(Clients), overrideForClient.Client.GetType());
		}

		public void TestClientDisplayName()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Incorrect client display name", "CCP", overrideForClient.ClientDisplayName);
		}

		public void TestHelpWebPage()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("HelpWebPage is supposed to be \"\"", "", overrideForClient.HelpWebPage);
		}

		public void TestDbSchemaUpgradeInfo()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertNotNull(overrideForClient.DbSchemaExtensionObjects);
			AssertEquals(typeof(ExtensionObjects), overrideForClient.DbSchemaExtensionObjects.GetType());
		}

		public void TestModuleOverrides()
		{
			ModuleOverrides overrides = new ClientOverrideForTest().ModuleOverrides;
			string typePath = overrides[ModuleIDs.CommercialInvoice, Enterprise.Core.Constants.CountryCodes.Australia].TypePath;
			AssertEquals(typeof(AUCommercialInvoiceModuleOverride), Type.GetType(typePath));
			typePath = overrides[ModuleIDs.CommercialInvoice, Enterprise.Core.Constants.CountryCodes.Singapore].TypePath;
			AssertEquals(typeof(SGCommercialInvoiceModuleOverride), Type.GetType(typePath));
			typePath = overrides[ModuleIDs.CommercialInvoice, Enterprise.Core.Constants.CountryCodes.UnitedStates].TypePath;
			AssertEquals(typeof(USCommercialInvoiceModuleOverride), Type.GetType(typePath));
			typePath = overrides[ModuleIDs.CommercialInvoice, Enterprise.Core.Constants.CountryCodes.PuertoRico].TypePath;
			AssertEquals(typeof(USCommercialInvoiceModuleOverride), Type.GetType(typePath));
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		class ClientOverrideForTest : ClientOverride
		{
			public ClientOverrideForTest()
			{
			}
		}
	}
}
