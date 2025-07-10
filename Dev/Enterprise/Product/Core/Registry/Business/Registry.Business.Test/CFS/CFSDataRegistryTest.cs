using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CFSDataRegistry))]
	sealed class CFSDataRegistryTest : RegistryItemSetTestCase<CFSDataRegistry>
	{
		public void TestGatepassCopiesToPrint()
		{
			AssertEquals("DefaultValue", 1, ItemSet.GatepassCopiesToPrint.DefaultValue);
			ItemSet.GatepassCopiesToPrint.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, 2);
			AssertEquals("GatepassCopiesToPrint.Value", 2, ItemSet.GatepassCopiesToPrint.Value);
		}

		public void TestCFSDataRegistryDeliveranceCutOver()
		{
			AssertEquals("DefaultValue", false, ItemSet.IgnoreCSAWithNoIMP.DefaultValue);
			ItemSet.IgnoreCSAWithNoIMP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("IgnoreCSAWi.Value", true, ItemSet.IgnoreCSAWithNoIMP.Value);
		}

		public void TestCFSAirFreightUseClientFreeDays()
		{
			AssertEquals("Default value is true", true, ItemSet.CFSAirFreightUseClientFreeDays.Value);
			ItemSet.CFSAirFreightUseClientFreeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Expected to change to false", false, ItemSet.CFSAirFreightUseClientFreeDays.Value);
			ItemSet.CFSAirFreightUseClientFreeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Expected to change to true", true, ItemSet.CFSAirFreightUseClientFreeDays.Value);
		}

		public void TestCFSSeaFreightUseClientFreeDays()
		{
			AssertEquals("Default value is true", true, ItemSet.CFSSeaFreightUseClientFreeDays.Value);
			ItemSet.CFSSeaFreightUseClientFreeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Expected to change to false", false, ItemSet.CFSSeaFreightUseClientFreeDays.Value);
			ItemSet.CFSSeaFreightUseClientFreeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Expected to change to true", true, ItemSet.CFSSeaFreightUseClientFreeDays.Value);
		}
	}
}
