using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute("IE", "CUSOF", "IEROS100", "Rosslare Departure Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "DEP");
			helper.CreateCusCodeListWithAttribute("IE", "CUSOF", "IEDUB100", "Dublin Departure Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "DEP");
			helper.CreateCusCodeListWithAttribute("IE", "CUSOF", "IEROS200", "Rosslare Arrival Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "ARR");
			helper.CreateCusCodeListWithAttribute("FR", "CUSOF", "FRCHE100", "France Departure Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "DEP");
			Factory.Save();

			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.Parent = Factory.NewWithValidTestData<NctsHeader>();
			cusGoodsLocation.CGL_Qualifier = "V";
			var collection = cusGoodsLocation.Lookups.CustomsOfficeList;
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "IEDUB100", "IEROS100" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}
	}
}
