using NUnit.Framework;

namespace Enterprise.ResourceStrings.Maintenance.Testing
{
	sealed class TranslationFileModuleMappingTest : TestCase
	{
		public void TestMappings()
		{
			CombineAssertions(() =>
			{
				AssertLookup("Forwarder", TranslationFileModuleMapping.ModuleTypes.GUI, "Enterprise.Freight");
				AssertLookup("Forwarder", TranslationFileModuleMapping.ModuleTypes.GUI, "Enterprise.Freight.Business");
				AssertLookup("Forwarder", TranslationFileModuleMapping.ModuleTypes.GUI, "Enterprise.Freight.Business.Something");
				AssertLookup("Shipping Manager", TranslationFileModuleMapping.ModuleTypes.GUI, "Enterprise.Freight.Agency");
				AssertLookup("Shipping Manager", TranslationFileModuleMapping.ModuleTypes.GUI, "Enterprise.Freight.Agency.Business");
				AssertLookup("Shipping Manager", TranslationFileModuleMapping.ModuleTypes.GUI, "Enterprise.Freight.Agency.Business");
				AssertLookup("Order Manager", TranslationFileModuleMapping.ModuleTypes.GUI, @"Enterprise.Freight.Forwarding.Orders");
				AssertLookup("Forwarder", TranslationFileModuleMapping.ModuleTypes.GUI, @"Enterprise.Freight.Forwarding");
				AssertLookup("Interface Connector", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, @"Enterprise.Freight.Forwarding.DataTransfer");
				AssertLookup("Core", TranslationFileModuleMapping.ModuleTypes.GUI, @"CargoWise.EntityFramework");
				AssertLookup("Resource Strings", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, @"CargoWise.ResourceStrings");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.BufferManagement.GUI");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.BufferManagement.NetworkVisualisation");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.BufferManagement.Business");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.MasterFiles.BufferManagement.Something");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.VisualBoards.GUI");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.ProcessManagement.GUI");
				AssertLookup("PAVE", TranslationFileModuleMapping.ModuleTypes.PAVE, @"Enterprise.NetworkVisualisation");
				AssertLookup("Shpping Instruction", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, @"Enterprise.Freight.Forwarding.ShippingInstruction.Business");
				AssertLookup("Port Messaging", TranslationFileModuleMapping.ModuleTypes.GUI, @"Enterprise.Freight.Forwarding.PortMessaging.GUI");
				AssertLookup("Port Messaging", TranslationFileModuleMapping.ModuleTypes.GUI, @"Enterprise.Freight.Forwarding.PortMessaging.Business");
				AssertLookup("Recruiter", TranslationFileModuleMapping.ModuleTypes.GUI, @"Enterprise.Recruiter.Business");
				AssertLookup("GLOW", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "Enterprise.Accounting.Netting");
				AssertLookup("GLOW", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "Enterprise.Accounting.Business.eNett");
				AssertLookup("GLOW", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "eTail");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "eManifest");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "eManifest.ServiceTasks");
				AssertLookup("Transit Warehouse", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "Enterprise.TransportCommon.Registry");
				AssertLookup("Transit Warehouse", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "Enterprise.Warehouse.Transit.Module");
				AssertLookup("Transit Warehouse", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "Enterprise.Warehouse.Integration");
				AssertLookup("Transit Warehouse", TranslationFileModuleMapping.ModuleTypes.DoNotTranslate, "Registry.Warehouse");

				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.GUI");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.CN.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.TW.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.DE.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.FR.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.IT.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.CA.Business");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.CN.GUI");
				AssertLookup("Customs", TranslationFileModuleMapping.ModuleTypes.Customs, @"Enterprise.Customs.CN.Module");
				AssertLookup("Documents", TranslationFileModuleMapping.ModuleTypes.GUI, @"Enterprise.DocumentWrappers.Customs.EU");
			});
		}

		void AssertLookup(string expectedModule, TranslationFileModuleMapping.ModuleTypes expectedModuleType, string ns)
		{
			AssertNode(expectedModule, expectedModuleType, TranslationFileModuleMapping.Instance.Lookup(ns));
		}

		void AssertNode(string expectedModule, TranslationFileModuleMapping.ModuleTypes expectedModuleType, TranslationFileModuleMapping.Node actualNode)
		{
			AssertEquals(expectedModule, actualNode.ModuleName);
			AssertEquals(expectedModuleType, actualNode.ModuleType);
		}
	}
}
