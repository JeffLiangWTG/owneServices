using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsCusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var lookups = nctsHeader.ArrivalMovementHeader.GoodsLocation.Lookups;
			var list = lookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "Y", list.CodesAsString);
				AssertSame("Cached", list, lookups.QualifierList);
			});
		}

		public void TestQualifierList_ParentIsNotMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var incidentLookups = nctsHeader.EnRouteIncidents.AddNew().GoodsLocation.Lookups;
			var list = incidentLookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "U, W, Z", list.CodesAsString);
				AssertSame("Cached", list, incidentLookups.QualifierList);
			});
		}

		public void TestAdditionalIdentifierList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsOfLocationType, "Location Codes");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "01", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "02", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "03", "Test 3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "04", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("EUN", RefCusCodeListTypes.Codes.GoodsOfLocationType, "05", "Test 5", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("EUN", RefCusCodeListTypes.Codes.GoodsOfLocationType, "06", "Test 6", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var goodsLocation = header.ArrivalMovementHeader.GoodsLocation;
				var lookups = new NctsCusGoodsLocationLookups(goodsLocation);
				var codeList = (Universal.ZZRefCusCodeListCombinedCollection)lookups.AdditionalIdentifierList;
				codeList.Load();
				AssertEquals("Number of codes in list", 4, codeList.Count);

				AssertCodeListValues("01");
				AssertCodeListValues("02");
				AssertCodeListValues("03");
				AssertCodeListValues("04");

				void AssertCodeListValues(string expectedCode)
				{
					Assert($"Expected code: {expectedCode}", codeList.Select(x => x.ZZD_Code).Contains(expectedCode));
				}

				AssertSame("Should be cached", codeList, lookups.AdditionalIdentifierList);
			}
		}
	}
}
