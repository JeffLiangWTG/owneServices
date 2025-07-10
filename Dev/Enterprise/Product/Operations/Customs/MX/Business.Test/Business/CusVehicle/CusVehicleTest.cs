using System;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(CusVehicle))]
	class CusVehicleTest : Customs.Business.Testing.CusVehicleAbstractTest
	{
		protected override Type ExpectedLookupsType => typeof(Customs.Business.CusVehicleLookups);
		protected override Type ExpectedValidationType => typeof(CusVehicleValidation);

		public void TestVehicleMileageUQByDefault()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals(0, vehicle.CVH_Mileage);
			AssertEquals("", vehicle.CVH_MileageUQ);
			vehicle.CVH_Mileage = 10;
			AssertEquals("KM", vehicle.CVH_MileageUQ);

			vehicle.CVH_Mileage = 0;
			AssertEquals("", vehicle.CVH_MileageUQ);
		}

		public void TestPopulateDataModelIfNeeded()
		{
			var vehicle = (CusVehicle)base.GetNewBusinessObject();
			Factory.Save();
			AssertEquals("MX", vehicle.CVH_DataModel);
		}
	}
}
