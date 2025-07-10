using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineLookupsTest : TestCaseWithFactory
	{
		public void TestOwnerReferenceTypeLookup()
		{
			var lookup = lookups.OwnerReferenceTypeList;
			AssertEquals("OwnerReferenceType Lookup should have 5 items", 5, lookup.Count);
		}

		public void TestUnionStatusLookup()
		{
			var lookup = lookups.UnionStatusList;
			AssertEquals("Union Status Lookup should have 5 items", 7, lookup.Count);
		}

		public void TestGoodsTypeLookup()
		{
			var lookup = lookups.GoodsTypeList;
			AssertEquals("Goods type lookup should have 3 items", 3, lookup.Count);
		}

		public void TestWeightUQLookup()
		{
			var lookup = lookups.WeightUQList;
			Assert("Lookup should contain KG for Kilograms", lookup.ContainsCode("KG"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			line = Factory.New<CusTempStorageLine>();
			lookups = new CusTempStorageLineLookups(line);
		}

		CusTempStorageLine line;
		CusTempStorageLineLookups lookups;
	}
}
