using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using LinkedCusAuthorisationRuleTypeList = Enterprise.Customs.Business.LinkedCusAuthorisationRuleTypeList;
using OfficeCodes_NCTS = Enterprise.Customs.EU.Business.OfficeCodes_NCTS;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIsNctsDepartureMovementHeaderPhase5LookupsType()
		{
			AssertEquals(true, typeof(NctsDepartureMovementHeaderPhase5Lookups).IsAssignableFrom(lookups.GetType()));
		}

		public void TestLocationOfGoodsCodeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "123", "123 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "456", "456 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "789", "789 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			(var authorisation, var rule) = principal.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "DEACR1", Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "123");
			rule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");
			authorisation.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "456");

			var customOffice = nctsHeader.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			customOffice.CY_Data = "DE003202";

			CombineAssertions(() =>
			{
				var agreedLocationOfGoodsList = lookups.LocationOfGoodsCodeList;
				AssertEquals("No Consignor", ZString.Empty, agreedLocationOfGoodsList.CodesAsString);
				AssertSame("No Consignor: Cached", agreedLocationOfGoodsList, lookups.LocationOfGoodsCodeList);

				nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
				agreedLocationOfGoodsList = lookups.LocationOfGoodsCodeList;
				AssertEquals("Consignor with Authorisation and matching Linked Rule", "123", lookups.LocationOfGoodsCodeList.CodesAsString);
				AssertSame("Consignor with Authorisation and matching Linked Rule: Cached", agreedLocationOfGoodsList, lookups.LocationOfGoodsCodeList);

				customOffice.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfGuarantee;
				agreedLocationOfGoodsList = lookups.LocationOfGoodsCodeList;
				AssertEquals("No DEP Custom Office", ZString.Empty, lookups.LocationOfGoodsCodeList.CodesAsString);
				AssertSame("No DEP Custom Office: Cached", agreedLocationOfGoodsList, lookups.LocationOfGoodsCodeList);
			});
		}

		public void TestDeclarationTypeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("T, T1, T2, T2F, TIR", lookups.DeclarationTypeList.CodesAsString);
				AssertSame("Cached", lookups.DeclarationTypeList, lookups.DeclarationTypeList);
			});
		}

		public void TestModeOfTransportList()
		{
			AssertEquals("Values", "1, 2, 3, 4, 5, 8, 9", lookups.ModeOfTransportList.CodesAsString);
			AssertSame("Cached", lookups.ModeOfTransportList, lookups.ModeOfTransportList);
		}

		public void TestBorderModeOfTransportList()
		{
			AssertEquals("Values", "1, 2, 3, 4, 5, 8, 9", lookups.BorderModeOfTransportList.CodesAsString);
			AssertSame("Cached", lookups.BorderModeOfTransportList, lookups.BorderModeOfTransportList);
		}

		public void TestOfficeCodeList()
		{
			movementHeader.CustomsOffices.RemoveAll();
			AddCustomsOffice("FR001", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AddCustomsOffice("FR002", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			AddCustomsOffice("FR003", OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			AddCustomsOffice("FR004", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			AddCustomsOffice("FR005", OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

			var officeCodeList = lookups.OfficeCodeList;
			AssertContainsExactElementsInAnyOrder(new[] { "FR001", "FR002", "FR003" }, officeCodeList.GetAllCodes());

			void AddCustomsOffice(ZString code, ZString role)
			{
				var office = movementHeader.CustomsOffices.AddNew();
				office.CY_Code = role;
				office.CY_Data = code;
			}
		}

		public void TestNctsTransitStatusList()
		{
			AssertType<NCTS5DepartureCustomsStatusList>(lookups.NctsTransitStatusList);
		}

		public void TestTransportAtDepartureTypeOfIdList_ShouldNotContain_Code_99_OwnPropulsion()
		{
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
						Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN,
						ZDate.Today, value: true))
			{
				AssertEquals("In Transition Period, Transport Id type 99 is excluded", expected: false,
					lookups.TransportAtDepartureTypeOfIdList.ContainsCode(NctsTransportTypeOfIdList.Codes._99));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
						Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN,
						ZDate.Today, value: false))
			{
				AssertEquals("Outside Transition Period, Transport Id type 99 is excluded", expected: false,
					lookups.TransportAtDepartureTypeOfIdList.ContainsCode(NctsTransportTypeOfIdList.Codes._99));
			}
		}

		public void TestTransportAtBorderTypeOfIdList_ShouldNotContain_Code_99()
		{
			var movementHeader = nctsHeader.MovementHeader;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
						Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN,
						ZDate.Today, value: true))
			{
				movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
				AssertEquals("In Transition Period, Transport Id type 99 is excluded", expected: false,
					lookups.TransportAtBorderTypeOfIdList.ContainsCode(NctsTransportTypeOfIdList.Codes._99));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			lookups = movementHeader.Lookups;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
		NctsDepartureMovementHeaderLookups lookups;
	}
}
