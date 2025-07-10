using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouseFilteredCollection))]
	sealed class CusSCAHouseFilteredCollectionTest : SubsetBusinessObjectCollectionTestCase<CusSCAHouseFilteredCollection, CusSCAHouse>
	{
		CusSCAOceanBill oceanBill;

		CusSCAOceanBill OceanBill => oceanBill ?? (oceanBill = Factory.New<CusSCAOceanBill>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusSCAHouse>();

		protected override CusSCAHouseFilteredCollection GetCollectionToTest() => OceanBill.FilteredHouseBills;
	}
}
