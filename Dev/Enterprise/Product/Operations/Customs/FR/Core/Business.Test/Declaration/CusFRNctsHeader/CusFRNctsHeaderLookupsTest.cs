using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusFRNctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNatureOfSealsList()
		{
			AssertSame(Factory.GetCachedValue<NatureOfSealsList>(), lookups.NatureOfSealsList);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "3" }, lookups.NatureOfSealsList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var frNctsHeader = nctsHeader.FRNctsHeader;
			lookups = new CusFRNctsHeaderLookups(frNctsHeader);
		}
		CusFRNctsHeaderLookups lookups;
	}
}
