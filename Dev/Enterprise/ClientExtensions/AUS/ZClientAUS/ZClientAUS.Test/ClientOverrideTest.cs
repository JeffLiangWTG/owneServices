using System;
using CargoWise.Definitions;
using Enterprise.Client.AUS.Modules;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestBizOverrides()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals("Client", Clients.AUS, @override.Client);
			AssertEquals("Client Display Name", "Austin International Trade Services P/L", @override.ClientDisplayName);
			AssertEquals("Help Web Page", "", @override.HelpWebPage);
		}

		public void TestNewClientModules()
		{
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[0].ID, ClientModuleRegistration.ProductImportAndExport);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[1].ID, ClientModuleRegistration.OriginPreferenceMapping);
		}

		public void TestNewClientControllers()
		{
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[0].ID, ClientControllerRegistration.ProductImportAndExport);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[1].ID, ClientControllerRegistration.OriginPreferenceMapping);
		}

		public void TestClientTables()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertNotNull("Schema Info should not be null", @override.DbSchemaExtensionObjects);
			AssertEquals("Client Table scripts", 3, @override.DbSchemaExtensionObjects.TableCreationScripts.Length);
		}

		#region Implementation
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
		#endregion
	}
}
