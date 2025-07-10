using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(VehiclesGridColumnLayout))]
	sealed class VehiclesGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<VehiclesGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(CusVehicle.Schema.CVH_VehicleIdentificationNumber, typeof(ZTextBoxColumnStyleInfo), 60),
			(CusVehicle.Schema.CVH_BrandName, typeof(ZTextBoxColumnStyleInfo), 60),
			(CusVehicle.Schema.CVH_ModelName, typeof(ZTextBoxColumnStyleInfo), 60),
		};

		protected override Type GridBoundEntityType => typeof(CusVehicle);
	}
}
