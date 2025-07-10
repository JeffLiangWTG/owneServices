using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(VehicleCollectionProvider))]
	sealed class VehicleCollectionProviderTest : CollectionProviderBaseTest
	{
		public void TestCreateCollectionCount()
		{
			CreateVehicle();
			CreateVehicle();
			CreateEquipment();

			AssertEquals(2, Provider.CollectionForFindbox.Count);
		}

		RefEquipment CreateVehicle()
		{
			var result = Factory.New<RefEquipment>();
			result.RQ_IsVehicle = true;
			return result;
		}

		RefEquipment CreateEquipment()
		{
			var result = Factory.New<RefEquipment>();
			result.RQ_IsVehicle = false;
			return result;
		}

		protected override Type ExpectedCollectionType => typeof(RefEquipmentCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefEquipment;
	}
}
