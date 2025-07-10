using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
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

		public void TestGoodsLocations()
		{
			var presenter = Factory.New<OrgHeader>();
			var addressA = presenter.Addresses.AddNew();
			var addressB = presenter.Addresses.AddNew();
			var authLocation1 = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Paris");
			var authLocation2 = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Toulouse");
			var whs = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("Marseille");
			var authLocation3 = addressB.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Lyon");
			line.Dec.StorageHeader.SJH_OA_Presenter = addressA.PK;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Paris", "Toulouse"
			}, lookups.GoodsLocations.GetAllCodes());
		}

		public void TestGoodsLocations_ShouldTrimTo35Chars()
		{
			var presenter = Factory.New<OrgHeader>();
			var addressA = presenter.Addresses.AddNew();
			var authLocation1 = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Saint-Horens de Gameville toulouse cedex");
			var authLocation2 = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Clermont-Ferrand");
			line.Dec.StorageHeader.SJH_OA_Presenter = addressA.PK;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Saint-Horens de Gameville toulouse", "Clermont-Ferrand"
			}, lookups.GoodsLocations.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();
			line = (CusTempStorageLine)header.CusTempStorageDec.CusTempStorageLines.AddNew();
			lookups = new CusTempStorageLineLookups(line);
		}

		CusTempStorageLine line;
		CusTempStorageLineLookups lookups;
	}
}
