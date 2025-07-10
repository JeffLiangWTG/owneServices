using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	class NctsArrivalMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckPortsOfUnloading()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Valid Customs Offices for Location", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "Role", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Germany);

			var aa01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA02", "Loading place AA02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa03 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA03", "Loading place AA03", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa04 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA04", "Loading place AA04", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa05 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "AA05", "Loading place AA05", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa06 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA06", "Loading place AA06", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE001501");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa01.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa02.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa03.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa03.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE001501");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa04.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa05.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa06.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var acrAuthorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "DEACR1");
			acrAuthorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Location, "AA06");

			var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "DEACE1");
			authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Location, "AA01");
			authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Location, "AA02");
			authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Location, "AA03");
			authorisation.CreateAuthorisationRule("ZZZ", "AA04");
			authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Location, "AA05");

			CombineAssertions(() =>
			{
				AssertEquals("DestinationTrader Organisation is null", 0, arrivalMovementHeader.Lookups.PortsOfUnloadingList.Count);

				nctsHeader.DestinationTrader.E2_OA_Address = orgHeader.MainAddress.PK;
				AssertEquals("DestinationCustomsOfficeCode is empty", 0, arrivalMovementHeader.Lookups.PortsOfUnloadingList.Count);

				arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "DE123456";
				AssertEquals("Invalid DestinationCustomsOfficeCode", 0, arrivalMovementHeader.Lookups.PortsOfUnloadingList.Count);

				arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "DE003202";
				var portsOfUnloadingList = arrivalMovementHeader.Lookups.PortsOfUnloadingList;
				AssertEquals("All valid", "AA02, AA03", portsOfUnloadingList.CodesAsString);
				AssertSame("List is cached", portsOfUnloadingList, arrivalMovementHeader.Lookups.PortsOfUnloadingList);
			});
		}

		public void TestNctsTransitStatusList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "CAN, CL1, CL3, DIS, CD4, CD2, RFR, RFA, UCN, URP, UAP, ULR, DDR, CRC", nctsHeader.ArrivalMovementHeader.Lookups.NctsTransitStatusList.CodesAsString);
				AssertSame("Cached", nctsHeader.ArrivalMovementHeader.Lookups.NctsTransitStatusList, nctsHeader.ArrivalMovementHeader.Lookups.NctsTransitStatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		}
		NctsHeader nctsHeader;
		NctsArrivalMovementHeader arrivalMovementHeader;
	}
}
