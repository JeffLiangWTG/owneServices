using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(HouseCargoAvailableEventRaiser))]
	sealed class HouseCargoAvailableEventRaiserTestForSea : HouseCargoAvailableEventRaiserTest<CusSCAPivot>
	{
		protected override CusSCAPivot CreateParentCore()
		{
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = container.Pivots.AddNew();
			pivot.CV_CA = house.PK;
			return pivot;
		}

		protected override SchemaColumn StatusSchemaColumn => CusSCAPivotSchema.CV_CargoStatus;
	}
}
