using System;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(BatchSailingValueObjectDataAdapterForTest))]
	sealed class BatchSailingValueObjectDataAdapterTest : SailingValueObjectDataAdapterTest_Sea
	{
		public new void TestValueOfRegistryDefaultForImporting()
		{
			SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BatchSailingValueObjectDataAdapterForTest adapter = new BatchSailingValueObjectDataAdapterForTest("a", "b", "c", "d", "d");
			AssertEquals("Adapter should be using UpdateScheduleSailingDuringAutomaticImport registry item", true, adapter.RegistryDefaultForImportingExposed);

			SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Adapter should be using shipment registry item which is currently true", false, adapter.RegistryDefaultForImportingExposed);
		}

		sealed class BatchSailingValueObjectDataAdapterForTest : BatchSailingValueObjectDataAdapter
		{
			public BatchSailingValueObjectDataAdapterForTest(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo)
				: base(transportMode, loadPort, dischargePort, vesselName, voyageNo, ZGuid.Empty)
			{
			}

			public bool RegistryDefaultForImportingExposed
			{
				get { return base.RegistryDefaultForImporting; }
			}
		}
	}
}
