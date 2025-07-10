using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureMovementHeaderPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocationOfGoodsCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsOfLocationType, "Locations");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Spain, parent: grouping);
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "01", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "02", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "03", "Test 3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.ImportAddDocAdditionalInformation, "04", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();

			var list = lookups.LocationOfGoodsCodeList;
			var listCached = lookups.LocationOfGoodsCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("01, 02, 03", list.CodesAsString);
				AssertEquals(list, listCached);
			});
		}

		public void TestSpecificCircumstanceIndicatorList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.SpecificCircumstanceIndicatorList;
				AssertEquals("Values for NCT", "A, C, D, E", list.CodesAsString);
				AssertSame("Cached for NCT", list, lookups.SpecificCircumstanceIndicatorList);
			});
		}

		public void TestNctsControlResultList()
		{
			var list = lookups.NctsControlResultList;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(ClearanceCriteriaCodeList), list.GetType());
				Assert(list.ContainsCode("A2"));
				var testList = list;
				AssertEquals(3, testList.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = new NctsDepartureMovementHeaderPhase4Lookups(nctsHeader.MovementHeader);
		}
		NctsDepartureMovementHeaderPhase4Lookups lookups;
	}
}
