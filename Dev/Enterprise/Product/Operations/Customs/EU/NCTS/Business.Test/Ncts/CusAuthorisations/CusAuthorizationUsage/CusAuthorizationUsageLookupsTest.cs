using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var cusAuthorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			var lookups = new CusAuthorizationUsageLookups(cusAuthorizationUsage);
			CombineAssertions(() =>
			{
				var codeList = lookups.CodeList as CodeDescriptionPairList;
				AssertEquals("CodesAsString", "ACR, SSE, TRD", codeList.CodesAsString);
				AssertSame("Cached", lookups.CodeList, codeList);
			});
		}

		public void TestCodeList_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var cusAuthorizationUsage = nctsHeader.CusAuthorizationUsages.AddNew();
			var lookups = new CusAuthorizationUsageLookups(cusAuthorizationUsage);
			AssertSame(lookups.CodeList, nctsHeader.ArrivalMovementHeader.Lookups.AuthorizationCodeList);
		}
	}
}
