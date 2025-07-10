using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class CusExitDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestArrivalNotificationCodeList()
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
				var exitHeader = Factory.New<CusExitControlHeader>();
				exitHeader.CEH_ParentTableCode = "JE";
				exitHeader.CEH_ReferenceNumber = "123";
				var exitDetail = Factory.New<CusExitDetail>();
				exitDetail.CED_CEH = exitHeader.PK;
				exitDetail.CED_Status = "TTT";
				var lookups = exitDetail.Lookups.ArrivalNotificationCodeList;
				AssertEquals("Number of codes in list", 4, lookups.Count);
				Assert(lookups.ContainsCode("01"));
				Assert(lookups.ContainsCode("02"));
				Assert(lookups.ContainsCode("03"));
				Assert(lookups.ContainsCode("04"));
				AssertEquals("Test 1", lookups.GetDescriptionFromCode("01"));

				AssertSame("Should be cached", lookups, exitDetail.Lookups.ArrivalNotificationCodeList);
			}
		}
	}
}
