using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypes()
		{
			AssertEquals("ExpectedPackageTypesLength", ExpectedPackageTypesLength, Lookups.PackageTypes.Count);
		}

		int ExpectedPackageTypesLength => 114;

		CusSCAPivotLookups lookups;
		CusSCAPivotLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new CusSCAPivotLookups(Pivot);
				}
				return lookups;
			}
		}

		CusSCAPivot pivot;
		CusSCAPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
					CusSCAHouse house = ocean.HouseBills.AddNew();
					pivot = house.Pivot.AddNew();
				}
				return pivot;
			}
		}
	}
}
