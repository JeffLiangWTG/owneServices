using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RFPNumberAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2022, 1, 18)]
		public void TestNetQuantityUnits()
		{
			var newFactory = new BusinessObjectFactory();
			var refDataHelper = new UniversalReferenceTestDataHelper(newFactory);

			var date1 = new ZDateTime(2022, 1, 1);
			var date2 = new ZDateTime(2022, 1, 3);
			var date3 = new ZDateTime(2022, 1, 31);

			refDataHelper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "ONZ", "OUNCE", date1, date2);
			refDataHelper.CreateNewOrGetExistingCusCodeList("US", "EUOM", "DZN", "DOZEN", date1, date2);
			refDataHelper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "DMT", "DECIMETRE", date1, date3);
			newFactory.Save();

			var netQuantityUnits = Factory.New<RFPNumber>().AddInfoLookups.NetQuantityUnits;
			AssertEquals("NetQuantityUnits Lookup count", 1, netQuantityUnits.Count);
			AssertContains("DMT", netQuantityUnits.CodesAsString);
		}

		public void TestPackageTypes()
		{
			AssertEquals("PackageTypes type", typeof(EXDOCPacakgeTypeCodes), Factory.New<RFPNumber>().AddInfoLookups.PackageTypes.GetType());
		}
	}
}
