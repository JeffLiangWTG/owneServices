using System;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusVehicle))]
	class CusVehicleTest : Customs.Business.Testing.CusVehicleAbstractTest
	{
		public void TestPopulateDataModelIfNeeded()
		{
			var vehicle = (CusVehicle)base.GetNewBusinessObject();
			Factory.Save();
			AssertEquals("ASY", vehicle.CVH_DataModel);
		}

		protected override Type ExpectedLookupsType => typeof(Customs.Business.CusVehicleLookups);
		protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
	}
}
