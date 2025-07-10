using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMZoneCapacityMultiplierCollection))]
	class BMZoneCapacityMultiplierCollectionTest : ActiveBusinessObjectCollectionTestCase<BMZoneCapacityMultiplierCollection>
	{
		protected override BMZoneCapacityMultiplierCollection GetCollectionToTest()
		{
			var component = Factory.NewWithValidTestData<BMComponent>();
			return component.ZoneCapacityMultipliers;
		}
	}
}
