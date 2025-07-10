using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsArrivalMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYesNoList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "N, Y", lookups.YesNoList.CodesAsString);
				AssertSame("Cached", lookups.YesNoList, lookups.YesNoList);
			});
		}

		public void TestOrganisations()
		{
			AssertType<OrgHeaderCollection>(lookups.Organisations);
		}

		public void TestAuthorizationCodeList()
		{
			var codeList = lookups.AuthorizationCodeList;

			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "ACE, ACT", codeList.CodesAsString);
				AssertSame("Cached", codeList, lookups.AuthorizationCodeList);
			});
		}

		public void TestAuthorizationNumberList()
		{
			var cusAuthorizationHeader1 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var cusAuthorizationHeader2 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			var cusAuthorizationHeader3 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			cusAuthorizationHeader3.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			var cusAuthorizationHeader4 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader4.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			cusAuthorizationHeader4.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			cusAuthorizationHeader4.CPH_OH_PermitHolder = ZGuid.BrettsGuid;
			CombineAssertions(() =>
			{
				arrivalMovementHeader.Header.Company.SetCountry(Core.Constants.CountryCodes.Belgium);
				AssertContainsExactElementsInExactOrder("Filter Country", new[] { cusAuthorizationHeader2.PK, cusAuthorizationHeader3.PK, cusAuthorizationHeader4.PK }, lookups.AuthorizationNumberList.Select(x => x.PK));
				arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
				AssertContainsExactElementsInExactOrder("Filter AuthorizationCode", new[] { cusAuthorizationHeader3.PK, cusAuthorizationHeader4.PK }, lookups.AuthorizationNumberList.Select(x => x.PK));
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				AssertContainsExactElementsInExactOrder("Filter AuthorizationOwner", new[] { cusAuthorizationHeader4.PK }, lookups.AuthorizationNumberList.Select(x => x.PK));
			});
		}

		public void TestDischargeTypeList()
		{
			AssertType<DischargeTypeList>(lookups.DischargeTypeList);
		}

		public void TestTransportAtDepartureNationalities()
		{
			AssertType<RefCountryCollection>(lookups.TransportAtDepartureNationalities);
		}

		public void TestModeOfTransportList()
		{
			AssertType<ModeOfTransportList>(lookups.ModeOfTransportList);
		}

		public void TestVessels()
		{
			AssertType<RefVesselCollection>(lookups.Vessels);
		}

		public void TestNctsTransitStatusList()
		{
			CombineAssertions(() =>
			{
				AssertType<NCTS5ArrivalCustomsStatusList>("Type", lookups.NctsTransitStatusList);
				AssertSame("Cached", lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
			});
		}

		public void TestAuthorizationRuleList()
		{
			AssertType<CodeDescriptionPairList>(lookups.AuthorizationRuleList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			lookups = arrivalMovementHeader.Lookups;
		}
		NctsArrivalMovementHeader arrivalMovementHeader;
		NctsArrivalMovementHeaderLookups lookups;
	}
}
