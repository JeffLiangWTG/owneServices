using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(Vehicle))]
	class VehicleTest : DataObjectTestCase<Vehicle>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>()
		{
			{ nameof(Vehicle.VIN), 17 },
			{ nameof(Vehicle.Brand), 60 },
			{ nameof(Vehicle.Model), 50 },
			{ nameof(Vehicle.RegistrationNumber), 17 },
		};
	}
}
