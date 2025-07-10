using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsPreviousProcedureMasterLookupsTest : TestCaseWithFactory
	{
		public void TestProcedureList()
		{
			var procedureList = lookups.ProcedureList;
			CombineAssertions(() =>
			{
				AssertEquals("List Contents", "N337, 9DEY, 9DEZ", procedureList.CodesAsString);
				AssertSame("Cached", procedureList, lookups.ProcedureList);
			});
		}

		public void TestCustomsOfficeList()
		{
			var dateInFuture = ZDate.Today.AddDays(4);
			var dateInPast = ZDate.Today.AddDays(-4);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BankCode");

			var zzd_DEPerfect = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1", "DE Valid", dateInPast, dateInFuture);
			var zzd_ITPerfect = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2", "IT Valid", dateInPast, dateInFuture);
			var zzd_InPast = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3", "Invalid", dateInPast, dateInPast.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "4", "Invalid", dateInFuture, dateInFuture.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5", "Invalid", dateInPast, dateInFuture);
			var zzd_FalseAttribute = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "6", "Invalid", dateInPast, dateInFuture);
			var zzd_AlsoPerfect = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8", "Valid", dateInPast, dateInFuture);

			helper.CreateCusCodeListAttribute(zzd_DEPerfect.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			helper.CreateCusCodeListAttribute(zzd_ITPerfect.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			helper.CreateCusCodeListAttribute(zzd_InPast.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			helper.CreateCusCodeListAttribute(zzd_FalseAttribute.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "False");
			helper.CreateCusCodeListAttribute(zzd_AlsoPerfect.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			Factory.Save();

			var customsOfficeList = lookups.CustomsOfficeList;
			customsOfficeList.Load();

			AssertContainsExactElementsInAnyOrder("DE Offices", new[] { "1", "8" }, customsOfficeList.Select(x => x.ZZD_Code));
		}

		public void TestAuthorizationNumberList_9DEY()
		{
			var principal = Factory.New<OrgHeader>();
			principal.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "PRINCIPALIPO");
			principal.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "PRINCIPALACT");
			var consignor = Factory.New<OrgHeader>();
			consignor.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "CONSIGNORIPO");
			consignor.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "CONSIGNORACT");

			AssertAuthorizationNumberList(NctsPreviousProcedureList.Codes._9DEY, new string[] { "CONSIGNORIPO", "PRINCIPALIPO" }, principal, consignor);
		}

		public void TestAuthorizationNumberList_9DEZ()
		{
			var principal = Factory.New<OrgHeader>();
			var principalAddress = principal.Addresses.AddNew();
			principalAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			principalAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER2");

			var consignor = Factory.New<OrgHeader>();
			var consignorAddress = consignor.Addresses.AddNew();
			consignorAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER3");
			consignorAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER4");

			CombineAssertions(() =>
			{
				var authorizationNumberList = lookups.AuthorizationNumberList;
				AssertEquals("No consignor/principal", 0, authorizationNumberList.Count);

				nctsHeader.Principal.E2_OA_Address = principalAddress.PK;
				nctsHeader.Consignor.E2_OA_Address = consignorAddress.PK;
				authorizationNumberList = lookups.AuthorizationNumberList;
				AssertEquals("PreviousProcedure is not AT-ZL", 0, authorizationNumberList.Count);

				goodsItem.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				authorizationNumberList = lookups.AuthorizationNumberList;
				AssertContainsExactElementsInAnyOrder("PreviousProcedure is AT-ZL", "NUMBER1, NUMBER2, NUMBER3, NUMBER4", authorizationNumberList.CodesAsString);
				AssertSame("Cached", authorizationNumberList, lookups.AuthorizationNumberList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			lookups = new NctsPreviousProcedureMasterLookups(goodsItem.PreviousProcedureMaster);
		}
		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousProcedureMasterLookups lookups;

		void AssertAuthorizationNumberList(string previousProcedure, string[] expectedResult, OrgHeader principal, OrgHeader consignor)
		{
			CombineAssertions(() =>
			{
				var authorizationNumberList = lookups.AuthorizationNumberList;
				AssertEquals("No consignor/principal", 0, authorizationNumberList.Count);

				nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
				nctsHeader.Consignor.E2_OA_Address = consignor.MainAddress.PK;
				authorizationNumberList = lookups.AuthorizationNumberList;
				AssertEquals($"PreviousProcedure is not {previousProcedure}", 0, authorizationNumberList.Count);

				goodsItem.PreviousProcedureMaster.CSI_Procedure = previousProcedure;
				authorizationNumberList = lookups.AuthorizationNumberList;
				AssertContainsExactElementsInAnyOrder($"PreviousProcedure is {previousProcedure}", expectedResult, authorizationNumberList.GetAllCodes());
				AssertSame($"PreviousProcedure is {previousProcedure} - Cached", authorizationNumberList, lookups.AuthorizationNumberList);
			});
		}
	}
}
