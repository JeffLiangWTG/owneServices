using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusVehicle))]
	class CusVehicleBaseOnlyTest : CusVehicleAbstractTest
	{
		protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);
		protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
	}
}
